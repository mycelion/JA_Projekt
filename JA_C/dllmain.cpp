// dllmain.cpp : Definiuje punkt wejścia dla aplikacji DLL.
#include "pch.h"

#include <iostream>
#include <fstream>
#include <thread>
#include <vector>
#include <windows.h> // For creating threads

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

extern "C" __declspec(dllexport) int test(int a, int b) {
    return a + b;
}

// Function to process each part of the image
void processImagePart(uint8_t* imageData, int startIdx, int endIdx, int width, int height) {
    // Just a placeholder for processing, such as color adjustments or filters
    // You can modify the image data in this section.
    for (int i = startIdx; i < endIdx; ++i) {
        // For example, just print pixel values (for demonstration)
        int x = (i % width);
        int y = (i / width);
        if (x < width && y < height) {
            uint8_t pixel = imageData[i];
            // Process each pixel here
            // You could apply transformations or filters
        }
    }
}

extern "C" __declspec(dllexport) void loadBitmapAndProcess(const char* filename) {
    // Open the bitmap file
    std::ifstream file(filename, std::ios::binary);
    if (!file.is_open()) {
        std::cerr << "Error opening file!" << std::endl;
        return;
    }

    // Read the bitmap file header
    BITMAPFILEHEADER fileHeader;
    file.read(reinterpret_cast<char*>(&fileHeader), sizeof(BITMAPFILEHEADER));

    // Check if the file is a BMP file
    if (fileHeader.bfType != 0x4D42) { // 'BM' in little endian
        std::cerr << "Not a valid BMP file!" << std::endl;
        return;
    }

    // Read the bitmap info header
    BITMAPINFOHEADER infoHeader;
    file.read(reinterpret_cast<char*>(&infoHeader), sizeof(BITMAPINFOHEADER));

    // Ensure it's a 24-bit BMP (common format)
    if (infoHeader.biBitCount != 24) {
        std::cerr << "Only 24-bit BMP files are supported!" << std::endl;
        return;
    }

    // Allocate memory for the image data
    std::vector<uint8_t> imageData(infoHeader.biSizeImage);

    // Move the file pointer to the start of the image data
    file.seekg(fileHeader.bfOffBits, std::ios::beg);

    // Read the image data
    file.read(reinterpret_cast<char*>(imageData.data()), infoHeader.biSizeImage);

    // Divide the image data into 4 parts and process in parallel
    int totalPixels = infoHeader.biWidth * infoHeader.biHeight;
    int quarter = totalPixels / 4;

    std::vector<std::thread> threads;

    // Create 4 threads to process the image
    for (int i = 0; i < 4; ++i) {
        int startIdx = i * quarter;
        int endIdx = (i == 3) ? totalPixels : (i + 1) * quarter;
        threads.push_back(std::thread(processImagePart, imageData.data(), startIdx, endIdx, infoHeader.biWidth, infoHeader.biHeight));
    }

    // Wait for all threads to finish
    for (auto& t : threads) {
        t.join();
    }

    // Optionally, you can save or further process the modified image here.
    // For simplicity, we'll skip that part.

    file.close();
}
