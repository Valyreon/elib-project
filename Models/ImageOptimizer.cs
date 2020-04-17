﻿using ImageProcessor;
using ImageProcessor.Imaging;
using System.Drawing;
using System.IO;

namespace Models
{
    public static class ImageOptimizer
    {
        public static byte[] ResizeAndFill(byte[] imgBytes, int Width = 400, int Height = 600)
        {
            using Image imgPhoto = Image.FromStream(new MemoryStream(imgBytes));

            Size size = new Size(Width, Height);

            ResizeLayer resizeLayer = new ResizeLayer(size, ResizeMode.Max);
            ResizeLayer resizeCropLayer = new ResizeLayer(size, ResizeMode.Crop, AnchorPosition.Center);

            using MemoryStream outStream = new MemoryStream();
            using ImageFactory imageFactory = new ImageFactory(preserveExifData: false);

            CropLayer cropLayer = new CropLayer(2, 2, imgPhoto.Width - 4, imgPhoto.Height - 4, CropMode.Pixels);

            // Resize cover image and stor in outstream
            imageFactory.Load(imgPhoto)
                .Crop(cropLayer)
                .Resize(resizeLayer)
                .Quality(100)
                .Save(outStream);

            using Image resizePhoto = Image.FromStream(outStream);

            ImageLayer resizedImage = new ImageLayer() { Image = resizePhoto };

            // If the picture fits do not blur the background
            if (resizedImage.Size == size)
                return outStream.ToArray();

            resizeLayer = new ResizeLayer(size, ResizeMode.Min);

            // Loads the original image resizes it to have the shortest side fit the dimensions, blurs it then overlays the original resized image
            imageFactory.Load(imgPhoto)
                .Resize(resizeLayer)
                .GaussianBlur(10)
                .Overlay(resizedImage)
                .Resize(resizeCropLayer)
                .Quality(100)
                .Save(outStream);

            return outStream.ToArray();
        }
    }
}