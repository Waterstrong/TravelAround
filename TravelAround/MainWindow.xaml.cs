using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
//using System.Windows.Shapes;

using System.Globalization;
using System.IO;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.BackgroundRemoval;

using NUI.Motion;
using NUI.Data;
using System.Windows.Threading;

namespace TravelAround
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        SkeletonSubject _skeSubject = new SkeletonSubject();
        private MotionSuper _motion = null; // 动作识别入口

        private DispatcherTimer _timer = new DispatcherTimer(); // 计时器
        private DispatcherTimer _timer2 = new DispatcherTimer();
        //private int _timerCount = 0; // 时间计数器

        //背景音乐
        private MediaPlayer _backgroundMusic = new MediaPlayer();
        //打中时的音效
        private MediaPlayer _sceneMusic = new MediaPlayer();

        /// <summary>
        /// Format we will use for the depth stream
        /// </summary>
        private const DepthImageFormat DepthFormat = DepthImageFormat.Resolution320x240Fps30;

        /// <summary>
        /// Format we will use for the color stream
        /// </summary>
        private const ColorImageFormat ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap foregroundBitmap;

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensorChooser sensorChooser;

        /// <summary>
        /// Our core library which does background 
        /// </summary>
        private BackgroundRemovedColorStream backgroundRemovedColorStream;

        /// <summary>
        /// Intermediate storage for the skeleton data received from the sensor
        /// </summary>
        private Skeleton[] skeletons;

        /// <summary>
        /// the skeleton that is currently tracked by the app
        /// </summary>
        private int currentlyTrackedSkeletonId;

        /// <summary>
        /// Track whether Dispose has been called
        /// </summary>
        private bool disposed;


        public MainWindow()
        {
            InitializeComponent();

            // initialize the sensor chooser and UI
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.KinectChanged += this.SensorChooserOnKinectChanged;
            this.sensorChooser.Start();
        }

        ///// <summary>
        ///// Finalizes an instance of the MainWindow class.
        ///// This destructor will run only if the Dispose method does not get called.
        ///// </summary>
        //~MainWindow()
        //{
            
        //}

        #region 基本事件
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FullScreen.GoFullscreen(this);
            image_scene.Source = new BitmapImage(new Uri("/scene/swust0.jpg", UriKind.Relative));
            image_left.Source = new BitmapImage(new Uri("/scene/left0.png", UriKind.Relative));
            image_right.Source = new BitmapImage(new Uri("/scene/right0.png", UriKind.Relative));

            try
            {
                // 创建处理链
                _motion = MotionFactory.CreateHandleChain();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.Close();
                //item_exit_Click(sender, e);
            }

            _skeSubject.DataBinding += new SkeletonSubject.DataBindingEventHandler(_skeSubject_DataBinding);

            _timer.Interval = TimeSpan.FromSeconds(0.01);
            _timer.Tick += new EventHandler(_timer_Tick);

            _timer2.Interval = TimeSpan.FromSeconds(0.01);
            _timer2.Tick += new EventHandler(_timer2_Tick);
            //_timer.Start();

            //播放背景音乐
           // _backgroundMusic.Open(new Uri(Environment.CurrentDirectory + "\\music\\bm.wma", UriKind.Relative));
           // _backgroundMusic.Play();
            //播放讲解
            _sceneMusic.Open(new Uri(Environment.CurrentDirectory + "\\music\\swust0.wma", UriKind.Relative));
            _sceneMusic.Play();
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _backgroundMusic.Stop();
            _sceneMusic.Stop();
            this.sensorChooser.Stop();
            this.sensorChooser = null;
            this.Dispose(false);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                FullScreen.ExitFullscreen(this); // 如果已经退出了全屏，则退出程序
            }
        }

        int _stretch = 0;
        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FullScreen.GoFullscreen(this);
            image_scene.Stretch = (Stretch)_stretch;//(Stretch)
            if (++_stretch > 3)
            {
                _stretch = 0;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        #endregion

        bool hasChangeScence = false;
        /// <summary>
        /// 计时器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _timer_Tick(object sender, EventArgs e)
        {
            if (hasChangeScence == false)
            {
                image_scene.Opacity -= 0.01d;
                if (image_scene.Opacity <= 0.1d)
                {
                    if (++_scene > 4)
                    {
                        _scene = 0;
                        _direct = "swust";
                        image_left.Visibility = Visibility.Visible;
                        image_right.Visibility = Visibility.Visible;
                    }
                    image_scene.Source = new BitmapImage(new Uri("/scene/" + _direct.ToString() +_scene.ToString() + ".jpg", UriKind.Relative));
                    //播放讲解
                    _sceneMusic.Open(new Uri(Environment.CurrentDirectory + "\\music\\" + _direct.ToString() + _scene.ToString() + ".wma", UriKind.Relative));
                    _sceneMusic.Play();
                    
                    hasChangeScence = true;
                }
            }
            else
            {
                image_scene.Opacity += 0.01d;
                if (image_scene.Opacity >= 1.0d)
                {
                    (sender as DispatcherTimer).Stop();
                    hasChangeScence = false;
                }
            }
           
        }
        
        int _movingDict = 0;
        void _timer2_Tick(object sender, EventArgs e)
        {
            image_mask.Margin = new Thickness(image_mask.Margin.Left + _movingDict * image_mask.Width, 0, 0, 70);
            (sender as DispatcherTimer).Stop();
        }


        private string _direct = "swust";
        private int _scene = 0;
        private int _walk = 0;
        private bool leftFoot = true;
        private const double TimeSecondsThreshold = 15d; // 时间阈值，以毫秒计算
        protected DateTime _dtBegin; // 时间戳开始

        /// <summary>
        /// 骨架信息生成了特征数据开始识别
        /// </summary>
        /// <param name="data"></param>
        void _skeSubject_DataBinding(FeatureData data)
        {
            if (_direct!= "swust" && _walk != 0) //识别走
            {
                if (DateTime.Now.Subtract(_dtBegin).TotalSeconds < TimeSecondsThreshold)
                {
                    if (_walk >= 6)
                    {
                        _walk = 0;
                        _timer.Start();
                    }
                }
                else
                {
                    _walk = 0; 
                }
            }
            
            try
            {
                // 动作进行识别
                int rec = _motion.HandleDataEx(data);
                switch (rec)
                {
                    case -1:
                        return; //未识别直接返回
                    case 1: // 左脚走
                        if (_walk == 0)
                        {
                            ++_walk;
                            _dtBegin = DateTime.Now; // 获取开始时间
                            leftFoot = true;
                        }
                        else if (leftFoot == false)
                        {
                            leftFoot = true;
                            ++_walk;
                        }
                        break;
                    case 2: // footrightup
                        if (_walk == 0)
                        {
                            ++_walk;
                            _dtBegin = DateTime.Now; // 获取开始时间
                            leftFoot = false;
                        }
                        else if (leftFoot == true)
                        {
                            leftFoot = false; 
                            ++_walk;
                        }
                        break;
                    case 32: //左滑动
                        if (_direct == "swust")
                        {
                            _direct = "cq";
                            image_left.Visibility = Visibility.Hidden;
                            image_right.Visibility = Visibility.Hidden;
                            _timer.Start();
                        }
                        break;
                    case 33: // 右滑动
                        if (_direct == "swust")
                        {
                            _direct = "jy";
                            image_left.Visibility = Visibility.Hidden;
                            image_right.Visibility = Visibility.Hidden;
                            _timer.Start();
                        }
                        break;
                    case 20: // 变大
                        if (image_mask.Height*2 > this.Height)
                        {
                            return;
                        }
                        image_mask.Height *= 2;
                        image_mask.Width *= 2;
                        
                        break;
                    case 22: // 变小
                        if (image_mask.Height/2 < 120)
                        {
                            return;
                        }
                        image_mask.Height /= 2;
                        image_mask.Width /= 2;
                        break;
                    case 30: // 左移动
                        _movingDict = -1;
                        //if (image_mask.Margin.Left + _movingDict * image_mask.Width < -image_mask.Width)
                        //{
                        //    return;
                        //}
                        _timer2.Start();
                        break;
                    case 31:
                        _movingDict = 1;
                        if (image_mask.Margin.Left + _movingDict * image_mask.Width > this.Width)
                        {
                            return;
                        }
                        _timer2.Start();
                        break;
                    case 21: // 游泳
                        if (_direct == "cq" && _scene == 4)
                        {
                            _walk += 2;
                        }
                        break;
                    default:
                        break;
                }

            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        #region Kinect相关以及背景去除

        /// <summary>
        /// Dispose the allocated frame buffers and reconstruction.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);

            // This object will be cleaned up by the Dispose method.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Frees all memory associated with the FusionImageFrame.
        /// </summary>
        /// <param name="disposing">Whether the function was called from Dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (null != this.backgroundRemovedColorStream)
                {
                    this.backgroundRemovedColorStream.Dispose();
                    this.backgroundRemovedColorStream = null;
                }

                this.disposed = true;
            }
        }


        /// <summary>
        /// Event handler for Kinect sensor's DepthFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            // in the middle of shutting down, or lingering events from previous sensor, do nothing here.
            if (null == this.sensorChooser || null == this.sensorChooser.Kinect || this.sensorChooser.Kinect != sender)
            {
                return;
            }

            try
            {
                using (var depthFrame = e.OpenDepthImageFrame())
                {
                    if (null != depthFrame)
                    {
                        this.backgroundRemovedColorStream.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);
                    }
                }

                using (var colorFrame = e.OpenColorImageFrame())
                {
                    if (null != colorFrame)
                    {
                        this.backgroundRemovedColorStream.ProcessColor(colorFrame.GetRawPixelData(), colorFrame.Timestamp);
                    }
                }

                using (var skeletonFrame = e.OpenSkeletonFrame())
                {
                    if (null != skeletonFrame)
                    {
                        skeletonFrame.CopySkeletonDataTo(this.skeletons);
                        this.backgroundRemovedColorStream.ProcessSkeleton(this.skeletons, skeletonFrame.Timestamp);
                    }
                }

                this.ChooseSkeleton();
            }
            catch (InvalidOperationException)
            {
                // Ignore the exception. 
            }
        }

        /// <summary>
        /// Handle the background removed color frame ready event. The frame obtained from the background removed
        /// color stream is in RGBA format.
        /// </summary>
        /// <param name="sender">object that sends the event</param>
        /// <param name="e">argument of the event</param>
        private void BackgroundRemovedFrameReadyHandler(object sender, BackgroundRemovedColorFrameReadyEventArgs e)
        {
            using (var backgroundRemovedFrame = e.OpenBackgroundRemovedColorFrame())
            {
                if (backgroundRemovedFrame != null)
                {
                    if (null == this.foregroundBitmap || this.foregroundBitmap.PixelWidth != backgroundRemovedFrame.Width
                        || this.foregroundBitmap.PixelHeight != backgroundRemovedFrame.Height)
                    {
                        this.foregroundBitmap = new WriteableBitmap(backgroundRemovedFrame.Width, backgroundRemovedFrame.Height, 96.0, 96.0, PixelFormats.Bgra32, null);

                        // Set the image we display to point to the bitmap where we'll put the image data
                        this.image_mask.Source = this.foregroundBitmap;
                    }

                    // Write the pixel data into our bitmap
                    this.foregroundBitmap.WritePixels(
                        new Int32Rect(0, 0, this.foregroundBitmap.PixelWidth, this.foregroundBitmap.PixelHeight),
                        backgroundRemovedFrame.GetRawPixelData(),
                        this.foregroundBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }

        /// <summary>
        /// Use the sticky skeleton logic to choose a player that we want to set as foreground. This means if the app
        /// is tracking a player already, we keep tracking the player until it leaves the sight of the camera, 
        /// and then pick the closest player to be tracked as foreground.
        /// </summary>
        private void ChooseSkeleton()
        {
            var isTrackedSkeltonVisible = false;
            var nearestDistance = float.MaxValue;
            var nearestSkeletonID = 0;

            foreach (var skel in this.skeletons)
            {
                if (null == skel)
                {
                    continue;
                }

                if (skel.TrackingState != SkeletonTrackingState.Tracked)
                {
                    continue;
                }

                if (skel.TrackingId == this.currentlyTrackedSkeletonId)
                {
                    isTrackedSkeltonVisible = true;
                    _skeSubject.Notify(skel); //进行特征提取
                    break;
                }

                if (skel.Position.Z < nearestDistance)
                {
                    nearestDistance = skel.Position.Z;
                    nearestSkeletonID = skel.TrackingId;
                }
            }

            if (!isTrackedSkeltonVisible && nearestSkeletonID != 0)
            {
                this.backgroundRemovedColorStream.SetTrackedPlayer(nearestSkeletonID);
                this.currentlyTrackedSkeletonId = nearestSkeletonID;
            }
            
        }

        /// <summary>
        /// Called when the KinectSensorChooser gets a new sensor
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="args">event arguments</param>
        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {
            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.AllFramesReady -= this.SensorAllFramesReady;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.ColorStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();

                    // Create the background removal stream to process the data and remove background, and initialize it.
                    if (null != this.backgroundRemovedColorStream)
                    {
                        this.backgroundRemovedColorStream.BackgroundRemovedFrameReady -= this.BackgroundRemovedFrameReadyHandler;
                        this.backgroundRemovedColorStream.Dispose();
                        this.backgroundRemovedColorStream = null;
                    }
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message);
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }

            if (args.NewSensor != null)
            {
                try
                {
                    args.NewSensor.DepthStream.Enable(DepthFormat);
                    args.NewSensor.ColorStream.Enable(ColorFormat);
                    args.NewSensor.SkeletonStream.Enable();

                    this.backgroundRemovedColorStream = new BackgroundRemovedColorStream(args.NewSensor);
                    this.backgroundRemovedColorStream.Enable(ColorFormat, DepthFormat);

                    // Allocate space to put the depth, color, and skeleton data we'll receive
                    if (null == this.skeletons)
                    {
                        this.skeletons = new Skeleton[args.NewSensor.SkeletonStream.FrameSkeletonArrayLength];
                    }

                    // Add an event handler to be called when the background removed color frame is ready, so that we can
                    // composite the image and output to the app
                    this.backgroundRemovedColorStream.BackgroundRemovedFrameReady += this.BackgroundRemovedFrameReadyHandler;

                    // Add an event handler to be called whenever there is new depth frame data
                    args.NewSensor.AllFramesReady += this.SensorAllFramesReady;

                    try
                    {
                        //waterstrong args.NewSensor.DepthStream.Range = this.checkBoxNearMode.IsChecked.GetValueOrDefault()
                        //                            ? DepthRange.Near
                        //                            : DepthRange.Default;
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // Non Kinect for Windows devices do not support Near mode, so reset back to default mode.
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }

                    //waterstrong this.statusBarText.Text = Properties.Resources.ReadyForScreenshot;
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message);
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }
        }


        /// <summary>
        /// Handles the user clicking on the screenshot button
        /// </summary>
        private void SreenshotSave()
        {
            if (null == this.sensorChooser || null == this.sensorChooser.Kinect)
            {
                //waterstrong this.statusBarText.Text = Properties.Resources.ConnectDeviceFirst;
                return;
            }

            int colorWidth = this.foregroundBitmap.PixelWidth;
            int colorHeight = this.foregroundBitmap.PixelHeight;

            // create a render target that we'll render our controls to
            var renderBitmap = new RenderTargetBitmap(colorWidth, colorHeight, 96.0, 96.0, PixelFormats.Pbgra32);

            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                // render the backdrop
                var backdropBrush = new VisualBrush(image_scene);
                dc.DrawRectangle(backdropBrush, null, new Rect(new Point(), new Size(colorWidth, colorHeight)));

                // render the color image masked out by players
                var colorBrush = new VisualBrush(image_mask);
                dc.DrawRectangle(colorBrush, null, new Rect(new Point(), new Size(colorWidth, colorHeight)));
            }

            renderBitmap.Render(dv);

            // create a png bitmap encoder which knows how to save a .png file
            BitmapEncoder encoder = new PngBitmapEncoder();

            // create frame from the writable bitmap and add to encoder
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            var time = DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

            var myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            var path = Path.Combine(myPhotos, "TravelAroundSnapshot-" + time + ".png");

            // write the new file to disk
            try
            {
                using (var fs = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(fs);
                }

                //waterstrong this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture, Properties.Resources.ScreenshotWriteSuccess, path);
            }
            catch (IOException)
            {
                //waterstrong this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture, Properties.Resources.ScreenshotWriteFailed, path);
            }
        }
        #endregion

    }
}
