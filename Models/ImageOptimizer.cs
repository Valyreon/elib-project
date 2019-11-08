using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using ImageProcessor.Imaging.Filters;
using System.IO;
using ImageProcessor;
using ImageProcessor.Imaging;

namespace Models
{
    public static class ImageOptimizer
    {
        public static byte[] ResizeAndFill(byte[] imgBytes, int Width = 200, int Height = 320)
        {
            Color fillColor = Color.GhostWhite;

            using Image imgPhoto = Image.FromStream(new MemoryStream(imgBytes));

            Size size = new Size(Width, Height);

            ResizeLayer resizeLayer = new ResizeLayer(size, ResizeMode.Max);

            using MemoryStream outStream = new MemoryStream();
            using ImageFactory imageFactory = new ImageFactory(preserveExifData: true);

            // Resize cover image and stor in outstream
            imageFactory.Load(imgPhoto)
                .Resize(resizeLayer)
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
                .Save(outStream);

            return outStream.ToArray();
        }
    }
}