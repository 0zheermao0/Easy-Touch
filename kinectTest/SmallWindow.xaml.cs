using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using kinectTest.Entitys;
using LaraNara.PowerPointDump;

namespace kinectTest
{
    /// <summary>
    /// SmallWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SmallWindow : Window
    {
        private byte[] screenBytes;

        private WriteableBitmap screenBitmap;

        public SmallWindow()
        {
            InitializeComponent();
            
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.screenBytes = new byte[Screen.PrimaryScreen.Bounds.Width * Screen.PrimaryScreen.Bounds.Height];
            this.screenBitmap = new WriteableBitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
            this.Image.Source = screenBitmap;
            while (true)
            {
                this.screenBytes = ScreenCapturing.GetDesktopWindowCaptureAsByteArray();
                this.screenBitmap.WritePixels(
                        new Int32Rect(0, 0, this.screenBitmap.PixelWidth, this.screenBitmap.PixelHeight),
                        screenBytes,
                        this.screenBitmap.PixelWidth * sizeof(int),
                        0);
            }
        }

    }
}
