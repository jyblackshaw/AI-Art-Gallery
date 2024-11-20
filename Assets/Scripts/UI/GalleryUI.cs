using UnityEngine;
using UnityEngine.UIElements;
using System;
using KinematicCharacterController.Walkthrough.AddingImpulses;
using Unity.Cinemachine;

public class GalleryUI : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private GameObject player;
    [SerializeField] private InitializeGallery initalizeGallery;

    [Header("Camera")]
    [SerializeField] private Camera startMenuCamera;
    [SerializeField] private CinemachineCamera dollyCinemaCamera;
    [SerializeField] private Camera dollyGameCamera;
    [SerializeField] public Camera playerCamera;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource ambience;
    [SerializeField] private AudioSource music;

    private string[] sampleThemes = new string[]
    {
        "Soviet Megastructures",
        "Lost Da Vinci Manuscripts",
        "Futuristic Spacecraft",
        "Steampunk Inventions",
        "Deep Sea Discoveries",
        "Cyberpunk Cities",
        "Mythological Creatures",
        "Urban Nature Fusion",
        "Battle Cats",
        "Cosmic Horror",
        "Ancient Civilizations",
        "Gothic Period",
        "Abstract"
    };

    private VisualElement welcomePanel;
    private VisualElement loadingPanel;
    private TextField themeInput;
    private Button startButton;
    private Label loadingStatus;
    private ProgressBar loadingProgress;

    private VisualElement confirmPanel;

    private int lastThemeIndex = -1;

    private void Start()
    {
        // Set Cameras (start menu):
        startMenuCamera.enabled = true;
        dollyCinemaCamera.enabled = false;
        dollyGameCamera.enabled = false;
        playerCamera.enabled = false;

        document = GetComponent<UIDocument>();
        if (document == null)
        {
            document = gameObject.AddComponent<UIDocument>();
        }

        if (initalizeGallery != null)
        {
            initalizeGallery.OnProgressUpdated += UpdateLoadingProgress;
        }

        player.SetActive(false);

        CreateUI();
    }

    private void CreateUI()
    {
        var root = document.rootVisualElement;

        // Create panels
        welcomePanel = CreateWelcomePanel();
        confirmPanel = CreateConfirmPanel();
        loadingPanel = CreateLoadingPanel();

        // Add to root
        root.Add(welcomePanel);
        root.Add(confirmPanel);
        root.Add(loadingPanel);

        // Initially hide loading and confirm panels
        loadingPanel.style.display = DisplayStyle.None;
        confirmPanel.style.display = DisplayStyle.None;

        root.style.width = Length.Percent(100);
        root.style.height = Length.Percent(100);
    }

    private VisualElement CreateConfirmPanel()
    {
        var panel = new VisualElement
        {
            name = "confirm-panel"
        };

        // Styles for the confirm panel (same background as welcome panel)
        panel.style.width = Length.Percent(100);
        panel.style.height = Length.Percent(100);
        panel.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0.9f));
        panel.style.alignItems = Align.Center;
        panel.style.justifyContent = Justify.Center;

        // Create content container
        var container = new VisualElement
        {
            name = "content-container"
        };
        container.style.width = 600;
        container.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0.9f));
        container.style.paddingTop = 20;
        container.style.paddingBottom = 20;
        container.style.paddingLeft = 20;
        container.style.paddingRight = 20;

        // Header text
        var headerText = new Label("Please Note")
        {
            name = "confirm-header"
        };
        headerText.style.fontSize = 24;
        headerText.style.color = new StyleColor(Color.white);
        headerText.style.unityTextAlign = TextAnchor.MiddleCenter;
        headerText.style.marginBottom = 20;

        // Explanation text
        var explanationText = new Label(
            "Due to OpenAI API rate limits, generating your gallery will take approximately 5-7 minutes. " +
            "During this time, you'll see a loading screen with progress updates. " +
            "Please be patient while we create your unique gallery experience."
        )
        {
            name = "confirm-explanation"
        };
        explanationText.style.color = new StyleColor(new Color(0.8f, 0.8f, 0.8f, 1));
        explanationText.style.whiteSpace = WhiteSpace.Normal;
        explanationText.style.marginBottom = 30;
        explanationText.style.fontSize = 16;

        // Button container for layout
        var buttonContainer = new VisualElement
        {
            name = "button-container"
        };
        buttonContainer.style.flexDirection = FlexDirection.Row;
        buttonContainer.style.justifyContent = Justify.Center;
        buttonContainer.style.width = Length.Percent(100);

        // Confirm button
        var confirmButton = new Button(() => BeginGalleryGeneration())
        {
            text = "Start Generation",
            name = "confirm-button"
        };
        confirmButton.style.width = 200;
        confirmButton.style.height = 50;
        confirmButton.style.backgroundColor = new StyleColor(Color.white);
        confirmButton.style.color = new StyleColor(Color.black);

        // Back button
        var backButton = new Button(() => ShowWelcomeScreen())
        {
            text = "Back",
            name = "back-button"
        };
        backButton.style.width = 200;
        backButton.style.height = 50;
        backButton.style.marginRight = 20;
        backButton.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f, 1));
        backButton.style.color = new StyleColor(Color.white);

        buttonContainer.Add(backButton);
        buttonContainer.Add(confirmButton);

        // Add all elements to container
        container.Add(headerText);
        container.Add(explanationText);
        container.Add(buttonContainer);

        panel.Add(container);

        return panel;
    }

    private void ShowConfirmScreen()
    {
        welcomePanel.style.display = DisplayStyle.None;
        confirmPanel.style.display = DisplayStyle.Flex;
        loadingPanel.style.display = DisplayStyle.None;
    }


    private VisualElement CreateWelcomePanel()
    {
        var panel = new VisualElement
        {
            name = "welcome-panel"
        };

        // Styles for the welcome panel
        panel.style.width = Length.Percent(100);
        panel.style.height = Length.Percent(100);
        panel.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0.9f));
        panel.style.alignItems = Align.Center;
        panel.style.justifyContent = Justify.Center;

        // Create content container
        var container = new VisualElement
        {
            name = "content-container"
        };
        container.style.width = 600;
        container.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0.6f));
        container.style.paddingTop = 20;
        container.style.paddingBottom = 20;
        container.style.paddingLeft = 20;
        container.style.paddingRight = 20;

        // Welcome text
        var welcomeText = new Label("Welcome to Dream Gallery")
        {
            name = "welcome-text"
        };
        welcomeText.style.fontSize = 32;
        welcomeText.style.color = new StyleColor(Color.white);
        welcomeText.style.unityTextAlign = TextAnchor.MiddleCenter;
        welcomeText.style.marginBottom = 20;

        // Explanation text
        var explanationText = new Label(
            "Experience a unique AI-generated art gallery based on your chosen theme. " +
            "Each artwork will be created specifically for your gallery, forming a cohesive " +
            "narrative around your theme."
        )
        {
            name = "explanation-text"
        };
        explanationText.style.color = new StyleColor(new Color(0.8f, 0.8f, 0.8f, 1));
        explanationText.style.whiteSpace = WhiteSpace.Normal;
        explanationText.style.marginBottom = 30;

        // Theme input container
        var inputContainer = new VisualElement
        {
            name = "input-container"
        };
        inputContainer.style.flexDirection = FlexDirection.Row;
        inputContainer.style.marginBottom = 20;

        // Theme input
        themeInput = new TextField()
        {
            name = "theme-input",
            label = ""
        };
        themeInput.style.flexGrow = 1;
        themeInput.style.marginRight = 10;
        themeInput.AddToClassList("theme-input-field");
        //themeInput.SetValueWithoutNotify(""); // Clear initial value
        //themeInput.Q(TextField.inputUssClassName).style.backgroundColor = new StyleColor(new Color(1, 1, 1, 0.1f));

        // Add placeholder text using Label
        var placeholderText = new Label("Enter your gallery theme...");
        placeholderText.AddToClassList("placeholder-text");
        placeholderText.style.color = new StyleColor(new Color(0.8f, 0.8f, 0.8f, 1));
        placeholderText.style.whiteSpace = WhiteSpace.Normal;
        themeInput.Add(placeholderText);

        // Random theme button
        var randomButton = new Button(() => GenerateRandomTheme())
        {
            text = "Random",
            name = "random-button"
        };
        randomButton.style.width = 100;
        randomButton.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f, 1));
        randomButton.style.color = new StyleColor(Color.white);

        inputContainer.Add(themeInput);
        inputContainer.Add(randomButton);

        // Start button
        startButton = new Button(() => StartGalleryGeneration())
        {
            text = "Generate Gallery",
            name = "start-button"
        };
        startButton.style.marginTop = 20;
        startButton.style.backgroundColor = new StyleColor(Color.white);
        startButton.style.color = new StyleColor(Color.black);
        startButton.style.height = 50;
        startButton.SetEnabled(false);

        // Add all elements to container
        container.Add(welcomeText);
        container.Add(explanationText);
        container.Add(inputContainer);
        container.Add(startButton);

        panel.Add(container);

        // Add input validation
        themeInput.RegisterValueChangedCallback(evt =>
        {
            startButton.SetEnabled(!string.IsNullOrWhiteSpace(evt.newValue) && evt.newValue.Length >= 3);
            placeholderText.style.display = string.IsNullOrEmpty(evt.newValue) ?
                DisplayStyle.Flex : DisplayStyle.None;
        });

        return panel;
    }

    private VisualElement CreateLoadingPanel()
    {
        var panel = new VisualElement
        {
            name = "loading-panel"
        };

        panel.style.width = Length.Percent(100);
        panel.style.height = Length.Percent(100);
        panel.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0.7f));
        panel.style.alignItems = Align.Center;
        panel.style.justifyContent = Justify.Center;

        var container = new VisualElement
        {
            name = "loading-container"
        };
        container.style.width = 400;
        container.style.alignItems = Align.Center;

        // Loading spinner
        var spinner = new VisualElement
        {
            name = "loading-spinner"
        };
        spinner.style.width = 64;
        spinner.style.height = 64;
        spinner.style.marginBottom = 30;

        // Add spinner class for USS animation
        spinner.AddToClassList("rotating");

        // Loading status
        loadingStatus = new Label("Initializing gallery generation...")
        {
            name = "loading-status"
        };
        loadingStatus.style.color = new StyleColor(Color.white);
        loadingStatus.style.fontSize = 18;
        loadingStatus.style.marginBottom = 20;

        // Progress bar
        loadingProgress = new ProgressBar
        {
            name = "loading-progress",
            title = "Progress"
        };
        loadingProgress.style.width = Length.Percent(100);
        loadingProgress.style.height = 6;
        loadingProgress.lowValue = 0;
        loadingProgress.highValue = 1;

        container.Add(spinner);
        container.Add(loadingStatus);
        container.Add(loadingProgress);

        panel.Add(container);

        return panel;
    }

    private void GenerateRandomTheme()
    {
        if (sampleThemes.Length <= 1)
        {
            // If we only have 0 or 1 themes, just use random normally
            int randomIndex = UnityEngine.Random.Range(0, sampleThemes.Length);
            themeInput.value = sampleThemes[randomIndex];
            lastThemeIndex = randomIndex;
            return;
        }

        // Generate a random index that's different from the last one
        int newIndex;
        do
        {
            newIndex = UnityEngine.Random.Range(0, sampleThemes.Length);
        } while (newIndex == lastThemeIndex);

        themeInput.value = sampleThemes[newIndex];
        lastThemeIndex = newIndex;
    }

    //private async void StartGalleryGeneration()
    //{
    //    try
    //    {
    //        welcomePanel.style.display = DisplayStyle.None;
    //        loadingPanel.style.display = DisplayStyle.Flex;

    //        // Update Cameras (dolly + brain)
    //        startMenuCamera.enabled = false;
    //        dollyCinemaCamera.enabled = true;
    //        dollyGameCamera.enabled = true;

    //        // Reset progress bar to 0
    //        UpdateLoadingProgress("Starting gallery generation...", 0f);

    //        string theme = themeInput?.value ?? string.Empty;

    //        // Start the gallery generation process
    //        bool success = await initalizeGallery.CreateGallery(theme);

    //        if (success)
    //        {
    //            // Hide loading panel after completion
    //            loadingPanel.style.display = DisplayStyle.None;
    //            player.SetActive(true);

    //            // Update Cameras (player)
    //            dollyCinemaCamera.enabled = false;
    //            dollyGameCamera.enabled = false;
    //            playerCamera.enabled = true;

    //            // Enable Audio Sources
    //            ambience.enabled = true;
    //            music.enabled = true;
    //        }
    //        else
    //        {
    //            // Show error and return to welcome panel
    //            Debug.LogError("Gallery generation failed");
    //            loadingPanel.style.display = DisplayStyle.None;
    //            welcomePanel.style.display = DisplayStyle.Flex;
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.LogError($"Error generating gallery: {e.Message}");
    //        loadingPanel.style.display = DisplayStyle.None;
    //        welcomePanel.style.display = DisplayStyle.Flex;
    //    }
    //}

    private void StartGalleryGeneration()
    {
        ShowConfirmScreen();
    }


    private async void BeginGalleryGeneration()
    {
        try
        {
            confirmPanel.style.display = DisplayStyle.None;
            loadingPanel.style.display = DisplayStyle.Flex;

            // Update Cameras (dolly + brain)
            startMenuCamera.enabled = false;
            dollyCinemaCamera.enabled = true;
            dollyGameCamera.enabled = true;

            // Reset progress bar to 0
            UpdateLoadingProgress("Starting gallery generation...", 0f);

            string theme = themeInput?.value ?? string.Empty;

            // Start the gallery generation process
            bool success = await initalizeGallery.CreateGallery(theme);

            if (success)
            {
                // Hide loading panel after completion
                loadingPanel.style.display = DisplayStyle.None;
                player.SetActive(true);

                // Update Cameras (player)
                dollyCinemaCamera.enabled = false;
                dollyGameCamera.enabled = false;
                playerCamera.enabled = true;

                // Enable Audio Sources
                ambience.enabled = true;
                music.enabled = true;
            }
            else
            {
                // Show error and return to welcome panel
                Debug.LogError("Gallery generation failed");
                loadingPanel.style.display = DisplayStyle.None;
                welcomePanel.style.display = DisplayStyle.Flex;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error generating gallery: {e.Message}");
            loadingPanel.style.display = DisplayStyle.None;
            welcomePanel.style.display = DisplayStyle.Flex;
        }
    }

    public void UpdateLoadingProgress(string status, float progress)
    {
        loadingStatus.text = status;
        loadingProgress.value = progress;
    }

    public void ShowWelcomeScreen()
    {
        welcomePanel.style.display = DisplayStyle.Flex;
        confirmPanel.style.display = DisplayStyle.None;
        loadingPanel.style.display = DisplayStyle.None;
    }

    public void ShowLoadingScreen()
    {
        welcomePanel.style.display = DisplayStyle.None;
        loadingPanel.style.display = DisplayStyle.Flex;
    }

    private void OnDestroy()
    {
        // Unsubscribe when destroyed
        if (initalizeGallery != null)
        {
            initalizeGallery.OnProgressUpdated -= UpdateLoadingProgress;
        }
    }

}