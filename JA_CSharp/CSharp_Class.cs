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

namespace JA_CSharp
{
    public class CSharp_Class
    {
        public int test(int a, int b)
        {
            return a + b;
        }

    public Image AddVignette(Image srcImage)
        {
            Bitmap targetImg = new Bitmap(srcImage.Width, srcImage.Height);

            using (Graphics g = Graphics.FromImage(targetImg))
            {
                g.Clear(Color.Black);
            }

            return targetImg;
        }
    }
}
