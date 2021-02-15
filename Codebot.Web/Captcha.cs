using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Web;
using System.Web.SessionState;

namespace Codebot.Web
{
    public class Captcha : IHttpHandler, IReadOnlySessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            int height = 80;
            int width = 190;
            var random = new Random();
            int[] fontEmStyles = { 15, 20, 25, 30, 35 };
            string[] fontNames = {
                "Comic Sans MS",
                "Arial",
                "Times New Roman",
                "Georgia",
                "Verdana",
                "Geneva"
            };
            FontStyle[] fontStyles = {
                FontStyle.Bold,
                FontStyle.Italic,
                FontStyle.Regular,
                FontStyle.Strikeout,
                FontStyle.Underline
            };
            HatchStyle[] hatchStyles = {
                HatchStyle.BackwardDiagonal, HatchStyle.Cross,
                HatchStyle.DashedDownwardDiagonal, HatchStyle.DashedHorizontal,
                HatchStyle.DashedUpwardDiagonal, HatchStyle.DashedVertical,
                HatchStyle.DiagonalBrick, HatchStyle.DiagonalCross,
                HatchStyle.Divot, HatchStyle.DottedDiamond, HatchStyle.DottedGrid,
                HatchStyle.ForwardDiagonal, HatchStyle.Horizontal,
                HatchStyle.HorizontalBrick, HatchStyle.LargeCheckerBoard,
                HatchStyle.LargeConfetti, HatchStyle.LargeGrid,
                HatchStyle.LightDownwardDiagonal, HatchStyle.LightHorizontal,
                HatchStyle.LightUpwardDiagonal, HatchStyle.LightVertical,
                HatchStyle.Max, HatchStyle.Min, HatchStyle.NarrowHorizontal,
                HatchStyle.NarrowVertical, HatchStyle.OutlinedDiamond,
                HatchStyle.Plaid, HatchStyle.Shingle, HatchStyle.SmallCheckerBoard,
                HatchStyle.SmallConfetti, HatchStyle.SmallGrid,
                HatchStyle.SolidDiamond, HatchStyle.Sphere, HatchStyle.Trellis,
                HatchStyle.Vertical, HatchStyle.Wave, HatchStyle.Weave,
                HatchStyle.WideDownwardDiagonal, HatchStyle.WideUpwardDiagonal, HatchStyle.ZigZag
            };
            var captchaText = context.Session["Captcha"].ToString();
            var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            var graphics = Graphics.FromImage(bitmap);
            graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            var rect = new RectangleF(0, 0, width, height);
            var brush = new HatchBrush(hatchStyles[random.Next
                (hatchStyles.Length - 1)], Color.FromArgb((random.Next(100, 255)),
                    (random.Next(100, 255)), (random.Next(100, 255))), Color.White);
            graphics.FillRectangle(brush, rect);
            var matrix = new Matrix();
            int i = 0;
            for (i = 0; i <= captchaText.Length - 1; i++)
            {
                matrix.Reset();
                int chars = captchaText.Length;
                int x = width / (chars + 1) * i;
                int y = height / 2;
                matrix.RotateAt(random.Next(-40, 40), new PointF(x, y));
                graphics.Transform = matrix;
                graphics.DrawString
                (
                    captchaText.Substring(i, 1),
                    new Font(fontNames[random.Next(fontNames.Length - 1)],
                        fontEmStyles[random.Next(fontEmStyles.Length - 1)],
                        fontStyles[random.Next(fontStyles.Length - 1)]),
                    new SolidBrush(Color.FromArgb(random.Next(0, 100),
                        random.Next(0, 100), random.Next(0, 100))),
                    x,
                    random.Next(10, 40)
                );
                graphics.ResetTransform();
            }
            var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);
            byte[] bytes = stream.GetBuffer();
            bitmap.Dispose();
            stream.Close();
            context.Response.BinaryWrite(bytes);
            context.Response.End();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}

