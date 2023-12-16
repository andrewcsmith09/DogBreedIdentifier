<h1 align='center'>
    Dog Breed Identifier
</h1>

<h3 align='center'>
    By Andrew Smith, Randall Krouth & Victor Brown 
</h3>
        
<p align='center'>
   <img src="https://github.com/andrewcsmith09/DogBreedIdentifier/assets/153587771/c00c76ae-c1c4-4a48-84a9-52157ab455f9" />
</p>

## Overview

This project is a dog breed classification system that utilizes a pre-trained ResNet50V2 model. It consists of a Flask backend for server-side processing and a Unity frontend for user interaction. The system allows users to upload dog images, predict the breed, and display the results in the Unity GUI. It can be built for any device through the Unity Editor. This project was created for Dr. Yu's Artificial Intelligence class at SUNY Brockport.  

#### Links:  
Training Dataset: https://www.kaggle.com/datasets/jessicali9530/stanford-dogs-dataset  
Source Code: https://techvidvan.com/tutorials/dog-breed-classification/

## Features

- Dog breed classification using a pre-trained ResNet50V2 model.
- Flask backend for serving predictions.
- Unity frontend for user interaction and image uploading.

## Prerequisites

- Python 3.7 or higher
- Flask
- Flask-HTTPAuth
- Pillow (PIL)
- TensorFlow
- Unity 2020.3 or higher
  
## Running the Identifier script and Flask server

### - Download both python scripts and Dog_Breed_Classification.zip from the DogBreedIdentifier folder
  
### Running the Flask server
#### 1. Unzip the Dog_Breed_Classification.zip folder
Ensure all necessary items are in the folder. You should have:  
- model_10 folder (pre-trained model)  
- train folder (training dataset)  
- test folder (testing dataset)  
- labels.csv (training labels)  
- labels_test.csv (testing labels)  

#### 2. Ensure files are in proper hiearchy
       -DogBreedIdentifier
            -flaskserver.py
            -dog_breed_identifier.py
            -Dog_Breed_Classification
                -model_10
                -train
                -test
                -labels.csv
                -labels_test.csv

#### 3. Install the required Python packages:

   ```bash
   pip install Flask Flask-HTTPAuth Pillow tensorflow
   ```

#### 4. Execute the Flask server script

    python flaskserver.py

  The dog breed identifier script is already linked to the server and will automatically run when a request is received.  
  You should see text in the console notifying you that the server is live along with the IP address and port number it is running on.

## Running the GUI in Unity

### Download and unzip the GUI_Unity_Project.zip file from the UnityGUI folder

#### Make sure you have the Unity Editor installed. If not, you can download the Unity Hub from https://unity.com/download

#### 1. Launch the Unity Hub and add the GUI_Unity_Project folder as a new project

#### 2. Open the project and allow domain and scripts to be compiled

#### 3. Either press play on the player or create a new build to run the GUI

## Usage

#### 1. Once the server and GUI are running, start by specifying the server address and port number in the input field on the GUI and press "Save Server Address"  
- You may use the private address on private networks, otherwise you will need to enter the public IP address of the machine running the server. You may also need to set up port forwarding so your router knows which device to send the traffic to.

#### 2. (Optional) You can specify the number of epochs by pressing the "Change # of epochs" button and then using the arrows to increase or decrease the epoch value
- Models are saved based on the particular number of epochs they were trained on. If a model for that epoch value has not been trained, training of the model will occur. (Note: This can take a significant amount of time)
- If you don't specify the number of epochs, the program will use the default pre-trained model (10 epochs)

#### 3. Either use the "Gallery" button to select an existing image on the device or if on mobile, select the "Camera" button to take a new image
- After the image is either selected or taken, it will be automatically sent to the server. The server will then respond with the predicted breed which will be displayed on the GUI.

## License
This project is licensed under the MIT License - see the LICENSE file for details.
  
    
