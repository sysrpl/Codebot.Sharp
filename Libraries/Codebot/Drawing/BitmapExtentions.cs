using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Codebot.Drawing
{
    public static class BitmapExtentions
    {
        public static Bitmap Resize(this Bitmap bitmap, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = InterpolationMode.High;
                g.DrawImage(bitmap, 0, 0, width, height);
            }
            return result;
        }

        public static Bitmap Square(this Bitmap bitmap, int size)
        {
            Bitmap result = new Bitmap(size, size);
            float ratio;
            if (bitmap.Width > bitmap.Height)
                ratio = size / (float)bitmap.Height;
            else
                ratio = size / (float)bitmap.Width;
            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = InterpolationMode.High;
                if (bitmap.Width > bitmap.Height)
                    g.DrawImage(bitmap, (bitmap.Height - bitmap.Width) / 2f * ratio,
                        0, bitmap.Width * ratio, size);
                else
                    g.DrawImage(bitmap, 0, (bitmap.Width - bitmap.Height) / 2f * ratio,
                        size, bitmap.Height * ratio);
            }
            return result;
        }

        public static Bitmap Stretch(this Bitmap bitmap, int width, int height)
        {
            float ratio = 1;
            if (bitmap.Width > width)
                ratio = width / (float)bitmap.Width;
            if (bitmap.Height > height)
                ratio = Math.Min(ratio, height / (float)bitmap.Height);
            Bitmap result = new Bitmap((int)(bitmap.Width * ratio), (int)(bitmap.Height * ratio));
            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = InterpolationMode.High;
                g.DrawImage(bitmap, 0, 0, bitmap.Width * ratio, bitmap.Height * ratio);
            }
            return result;
        }
    }
}

