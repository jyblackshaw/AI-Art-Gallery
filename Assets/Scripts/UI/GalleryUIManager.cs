using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using DG.Tweening; // Optional: for smooth animations

public class GalleryUIManager : MonoBehaviour
{
    [Header("Welcome Screen")]
    [SerializeField] private GameObject welcomePanel;
    [SerializeField] private TextMeshProUGUI welcomeText;
    [SerializeField] private TextMeshProUGUI explanationText;
    [SerializeField] private TMP_InputField themeInput;
    [SerializeField] private Button startButton;
    [SerializeField] private Button randomThemeButton;

    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Image loadingSpinner;

    [Header("Theme Suggestions")]
    [SerializeField]
    private string[] sampleThemes = new string[]
    {
        "Soviet Megastructures",
        "Memories of Tomorrow",
        "Urban Nature Fusion",
        "Digital Emotions",
        "Cosmic Horror",
        "Ancient Aliens",
        "Ethereal Landscapes"
    };

    private InitializeGallery galleryInitializer;

    private void Start()
    {
        galleryInitializer = FindObjectOfType<InitializeGallery>();
        SetupUI();
        ShowWelcomeScreen();
    }

    private void SetupUI()
    {
        welcomeText.text = "Welcome to Dream Gallery";

        explanationText.text = "Experience a unique AI-generated art gallery based on your chosen theme. " +
                             "Each artwork will be created specifically for your gallery, forming a cohesive " +
                             "narrative around your theme.\n\n" +
                             "Enter a theme below or try our random theme generator for inspiration.";

        themeInput.placeholder.GetComponent<TextMeshProUGUI>().text = "Enter your gallery theme...";

        startButton.onClick.AddListener(OnStartGalleryClicked);
        randomThemeButton.onClick.AddListener(GenerateRandomTheme);

        // Input validation
        themeInput.onValueChanged.AddListener(ValidateInput);
        startButton.interactable = false;
    }

    private void ValidateInput(string input)
    {
        startButton.interactable = !string.IsNullOrWhiteSpace(input) && input.Length >= 3;
    }

    private void ShowWelcomeScreen()
    {
        loadingPanel.SetActive(false);
        welcomePanel.SetActive(true);

        // Optional: Animate elements in
        CanvasGroup[] elements = welcomePanel.GetComponentsInChildren<CanvasGroup>();
        foreach (var element in elements)
        {
            element.alpha = 0;
            element.DOFade(1, 1f).SetDelay(0.2f);
        }
    }

    private void GenerateRandomTheme()
    {
        string randomTheme = sampleThemes[UnityEngine.Random.Range(0, sampleThemes.Length)];
        themeInput.text = randomTheme;
    }

    private async void OnStartGalleryClicked()
    {
        string theme = themeInput.text.Trim();
        if (string.IsNullOrEmpty(theme)) return;

        welcomePanel.SetActive(false);
        ShowLoadingScreen();

        try
        {
            await galleryInitializer.CreateGallery(theme);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error initializing gallery: {e.Message}");
            // Show error UI
        }
    }

    public void ShowLoadingScreen()
    {
        loadingPanel.SetActive(true);
        StartCoroutine(AnimateLoadingScreen());
    }

    public void UpdateLoadingProgress(string status, float progress)
    {
        loadingText.text = status;
        progressBar.fillAmount = progress;
        progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
    }

    private IEnumerator AnimateLoadingScreen()
    {
        float rotation = 0f;
        while (loadingPanel.activeSelf)
        {
            rotation -= 360f * Time.deltaTime; // Rotate once per second
            loadingSpinner.transform.rotation = Quaternion.Euler(0, 0, rotation);
            yield return null;
        }
    }

    public void HideLoadingScreen()
    {
        loadingPanel.SetActive(false);
    }
}
