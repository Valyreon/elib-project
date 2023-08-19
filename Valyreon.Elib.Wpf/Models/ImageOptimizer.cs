using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace Valyreon.Elib.Wpf.Models
{
    public static class ImageOptimizer
    {
        public static byte[] ResizeAndFill(byte[] imgBytes, int width = 200, int height = 300)
        {
            using var image = Image.Load(imgBytes);
            using var image2 = Image.Load(imgBytes);

            var targetSize = new Size(width, height);
            var resizeOptions = new ResizeOptions
            {
                Size = targetSize,
                Mode = ResizeMode.Max
            };

            var cropRect = new Rectangle(2, 2, image.Width - 4, image.Height - 4);
            image.Mutate(x => x.Crop(cropRect).Resize(resizeOptions));

            if (image.Size == targetSize)
            {
                using var outStream = new MemoryStream();
                image.Save(outStream, new PngEncoder());
                return outStream.ToArray();
            }

            resizeOptions.Mode = ResizeMode.Min;
            image2.Mutate(x => x.Resize(resizeOptions)
                                .GaussianBlur()
                                .DrawImage(image, new Point((image2.Width - image.Width) / 2, (image2.Height - image.Height) / 2), 1));

            resizeOptions.Mode = ResizeMode.Crop;
            image2.Mutate(x => x.Resize(resizeOptions));

            using var outStream2 = new MemoryStream();
            image2.Save(outStream2, new PngEncoder());
            return outStream2.ToArray();
        }
    }
}
