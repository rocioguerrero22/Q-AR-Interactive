using System;
using System.IO;
using System.Linq;
using Microsoft.Kinect;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Collections.Generic;
using Microsoft.Samples.Kinect.SwipeGestureRecognizer;

namespace gesto_mano_derecha
{
  
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        Skeleton[] totalSkeleton = new Skeleton[6];
        private KinectSensor sensor;
        private byte[] pixelData;
        private DispatcherTimer intervalo = new DispatcherTimer();
        private readonly Recognizer activeRecognizer;

        public MainWindow()
        {
            InitializeComponent();
            // Create the gesture recognizer.
            this.activeRecognizer = this.CreateRecognizer();
        }

        //detectar sensor kinect
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (KinectSensor.KinectSensors.Count > 0)
            {
                this.sensor = KinectSensor.KinectSensors.FirstOrDefault(sensorItem => sensorItem.Status == KinectStatus.Connected);
                this.sensor.Start();
                this.sensor.ColorStream.Enable();
                this.sensor.ColorFrameReady += this.sensor_ColorFrameReady;

                if (!this.sensor.SkeletonStream.IsEnabled)
                {
                    this.sensor.SkeletonStream.Enable();
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    this.sensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(sensor_SkeletonFrameReady);
                }

            }
            else
            {
                MessageBox.Show("No esta conectado el Kinect a su ordenador");
                this.Close();
            }
        }

        private void BtnBgn_Click(object sender, RoutedEventArgs e)
        {
            if (this.sensor != null && this.sensor.IsRunning)
                this.sensor.Start();
        }

        

        //Reconocedor de gesto
        private Recognizer CreateRecognizer() {
            // Instantiate a recognizer.
            var recognizer = new Recognizer();

            // Wire-up swipe right to manually advance picture.
            recognizer.SwipeRightDetected += (s, e) =>
            {
                    txtestatus.Content = "Next";
                    
            };

            // Wire-up swipe left to manually reverse picture.
            recognizer.SwipeLeftDetected += (s, e) =>
            {
                    txtestatus.Content = "Previous";
                   
            };


            return recognizer;

        }


        //parte del esqueleto
        private void sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                // Verifica que se encontró un esqueleto.
                if (skeletonFrame == null)
                {
                    return;
                }

                //copia la información del frame en la colección
                skeletonFrame.CopySkeletonDataTo(totalSkeleton);

                //Obtiene el primer esqueleto
                Skeleton firstSkeleton = (from trackskeleton in totalSkeleton
                                          where trackskeleton.TrackingState == SkeletonTrackingState.Tracked
                                          select trackskeleton).FirstOrDefault();


                //Aqui verificamos si el primer esqueleto regresa nulo, es decir no encontrado
                if (firstSkeleton == null)
                {
                    return;
                }

                // Pass skeletons to recognizer.
                this.activeRecognizer.Recognize(sender, skeletonFrame, this.totalSkeleton);

            }
        }

        //frame color
        private void sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame imageFrame = e.OpenColorImageFrame())
            {
                if (imageFrame == null)
                    return;
                else
                {
                    this.pixelData = new byte[imageFrame.PixelDataLength];
                    imageFrame.CopyPixelDataTo(this.pixelData);
                    int stride = imageFrame.Width * imageFrame.BytesPerPixel;
                    this.VideoControl.Source = BitmapSource.Create(imageFrame.Width, imageFrame.Height, 96, 96, PixelFormats.Bgr32,
                    null, pixelData, stride);

                }
            }
        }


        public void starTimer()
        {
            this.intervalo.Interval = new TimeSpan(0, 0, 10);
            this.intervalo.Start();
           
        }

        public void StopTimer()
        {
            this.intervalo.Stop();
         
        }


    }
}
