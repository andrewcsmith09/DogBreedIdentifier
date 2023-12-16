using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System;
using System.Globalization;
using TMPro;
using System.Text;
using UnityEngine.Networking;
using SFB;

namespace DogBreed.Images
{
    public class ImageUploaderWithGallery : MonoBehaviour
    {
        // UI elements
        public Button imageGalleryButton;
        public Button cameraButton;
        public Button connect;
        public Image imageDisplay;
        public Text predictionText;
        public TMP_InputField inputField;
        public Text epochs;
        private int currentEpochs = 10;
        public Button reduceEpoch;
        public Button increaseEpoch;
        public string serverAddress;
        public Text connectText;
        public Text popupMessage;
        public Text epochWarning;
        public Button exitButton;
        public Button changeEpochsButton;
        public bool areButtonsInteractable = false;


        private void Start()
        {
            // Button event listeners
            imageGalleryButton.onClick.AddListener(OpenGallery);
            connect.onClick.AddListener(SaveServerAddress);
            reduceEpoch.onClick.AddListener(ReduceEpoch);
            increaseEpoch.onClick.AddListener(IncreaseEpoch);
            changeEpochsButton.onClick.AddListener(OnChangeEpochsButtonClick);
            cameraButton.onClick.AddListener(DisableWarning);

            // Set the initial state
            SetButtonInteractivity(false);

            if (Application.isMobilePlatform)
            {
                // Disable exit button for mobile platforms
                exitButton.interactable = false;
                exitButton.gameObject.SetActive(false);
            }

            // Exit button event listener
            exitButton.onClick.AddListener(QuitApplication);
        }

        void OpenGallery()
        {
            // Check if the serverAddress is set
            if (string.IsNullOrEmpty(serverAddress))
            {
                // Display a dialogue box, show a warning, or handle it in a way appropriate for your application
                popupMessage.gameObject.SetActive(true);
                popupMessage.text = "Server address is not set. Please set the server address before opening the gallery.";
                return;
            }

            // Disable the gallery button to prevent multiple presses
            imageGalleryButton.interactable = false;
            cameraButton.interactable = false;

#if UNITY_ANDROID || UNITY_IOS
            // Open gallery on mobile platforms
            NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
            {
                if (path != null)
                {
                    predictionText.text = "Analyzing Image.... Please wait.";
                    StartCoroutine(LoadImage(path));
                }
                else
                {
                    UnityEngine.Debug.LogWarning("Failed to load image from gallery.");
                    // Enable the gallery button in case of an error
                    imageGalleryButton.interactable = true;
                    cameraButton.interactable = true;
                }
            }, "Select Image", "image/*");
#else
            // Open file browser on non-mobile platforms
            SFB.ExtensionFilter[] extensions = new[]
            {
                new ExtensionFilter("Image Files", "png", "jpg", "jpeg")
            };

            string[] file_browser = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);

            if (file_browser.Length > 0)
            {
                string path = file_browser[0].Replace(":\\", "://").Replace("\\", "/");
                if (!string.IsNullOrEmpty(path))
                {
                    string[] file_extensions = { "jpg", "jpeg", "png" };
                    string extension = System.IO.Path.GetExtension(path).Substring(1).ToLower();

                    if (System.Array.Exists(file_extensions, element => element == extension))
                    {
                        predictionText.text = "Analyzing Image.... Please wait.";
                        StartCoroutine(LoadImage(path));
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning("Invalid file format. Please select a valid image file.");
                        // Enable the gallery button in case of an error
                        imageGalleryButton.interactable = true;
                        cameraButton.interactable = true;
                    }
                }
                else
                {
                    // Enable the gallery button in case of an error
                    imageGalleryButton.interactable = true;
                    cameraButton.interactable = true;
                }
            }
            else
            {
                // Enable the gallery button in case of an error
                imageGalleryButton.interactable = true;
                cameraButton.interactable = true;
            }
#endif
        }

        IEnumerator LoadImage(string path)
        {
            // Load image from path
            WWW www = new WWW("file://" + path);
            yield return www;

            // Create sprite from loaded texture
            Texture2D texture = www.texture;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);

            imageDisplay.sprite = sprite;

            // Perform the image upload after loading from the gallery or file explorer
            yield return StartCoroutine(UploadImage(path));
        }

        IEnumerator UploadImage(string imagePath)
        {
            // Credentials for server authentication
            string username = "client";
            string password = "pF5dzH32x3cAXOIx";

            // Create form for uploading image
            WWWForm form = new WWWForm();

            byte[] imageBytes = File.ReadAllBytes(imagePath);
            form.AddBinaryData("image", imageBytes, "image.png", "image/png");

            // Add the number of epochs to the form
            form.AddField("epochs", currentEpochs.ToString());

            using (UnityWebRequest www = UnityWebRequest.Post(serverAddress, form))
            {
                // Add basic authentication credentials to the request
                string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
                www.SetRequestHeader("Authorization", "Basic " + credentials);

                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    UnityEngine.Debug.LogError("Failed to upload image: " + www.error);
                }
                else
                {
                    UnityEngine.Debug.Log("Image uploaded successfully!");
                    string response = www.downloadHandler.text;
                    UnityEngine.Debug.Log("Server response: " + response);

                    // Format the prediction and display it in the Unity GUI
                    response = FormatPrediction(response);
                    predictionText.text = "Prediction: " + response;
                    imageGalleryButton.interactable = true;
                    cameraButton.interactable = true;
                }
            }
        }

        private void SaveServerAddress()
        {
            // Check if the serverAddress is not null or empty
            if (!string.IsNullOrEmpty(inputField.text))
            {
                serverAddress = inputField.text;
                string enteredAddress = serverAddress;

                // Check if the entered address already contains the protocol
                if (!enteredAddress.StartsWith("http://") && !enteredAddress.StartsWith("https://"))
                {
                    // If not, add "http://"
                    enteredAddress = "http://" + enteredAddress;
                }

                // Append "/predict_dog" to the address
                serverAddress = enteredAddress + "/predict_dog";

                connectText.text = "Server Address Saved";

                // Disable pop up message prompting user to enter server address before opening gallery
                popupMessage.gameObject.SetActive(false);
            }
            else
            {
                // Handle the case where serverAddress is not set
                UnityEngine.Debug.LogError("Server address is not set.");
            }
            UnityEngine.Debug.Log("Server address is: " + serverAddress);
        }

        string FormatPrediction(string prediction)
        {
            // Remove underscores and capitalize the first letter of every word
            prediction = prediction.Replace("_", " ");

            // Remove single and double quotations
            prediction = prediction.Replace("'", "").Replace("\"", "");

            // Create a TextInfo object using the "en-US" culture and convert the prediction to title case
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            prediction = textInfo.ToTitleCase(prediction.ToLower());

            // Return the formatted prediction
            return prediction;
        }

        private void ReduceEpoch()
        {
            // Decrease the number of epochs
            currentEpochs -= 1;

            if (currentEpochs <= 1)
            {
                currentEpochs = 1;
            }

            epochs.text = "Epochs: " + currentEpochs.ToString();
        }

        private void IncreaseEpoch()
        {
            // Increase the number of epochs
            currentEpochs += 1;

            epochs.text = "Epochs: " + currentEpochs.ToString();
        }

        private void OnChangeEpochsButtonClick()
        {
            // Toggle the interactability state
            areButtonsInteractable = !areButtonsInteractable;

            // Update the interactability of the buttons
            SetButtonInteractivity(areButtonsInteractable);

            if (areButtonsInteractable)
            {
                // Display a warning message when buttons are interactable
                epochWarning.gameObject.SetActive(true);
                epochWarning.text = "Changing # of epochs will cause retraining of the model if not already saved.";
            }
            else
            {
                // Reset the currentEpochs value and hide the warning when buttons are not interactable
                currentEpochs = 10;
                epochs.text = "Epochs: " + currentEpochs.ToString();
                epochWarning.gameObject.SetActive(false);
            }
        }

        public void SetButtonInteractivity(bool interactable)
        {
            // Set interactability of buttons
            reduceEpoch.interactable = interactable;
            increaseEpoch.interactable = interactable;

            if (!interactable)
            {
                // Reset the currentEpochs value when buttons are not interactable
                currentEpochs = 10;
                epochs.text = "Epochs: " + currentEpochs.ToString();
            }
        }

        public int CurrentEpochs
        {
            get { return currentEpochs; }
            set { currentEpochs = value; }
        }

        private void DisableWarning()
        {
            // Disable gallery pop up when camera button is pressed
            popupMessage.gameObject.SetActive(false);
        }

        private void QuitApplication()
        {
            // Quit the application
            Application.Quit();
        }
    }
}
