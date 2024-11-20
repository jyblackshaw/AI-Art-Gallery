using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenAI;
using UnityEngine.Networking;
using System;

public class InitializeGallery : MonoBehaviour
{
    private readonly Queue<System.DateTime> imageRequestTimes = new Queue<System.DateTime>();
    private readonly int maxRequestsPerMinute = 5;
    private readonly float minDelayBetweenRequests = 12f; // 12 seconds = 5 images per minute
    private int maxRetries = 3;
     float retryDelay = 5f;

    [System.Serializable]
    public class GalleryNarrative
    {
        public string mainStory;
        public List<ArtworkPrompt> artworkPrompts;
    }

    [System.Serializable]
    public class ArtworkPrompt
    {
        public string imagePrompt;
        public string description;
        public string title;
    }

    [SerializeField] private string defaultTheme = "Soviet Megastructures";

    // Event for UI progress updates
    public System.Action<string, float> OnProgressUpdated;

    private List<Painting> paintings = new List<Painting>();
    private OpenAIApi openai = new OpenAIApi();
    private bool isGenerating = false;

    private readonly string narrativePrompt = @"
Create a cohesive narrative for an art gallery based on the theme: '{0}'.
Format your response EXACTLY as a valid JSON object with this structure, ensuring all quotes are properly escaped:
{{
    ""mainStory"": ""The overarching narrative or concept that ties the gallery together, explaining the significance of the theme and how the artworks interact to create a meaningful experience."",
    ""artworkPrompts"": [
        {{
            ""title"": ""Title of the artwork"",
            ""imagePrompt"": ""Detailed prompt for DALL-E image generation that connects to the overall narrative. Include specific visual elements, style, mood, and composition details."",
            ""description"": ""Write a compelling story about this piece that captures its essence. Move beyond traditional gallery descriptions. Each description should take a unique approach - perhaps a personal anecdote, a philosophical musing, a historical parallel, a scientific observation, or even a poetic interpretation. Include surprising connections, provocative questions, or unexpected perspectives. Avoid generic phrases like 'this piece represents' or 'this work explores.' Instead, make the reader feel something, discover something, or question something. 75 to 150 words.""
        }}
    ]
}}

Generate exactly {1} different artwork prompts that weave together to create an immersive gallery experience. Each artwork should feel like a distinct chapter in a larger story, with its own voice and emotional resonance.

Guidelines for the mainStory:
- Provide context for the theme's significance
- Explain how the artworks interconnect
- Set the emotional and intellectual tone for the experience
- Include any relevant historical or cultural context
- Keep it concise but impactful (100-200 words)

Guidelines for descriptions:
- Never start with ""This piece"" or ""This artwork""
- Incorporate sensory details and emotional responses
- Use varied sentence structures and rhythms
- Include specific, vivid details rather than abstract concepts
- Draw unexpected connections to history, science, mythology, or contemporary life
- Sometimes pose thought-provoking questions
- Occasionally incorporate elements of mystery or ambiguity
- Vary between personal and universal perspectives
- Each description should be unique in its approach and voice
- Mix different writing styles - some descriptions might be introspective and intimate, others bold and assertive, others mysterious and questioning

IMPORTANT FORMAT NOTES:
1. Ensure all quotes within text are properly escaped using \"" 
2. Maintain valid JSON structure - no trailing commas
3. Keep the exact structure shown above
4. Include all required fields for each artwork
5. Generate exactly the requested number of artworks ({1})
6. Use proper JSON array formatting with square brackets
7. Ensure all JSON objects are properly closed with curly braces
8. Do not include any text outside the JSON structure
9. Do not include markdown formatting (```json)
10. Verify the JSON is valid before completing the response";

    public async Task<bool> CreateGallery(string theme = "")
    {
        if (isGenerating) return false;
        isGenerating = true;

        try
        {
            if (string.IsNullOrEmpty(theme)) theme = defaultTheme;

            // Initialize collections - Modified to avoid double-counting
            UpdateProgress("Finding paintings in scene...", 0.1f);

            // Get all paintings first
            var allPaintings = FindObjectsByType<Painting>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).ToList();

            // Separate regular paintings from big paintings
            paintings = allPaintings.OfType<Painting>().ToList();

            int totalArtworks = paintings.Count;

            if (totalArtworks == 0)
            {
                Debug.LogError("No paintings found in scene");
                return false;
            }

            // Rest of your code remains the same
            UpdateProgress("Generating gallery narrative...", 0.2f);
            var narrative = await GenerateNarrative(theme, totalArtworks);
            if (narrative == null || narrative.artworkPrompts == null)
            {
                Debug.LogError("Failed to generate narrative");
                return false;
            }

            // Save the full gallery narrative
            //narrative.SaveGalleryNarrative(theme);

            // Generate and assign artwork
            await GenerateAndAssignArtworks(narrative);

            return true;

            if (narrative == null || narrative.artworkPrompts == null)
            {
                Debug.LogError("Failed to generate narrative");
                return false;
            }

            // Generate and assign artwork
            await GenerateAndAssignArtworks(narrative);

            UpdateProgress("Gallery generation complete!", 1f);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in CreateGallery: {e.Message}");
            return false;
        }
        finally
        {
            isGenerating = false;
        }
    }

    private async Task<GalleryNarrative> GenerateNarrative(string theme, int artworkCount)
    {
        try
        {
            UpdateProgress("Connecting to OpenAI...", 0.3f);

            var completion = await openai.CreateChatCompletion(new CreateChatCompletionRequest
            {
                Model = "gpt-4",
                Temperature = 0.7f,
                MaxTokens = 4000,
                Messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    Role = "system",
                    Content = @"You are a curator and art critic specializing in creating immersive gallery experiences. 
                        Generate detailed, thought-provoking artwork descriptions (75-150 words each) that go beyond traditional gallery text.
                        Return your response as clean, properly formatted JSON with properly escaped quotes."
                },
                new ChatMessage
                {
                    Role = "user",
                    Content = string.Format(narrativePrompt, theme, artworkCount)  // Use the actual narrative prompt
                }
            }
            });

            if (completion.Choices != null && completion.Choices.Count > 0)
            {
                UpdateProgress("Processing narrative...", 0.4f);
                string jsonResponse = completion.Choices[0].Message.Content;

                // Log the raw response
                Debug.Log($"Raw GPT response:\n{jsonResponse}");

                // Clean the JSON response
                jsonResponse = CleanJsonResponse(jsonResponse);
                Debug.Log($"Cleaned JSON:\n{jsonResponse}");

                try
                {
                    // Remove any escaped quotes that might have been added during cleaning
                    jsonResponse = jsonResponse.Replace("\\\"", "\"");

                    // Remove any markdown formatting
                    jsonResponse = System.Text.RegularExpressions.Regex.Replace(jsonResponse, "```json\\s*|```\\s*", "");

                    // Trim whitespace
                    jsonResponse = jsonResponse.Trim();

                    var narrative = JsonUtility.FromJson<GalleryNarrative>(jsonResponse);

                    if (narrative == null)
                    {
                        Debug.LogError("Narrative parsed as null");
                        return null;
                    }

                    // Validate the parsed object
                    if (string.IsNullOrEmpty(narrative.mainStory))
                    {
                        Debug.LogError("Main story is null or empty");
                        return null;
                    }

                    if (narrative.artworkPrompts == null || narrative.artworkPrompts.Count == 0)
                    {
                        Debug.LogError("No artwork prompts found in response");
                        return null;
                    }

                    return narrative;
                }
                catch (System.Exception parseException)
                {
                    Debug.LogError($"JSON Parse Error Details:\nMessage: {parseException.Message}\nStack Trace: {parseException.StackTrace}");

                    // Try parsing with Newtonsoft.Json if available
                    try
                    {
#if UNITY_EDITOR
                        // This is a fallback parse attempt using string manipulation
                        string tempJson = jsonResponse.Replace("\\\"", "\"")
                                                    .Replace("\"{", "{")
                                                    .Replace("}\"", "}")
                                                    .Trim();
                        
                        if (tempJson.StartsWith("\"") && tempJson.EndsWith("\""))
                        {
                            tempJson = tempJson.Substring(1, tempJson.Length - 2);
                        }
                        
                        var narrative = JsonUtility.FromJson<GalleryNarrative>(tempJson);
                        if (narrative != null && !string.IsNullOrEmpty(narrative.mainStory))
                        {
                            return narrative;
                        }
#endif
                    }
                    catch (System.Exception fallbackException)
                    {
                        Debug.LogError($"Fallback parsing also failed: {fallbackException.Message}");
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error generating narrative: {e.Message}\nStack trace: {e.StackTrace}");
            UpdateProgress("Error generating narrative", 0f);
        }

        return null;
    }

    private string CleanJsonResponse(string json)
    {
        if (string.IsNullOrEmpty(json)) return json;

        try
        {
            // Remove any markdown formatting
            json = System.Text.RegularExpressions.Regex.Replace(json, "```json\\s*|```\\s*", "");

            // Remove any BOM or invalid characters
            json = System.Text.RegularExpressions.Regex.Replace(json, @"[\u0000-\u001F\u007F-\u009F]", "");

            // Trim whitespace
            json = json.Trim();

            // Handle cases where the entire JSON is wrapped in quotes
            if (json.StartsWith("\"") && json.EndsWith("\""))
            {
                json = json.Substring(1, json.Length - 2);
            }

            // Remove extra escaping of quotes
            json = json.Replace("\\\\\"", "\"").Replace("\\\"", "\"");

            return json;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error cleaning JSON: {e.Message}");
            return json;
        }
    }

    private void ValidateJsonStructure(string json)
    {
        try
        {
            // Check for basic JSON structure
            if (!json.StartsWith("{") || !json.EndsWith("}"))
            {
                Debug.LogError("JSON doesn't have proper object brackets");
                return;
            }

            // Check for required fields
            if (!json.Contains("\"mainStory\""))
            {
                Debug.LogError("Missing mainStory field");
            }

            if (!json.Contains("\"artworkPrompts\""))
            {
                Debug.LogError("Missing artworkPrompts field");
            }

            // Check for array brackets
            if (!json.Contains("[") || !json.Contains("]"))
            {
                Debug.LogError("Missing array brackets for artworkPrompts");
            }

            // Count opening and closing braces
            int openBraces = json.Count(c => c == '{');
            int closeBraces = json.Count(c => c == '}');
            if (openBraces != closeBraces)
            {
                Debug.LogError($"Mismatched braces: {openBraces} opening vs {closeBraces} closing");
            }

            // Check for common JSON syntax errors
            if (json.Contains(",,") || json.EndsWith(",}") || json.Contains(",]"))
            {
                Debug.LogError("Invalid comma usage detected");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error validating JSON structure: {e.Message}");
        }
    }

    private async Task GenerateAndAssignArtworks(GalleryNarrative narrative)
    {
        int promptIndex = 0;
        float baseProgress = 0.4f;
        float progressPerArtwork = 0.6f / paintings.Count;

        // Generate for regular paintings
        for (int i = 0; i < paintings.Count && promptIndex < narrative.artworkPrompts.Count; i++)
        {
            UpdateProgress($"Generating artwork {promptIndex + 1} of {narrative.artworkPrompts.Count}...",
                          baseProgress + (progressPerArtwork * promptIndex));

            // Wait if we need to respect rate limits
            await WaitForRateLimit();

            await GenerateAndAssignArtwork(paintings[i], narrative.artworkPrompts[promptIndex]);
            promptIndex++;
        }
    }

    private async Task WaitForRateLimit()
    {
        // Remove old requests from the queue
        while (imageRequestTimes.Count > 0 &&
               (System.DateTime.Now - imageRequestTimes.Peek()).TotalMinutes >= 1)
        {
            imageRequestTimes.Dequeue();
        }

        // If we've hit the rate limit, wait until we can make another request
        if (imageRequestTimes.Count >= maxRequestsPerMinute)
        {
            var oldestRequest = imageRequestTimes.Peek();
            var timeToWait = oldestRequest.AddMinutes(1) - System.DateTime.Now;

            if (timeToWait.TotalSeconds > 0)
            {
                UpdateProgress($"Rate limit reached. Waiting {timeToWait.TotalSeconds:F0} seconds...", -1);
                await Task.Delay(timeToWait);
            }
        }
        else
        {
            // Always wait at least minDelayBetweenRequests seconds between requests
            await Task.Delay(TimeSpan.FromSeconds(minDelayBetweenRequests));
        }

        // Add this request to the queue
        imageRequestTimes.Enqueue(System.DateTime.Now);
    }

    private async Task GenerateAndAssignArtwork(MonoBehaviour artwork, ArtworkPrompt prompt)
    {
        int retryCount = 0;
        bool success = false;

        while (!success && retryCount < maxRetries)
        {
            try
            {
                // Generate image
                var response = await openai.CreateImage(new CreateImageRequest
                {
                    Prompt = prompt.imagePrompt,
                    Size = ImageSize.Size1024
                });

                if (response.Data != null && response.Data.Count > 0)
                {
                    // Download image
                    using (var request = new UnityWebRequest(response.Data[0].Url))
                    {
                        request.downloadHandler = new DownloadHandlerBuffer();
                        request.SetRequestHeader("Access-Control-Allow-Origin", "*");
                        await request.SendWebRequest();

                        if (request.result != UnityWebRequest.Result.Success)
                        {
                            throw new Exception($"Failed to download image: {request.error}");
                        }

                        // Create texture and assign to material
                        Texture2D texture = new Texture2D(2, 2);
                        if (texture.LoadImage(request.downloadHandler.data))
                        {
                            // Save image to Assets/Records folder
                            SaveImageToRecords(texture, prompt, artwork.name);

                            // Assign to appropriate artwork type
                            if (artwork is Painting painting)
                            {
                                AssignToRegularPainting(painting, texture, prompt);
                            }
                            success = true;
                        }
                        else
                        {
                            throw new Exception("Failed to load image data into texture");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                retryCount++;
                Debug.LogWarning($"Attempt {retryCount} failed: {e.Message}");

                if (retryCount < maxRetries)
                {
                    UpdateProgress($"Retrying image generation ({retryCount}/{maxRetries})...", -1);
                    await Task.Delay(TimeSpan.FromSeconds(retryDelay));
                }
                else
                {
                    Debug.LogError($"Failed to generate image after {maxRetries} attempts. Using fallback texture.");
                }
            }
        }
    }


    private void SaveImageToRecords(Texture2D texture, ArtworkPrompt prompt, string artworkName)
    {
        try
        {
            // Create directory if it doesn't exist
            string directoryPath = "Assets/Records";
            if (!System.IO.Directory.Exists(directoryPath))
            {
                System.IO.Directory.CreateDirectory(directoryPath);
            }

            // Create a subdirectory with timestamp
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string galleryPath = $"{directoryPath}/{timestamp}";
            if (!System.IO.Directory.Exists(galleryPath))
            {
                System.IO.Directory.CreateDirectory(galleryPath);
            }

            // Save the image
            byte[] bytes = texture.EncodeToPNG();
            string sanitizedTitle = SanitizeFileName(prompt.title);
            string filePath = $"{galleryPath}/{sanitizedTitle}_{artworkName}.png";
            System.IO.File.WriteAllBytes(filePath, bytes);

            // Save metadata
            string metadataPath = $"{galleryPath}/{sanitizedTitle}_{artworkName}_metadata.txt";
            string metadata = $"Title: {prompt.title}\n" +
                             $"Description: {prompt.description}\n" +
                             $"Prompt: {prompt.imagePrompt}\n" +
                             $"Generated: {timestamp}\n" +
                             $"Artwork Object: {artworkName}";
            System.IO.File.WriteAllText(metadataPath, metadata);

#if UNITY_EDITOR
            // Refresh the Asset Database to show the new files
            UnityEditor.AssetDatabase.Refresh();
#endif

            Debug.Log($"Saved image and metadata to: {galleryPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving image to records: {e.Message}");
        }
    }

    private string SanitizeFileName(string fileName)
    {
        // Remove invalid characters from filename
        char[] invalid = System.IO.Path.GetInvalidFileNameChars();
        foreach (char c in invalid)
        {
            fileName = fileName.Replace(c, '_');
        }
        return fileName;
    }

    private void AssignToRegularPainting(Painting painting, Texture2D texture, ArtworkPrompt prompt)
    {
        try
        {
            if (texture == null)
            {
                Debug.LogError($"Null texture provided for painting {painting.name}. Using fallback.");
            }

            var material = painting.GetComponent<Renderer>().material;
            material.mainTexture = texture;

            painting.SetTitle(prompt.title);
            Debug.Log($"Setting title: {prompt.title} for painting: {painting.name}");

            painting.SetDescription(prompt.description);
            Debug.Log($"Setting description: {prompt.description} for painting: {painting.name}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error assigning to painting {painting.name}: {e.Message}\n{e.StackTrace}");
        }
    }

    private void UpdateProgress(string status, float progress)
    {
        Debug.Log($"Gallery Progress: {status} ({progress:P0})");
        OnProgressUpdated?.Invoke(status, progress);
    }
}