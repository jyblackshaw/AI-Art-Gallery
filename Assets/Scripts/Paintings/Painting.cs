using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System.Threading;
using System.Threading.Tasks;

public class Painting : MonoBehaviour
{
    [System.Serializable]
    public class UIAnimationSettings
    {
        public float duration = 0.3f;
        public Ease showEase = Ease.OutBack;
        public Ease hideEase = Ease.InBack;
        public float startScale = 0.3f;
        public float overshoot = 1.2f;
        public float fadeDelay = 0.1f;
        public float textStagger = 0.05f;

        //typewriter effect
        public float charactersPerSecond = 30f;
        public float initialDelay = 0.2f;
        public Ease typewriterEase = Ease.Linear;
    }

    [Header("Core References")]
    [SerializeField] protected PaintingSettings settings;
    [SerializeField] protected Canvas worldCanvas;
    [SerializeField] protected Material artworkMaterial;
    [SerializeField] protected Camera playerCamera;

    [Header("Animation")]
    [SerializeField] protected UIAnimationSettings animSettings = new UIAnimationSettings();

    [Header("Interaction")]
    [SerializeField] protected float viewDistance = 3f;
    [SerializeField] bool diag;

    // Protected UI References
    protected TextMeshProUGUI titleText;
    protected TextMeshProUGUI descriptionText;
    protected GameObject infoPanel;
    protected RectTransform uiRectTransform;

    // Protected State
    protected bool isInRange = false;
    protected Vector3 originalScale;
    protected Sequence currentAnimation;
    protected string fullDescription = "";
    protected bool hasCompletedTypewriter = false;
    private bool isAnimating = false;
    private bool isTransitioning = false;
    private CancellationTokenSource animationCancellation;


    private GameObject positionHelper;

    protected virtual void Start()
    {
        InitializeComponents();
        SetupUI();
    }

    protected virtual void Update()
    {
        CheckPlayerDistance();
        //UpdateUIPosition();
    }

    protected virtual void OnDestroy()
    {
        CleanupResources();
    }

    protected virtual void InitializeComponents()
    {
        if (artworkMaterial == null)
        {
            artworkMaterial = GetComponent<Renderer>()?.material;
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    protected virtual void SetupUI()
    {
        if (settings?.uiPrefab == null || worldCanvas == null) return;

        try
        {
            // Create helper as child first (this worked before)
            positionHelper = new GameObject("UI_Position_Helper");
            positionHelper.transform.parent = transform;
            positionHelper.transform.localPosition = -Vector3.right * settings.uiOffset;

            if (diag)
            {
                positionHelper.transform.localPosition += .05f * -Vector3.right;
                positionHelper.transform.localPosition += .7f * Vector3.forward;
            }

            // Create UI at helper's position
            GameObject uiInstance = Instantiate(settings.uiPrefab, worldCanvas.transform);
            uiInstance.transform.position = positionHelper.transform.position;
            uiInstance.transform.rotation = this.transform.rotation;

            if (diag)
            {
                uiInstance.transform.rotation *= Quaternion.Euler(0, 45, 0);
            }

            uiRectTransform = uiInstance.GetComponent<RectTransform>();
            infoPanel = uiInstance.transform.Find("InfoPanel")?.gameObject;

            SetupTextComponents();
            SetupAnimationComponents();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error setting up UI for painting {gameObject.name}: {e.Message}");
        }
    }



    protected virtual void SetupTextComponents()
    {
        if (infoPanel == null) return;

        titleText = infoPanel.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
        descriptionText = infoPanel.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();

        titleText.GetComponent<TextMeshProUGUI>().fontSharedMaterial.EnableKeyword("CULL_BACK");
        //descriptionText.GetComponent<TextMeshPro>().fontSharedMaterial.EnableKeyword("CULL_BACK");/
    }

    protected virtual void SetupAnimationComponents()
    {
        if (infoPanel == null) return;

        originalScale = infoPanel.transform.localScale;
        infoPanel.transform.localScale = Vector3.zero;

        // Initialize all UI elements to transparent
        var texts = infoPanel.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var text in texts)
        {
            var color = text.color;
            color.a = 0;
            text.color = color;
        }

        var images = infoPanel.GetComponentsInChildren<UnityEngine.UI.Image>();
        foreach (var image in images)
        {
            var color = image.color;
            color.a = 0;
            image.color = color;
        }

        infoPanel.SetActive(false);
    }

    protected virtual void CheckPlayerDistance()
    {
        if (playerCamera == null) return;

        float distance = Vector3.Distance(transform.position, playerCamera.transform.position);
        bool wasInRange = isInRange;
        isInRange = distance <= viewDistance;

        // If state changed, update UI
        if (wasInRange != isInRange)
        {
            // Kill any existing animations to prevent conflicts
            currentAnimation?.Kill();

            if (isInRange)
            {
                ShowInfoPanel();
            }
            else
            {
                HideInfoPanel();
            }
        }
    }


    protected virtual void ShowInfoPanel()
    {
        if (infoPanel == null || settings == null) return;

        // Kill any existing animations
        currentAnimation?.Kill();

        // Ensure panel is active and visible
        infoPanel.SetActive(true);
        infoPanel.transform.localScale = Vector3.zero;

        // Get UI components
        var texts = infoPanel.GetComponentsInChildren<TextMeshProUGUI>();
        var images = infoPanel.GetComponentsInChildren<UnityEngine.UI.Image>();

        // Reset all colors to transparent
        foreach (var image in images)
        {
            if (image != null)
            {
                image.color = new Color(1, 1, 1, 0);
            }
        }

        foreach (var text in texts)
        {
            if (text != null)
            {
                text.color = text == titleText ?
                    settings.titleColor * new Color(1, 1, 1, 0) :
                    settings.descriptionColor * new Color(1, 1, 1, 0);
            }
        }

        // Clear description text if we haven't completed typewriter
        if (!hasCompletedTypewriter && descriptionText != null)
        {
            descriptionText.text = "";
        }

        // Create animation sequence
        currentAnimation = DOTween.Sequence()
            .SetAutoKill(true)
            .SetRecyclable(true);

        // Scale animation
        currentAnimation.Append(
            infoPanel.transform
                .DOScale(originalScale, settings.animationDuration)
                .SetEase(settings.showEase)
        );

        // Fade in images
        if (images.Length > 0)
        {
            currentAnimation.Insert(settings.fadeDelay,
                DOTween.To(
                    () => images[0].color.a,
                    (x) =>
                    {
                        var c = new Color(1, 1, 1, x);
                        foreach (var img in images)
                        {
                            if (img != null) img.color = c;
                        }
                    },
                    1f,
                    settings.animationDuration
                )
            );
        }

        // Fade in title
        if (titleText != null)
        {
            currentAnimation.Insert(settings.fadeDelay,
                DOTween.To(
                    () => titleText.color.a,
                    (x) => titleText.color = settings.titleColor * new Color(1, 1, 1, x),
                    1f,
                    settings.animationDuration
                )
            );
        }

        // Handle description text
        if (descriptionText != null && !string.IsNullOrEmpty(fullDescription))
        {
            // If we've already shown the typewriter effect once, just fade in
            if (hasCompletedTypewriter)
            {
                descriptionText.text = fullDescription;
                currentAnimation.Insert(settings.fadeDelay,
                    DOTween.To(
                        () => descriptionText.color.a,
                        (x) => descriptionText.color = settings.descriptionColor * new Color(1, 1, 1, x),
                        1f,
                        settings.animationDuration
                    )
                );
            }
            // Otherwise, do the typewriter effect
            else
            {
                // Fade in the text component first
                currentAnimation.Insert(settings.fadeDelay,
                    DOTween.To(
                        () => descriptionText.color.a,
                        (x) => descriptionText.color = settings.descriptionColor * new Color(1, 1, 1, x),
                        1f,
                        settings.animationDuration
                    )
                );

                // Calculate typewriter duration
                float typeDuration = fullDescription.Length / settings.charactersPerSecond;

                // Add delay before typewriter starts
                currentAnimation.AppendInterval(settings.typewriterDelay);

                // Add the typewriter effect
                currentAnimation.Append(
                    DOTween.To(
                        () => 0f,
                        (x) =>
                        {
                            if (descriptionText != null)
                            {
                                int characterCount = Mathf.RoundToInt(x * fullDescription.Length);
                                descriptionText.text = fullDescription.Substring(0, characterCount);
                            }
                        },
                        1f,
                        typeDuration
                    )
                    .SetEase(settings.typewriterEase)
                );

                // Mark as completed when done
                currentAnimation.OnComplete(() =>
                {
                    hasCompletedTypewriter = true;
                    if (descriptionText != null)
                    {
                        descriptionText.text = fullDescription;
                    }
                });
            }
        }
    }
    protected virtual void HideInfoPanel()
    {
        if (infoPanel == null) return;

        // Kill any existing animations
        currentAnimation?.Kill();

        currentAnimation = DOTween.Sequence()
            .SetAutoKill(true)
            .SetRecyclable(true);

        // Scale to zero
        currentAnimation.Append(
            infoPanel.transform
                .DOScale(Vector3.zero, settings.animationDuration)
                .SetEase(settings.hideEase)
        );

        // Deactivate panel when animation completes
        currentAnimation.OnComplete(() => {
            if (infoPanel != null)
            {
                infoPanel.SetActive(false);
            }
        });
    }

    protected virtual void StartTypewriterEffect()
    {
        if (descriptionText == null || string.IsNullOrEmpty(fullDescription)) return;

        descriptionText.text = "";

        float typeDuration = fullDescription.Length / settings.charactersPerSecond;

        Sequence typewriterSequence = DOTween.Sequence()
            .SetAutoKill(true)
            .SetRecyclable(true);

        typewriterSequence.AppendInterval(settings.typewriterDelay);

        typewriterSequence.Append(
            DOTween.To(() => 0f,
                (x) => {
                    int characterCount = Mathf.RoundToInt(x * fullDescription.Length);
                    descriptionText.text = fullDescription.Substring(0, characterCount);
                },
                1f,
                typeDuration)
            .SetEase(settings.typewriterEase)
        );

        // Set the flag when typewriter completes
        typewriterSequence.OnComplete(() => {
            hasCompletedTypewriter = true;
        });

        currentAnimation.Join(typewriterSequence);
    }

    // Reset the typewriter state when setting new description
    public virtual void SetDescription(string description)
    {
        fullDescription = description;
        hasCompletedTypewriter = false;  // Reset the flag when description changes
        if (descriptionText != null)
        {
            if (!isInRange)
            {
                descriptionText.text = description;
            }
        }
    }

    public virtual void SetTitle(string title)
    {
        if (titleText != null)
        {
            titleText.text = title;
        }
    }

    public virtual void SetArtwork(Texture2D texture)
    {
        if (artworkMaterial != null)
        {
            artworkMaterial.mainTexture = texture;
        }
    }

    protected virtual void CleanupResources()
    {
        currentAnimation?.Kill();

        if (artworkMaterial != null && artworkMaterial.mainTexture != null)
        {
            Destroy(artworkMaterial.mainTexture);
        }

        if (infoPanel != null)
        {
            Destroy(infoPanel.gameObject);
        }
    }
}