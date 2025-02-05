using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using System.Drawing;

using JA_CSharp;
using System.Drawing.Imaging;
using System.IO;

namespace JA_Projekt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        [DllImport(@"E:\JAProjekt\JA_Projekt\x64\Debug\JA_Asm.dll")]
        public static extern void ApplyVignette(IntPtr bitmapBuffer, int img_width, int img_height, int stride, double pow, double maxR);


        private void convert(object sender, RoutedEventArgs e)
        {
            DateTime dateTimePrev = DateTime.Now;
            float radius = (float)radiusSlider.Value;
            float power = (float)powerSlider.Value;

            // Get the source image from sourcePic
            BitmapSource? sourceBitmap = sourcePic.Source as BitmapSource;
            if (sourceBitmap == null)
            {
                MessageBox.Show("No source image is loaded.");
                return;
            }

            if (asembler.IsChecked ?? false)
            {
                // Convert to System.Drawing.Bitmap for direct memory access
                using (var bitmap = new System.Drawing.Bitmap(
                    BitmapSourceToDrawingImage(sourceBitmap)))
                {
                    // Lock the bitmap bits
                    var bitmapData = bitmap.LockBits(
                        new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        ImageLockMode.ReadWrite,
                        System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                    try
                    {
                        // Call ASM function
                        ApplyVignette(
                            bitmapData.Scan0,
                            bitmap.Width,
                            bitmap.Height,
                            bitmapData.Stride,
                            power,
                            radius);
                    }
                    finally
                    {
                        // Unlock bitmap regardless of errors
                        bitmap.UnlockBits(bitmapData);
                    }

                    // Convert back to WPF BitmapImage
                    resultPic.Source = BitmapToBitmapImage(bitmap);
                }
            }
            else
            {
                CSharp_Class cSharp = new CSharp_Class();
                System.Drawing.Image drawingImage = BitmapSourceToDrawingImage(sourceBitmap);
                BitmapImage vignetteImage = cSharp.AddVignette(drawingImage, radius, power);
                resultPic.Source = vignetteImage;
            }

            DateTime dateTimeNext = DateTime.Now;
            TimeSpan difference = dateTimeNext - dateTimePrev;
            time.Content = $"{difference.TotalMilliseconds:F2} ms";
        }

        private void changePicture(object sender, RoutedEventArgs e)
        {
            var imgDialog = new OpenFileDialog()
            {
                Filter = "BMP Files (*.bmp)|*.bmp"
            };
            Nullable<bool> result = imgDialog.ShowDialog();
            if (result == true)
            {
                sourcePic.Source = new BitmapImage(new Uri(imgDialog.FileName));
            }
        }

        private System.Drawing.Image BitmapSourceToDrawingImage(BitmapSource bitmapSource)
        {
            using (var memoryStream = new System.IO.MemoryStream())
            {
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(memoryStream);
                memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                return System.Drawing.Image.FromStream(memoryStream);
            }
        }

        // Helper to convert System.Drawing.Bitmap to BitmapImage
        private BitmapImage BitmapToBitmapImage(System.Drawing.Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

    }
}