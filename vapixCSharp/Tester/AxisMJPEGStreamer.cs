using System;
using Axis;
using AForge.Video;
using System.IO;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace Tester
{
    class AxisMJPEGStreamer : IMJPEGStreamer
    {
        private MJPEGStream stream;
        private Task streamingTask;

        private void NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                eventArgs.Frame.Save(memoryStream, ImageFormat.Jpeg);
                byte[] imageBytes = memoryStream.ToArray();
                NewFrameHandler(imageBytes);
            }
        }

        public event Action<byte[]> NewFrameHandler;

        public AxisMJPEGStreamer(string serverName)
        {   
            string videoRequest = String.Format("http://{0}/axis-cgi/mjpg/video.cgi", serverName);
            stream = new MJPEGStream(videoRequest);
            stream.ForceBasicAuthentication = true;
            stream.NewFrame += NewFrame;
        }

        public void StartStream(string login, string password)
        {
            stream.Login = login;
            stream.Password = password;

            if (streamingTask == null)
            {
                streamingTask = Task.Factory.StartNew(() =>
               {
                   stream.Start();
               });
            } else
            {                
                throw new InvalidOperationException("Streaming already started");
            }
        }

        public void StopStream()
        {
            stream.Stop();
            streamingTask = null;            
        }
    }
}
