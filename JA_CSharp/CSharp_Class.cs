using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace JA_CSharp
{
    public class CSharp_Class
    {
        public BitmapImage AddVignette(System.Drawing.Image srcImage, float radius, float power, int numThreads)
        {
            Bitmap targetImg = new Bitmap(srcImage);
            int width = targetImg.Width;
            int height = targetImg.Height;

            // Bezpośredni dostęp do danych pikseli
            BitmapData bitmapData = targetImg.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb
            );

            unsafe
            {
                byte* ptr = (byte*)bitmapData.Scan0;
                int stride = bitmapData.Stride;

                // Obliczenia współczynnika równolegle
                Parallel.For(0, height, new ParallelOptions { MaxDegreeOfParallelism = numThreads }, y =>
                {
                    double centerY = height / 2.0;
                    double centerX = width / 2.0;
                    double maxRadius = Math.Min(width, height) * radius / 2.0;

                    for (int x = 0; x < width; x++)
                    {
                        // Oblicz odległość od środka
                        double dx = (x - centerX);
                        double dy = (y - centerY);
                        double distance = Math.Sqrt(dx * dx + dy * dy);

                        // Oblicz współczynnik winiety
                        double factor = Math.Max(0, 1 - (distance / maxRadius) * power);
                        factor = Math.Clamp(factor, 0, 1);

                        // Zastosuj do piksela (Format 24bppRgb: BGR)
                        int offset = y * stride + x * 3;
                        ptr[offset] = (byte)(ptr[offset] * factor);     // Blue
                        ptr[offset + 1] = (byte)(ptr[offset + 1] * factor); // Green
                        ptr[offset + 2] = (byte)(ptr[offset + 2] * factor); // Red
                    }
                });
            }

            targetImg.UnlockBits(bitmapData);

            // Konwersja do BitmapImage
            using (var memoryStream = new System.IO.MemoryStream())
            {
                targetImg.Save(memoryStream, ImageFormat.Bmp);
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