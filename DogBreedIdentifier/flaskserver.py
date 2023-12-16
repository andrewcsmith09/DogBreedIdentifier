from flask import Flask, request, jsonify
from flask_httpauth import HTTPBasicAuth
import io
from PIL import Image
import dog_breed_identifier  # Import the dog prediction program

app = Flask(__name__)
auth = HTTPBasicAuth()

# Hardcoded username and password for gui
USERNAME = 'client'
PASSWORD = 'pF5dzH32x3cAXOIx'
@auth.verify_password
def verify_password(username, password):
    return username == USERNAME and password == PASSWORD

@app.route('/predict_dog', methods=['POST'])
@auth.login_required
def predict_dog():
    # Check if the request contains an image file
    if 'image' not in request.files:
        app.logger.error('No image part in the request')
        return jsonify({'error': 'No image part'})

    # Define image file    
    image_file = request.files['image']

    # Check if the file is empty
    if image_file.filename == '':
        app.logger.error('No selected file')
        return jsonify({'error': 'No selected file'})

    # Read the image file and convert it to a format suitable for prediction
    image_bytes = image_file.read()

    # Print the type of image_bytes to console
    print("Type of image_bytes:", type(image_bytes))

    # Convert image file to image
    image = Image.open(io.BytesIO(image_bytes))

    # Log information about the received image
    app.logger.info('Received image for prediction')

    # Extract the number of epochs from the request
    epochs = int(request.form.get('epochs', 10))  # Default to 10 if not provided

    # Print the number of epochs received to console
    print("Number of epochs received:", epochs)

    # Call the prediction function with the specified epochs
    prediction = dog_breed_identifier.main(image, epochs)

    return jsonify(prediction)
    
if __name__ == '__main__':
    app.run(host="0.0.0.0", port=5000)
