using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Headstart.Common.Extensions
{
    public static class ImageExtensions
    {
        public static Image ResizeSmallerDimensionToTarget(this Image srcImage, int targetSize)
        {
            var scaleFactor = targetSize / (double)Math.Min(srcImage.Width, srcImage.Height);
            if (scaleFactor > 1) return srcImage; // Don't increase image size
            var targetWidth = (int)(srcImage.Width * scaleFactor);
            var targetHeight = (int)(srcImage.Height * scaleFactor);

            var destImage = new Bitmap(targetWidth, targetHeight);
            destImage.SetResolution(srcImage.HorizontalResolution, srcImage.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.Default;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    var destRect = new Rectangle(0, 0, targetWidth, targetHeight);
                    graphics.DrawImage(srcImage, destRect, 0, 0, srcImage.Width, srcImage.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static byte[] ToBytes(this Image image, ImageFormat format)
        {
            using (var stream = new MemoryStream())
            {
                image.Save(stream, format);
                return stream.ToArray();
            }
        }
    }
}
