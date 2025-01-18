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

            [DllImport(@"E:\JAProjekt\JA_Projekt\x64\Debug\JA_C.dll")]
            static extern int test(int a, int b);
            static extern void loadBitmapAndProcess(string filename);

            DateTime dateTimePrev = DateTime.Now;
            int wynik;

            if (asembler.IsChecked ?? false)
            {
                wynik = MyProc1(1, 2);
            }
            else
            {
                wynik = test(3, 4);
                loadBitmapAndProcess("C:\\Users\\Mycelion\\Pictures\\miku.bmp");
            }

            DateTime dateTimeNext = DateTime.Now;
            TimeSpan difference = dateTimeNext - dateTimePrev;

            output.Content = wynik;
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
                string filename = imgDialog.FileName;
                sourcePic.Source = new BitmapImage(new Uri(imgDialog.FileName));
            }
        }
    }
}