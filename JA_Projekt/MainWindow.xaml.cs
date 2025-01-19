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

        private void convert(object sender, RoutedEventArgs e)
        {
            [DllImport(@"E:\JAProjekt\JA_Projekt\x64\Debug\JA_Asm.dll")]
            static extern int MyProc1(int a, int b);

            DateTime dateTimePrev = DateTime.Now;
            int wynik;
            float radius = (float)radiusSlider.Value;
            float power = (float)powerSlider.Value;

            if (asembler.IsChecked ?? false)
            {
                wynik = MyProc1(1, 2);
            }
            else
            {
                CSharp_Class cSharp = new CSharp_Class();

                // Get the source image from sourcePic
                BitmapSource sourceBitmap = sourcePic.Source as BitmapSource;

                if (sourceBitmap == null)
                {
                    MessageBox.Show("No source image is loaded.");
                    return;
                }

                // Convert BitmapSource to System.Drawing.Image
                System.Drawing.Image drawingImage = BitmapSourceToDrawingImage(sourceBitmap);

                // Apply vignette
                BitmapImage vignetteImage = cSharp.AddVignette(drawingImage, radius, power);
                resultPic.Source = vignetteImage;
            }

            DateTime dateTimeNext = DateTime.Now;
            TimeSpan difference = dateTimeNext - dateTimePrev;

            output.Content = powerSlider.Value;
            time.Content = difference.TotalMilliseconds;
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
    }
}