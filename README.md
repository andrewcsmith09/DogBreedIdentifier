<h1 align='center'>
    Dog Breed Identifier
</h1>

<p align='center'>
    By Andrew Smith, Randall Krouth, & Victor Brown 
</p>
        
<p align='center'>
   <img src="https://github.com/andrewcsmith09/DogBreedIdentifier/assets/153587771/c00c76ae-c1c4-4a48-84a9-52157ab455f9" />
</p>

## Overview

This project is a dog breed classification system that utilizes a pre-trained ResNet50V2 model. It consists of a Flask backend for server-side processing and a Unity frontend for user interaction. The system allows users to upload dog images, predict the breed, and display the results in the Unity GUI. This project was created for Dr. Yu's Artificial Intelligence class at SUNY Brockport.

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

### Download both python scripts and Dog_Breed_Classification.zip in the DogBreedIdentifier folder

1. Install the required Python packages:

   ```bash
   pip install Flask Flask-HTTPAuth Pillow tensorflow
