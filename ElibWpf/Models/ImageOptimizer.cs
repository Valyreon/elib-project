﻿using ImageProcessor;
using ImageProcessor.Imaging;
using System;
using System.Drawing;
using System.IO;

namespace ElibWpf.Models
{
	public static class ImageOptimizer
	{
		public static byte[] ResizeAndFill(byte[] imgBytes, int width = 400, int height = 600)
		{
			try
			{
				using var ms = new MemoryStream(imgBytes);
				using var imgPhoto = Image.FromStream(ms);

				var size = new Size(width, height);

				var resizeLayer = new ResizeLayer(size, ResizeMode.Max);
				var resizeCropLayer = new ResizeLayer(size, ResizeMode.Crop);

				using var outStream = new MemoryStream();
				using var imageFactory = new ImageFactory();

				var cropLayer = new CropLayer(2, 2, imgPhoto.Width - 4, imgPhoto.Height - 4, CropMode.Pixels);

				// Resize cover image and stor in outstream
				imageFactory.Load(imgPhoto)
					.Crop(cropLayer)
					.Resize(resizeLayer)
					.Quality(100)
					.Save(outStream);

				using var resizePhoto = Image.FromStream(outStream);

				var resizedImage = new ImageLayer { Image = resizePhoto };

				// If the picture fits do not blur the background
				if(resizedImage.Size == size)
				{
					return outStream.ToArray();
				}

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
			catch(Exception)
			{
				return null;
			}
		}
	}
}