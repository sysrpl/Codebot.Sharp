using System;
using System.Web;
using System.Drawing;
using System.IO;
using System.Web.SessionState;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Drawing.Drawing2D;

namespace Codebot.Web
{
    public class Captcha : IHttpHandler, IReadOnlySessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            int height = 80;
            int width = 190;
            Random random = new Random();
            int[] fontEmStyles = new int[] { 15, 20, 25, 30, 35 };
            string[] fontNames = new string[]
            {
                "Comic Sans MS",
                "Arial",
                "Times New Roman",
                "Georgia",
                "Verdana",
                "Geneva"
            };
            FontStyle[] fontStyles = new FontStyle[]
            {
                FontStyle.Bold,
                FontStyle.Italic,
                FontStyle.Regular,
                FontStyle.Strikeout,
                FontStyle.Underline
            };
            HatchStyle[] hatchStyles = new HatchStyle[]
            {
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
            string captchaText = context.Session["Captcha"].ToString();
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            RectangleF rect = new RectangleF(0, 0, width, height);
            Brush brush = default(Brush);
            brush = new HatchBrush(hatchStyles[random.Next
                (hatchStyles.Length - 1)], Color.FromArgb((random.Next(100, 255)),
                    (random.Next(100, 255)), (random.Next(100, 255))), Color.White);
            graphics.FillRectangle(brush, rect);
            System.Drawing.Drawing2D.Matrix matrix = new System.Drawing.Drawing2D.Matrix();
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
            MemoryStream stream = new MemoryStream();
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
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

