using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using kinectTest.Entities;
using Microsoft.Kinect;
/// <summary>
/// 测试Kinect
/// </summary>
namespace kinectTest
{
    /// <summary>
    /// CalibrationWindow.xaml 的交互逻辑
    /// </summary>

    public partial class CalibrationWindow : Window
    {

        #region 窗口的移动、结束、关闭操作
        void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CloseBtnClick(object sender, FormClosingEventArgs e)
        {
            this.Close();
            System.Environment.Exit(0);
        }
        #endregion

        public bool canWrite = false;

        private PixProcessor processor = new PixProcessor();

        private Boolean imagePrepared = false;

        public byte[] snapshoot = null;

        private WriteableBitmap colorBitmap = new WriteableBitmap(640, 480, 96.0, 96.0, PixelFormats.Bgr32, null);

        private WriteableBitmap staticBitmap = new WriteableBitmap(640, 480, 96.0, 96.0, PixelFormats.Bgr32, null);


        private System.Windows.Point currSelectedBoundPoint = new System.Windows.Point();

        private System.Windows.Shapes.Ellipse currSelectedCycle = null;

        //image控件的区域
        private ScreenRectangle imageBeforeRectangle = new ScreenRectangle();

        private bool isDrag = false;

        //选定的image控件区域
        private ScreenRectangle imageRectangle = new ScreenRectangle();


        //屏幕空间抽象层
        public ScreenRectangle screenRectangle = new ScreenRectangle();

        //像素空间抽象层
        public ScreenRectangle pixedRectangle = new ScreenRectangle();

        //像素空间选区区域抽象层
        private ScreenRectangle pixedSelectedRectangle = new ScreenRectangle();

        //控件到像素层映射矩阵
        private Warper imageToPix = new Warper();

        //像素层到屏幕层映射矩阵
        private Warper pixToScreen = new Warper();

        //屏幕高度和屏幕宽度
        public int screenHeight = -1, screenWidth = -1;

        //显示四个点坐标的椭圆
        System.Windows.Shapes.Ellipse leftTopCycle = new System.Windows.Shapes.Ellipse();
        System.Windows.Shapes.Ellipse leftDownCycle = new System.Windows.Shapes.Ellipse();
        System.Windows.Shapes.Ellipse rightTopCycle = new System.Windows.Shapes.Ellipse();
        System.Windows.Shapes.Ellipse rightDownCycle = new System.Windows.Shapes.Ellipse();



        //原先MainWindow中的参数

        /// <summary>
        /// kinect传感器
        /// </summary>
        public KinectSensor sensor;
        public KinectSensor sensor2;

        private PixProcessor pixProcessor = new PixProcessor();
        private int timestamp = 0;
        private DateTime current = DateTime.Now;
        //System.DateTime start;
        //System.DateTime end;

        /// <summary>
        /// 保存像素映射
        /// </summary>



        private WriteableBitmap depthBitmap;


        //定义初始化背景的次数
        int processingBackgroundTimes = 60;
        //已经初始化的背景次数
        int nProcessingBackground = 0;
        //接触点数目
        //int blobCount = 0;

        //新加部分
        //数组阈值（单击阈值），即数组中存储的点大于6时开始判断是否为右键单击或拖拽
        private const int I = 6;
        //数组当前位置
        private static int index = 0;
        //最多传入500个点
        static double[,] points_store = new double[10000, 2];
        //是否当前为拖拽状态
        static bool ifDragNow = false;
        static bool ifRightClick = false;
        // 当前拖拽点在数组中位置
        static int dragIndex = 0;
        //没有有效点帧个数
        int noPointFrameNum = 0;

        //从Kinect获取像素数据
        private byte[] colorPixels;
        //从Kinect获取深度数据
        private short[] depthPixels;

        //校准
        double xOffset = 0;
        double yOffset = 0;
        double xWhole = 0;
        double yWhole = 0;

        private RedCheckCircle redCheckCircle = new RedCheckCircle();
        //private Thread dragThread = new Thread(new ThreadStart(doMouseEventDrag));

        public CalibrationWindow()
        {
            InitializeComponent();
            realTimeImage.Source = colorBitmap;

            //左上角圆点初始化参数
            leftTopCycle.Width = 10;
            leftTopCycle.Height = 10;
            leftTopCycle.Fill = new SolidColorBrush(Colors.Red);
            leftTopCycle.MouseDown += BoundPoint_MouseDown;

            //左下角圆点初始化参数
            leftDownCycle.Width = 10;
            leftDownCycle.Height = 10;
            leftDownCycle.Fill = new SolidColorBrush(Colors.Blue);
            leftDownCycle.MouseDown += BoundPoint_MouseDown;

            //右上角圆点初始化参数
            rightTopCycle.Width = 10;
            rightTopCycle.Height = 10;
            rightTopCycle.Fill = new SolidColorBrush(Colors.Green);
            rightTopCycle.MouseDown += BoundPoint_MouseDown;

            //右下角圆点初始化参数
            rightDownCycle.Width = 10;
            rightDownCycle.Height = 10;
            rightDownCycle.Fill = new SolidColorBrush(Colors.Yellow);
            rightDownCycle.MouseDown += BoundPoint_MouseDown;
        }

        /// <summary>
        /// 加载窗口方法
        /// </summary>
        /// <param name="sender">传输事件的对象</param>
        /// <param name="e">事件参数</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.statusBarText.Text = " 当前无Kinect传感器可用，请插入Kinect传感器，程序将继续监控10s";
            //dragThread.Start();
            //dragThread.Suspend();


            // 等待Kinect连接上限为10s
            DateTime current = DateTime.Now;
            int devicesNumber = 0;
            String firstDevice = "";
            while (current.AddSeconds(10) > DateTime.Now)
            {

                foreach (var potentialSensor in KinectSensor.KinectSensors)
                {
                    if (potentialSensor.Status == KinectStatus.Connected)
                    {
                        this.sensor = potentialSensor;
                        break;
                    }
                }

                if (this.sensor != null)
                {
                    break;
                }



            }

            // 10s后依然无可用的Kinect连接
            if (this.sensor == null)
            {
                this.statusBarText.Text = " 当前无Kinect传感器可用，请关闭本程序，插入Kinect传感器后再打开本程序";
            }
            else //如果有可用Kinect
            {
                //开启像素流用于接收像素帧 640 * 480，帧数为30
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                //this.sensor2.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                //启动深度数据流，像素为640*480，帧数为30
                this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);


                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
                this.depthPixels = new short[this.sensor.DepthStream.FramePixelDataLength];

                this.depthBitmap = new WriteableBitmap(this.sensor.DepthStream.FrameWidth, this.sensor.DepthStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                //重新初始化背景处理次数为0
                this.nProcessingBackground = 0;

                //初始化像素矩阵
                this.pixedRectangle.setLeftTop(0, 0);
                this.pixedRectangle.setLeftDown(0, this.sensor.ColorStream.FrameHeight);
                this.pixedRectangle.setRightTop(this.sensor.ColorStream.FrameWidth, 0);
                this.pixedRectangle.setRightDown(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight);

                //初始化屏幕矩阵
                this.screenHeight = Screen.PrimaryScreen.Bounds.Height;//获取屏幕高度
                this.screenWidth = Screen.PrimaryScreen.Bounds.Width;//获取屏幕宽度
                this.screenRectangle.setLeftTop(0, 0);
                this.screenRectangle.setLeftDown(0, this.screenHeight);
                this.screenRectangle.setRightTop(this.screenWidth, 0);
                this.screenRectangle.setRightDown(this.screenWidth, this.screenHeight);
                this.statusBarText.Text = this.sensor.ColorStream.FrameWidth + "," + this.sensor.ColorStream.FrameHeight;



                //注册FrameReady处理函数
                this.sensor.ColorFrameReady += this.SensorColorFrameReady;

                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }
        }

        /// <summary>
        /// Handles the user clicking on the screenshot button
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void ButtonScreenshotClick(object sender, RoutedEventArgs e)
        {
            //MouseClicker.DoMouseClick(60000,60000);
            MouseClicker.DoMouseMove(960, 540);
        }


        /// <summary>
        /// 重要！输入Kinect坐标，返回屏幕坐标。
        /// 即像素层到屏幕层的坐标转换
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public void positionTransfer(double x, double y, ref double xtemp, ref double ytemp)
        {
            pixToScreen.warp(x, y, ref xtemp, ref ytemp);
            if (!judgeInRange(xtemp, ytemp))
            {
                xtemp = -1;
                ytemp = -1;
            }
        }

        /// <summary>
        /// 判断输入点是否在所选择范围内方法
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private Boolean judgeInRange(double x, double y)
        {
            if (x == -1 && y == -1)
            {
                //未初始化
                return false;
            }
            else
            {
                if (x >= 0 && x <= screenWidth && y >= 0 && y <= screenHeight)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            };
        }

        private void LeftTopBound_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDrag == true)
            {
                isDrag = false;
                System.Windows.Shapes.Ellipse cycle = (System.Windows.Shapes.Ellipse)sender;
                cycle.SetValue(Canvas.LeftProperty, e.GetPosition(staticImageCanvas).X);
                cycle.SetValue(Canvas.TopProperty, e.GetPosition(staticImageCanvas).Y);
                cycle.ReleaseMouseCapture();

                //修改imageRectangle的对应点的坐标
                imageRectangle.getLeftTop().setX(e.GetPosition(staticImageCanvas).X);
                imageRectangle.getLeftTop().setY(e.GetPosition(staticImageCanvas).Y);
            }
        }

        private void LeftDownBound_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDrag == true)
            {
                isDrag = false;
                System.Windows.Shapes.Ellipse cycle = (System.Windows.Shapes.Ellipse)sender;
                cycle.SetValue(Canvas.LeftProperty, e.GetPosition(staticImageCanvas).X);
                cycle.SetValue(Canvas.TopProperty, e.GetPosition(staticImageCanvas).Y);
                cycle.ReleaseMouseCapture();

                //修改imageRectangle的对应点的坐标
                imageRectangle.getLeftDown().setX(e.GetPosition(staticImageCanvas).X);
                imageRectangle.getLeftDown().setY(e.GetPosition(staticImageCanvas).Y);
            }
        }

        private void RightTopBound_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDrag == true)
            {
                isDrag = false;
                System.Windows.Shapes.Ellipse cycle = (System.Windows.Shapes.Ellipse)sender;
                cycle.SetValue(Canvas.LeftProperty, e.GetPosition(staticImageCanvas).X);
                cycle.SetValue(Canvas.TopProperty, e.GetPosition(staticImageCanvas).Y);
                cycle.ReleaseMouseCapture();


                //修改imageRectangle的对应点的坐标
                imageRectangle.getRightTop().setX(e.GetPosition(staticImageCanvas).X);
                imageRectangle.getRightTop().setY(e.GetPosition(staticImageCanvas).Y);
            }
        }

        private void RightDownBound_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDrag == true)
            {
                isDrag = false;
                System.Windows.Shapes.Ellipse cycle = (System.Windows.Shapes.Ellipse)sender;
                cycle.SetValue(Canvas.LeftProperty, e.GetPosition(staticImageCanvas).X);
                cycle.SetValue(Canvas.TopProperty, e.GetPosition(staticImageCanvas).Y);
                cycle.ReleaseMouseCapture();

                //修改imageRectangle的对应点的坐标
                imageRectangle.getRightDown().setX(e.GetPosition(staticImageCanvas).X);
                imageRectangle.getRightDown().setY(e.GetPosition(staticImageCanvas).Y);
            }
        }

        private void BoundPoint_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isDrag == true)
            {
                System.Windows.Shapes.Ellipse cycle = (System.Windows.Shapes.Ellipse)sender;
                cycle.SetValue(Canvas.LeftProperty, e.GetPosition(staticImageCanvas).X);
                cycle.SetValue(Canvas.TopProperty, e.GetPosition(staticImageCanvas).Y);
            }
        }

        public void setPixels(byte[] realTimePixels)
        {
            processor.setSource(realTimePixels, 640, 480);
            snapshoot = realTimePixels;
            //向自动识别图像中写入数据
            byte[] autoPixes = processor.process();

            this.colorBitmap.WritePixels(
           new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
           autoPixes,
           this.colorBitmap.PixelWidth * sizeof(int),
           0);
        }


        // 静态图进行预览

        private void Preview(object sender, RoutedEventArgs e)
        {

            staticImage.Source = staticBitmap;
            // 加载静态图片
            this.staticBitmap.WritePixels(
                new Int32Rect(0, 0, this.staticBitmap.PixelWidth, this.staticBitmap.PixelHeight),
                snapshoot,  
                this.staticBitmap.PixelWidth * sizeof(int),
                0);

            // 加载Canvas层
            double scaleDownRatio = staticImage.Height * 1.0 / realTimeImage.Height;

            if (!staticImageCanvas.Children.Contains(leftTopCycle))
                staticImageCanvas.Children.Add(leftTopCycle);
            if (!staticImageCanvas.Children.Contains(leftDownCycle))
                staticImageCanvas.Children.Add(leftDownCycle);
            if (!staticImageCanvas.Children.Contains(rightTopCycle))
                staticImageCanvas.Children.Add(rightTopCycle);
            if (!staticImageCanvas.Children.Contains(rightDownCycle))
                staticImageCanvas.Children.Add(rightDownCycle);

            int[,] leftTop = processor.getLeftTopIndex();

            Canvas.SetLeft(leftTopCycle, scaleDownRatio * leftTop[0, 1]);
            Canvas.SetTop(leftTopCycle, scaleDownRatio * leftTop[0, 0]);

            int[,] rightTop = processor.getRightTopIndex();
            Canvas.SetLeft(rightTopCycle, scaleDownRatio * rightTop[0, 1]);
            Canvas.SetTop(rightTopCycle, scaleDownRatio * rightTop[0, 0]);

            int[,] leftBottom = processor.getLeftBottomIndex();

            Canvas.SetLeft(leftDownCycle, scaleDownRatio * leftBottom[0, 1]);
            Canvas.SetTop(leftDownCycle, scaleDownRatio * leftBottom[0, 0]);

            int[,] rightBottom = processor.getRightBottomIndex();

            Canvas.SetLeft(rightDownCycle, scaleDownRatio * rightBottom[0, 1]);
            Canvas.SetTop(rightDownCycle, scaleDownRatio * rightBottom[0, 0]);

            TabItem item = staticImagePage as TabItem;
            tabControl.SelectedItem = item;
        }

        // 应用生成的拖拽点
        private void Apply(object sender, RoutedEventArgs e)
        {
            // 设置左上角
            this.imageRectangle.getLeftTop().setX(Canvas.GetLeft(rightTopCycle));
            this.imageRectangle.getLeftTop().setY(Canvas.GetTop(rightTopCycle));

            this.imageRectangle.getRightTop().setX(Canvas.GetLeft(leftTopCycle));
            this.imageRectangle.getRightTop().setY(Canvas.GetTop(leftTopCycle));

            this.imageRectangle.getLeftDown().setX(Canvas.GetLeft(rightDownCycle));
            this.imageRectangle.getLeftDown().setY(Canvas.GetTop(rightDownCycle));

            this.imageRectangle.getRightDown().setX(Canvas.GetLeft(leftDownCycle));
            this.imageRectangle.getRightDown().setY(Canvas.GetTop(leftDownCycle));

            //初始化控件整体矩阵，对前空间层进行初始化
            imageBeforeRectangle.setLeftTop(0, 0);
            imageBeforeRectangle.setLeftDown(0, this.realTimeImage.Height);
            imageBeforeRectangle.setRightTop(this.realTimeImage.Width, 0);
            imageBeforeRectangle.setRightDown(this.realTimeImage.Width, this.realTimeImage.Height);



            imageToPix.setSource(imageBeforeRectangle.getLeftTop().getX(),
                        imageBeforeRectangle.getLeftTop().getY(),
                        imageBeforeRectangle.getRightTop().getX(),
                        imageBeforeRectangle.getRightTop().getY(),
                        imageBeforeRectangle.getLeftDown().getX(),
                        imageBeforeRectangle.getLeftDown().getY(),
                        imageBeforeRectangle.getRightDown().getX(),
                        imageBeforeRectangle.getRightDown().getY());

            // 对像素层的坐标进行输出


            imageToPix.setDestination(pixedRectangle.getLeftTop().getX(),
                pixedRectangle.getLeftTop().getY(),
                pixedRectangle.getRightTop().getX(),
                pixedRectangle.getRightTop().getY(),
                pixedRectangle.getLeftDown().getX(),
                pixedRectangle.getLeftDown().getY(),
                pixedRectangle.getRightDown().getX(),
                pixedRectangle.getRightDown().getY());

            imageToPix.computeWarp();

            double xTemp = 0, yTemp = 0;

            //初始化像素选择层

            // 使用变换矩阵将选择控件区域的边界点 映射到 像素选择层
            imageToPix.warp(imageRectangle.getLeftTop().getX(), imageRectangle.getLeftTop().getY(), ref xTemp, ref yTemp);
            pixedSelectedRectangle.setLeftTop(xTemp, yTemp);

            imageToPix.warp(imageRectangle.getLeftDown().getX(), imageRectangle.getLeftDown().getY(), ref xTemp, ref yTemp);
            pixedSelectedRectangle.setLeftDown(xTemp, yTemp);

            imageToPix.warp(imageRectangle.getRightTop().getX(), imageRectangle.getRightTop().getY(), ref xTemp, ref yTemp);
            pixedSelectedRectangle.setRightTop(xTemp, yTemp);

            imageToPix.warp(imageRectangle.getRightDown().getX(), imageRectangle.getRightDown().getY(), ref xTemp, ref yTemp);
            pixedSelectedRectangle.setRightDown(xTemp, yTemp);


            //计算像素层到屏幕层映射矩阵
            pixToScreen.setSource(pixedSelectedRectangle.getLeftTop().getX(),
                pixedSelectedRectangle.getLeftTop().getY(),
                pixedSelectedRectangle.getRightTop().getX(),
                pixedSelectedRectangle.getRightTop().getY(),
                pixedSelectedRectangle.getLeftDown().getX(),
                pixedSelectedRectangle.getLeftDown().getY(),
                pixedSelectedRectangle.getRightDown().getX(),
                pixedSelectedRectangle.getRightDown().getY());

            pixToScreen.setDestination(screenRectangle.getLeftTop().getX(),
                screenRectangle.getLeftTop().getY(),
                screenRectangle.getRightTop().getX(),
                screenRectangle.getRightTop().getY(),
                screenRectangle.getLeftDown().getX(),
                screenRectangle.getLeftDown().getY(),
                screenRectangle.getRightDown().getX(),
                screenRectangle.getRightDown().getY());

            pixToScreen.computeWarp();


            System.Windows.MessageBox.Show("坐标设置完成，请进行背景初始化");

            tabControl.SelectedItem = InitializationPage;
        }


        //边界原点的事件监听器
        private void BoundPoint_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isDrag == false)
            {
                isDrag = true;
                System.Windows.Shapes.Ellipse cycle = (System.Windows.Shapes.Ellipse)sender;
                cycle.MouseMove += BoundPoint_MouseMove;
                if (cycle == leftTopCycle)
                    cycle.MouseLeftButtonUp += LeftTopBound_MouseUp;
                else if (cycle == leftDownCycle)
                    cycle.MouseLeftButtonUp += LeftDownBound_MouseUp;
                else if (cycle == rightTopCycle)
                    cycle.MouseLeftButtonUp += RightTopBound_MouseUp;
                else if (cycle == rightDownCycle)
                    cycle.MouseLeftButtonUp += RightDownBound_MouseUp;
                currSelectedBoundPoint = e.GetPosition(cycle);

                cycle.CaptureMouse();
            }
            //currSelectedBoundPoint = e.GetPosition(autoImageCanvas);
        }



        //原MainWindow方法

        //切换到初始化背景
        public void ChangeToPrepareBackGround(object sender, RoutedEventArgs e)
        {
            nProcessingBackground = 0;

            // 取消对sensor_AllFramesReady事件的注册
            this.sensor.AllFramesReady -= this.sensor_AllFramesReady;

            // 注册prepareBackground事件
            this.sensor.AllFramesReady += this.prepareBackground;//SensorDepthFrameReady
            System.Windows.MessageBox.Show("背景初始化完成");
        }

        //切换到手动校准，已在设计界面代码中绑定
        public void ChangeToCheck(object sender, RoutedEventArgs e)
        {
            redCheckCircle.Show();
            index = 0;

            this.sensor.AllFramesReady -= this.sensor_AllFramesReady;
            this.sensor.AllFramesReady += this.sensor_CheckFramesReady;//SensorDepthFrameReady
        }

        /// <summary>
        /// 初始化背景，处理深度数据
        /// 
        /// 当前采用的是平均处理的方式 ==> can be better 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void prepareBackground(object sender, AllFramesReadyEventArgs e)
        {
            Console.WriteLine("准备背景中..");
            //定义背景资源为空
            //BitmapSource backgroundBmp;
            //打开颜色帧
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                //打开深度帧
                using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
                {
                    //如果深度帧数据不为空，则进行处理
                    if (depthFrame != null)
                    {
                        //处理次数判断
                        if (this.nProcessingBackground < this.processingBackgroundTimes)
                        {
                            //在进行任何处理之前，则调用ImageHelpers.cs中的初始化方法，将深度帧传入，同步深度帧像素长度
                            if (this.nProcessingBackground == 0)
                            {
                                depthFrame.InitializeBackground();
                            }
                            //Console.WriteLine("深度帧的像素长度为：" + depthFrame.PixelDataLength);
                            //把滑块选中的最小值和最大值之间的depth选出
                            depthFrame.PrepareBackgroundDepth((int)this.sliderMin.Value, (int)this.sliderMax.Value);
                            this.nProcessingBackground++;
                        }
                        //判断处理次数达到预定次数
                        else if (this.nProcessingBackground == this.processingBackgroundTimes)
                        {
                            Console.WriteLine("完成");
            
                            nProcessingBackground++;

                            // 进行背景平均

                            depthFrame.averageBackground(this.processingBackgroundTimes, (int)depthBias.Value);
                            //AllFramesReady为当从每个Kinect传感器的活动流中获得新帧时触发的事件
                            //我们在此将该事件的触发方法由prepareBackground方法更改到sensor_AllFramesReady方法
                            this.sensor.ColorFrameReady -= this.SensorColorFrameReady;
                            this.sensor.AllFramesReady -= this.prepareBackground;
                            this.sensor.AllFramesReady += this.sensor_AllFramesReady;
                        }
                    }

                }
            }
        }

        /// <summary>
        /// 初始化背景后，调用该方法进行实时深度数据监听
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sensor_CheckFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            //将点存入队列中
            Queue<double[]> screen_points = new Queue<double[]>();
            //初始化一个位图资源为空
            BitmapSource depthBmp = null;

            //打开深度帧
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                //判断深度帧不为空
                if (depthFrame != null)
                {
                    //重新令识别结果数为0
                    //blobCount = 0;

                    // 把滑块选中的最小值和最大值之间的depth选出
                    // 去掉一个最大值和最小值
                //    depthBmp = depthFrame.SliceDepthImage((int)this.sliderDMin.Value, (int)this.sliderDMax.Value);

                //    //定义彩色图像
                //    Image<Bgr, byte> openCVImg = new Image<Bgr, byte>(depthBmp.ToBitmap());
                //    //定义灰度图像
                //    Image<Gray, byte> gray_image = openCVImg.Convert<Gray, byte>();
                //    //Image<Gray, byte> gray_image = new Image<Gray, byte>(depthBmp.ToBitmap());
                //    //创建一个OpenCV内存
                //    using (MemStorage stor = new MemStorage())
                //    {
                //        //找轮廓
                //        Contour<System.Drawing.Point> contours = gray_image.FindContours(
                //         Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE,  // 只保留轮廓的拐点信息
                //         Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_EXTERNAL,     // 只检测最外围轮廓，包含在外围轮廓之内的内围轮廓被忽略
                //         stor);

                //        //遍历轮廓，HNext是一个指针，与CvSeq相同
                //        for (int i = 0; contours != null; contours = contours.HNext)
                //        {
                //            i++;
                //            //判断轮廓区域是否满足我们在界面上设定的条件 对应minSize和maxSize
                //            if ((contours.Area > Math.Pow(this.sliderMinSize.Value, 2)) && (contours.Area < Math.Pow(this.sliderMaxSize.Value, 2)))
                //            {

                //                //得到最小面积矩形区域并返回，MCvBox2D相当于CvBox2D的托管结构
                //                MCvBox2D box = contours.GetMinAreaRect();
                //                //将序列返还为数组
                //                System.Drawing.Point[] points = contours.ToArray();
                //                //找到点集的中心点
                //                System.Drawing.Point center = findCenterByMoments(contours);
                //                //System.Drawing.Point center = findCenter(points);

                //                DepthImagePoint depthImagePoint = new DepthImagePoint();
                //                depthImagePoint.X = center.X;
                //                depthImagePoint.Y = center.Y;
                //                int depthIndex = depthFrame.Width * center.Y + center.X;
                //                depthImagePoint.Depth = depthPixels[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                //                //if (depthImagePoint.Depth < 10)
                //                //{
                //                //    for (int searchIndex = depthIndex; searchIndex < this.sensor.DepthStream.FramePixelDataLength; searchIndex++)
                //                //    {
                //                //        if (searchIndex == this.sensor.DepthStream.FramePixelDataLength - 1)
                //                //        {
                //                //            searchIndex = 0;
                //                //        }
                //                //        else
                //                //        {
                //                //            searchIndex++;
                //                //        }
                //                //        if (depthPixels[searchIndex] > 10)
                //                //        {
                //                //            depthImagePoint.Depth = depthPixels[searchIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                //                //            break;
                //                //        }

                //                //    }
                //                //}
                //                ColorImagePoint colorImagePoint = this.sensor.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, depthImagePoint, ColorImageFormat.RgbResolution640x480Fps30);
                //                //Console.WriteLine("转换前深度：" + depthImagePoint.Depth);

                //                //Console.WriteLine("获取到原始X坐标：" + center.X);
                //                //Console.WriteLine("获取到原始Y坐标：" + center.Y);
                //                double x_virtual = 0;
                //                double y_virtual = 0;
                //                //以中心点为圆心，5为半径的圆形
                //                CircleF circle = new CircleF(center, 5);
                //                //画出该圆，其中厚度为3
                //                openCVImg.Draw(circle, new Bgr(System.Drawing.Color.Red), 3);


                //                center.X = (center.X - 320) * 2 + 640;
                //                center.Y = (center.Y - 240) * 2 + 480;
                //                this.positionTransfer(colorImagePoint.X, colorImagePoint.Y, ref x_virtual, ref y_virtual);


                //                //加入点
                //                if(x_virtual != -1 || y_virtual != -1)
                //                    screen_points.Enqueue(new double[2] { x_virtual, y_virtual });

                //                //Console.WriteLine("获取到X坐标：" + x_virtual);
                //                //Console.WriteLine("获取到Y坐标：" + y_virtual);

                //                //结果+1                                   
                //                //blobCount++;
                //            }
                //        }

                //        if (screen_points.Count != 0)
                //        {
                //            //取出点
                //            double[] screen_point = screen_points.Peek();
                //            //点数目++
                //            index++;
                //            points_store[index - 1, 0] = screen_point[0];
                //            points_store[index - 1, 1] = screen_point[1];

                //            if (index > 30)
                //            {
                //                Console.WriteLine("开始矫正");
                //                Console.WriteLine(points_store[index - 1, 0] + "  " + points_store[index - 1, 1]);
                //                xWhole += points_store[index - 1, 0];
                //                yWhole += points_store[index - 1, 1];
                //                MouseClicker.DoMouseMove((int)points_store[index - 1, 0], (int)points_store[index - 1, 1]);
                //            }
                //            else
                //            {
                //                MouseClicker.DoMouseMove((int)points_store[index - 1, 0], (int)points_store[index - 1, 1]);
                //            }
                //            if (index == 60)
                //            {
                //                double xAve = xWhole / 30;
                //                double yAve = yWhole / 30;
                //                Console.WriteLine("xAve:" + xAve + " yAve:" + yAve);
                //                double xCenter = Screen.PrimaryScreen.Bounds.Width / 2;
                //                double yCenter = Screen.PrimaryScreen.Bounds.Height / 2;
                //                Console.WriteLine("xCenter:" + xCenter + " yCenter:" + yCenter);
                //                xOffset = xAve - xCenter;
                //                yOffset = yAve - yCenter;
                //                Console.WriteLine("xOffset:" + xOffset + " yOffset:" + yOffset);
                //                redCheckCircle.Close();
                //                Console.WriteLine("更改挂载");
                //                Array.Clear(points_store, 0, index);
                //                index = 0;
                //                this.sensor.AllFramesReady -= this.sensor_CheckFramesReady;
                //                this.sensor.AllFramesReady += this.sensor_AllFramesReady;
                //            }
                //        }
                //    }
                //    //将处理后的深度识别结果输出到WPF界面上
                //    this.outImg.Source = ImageHelpers.ToBitmapSource(openCVImg);
                }
            }
        }

        /// <summary>
        /// 初始化背景后，调用该方法进行实时深度数据监听
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {

            //将点存入队列中
            Queue<double[]> screen_points = new Queue<double[]>();
            //识别结果数
            //blobCount = 0;

            //打开深度帧
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                //判断深度帧不为空
                if (depthFrame != null)
                {
                    //初始化一个位图资源为空
                    //重新令识别结果数为0
                    //blobCount = 0;
                    // 把滑块选中(用户设定)的最小值和最大值之间的depth选出

                    long totalStart = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                    long start = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();

                   
                    System.Drawing.Bitmap depthBmp= depthFrame.SliceDepthImage((int)this.sliderDMin.Value, (int)this.sliderDMax.Value);
                    long end = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                    //Console.Write("识别深度:{0}\t",end-start);


                    start = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                    depthFrame.CopyPixelDataTo(depthPixels);
                    end = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                    Console.Write("复制:{0}\t", end - start);

                    Image<Gray, byte> gray_image = new Image<Gray, byte>(depthBmp);
                    var thres_image = gray_image.CopyBlank();

                    // 对灰度图像进行高斯平滑处理和Otsu二值化（更多阈值化方法？）
                    gray_image.SmoothMedian(5);
                    CvInvoke.cvThreshold(gray_image, gray_image, 0, 255, Emgu.CV.CvEnum.THRESH.CV_THRESH_OTSU);

                    //Image<Gray, byte> gray_image = new Image<Gray, byte>(depthBmp.ToBitmap());
                    //创建一个OpenCV内存
                    using (MemStorage stor = new MemStorage())
                    {
                        //找轮廓
                        start = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                        Contour<System.Drawing.Point> contours = gray_image.FindContours(
                         Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE,
                         Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_EXTERNAL,
                         stor);
                        end = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                        Console.Write("识别轮廓:{0}\t", end - start);
                        
                        long contourStart= new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                        //遍历轮廓，HNext是一个指针，与CvSeq相同
                        for (int i = 0; contours != null; contours = contours.HNext)
                        {
                            i++;
                            //判断轮廓区域是否满足我们在界面上设定的条件 对应minSize和maxSize
                            if ((contours.Area > Math.Pow(this.sliderMinSize.Value, 2)) && (contours.Area < Math.Pow(this.sliderMaxSize.Value, 2)))
                            {
                                start = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                                //得到最小面积矩形区域并返回，MCvBox2D相当于CvBox2D的托管结构
                                
                                MCvBox2D box = contours.GetMinAreaRect();
                                //MCvBox2D box1 = contours
                                //将序列返还为数组
                                System.Drawing.Point[] points = contours.ToArray();

                                //找到点集的中心点?顶点？                   
                                //System.Drawing.Point center = findCenterByMoments(contours);
                                end = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                                System.Drawing.Point center = findTop(points);


                                DepthImagePoint depthImagePoint = new DepthImagePoint
                                {
                                    X = center.X + (int)xBias.Value,
                                    Y = center.Y + (int)yBias.Value
                                };
                                ////depthImagePoint.Depth = depthPixels[depthFrame.Width * center.Y + center.X];
                                //depthImagePoint.Depth = 1410;
                                //ColorImagePoint colorImagePoint = this.sensor.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, depthImagePoint, ColorImageFormat.RgbResolution640x480Fps30);

                                // 估算出中心点的下标
                                int depthIndex = depthFrame.Width * center.Y + center.X;


                                depthImagePoint.Depth = depthPixels[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                                ColorImagePoint colorImagePoint = this.sensor.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, depthImagePoint, ColorImageFormat.RgbResolution640x480Fps30);
                                //Console.WriteLine("转换前深度：" + depthImagePoint.Depth);

                                //Console.WriteLine("获取到原始X坐标：" + center.X);
                                //Console.WriteLine("获取到原始Y坐标：" + center.Y);
                                double x_virtual = 0;
                                double y_virtual = 0;
                                //以中心点为圆心，5为半径的圆形
                                //CircleF circle = new CircleF(center, 5);
                                //画出该圆，其中厚度为3
                                //openCVImg.Draw(circle, new Bgr(System.Drawing.Color.Red), 3);

                                start = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                                //openCVImg.DrawPolyline(points, true, new Bgr(System.Drawing.Color.Blue), 3);
                                end = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                                Console.Write("生成线:{0}\t", end - start);


                                center.X = (center.X - 320) * 2 + 640;
                                center.Y = (center.Y - 240) * 2 + 480;
                                start = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                                this.positionTransfer(colorImagePoint.X, colorImagePoint.Y, ref x_virtual, ref y_virtual);
                                if (x_virtual != -1 || y_virtual != -1)
                                    screen_points.Enqueue(new double[2] { x_virtual, y_virtual });
                                end = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                                Console.Write("转换坐标:{0}\t", end - start);

                                //加入点
                                

                                //Console.WriteLine("获取到X坐标：" + x_virtual);
                                //Console.WriteLine("获取到Y坐标：" + y_virtual);

                                //结果+1                                   
                                //blobCount++;
                            }
                        }
                        long contourEnd= new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                        Console.Write("寻找轮廓{0}\t", contourEnd - contourStart);


                        // 已经获取到屏幕上的一系列点，接下来作出动作判断 

                        start = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                        if (screen_points.Count != 0 && !ifRightClick)
                        {
                            //取出点
                            double[] screen_point = screen_points.Peek();
                            //点数目++
                            index++;
                            points_store[index - 1, 0] = screen_point[0];
                            points_store[index - 1, 1] = screen_point[1];

                            MouseClicker.DoMouseMove((int)points_store[index - 1, 0], (int)points_store[index - 1, 1]);
                            //当index等于I+1时，判断是右键还是拖拽
                            if (index == I + 1)
                            {
                                //当第I + 1个点和第一个点之间的欧氏距离大于10个像素，则判定为拖拽
                            if (Math.Pow((Math.Pow((points_store[I, 0] - points_store[0, 0]), 2) + Math.Pow((points_store[I, 1] - points_store[0, 1]), 2)), 0.5) > 40)
                                {
                                    //判定为拖拽开始
                                    MouseClicker.MouseDragStart((int)(points_store[0, 0] - xOffset), (int)(points_store[0, 1] - yOffset));
                                    for (; dragIndex < (index - 1); dragIndex++)
                                    {
                                        MouseClicker.MouseDragMove((int)(points_store[dragIndex, 0] - xOffset), (int)(points_store[dragIndex, 1] - yOffset));
                                    }
                                    
                                    
                                    
                                    ifDragNow = true;
                                    //线程开始
                                    //dragThread.Resume();
                                }
                                else
                                {
                                    //判定为右键单击
                                    //MouseClicker.MouseRightSingleClick((int)(points_store[0, 0] - xOffset), (int)(points_store[0, 1] - yOffset));
                                    //ifRightClick = true;
                                    //有效点数目大于0小于等于I，此时构成单击事件
                                    //判定为拖拽开始
                                    MouseClicker.MouseDragStart((int)(points_store[0, 0] - xOffset), (int)(points_store[0, 1] - yOffset));
                                    for (; dragIndex < (index - 1); dragIndex++)
                                    {
                                        MouseClicker.MouseDragMove((int)(points_store[dragIndex, 0] - xOffset), (int)(points_store[dragIndex, 1] - yOffset));
                                    }



                                    ifDragNow = true;
                                }
                            }
                            //if (dragIndex % 2 == 0)
                            if (ifDragNow)
                            {
                                MouseClicker.MouseDragMove((int)(points_store[dragIndex, 0] - xOffset), (int)(points_store[dragIndex, 1] - yOffset));
                                dragIndex++;
                            }
                        }
                        else if (screen_points.Count == 0 && index != 0)//此帧没有有效点传入，进行事件判断
                        {
                            if (ifRightClick)
                            {
                                //初始化数组points_score
                                Array.Clear(points_store, 0, index);
                                //初始化index
                                index = 0;
                                ifRightClick = false;
                                goto stop;
                            }
                            if (index > 0 && index <= I)
                            {
                                //有效点数目大于0小于等于I，此时构成单击事件
                                MouseClicker.MouseLeftSingleClick((int)(points_store[0, 0] - xOffset), (int)(points_store[0, 1] - yOffset));
                                //初始化数组points_score
                                Array.Clear(points_store, 0, index);
                                //初始化index
                                index = 0;
                            }
                            if (ifDragNow && noPointFrameNum > 4)
                            {
                                //ifDragNow为true代表上一帧时仍处于拖拽状态，因此我们在这里取消拖拽状态
                                MouseClicker.MouseDragEnd((int)(points_store[0, 0] - xOffset), (int)(points_store[0, 1] - yOffset));
                                ifDragNow = false;
                                dragIndex = 0;
                                Array.Clear(points_store, 0, index);
                                index = 0;
                                noPointFrameNum = 0;
                                //dragThread.Suspend();
                            }
                            else if (ifDragNow)
                            {
                                noPointFrameNum++;
                            }
                        stop:;
                        }

                    }
                    end = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                    Console.Write("操作:{0}\t", end - start);
                    
                    //将处理后的深度识别结果输出到WPF界面上
                    start = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                    this.outImg.Source = ImageHelpers.ToBitmapSource(gray_image);
                    end = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                    Console.Write("画图:{0}\t", end - start);
                    long totalEnd = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                    Console.Write("总共:{0}\n", totalEnd - totalStart);
                }
            }
        }


        private System.Drawing.Point findCenter(System.Drawing.Point[] points)
        {
            // 直接平均求解质心
            int x = 0;
            int y = 0;
            //得到x轴和y轴的总长度
            for (int i = 0; i < points.Length; i++)
            {
                x += points[i].X;
                y += points[i].Y;
            }
            //除以x轴和y轴的点数量
            x /= points.Length;
            y /= points.Length;


            Console.WriteLine("利用中值求得中心点的坐标为({0},{1}\n)", x, y);

            //返还中心点
            return new System.Drawing.Point(x, y);
        }

        // 获取顶点
        private System.Drawing.Point findTop(System.Drawing.Point[] points)
        {
            // 直接平均求解质心
            int x = 0;
            int y = 0;
            //得到x轴和y轴的总长度
            for (int i = 0; i < points.Length; i++)
            {
                x += points[i].X;
                y += points[i].Y;
            }
            //除以x轴和y轴的点数量
            x /= points.Length;
            y /= points.Length;


            Console.WriteLine("利用中值求得中心点的坐标为({0},{1}\n)", 2 * x, 2 * y);

            //返还中心点
            return new System.Drawing.Point(x, y);
        }

        /// <summary>
        /// 找到点集的中心
        /// </summary>
        /// <param name="contours"></param>
        /// <returns></returns>
        private System.Drawing.Point findCenterByMoments(Contour<System.Drawing.Point> contours)
        {

            // 使用图像矩计算图像的质心
            MCvMoments moments = contours.GetMoments();
            int x = (int)(moments.m10 / moments.m00);
            int y = (int)(moments.m01 / moments.m00);
            return new System.Drawing.Point(x, y);
        }

        /// <summary>
        /// 当像素流更新的时候，触发该事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    colorFrame.CopyPixelDataTo(this.colorPixels);

                    // 实时识别
                    if (++timestamp % 10 == 0)
                    {
                        this.setPixels(this.colorPixels);
                    }
                }
            }
        }

        /// <summary>
        /// 窗口关闭触发事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowClosing(object sender, CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
            Environment.Exit(0);
        }
    }
}
