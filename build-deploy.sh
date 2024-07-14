#!/bin/bash

# Define the folders
folders=(
    "BooksApiNative.Functions.AddEditBook"
    "BooksApiNative.Functions.GetBooks",
    "BooksApiNative.Functions.GetBook"
)

# Loop through each folder and run the dotnet publish command
for folder in "${folders[@]}"; do
    echo "Publishing $folder..."
    cd "$folder" || exit
    dotnet publish -c Release -r linux-arm64
    cd - || exit # Return to the parent directory
done

cd deploy
npm install
yarn build
yarn cdk deploy --parameters LambdaArchitecture=arm64

cd ..


echo "All tasks completed."
