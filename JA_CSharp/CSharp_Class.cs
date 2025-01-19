using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Reflection.Emit;
using System.Windows.Media.Effects;
using System.Drawing.Imaging;
using System.Windows.Media.Media3D;

namespace JA_CSharp
{
    public class CSharp_Class
    {
        public int test(int a, int b)
        {
            return a + b;
        }

        public BitmapImage AddVignette(System.Drawing.Image srcImage, float radius, float power)
        {
            // Convert the System.Drawing.Image to a Bitmap
            System.Drawing.Bitmap targetImg = new System.Drawing.Bitmap(srcImage);

            int width = targetImg.Width;
            int height = targetImg.Height;

            using (Graphics g = Graphics.FromImage(targetImg))
            {
                // Create a radial gradient brush to simulate the vignette effect
                using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    // Adjust the ellipse size based on the radius parameter
                    float ellipseWidth = width * radius;
                    float ellipseHeight = height * radius;
                    float ellipseX = (width - ellipseWidth) / 2;
                    float ellipseY = (height - ellipseHeight) / 2;

                    path.AddEllipse(ellipseX, ellipseY, ellipseWidth, ellipseHeight);

                    using (var brush = new System.Drawing.Drawing2D.PathGradientBrush(path))
                    {
                        // Calculate the transparency level based on the power parameter
                        byte alpha = (byte)(255 * Math.Clamp(power, 0, 1)); // Ensure power is clamped between 0 and 1

                        // Center color is fully transparent, edges are dark with the chosen opacity
                        brush.CenterColor = System.Drawing.Color.FromArgb(0, 0, 0, 0); // Fully transparent
                        brush.SurroundColors = new[] { System.Drawing.Color.FromArgb(alpha, 0, 0, 0) };

                        // Set the center point of the gradient
                        brush.CenterPoint = new System.Drawing.PointF(width / 2, height / 2);

                        // Draw the gradient overlay
                        g.FillRectangle(brush, 0, 0, width, height);
                    }
                }
            }

            // Convert the Bitmap to a BitmapImage for WPF
            using (var memoryStream = new System.IO.MemoryStream())
            {
                targetImg.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                memoryStream.Seek(0, System.IO.SeekOrigin.Begin);

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }
    }
}
