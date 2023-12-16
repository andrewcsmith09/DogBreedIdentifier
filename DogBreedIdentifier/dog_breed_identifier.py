from PIL import Image
import numpy as np
import pandas as pd
from tensorflow.keras.preprocessing.image import ImageDataGenerator
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import LabelEncoder
from tensorflow.keras.models import load_model, Model
from tensorflow.keras.optimizers import RMSprop
from tensorflow.keras.layers import Dense, GlobalAveragePooling2D, Dropout, BatchNormalization
from tensorflow.keras.applications.resnet_v2 import ResNet50V2, preprocess_input
import os
import sys

def main(image_data, epochs):
    # Define variables
    encoder = LabelEncoder()
    im_size = 224
    batch_size = 64
    
    # Set directory path for information folder
    directory_path = os.path.dirname(os.path.abspath(sys.argv[0])).replace(":\\", "://").replace("\\", "/") \
        + "/Dog_Breed_Classification/"    

    # Assign the labels for images
    df_labels = pd.read_csv(directory_path + "labels.csv")

    # Create a new column for image file names with the image extension
    df_labels['img_file'] = df_labels['id'].apply(lambda x: x + ".jpg")

    # Encode the breed labels into numerical format
    train_y = encoder.fit_transform(df_labels["breed"].values)
    train_x = np.zeros((len(df_labels), im_size, im_size, 3), dtype='float32')
    
    # Set naming convention for models
    model_filename = f"model_{epochs}"
    
    if os.path.exists(directory_path + model_filename):
        # If the model exists, load it and make predictions
        model = load_model(directory_path + model_filename)
    else:
        # If the model doesn't exist, create and train it
        return load_dataset(df_labels, im_size, train_x, train_y, batch_size, image_data, encoder, directory_path, epochs)

    # If the model exists, perform prediction
    image = image_data
    return predict_breed(image, im_size, model, encoder, directory_path)

def load_dataset(df_labels, im_size, train_x, train_y, batch_size, image_data, encoder, directory_path, epochs):
    # Load and preprocess the training images
    for i, img_id in enumerate(df_labels['img_file']):
        img_path = directory_path + 'train/' + img_id
        image = Image.open(img_path)
      
        # Convert the image to a numpy array
        img_array = np.array(image)
    
        # Perform preprocessing on array
        img_array = preprocess_input(np.expand_dims(img_array, axis=0))
    
        # Update the train_x variable with the processed image
        train_x[i] = img_array

    # Split the dataset into training and valdation sets
    x_train, x_val, y_train, y_val = train_test_split(train_x, train_y, test_size=0.2, random_state=42)
    
    # Generate images for the training set
    train_datagen = ImageDataGenerator()
    train_generator = train_datagen.flow(x_train,
                                         y_train,
                                         batch_size=batch_size)
    
    # Generate image for the valdation set
    val_datagen = ImageDataGenerator()
    val_generator = val_datagen.flow(x_val,
                                       y_val,
                                       batch_size=batch_size)
    
    return create_model(im_size, train_generator, x_train, batch_size, val_generator, x_val, encoder, directory_path, image_data, epochs, y_train)

def create_model(im_size, train_generator, x_train, batch_size, val_generator, x_val, encoder, directory_path, image_data, epochs, y_train):
   
    # Build the model using ResNet50V2
    resnet = ResNet50V2(input_shape=[im_size, im_size, 3], weights='imagenet', include_top=False)

    # Freeze all trainable layers and train only the top layers
    for layer in resnet.layers:
        layer.trainable = False

    # Extract the output tensor from the pre-trained ResNet50V2 model
    x = resnet.output
    # Batch normalization to normalize the activations and improve training stability
    x = BatchNormalization()(x)
    # Global Average Pooling 2D to reduce spatial dimensions and retain important information
    x = GlobalAveragePooling2D()(x)
    # Dropout regularization to randomly set 50% of input units to zero during training
    x = Dropout(0.5)(x)
    # Dense layer with 1024 units and ReLU activation for high-level feature learning
    x = Dense(1024, activation='relu')(x)
    # Another dropout layer for additional regularization
    x = Dropout(0.5)(x)

    # Calculate the number of unique breeds dynamically
    num_breeds = len(np.unique(y_train))
    predictions = Dense(num_breeds, activation='softmax')(x)

    # Define model
    model = Model(inputs=resnet.input, outputs=predictions)
    
    # Define learning rate and optimizer
    learning_rate = 1e-3
    optimizer = RMSprop(learning_rate=learning_rate, rho=0.9)
    model.compile(optimizer=optimizer, loss='sparse_categorical_crossentropy', metrics=["accuracy"])
    

    return train_model(model, train_generator, x_train, batch_size, epochs, val_generator, x_val, encoder, directory_path, im_size, image_data)

def train_model(model, train_generator, x_train, batch_size, epochs, val_generator, x_val, encoder, directory_path, im_size, image_data):    
    # Train the model
    model.fit(train_generator,
              steps_per_epoch=len(x_train) // batch_size,  
              epochs=epochs,
              validation_data=val_generator,
              validation_steps=len(x_val) // batch_size)

    # Save the trained model
    model_filename = f"model_{epochs}"
    model.save(directory_path + model_filename)
    
    return predict_breed(image_data, im_size, model, encoder, directory_path)

def predict_breed(image_data, im_size, model, encoder, directory_path):
    # Load the image for prediction
    image = image_data.resize((im_size, im_size))
    # Convert image to numpy array
    pred_img_array = np.array(image)
    # Close the image after use
    image.close()
    
    # Preprocess the image for prediction
    pred_img_array = preprocess_input(np.expand_dims(pred_img_array, axis=0))
    
    # Predict the breed of the dog
    pred_val = model.predict(pred_img_array)    
    predicted_breed_index = np.argmax(pred_val)
    
    # Inverse transform the predicted label to get the breed name
    predicted_breed = encoder.inverse_transform([predicted_breed_index])[0]

    # Print breed to console
    print(predicted_breed)
    # Return breed to server
    return predicted_breed
