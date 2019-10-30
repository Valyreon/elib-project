using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace Models
{
    public static class ImageOptimizer
    {
        public static byte[] ResizeAndFill(byte[] imgBytes, int Width, int Height, Color fillColor)
        {
            Image imgPhoto = Image.FromStream(new MemoryStream(imgBytes));

            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;

            Width = (int)(((float)sourceWidth * (float)Height) / (float)sourceHeight);

            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = Convert.ToInt16((Width - (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = Convert.ToInt16((Height - (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(Width, Height,
                              PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                             imgPhoto.VerticalResolution);

            using Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(fillColor);
            grPhoto.InterpolationMode =
                    InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            using MemoryStream outStr = new MemoryStream();
            bmPhoto.Save(outStr, ImageFormat.Jpeg);
            return outStr.ToArray();
        }
    }
}
