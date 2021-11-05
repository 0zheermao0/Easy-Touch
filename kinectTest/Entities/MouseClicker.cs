using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
 
namespace kinectTest.Entities
{
    class MouseClicker
    {

        /// <summary>
        /// 鼠标事件
        /// </summary>
        /// <param name="flags">事件类型</param>
        /// <param name="dx">x坐标值(0~65535)</param>
        /// <param name="dy">y坐标值(0~65535)</param>
        /// <param name="data">滚动值(120一个单位)</param>
        /// <param name="extraInfo">不支持</param>
        [DllImport("user32")]
        private static extern int mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        /// <summary>
        ///  设置鼠标的位置
        /// </summary>
        /// <param name="x">x 的坐标值</param>
        /// <param name="y">y 的坐标值</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern int SetCursorPos(int x, int y);

        //移动鼠标 
        const int MOUSEEVENTF_MOVE = 0x0001;
        //模拟鼠标左键按下 
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        //模拟鼠标左键抬起 
        const int MOUSEEVENTF_LEFTUP = 0x0004;
        //模拟鼠标右键按下 
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        //模拟鼠标右键抬起 
        const int MOUSEEVENTF_RIGHTUP = 0x0010;
        //模拟鼠标中键按下 
        const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        //模拟鼠标中键抬起 
        const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        //标示是否采用绝对坐标 
        const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        static int SH = Screen.PrimaryScreen.Bounds.Height;
        static int SW = Screen.PrimaryScreen.Bounds.Width;

        /// <summary>
        /// 触发鼠标事件
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// 
        public static void DoMouseClick(int x, int y)
        {
            SetCursorPos(x, y);
            mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP | MOUSEEVENTF_ABSOLUTE, x, y, 0, 0); //点击
        }

        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        /// <param name="x">屏幕宽度，本机为1920</param>
        /// <param name="y">屏幕高度，本机为1080</param>
        public static void DoMouseMove(int x, int y)
        {
            SetCursorPos(x, y);
        }

        /// <summary>
        /// 鼠标左键单击
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void MouseLeftSingleClick(int x, int y)
        {
            SetCursorPos(x, y);
            mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, x, y, 0, 0);
            //Console.WriteLine("左键单击事件触发；");
        }

        /// <summary>
        /// 鼠标右键单击
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void MouseRightSingleClick(int x, int y)
        {
            SetCursorPos(x, y);
            mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, x, y, 0, 0);
            //Console.WriteLine("右键单击事件触发；");
        }

        /// <summary>
        /// 鼠标拖动开始
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void MouseDragStart(int x, int y)
        {
            SetCursorPos(x, y);
            mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
            //Console.WriteLine("拖拽开始事件触发；");
        }

        public static void MouseDragMove(int x, int y)
        {
            //Console.WriteLine("移动中B；" + x + " " + y);
            //Console.WriteLine("移动前B；" + x + " " + y);
            //SetCursorPos((int)x, (int)y);
            x = x * 65535 / SW;
            y = y * 65535 / SH;
            mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, (int)x, (int)y, 0, 0);
        }

        /// <summary>
        /// 鼠标拖动结束
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void MouseDragEnd(int x, int y)
        {
            mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_LEFTUP, x, y, 0, 0);
            //Console.WriteLine("拖拽结束事件触发；");
        }
    }
}
