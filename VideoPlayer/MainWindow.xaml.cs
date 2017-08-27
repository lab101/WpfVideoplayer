using System;
using System.Windows;
using System.IO;
using System.Net;
using SimpleWebServer;

using System.Windows.Forms;
using System.Windows.Media;


namespace VideoPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string[] videoFiles;
        int videoIndex = 0;

        string activeVideoFile;
        string status;
      

        public MainWindow()
        {
            InitializeComponent();

            // automaticly move to second screen
            if (Screen.AllScreens.Length > 1)
            {
                Screen s2 = Screen.AllScreens[0];

                this.Left = s2.WorkingArea.Width;
                this.Top = 0.0;
                this.Width = Screen.AllScreens[1].WorkingArea.Width;
                this.Height = Screen.AllScreens[1].WorkingArea.Height;
            }
 
            this.WindowStyle = System.Windows.WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;


            loadFilesFromVideoFolder();

            WebServer ws = new WebServer(SendResponse, "http://localhost:8080/");
            ws.Run();

            this.Loaded +=MainWindow_Loaded;


        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Maximized;
        }


        public  string SendResponse(HttpListenerRequest request)
        {

            if (request.Url.Segments.Length == 2)
            {
                if (request.Url.Segments[1] == "status")
                {
                    return activeVideoFile + ":" + status;
                }
            }

            if (request.Url.Segments.Length <= 2) return "error";



           string fileName = request.Url.Segments[2];
           string fullPath = AppDomain.CurrentDomain.BaseDirectory + "\\videos\\" + fileName;

           if (File.Exists(fullPath))
           {

               activeVideoFile = fileName;

               // Run this on the mainthread. Because request is comming from a separte thread
               Dispatcher.BeginInvoke(new Action(() =>
               {

                   // checking for options
                   int flip = 1;

                   if (request.QueryString["flip"] != null)
                   {
                       flip = -1;
                   }


                   ScaleTransform flipTrans = new ScaleTransform(1, flip);
                   videoPlayer.RenderTransform = flipTrans;

                   videoPlayer.Visibility = System.Windows.Visibility.Visible;
                   videoPlayer.Source = new Uri(fullPath);

                   status = "started";
               }

              ));

               return activeVideoFile + ":found";
           }
           else
           {
               return activeVideoFile + ":notfound";
           }
             
        }


        void loadFilesFromVideoFolder()
        {
            string videoFolderPath = AppDomain.CurrentDomain.BaseDirectory + "\\videos\\";

            if(!Directory.Exists(videoFolderPath)){
                Directory.CreateDirectory(videoFolderPath);
            }

            videoFiles = Directory.GetFiles(videoFolderPath , "*.mp4");
            if (videoFiles.Length == 0)
            {
                txtInfo.Text = "no videos found";
                return;
            }
        }

  

        private void videoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            //if (videoFiles.Length > 1)
            //{               
 
            //    // disabled because video stops after a while.
            //    // best fix is to recreate the video object .. ?
            //   nextVideo();

            //}
            
            //videoPlayer.Position = TimeSpan.FromSeconds(0);
            //videoPlayer.Play();
            videoPlayer.Visibility = System.Windows.Visibility.Hidden;
            status = "stopped";
        }


        private void videoPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            videoPlayer.Play();
        }
    }
}
