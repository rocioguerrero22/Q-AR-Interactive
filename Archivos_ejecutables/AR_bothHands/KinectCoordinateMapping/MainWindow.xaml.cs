using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KinectCoordinateMapping
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CameraMode _mode = CameraMode.Color;
        KinectSensor _sensor;
        Skeleton[] _bodies = new Skeleton[6];

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor = KinectSensor.KinectSensors.Where(s => s.Status == KinectStatus.Connected).FirstOrDefault();

            if (_sensor != null)
            {
                _sensor.ColorStream.Enable();
                _sensor.DepthStream.Enable();
                _sensor.SkeletonStream.Enable();

                _sensor.AllFramesReady += Sensor_AllFramesReady;

                _sensor.Start();
            }
        }

        void Sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            // Color
            using (var frame = e.OpenColorImageFrame())
            {
                if (frame != null)
                {
                    if (_mode == CameraMode.Color)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Depth
            using (var frame = e.OpenDepthImageFrame())
            {
                if (frame != null)
                {
                    if (_mode == CameraMode.Depth)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Body
            using (var frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    canvas.Children.Clear();

                    frame.CopySkeletonDataTo(_bodies);

                    foreach (var body in _bodies)
                    {
                        if (body.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            Point leftHand = new Point();
                            Point rightHand = new Point();
                            // COORDINATE MAPPING
                            foreach (Joint joint in body.Joints)
                            {
                                if (joint.JointType.ToString() == "HandLeft")
                                {
                                    // 3D coordinates in meters
                                    SkeletonPoint skeletonPoint = joint.Position;
                                    //Console.WriteLine(joint.JointType);
                                    // 2D coordinates in pixels
                                    //Point point = new Point();
                                    canvas.Children.Clear();
                                    if (_mode == CameraMode.Color)
                                    {
                                        // Skeleton-to-Color mapping
                                        ColorImagePoint colorPoint = _sensor.CoordinateMapper.MapSkeletonPointToColorPoint(skeletonPoint, ColorImageFormat.RgbResolution640x480Fps30);

                                        leftHand.X = colorPoint.X;
                                        leftHand.Y = colorPoint.Y;
                                    }
                                    else if (_mode == CameraMode.Depth) // Remember to change the Image and Canvas size to 320x240.
                                    {
                                        // Skeleton-to-Depth mapping
                                        DepthImagePoint depthPoint = _sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skeletonPoint, DepthImageFormat.Resolution320x240Fps30);

                                        leftHand.X = depthPoint.X;
                                        leftHand.Y = depthPoint.Y;
                                    }

                                    // DRAWING...
                                    Rectangle rect = new Rectangle
                                    {
                                        //Fill = Brushes.Transparent,
                                        Stroke = Brushes.Yellow,
                                        Width = 80,
                                        Height = 80
                                    };

                                    rect.Fill = new ImageBrush
                                    {
                                        ImageSource = new BitmapImage(new Uri(@"C:\Users\ROCIO\Desktop\PROTOTIPOS\AR_bothHands\KinectCoordinateMapping\atomocarbono.jpg"))
                                    };

                                    Canvas.SetLeft(rect, leftHand.X - rect.Width / 2);
                                    Canvas.SetTop(rect, leftHand.Y - 100);

                                    //canvas.Children.Add(ellipse);
                                    canvas.Children.Add(rect);
                                }
                                else if (joint.JointType.ToString() == "HandRight")
                                {
                                    // 3D coordinates in meters
                                    SkeletonPoint skeletonPoint = joint.Position;
                                    //Console.WriteLine(joint.JointType);
                                    // 2D coordinates in pixels
                                    //Point point = new Point();
                                    //canvas.Children.Clear();
                                    if (_mode == CameraMode.Color)
                                    {
                                        // Skeleton-to-Color mapping
                                        ColorImagePoint colorPoint = _sensor.CoordinateMapper.MapSkeletonPointToColorPoint(skeletonPoint, ColorImageFormat.RgbResolution640x480Fps30);

                                        rightHand.X = colorPoint.X;
                                        rightHand.Y = colorPoint.Y;
                                    }
                                    else if (_mode == CameraMode.Depth) // Remember to change the Image and Canvas size to 320x240.
                                    {
                                        // Skeleton-to-Depth mapping
                                        DepthImagePoint depthPoint = _sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skeletonPoint, DepthImageFormat.Resolution320x240Fps30);

                                        rightHand.X = depthPoint.X;
                                        rightHand.Y = depthPoint.Y;
                                    }

                                    // DRAWING...
                                    Rectangle rect = new Rectangle
                                    {
                                        //Fill = Brushes.Transparent,
                                        Stroke = Brushes.Yellow,
                                        Width = 80,
                                        Height = 80
                                    };

                                    rect.Fill = new ImageBrush
                                    {
                                        ImageSource = new BitmapImage(new Uri(@"C:\Users\ROCIO\Desktop\PROTOTIPOS\AR_bothHands\KinectCoordinateMapping\atomocarbono1.jpg"))
                                    };

                                    Canvas.SetLeft(rect, rightHand.X - rect.Width / 2);
                                    Canvas.SetTop(rect, rightHand.Y - 100);

                                    //canvas.Children.Add(ellipse);
                                    canvas.Children.Add(rect);
                                    //System.Console.WriteLine(leftHand.X - rightHand.X);
                                    //System.Console.WriteLine(rightHand.X);
                                    //System.Console.WriteLine(leftHand.X);
                                }
                                if ((leftHand.X - rightHand.X) < 80 && (leftHand.X - rightHand.X) > -80)
                                {
                                    //System.Console.WriteLine(rightHand.X);
                                    //System.Console.WriteLine(leftHand.X);
                                    canvas.Children.Clear();
                                    Rectangle rect = new Rectangle
                                    {
                                        Stroke = Brushes.Yellow,
                                        Width = 200,
                                        Height = 200
                                    };

                                    rect.Fill = new ImageBrush
                                    {
                                        ImageSource = new BitmapImage(new Uri(@"C:\Users\ROCIO\Desktop\PROTOTIPOS\AR_bothHands\KinectCoordinateMapping\atomofinal.jpg"))
                                    };

                                    Canvas.SetLeft(rect, rightHand.X - rect.Width / 2);
                                    Canvas.SetTop(rect, rightHand.Y - 100);
                                    
                                    canvas.Children.Add(rect);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_sensor != null)
            {
                _sensor.Stop();
            }
        }
    }

    enum CameraMode
    {
        Color,
        Depth
    }
}
