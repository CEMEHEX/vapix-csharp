using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Axis
{
    public class Camera
    {
        private readonly string serverName;
        private readonly string login;
        private readonly string password;
        private readonly int timeout;

        private WebResponse CGIRequest(string subDirs, string cgi, string ext, string arguments = "")
        {
            string baseString = "http://{0}/axis-cgi/{1}/{2}.{3}";
            baseString += (arguments.Length == 0) ? "{4}" : "?{4}";
            string request = String.Format(baseString, serverName, subDirs, cgi, ext, arguments);
            WebRequest cgiRequest = WebRequest.Create(request);

            cgiRequest.Proxy = null;
            cgiRequest.Timeout = timeout;
            cgiRequest.Credentials = new NetworkCredential(login, password);

            WebResponse response = cgiRequest.GetResponse();
            return response;
        }

        private void PTZRequest(string arguments)
        {
            using (WebResponse response = CGIRequest("com", "ptz", "cgi", arguments)) { }
        }
//http://10.4.13.204/axis-cgi/com/ptz.cgi?query=position
        private Dictionary<string, string> PositionRequest()
        {
            using (WebResponse response = CGIRequest("com", "ptz", "cgi", "query=position"))
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string responseText = reader.ReadToEnd().Replace('.', ',');

                string pattern = @"(\w+)=(-?[\w,]+)";
                Regex regex = new Regex(pattern);
                Match match = regex.Match(responseText);

                Dictionary<string, string> parameters = new Dictionary<string, string>();
                while (match.Success)
                {
                    string param = match.Groups[1].Value;
                    string value = match.Groups[2].Value;
                    parameters[param] = value;
                    match = match.NextMatch();
                }
                return parameters;
            }
        }

        /*
         * Creates instance of Camera which provides access to main vapix functions.
         */
        public Camera(string serverName, string login, string password, int timeout = 3000)
        {
            this.serverName = serverName;
            this.login = login;
            this.password = password;
            this.timeout = timeout;
        }

        /*
         * Center the camera on positions x, y
         * where x, y are pixel coordinates in
         * the client video stream.
         */
        public void Center(int x, int y)
        {
            string arguments = String.Format("center={0},{1}", x, y);
            PTZRequest(arguments);
        }

        /*
         * Pans the device to the specified
         * absolute coordinates.
         */
        public void Pan(double degrees)
        {
            string arguments = String.Format("pan={0}", degrees);
            PTZRequest(arguments);
        }

        /*
         * Tilts the device to the specified
         * absolute coordinates.
         */
        public void Tilt(double degrees)
        {
            string arguments = String.Format("tilt={0}", degrees);
            PTZRequest(arguments);
        }

        /*
         * Zooms the device n steps to the specified
         * absolute position. A high value means zoom in,
         * a low value means zoom out.
         */
        public void Zoom(int n)
        {
            string arguments = String.Format("zoom={0}", n);
            PTZRequest(arguments);
        }

        /*
         * Pans the device n degrees relative to the current position.
         */
        public void RelativePan(double n)
        {
            string arguments = String.Format("rpan={0}", n);
            PTZRequest(arguments);
        }

        /*
         * Tilts the device n degrees relative to the current position.
         */
        public void RelativeTilt(double n)
        {
            string arguments = String.Format("rtilt={0}", n);
            PTZRequest(arguments);
        }

        /*
         * Zooms the device n steps relative to the
         * current position. Positive values mean
         * zoom in, negative values mean zoom out.
         */
        public void RelativeZoom(int n)
        {
            string arguments = String.Format("rzoom={0}", n);
            PTZRequest(arguments);
        }

        /*
         * Request default image and save it in specified file.
         */
        public void SaveJPEG(string path)
        {
            using (var response = CGIRequest("jpg", "image", "cgi"))
            {
                Image jpeg = Image.FromStream(response.GetResponseStream());
                jpeg.Save(path);
            }
        }

        /*
         * Returns current pan position.
         */
        public double GetPan()
        {
            Dictionary<string, string> parameters = PositionRequest();
            double pan = double.Parse(parameters["pan"]);
            return pan;
        }

        /*
         * Returns current tilt position.
         */
        public double GetTilt()
        {
            Dictionary<string, string> parameters = PositionRequest();
            double tilt = double.Parse(parameters["tilt"]);
            return tilt;
        }

        /*
         * Returns current zoom position.
         */
        public int GetZoom()
        {
            Dictionary<string, string> parameters = PositionRequest();
            int zoom = int.Parse(parameters["zoom"]);
            return zoom;
        }
    }
}
