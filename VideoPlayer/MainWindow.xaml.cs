using SimpleWebServer;
using System;
using System.IO;
using System.Net;
using System.Windows;
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

        string activeVideoFile;
        string status = "loaded";
        bool isLoopableActive = false;

        public MainWindow()
        {
            InitializeComponent();

            // automaticly move to second screen on the left
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


        public string getVideoPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory + "\\videos\\";

        }

        public string SendResponse(HttpListenerRequest request)
        {

            string command = "";

            // check for valid url
            if (request.Url.Segments.Length >= 2)
            {
                command = request.Url.Segments[1];
                command = command.Replace("/", "");
            }
            else
            {
                return "no valid url";
            }

            // get status
            if (command == "status")
            {
               return activeVideoFile + ":" + status + ";loop:" + isLoopableActive ;
            }

            if (command == "stop")
            {
                stopVideo();
                return "stopped";
            }


            if (command == "start")
            {
                string fileName = request.Url.Segments[2];
                // checking for options
                int flipX = 1;
                int flipY = 1;

                if (request.QueryString["flipX"] != null)
                {
                    flipX = -1;
                }
                if (request.QueryString["flipY"] != null)
                {
                    flipY = -1;
                }

                // check if video needs to be in a loop
                isLoopableActive = (request.QueryString["loop"] != null) && request.QueryString["loop"] == "1";


                bool videoStatus =  startVideo(fileName,flipX,flipY);
                return fileName + (videoStatus ? ":found" : ":notfound");
            }

            return "UNKNOWN";


             
        }

        void stopVideo()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {

                videoPlayer.Visibility = System.Windows.Visibility.Hidden;
                videoPlayer.Stop();
                status = "stopped";

            }
            ));

        }


        bool startVideo(string fileName, int flipX, int flipY)
        {
            string fullPath = getVideoPath() + fileName;

            if (File.Exists(fullPath))
            {

                activeVideoFile = fileName;

                // Run this on the mainthread. Because request is comming from a separte thread
                Dispatcher.BeginInvoke(new Action(() =>
                {

                    ScaleTransform flipTrans = new ScaleTransform(flipX, flipY);
                    videoPlayer.RenderTransform = flipTrans;

                    videoPlayer.Visibility = System.Windows.Visibility.Visible;
                    videoPlayer.Source = new Uri(fullPath);
                    status = "started";

                }

               ));


                return true;

            } else{
                return false;
            }
        }



        void loadFilesFromVideoFolder()
        {
            string videoFolderPath = getVideoPath();

            if (!Directory.Exists(videoFolderPath)){
                Directory.CreateDirectory(videoFolderPath);
            }

            videoFiles = Directory.GetFiles(videoFolderPath , "*.mp4");
            if (videoFiles.Length == 0)
            {
                txtInfo.Text = "no videos found\n" + videoFolderPath;
                return;
            }
        }

  

        private void videoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (isLoopableActive)
            {
                videoPlayer.Position = TimeSpan.FromSeconds(0);
                videoPlayer.Play();
            }else
            {
                videoPlayer.Visibility = System.Windows.Visibility.Hidden;
                status = "stopped";
            }
        }


        private void videoPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            videoPlayer.Play();
        }
    }
}
