using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System.Globalization;
using DogBreed.Images;
using System.Text;
using System;

public class Camera_Script : MonoBehaviour
{
    // UI elements
    public Button cameraButton;
    public Button imageGalleryButton;
    public Button connect;
    public Image image;
    public TMP_InputField inputField;
    public Text predictionText;
    public Text popupMessage;
    public Text epochs;
    public Button reduceEpoch;
    public Button increaseEpoch;
    public Button changeEpochs;
    private int currentEpochs = 10;

    public ImageUploaderWithGallery uploaderScript; // Reference to the ImageUploaderWithGallery script

    void Start()
    {
        // Get the TextMeshProUGUI component from the cameraButton
        TextMeshProUGUI cameraButtonText = cameraButton.GetComponentInChildren<TextMeshProUGUI>();

        if (!Application.isMobilePlatform)
        {
            cameraButton.interactable = false;
            cameraButton.gameObject.SetActive(false);
            return;
        }

        // Add listeners to buttons
        connect.onClick.AddListener(DisablePopup);
        imageGalleryButton.onClick.AddListener(DisableWarning);
        epochs.text = "Epochs:" + currentEpochs;
        changeEpochs.onClick.AddListener(OnChangeEpochsClick);

        // Check if there are any cameras available
        if (WebCamTexture.devices.Length > 0)
        {
            cameraButton.onClick.AddListener(ToggleCamera);

            // Check and update the cameraButtonText if it was set to "Disconnected"
            if (cameraButtonText.text == "Disconnected")
            {
                cameraButtonText.text = "Camera";
            }
        }
        else
        {
            // Set cameraButtonText to "Disconnected" if no webcams are available
            cameraButtonText.text = "Disconnected";
        }
    }

    private void ToggleCamera()
    {
        // Check if the serverAddress is set
        if (string.IsNullOrEmpty(uploaderScript.serverAddress))
        {
            // Display a dialogue box, show a warning, or handle it in a way appropriate for your application
            popupMessage.gameObject.SetActive(true);
            popupMessage.text = "Server address is not set. Please set the server address before using camera.";
            return;
        }

        // Disable buttons during camera operation
        cameraButton.interactable = false;
        imageGalleryButton.interactable = false;

        int maxSize = 1024;

        NativeCamera.Permission permission = NativeCamera.TakePicture((path) =>
        {
            if (path != null)
            {
                // Load the image and create a Sprite
                Texture2D texture = NativeCamera.LoadImageAtPath(path, maxSize, false);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);

                // Set the loaded sprite to the image UI element
                image.sprite = sprite;
                predictionText.text = "Analyzing Image.... Please wait.";

                // Start coroutine to load image to server
                StartCoroutine(LoadImage(path));

                if (texture == null)
                {
                    // Handle the case where texture couldn't be loaded
                    Debug.Log("Couldn't load texture from " + path);
                    predictionText.text = "Couldn't load image from " + path;
                    return;
                }

                // Encode the texture to PNG format
                byte[] bytes = texture.EncodeToPNG();

                // Save the image to the gallery
                NativeGallery.Permission galleryPermission = NativeGallery.SaveImageToGallery(bytes, "MyGallery", "MyImage.png", (success, galleryPath) =>
                {
                    if (success)
                    {
                        Debug.Log("Image saved to gallery: " + galleryPath);
                    }
                    else
                    {
                        // Handle the case where saving to gallery failed
                        Debug.LogError("Failed to save image to gallery");
                        predictionText.text = "Failed to save image to gallery";
                    }
                });

                // Check the permission status
                if (galleryPermission == NativeGallery.Permission.Denied)
                {
                    // Handle the case where the user denied gallery access
                    Debug.LogError("Gallery permission denied");
                    predictionText.text = "Gallery permission denied";
                }
            }
            else
            {
                // Enable buttons if the user canceled taking a picture
                EnableButtons();
            }
        }, maxSize);
    }

    // Coroutine to load image from a file path
    IEnumerator LoadImage(string path)
    {
        WWW www = new WWW("file://" + path);
        yield return www;

        // Perform the image upload after loading from the gallery or file explorer
        yield return StartCoroutine(UploadImage(path));
    }

    // Coroutine to upload an image to the server
    IEnumerator UploadImage(string imagePath)
    {
        string username = "client";
        string password = "pF5dzH32x3cAXOIx";

        WWWForm form = new WWWForm();

        // Read image bytes from file
        byte[] imageBytes = File.ReadAllBytes(imagePath);
        form.AddBinaryData("image", imageBytes, "image.png", "image/png");

        // Add the number of epochs to the form
        form.AddField("epochs", currentEpochs.ToString());

        using (UnityWebRequest www = UnityWebRequest.Post(uploaderScript.serverAddress, form))
        {
            // Add basic authentication credentials to the request
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
            www.SetRequestHeader("Authorization", "Basic " + credentials);

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                // Handle the case where image upload failed
                UnityEngine.Debug.LogError("Failed to upload image: " + www.error);
                predictionText.text = "Failed to upload image: " + www.error;
                cameraButton.interactable = true;
            }
            else
            {
                // Log success and update the UI
                UnityEngine.Debug.Log("Image uploaded successfully!");
                string response = www.downloadHandler.text;
                UnityEngine.Debug.Log("Server response: " + response);

                // Format the prediction and display it in the Unity GUI
                response = FormatPrediction(response);
                predictionText.text = "Prediction: " + response;

                // Enable buttons after successful upload
                EnableButtons();
            }
        }
    }

    // Function to format the prediction text
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

    // Method to enable camera and gallery buttons
    private void EnableButtons()
    {
        cameraButton.interactable = true;
        imageGalleryButton.interactable = true;
    }

    private void OnChangeEpochsClick()
    {
        // Reset current epochs when button is toggled off
        if (!uploaderScript.areButtonsInteractable)
        {
            currentEpochs = 10;
        }
    }


    private void DisablePopup()
    {
        if (!string.IsNullOrEmpty(inputField.text))
        {
            // Disable pop up message prompting user to enter server address before opening gallery
            popupMessage.gameObject.SetActive(false);
        }
    }

    private void DisableWarning()
    {
        // Disable camera pop up when gallery button is pressed
        popupMessage.gameObject.SetActive(false);
    }
}
