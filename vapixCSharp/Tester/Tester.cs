using static System.Console;
using System;
using Axis;
using Tester;
using System.Drawing;
using System.IO;

namespace Test
{
    class Tester
    {
        private const string Usage = "Usage: Tester <serverName> <login> <password> [<timeout>]\n";
        private const string Help =
            @"Valid commands:
               exit
               help
               ex
               center <x> <y>
               pan <degrees>
               tilt <degrees>
               zoom <n>
               rpan <degrees>
               rtilt <degrees>
               rzoom <n>
               savejpg <path>
               getpan
               gettilt
               getzoom
               startstream
               stopstream
               ";

        private static void StreamSaveAllFrames(byte[] imageData)
        {
            //Console.WriteLine("New frame arrived");
            using (MemoryStream memoryStream = new MemoryStream(imageData))
            {
                Image jpeg = Image.FromStream(memoryStream);
                jpeg.Save(String.Format("E:\\tmp\\mJpegTest\\{0}.jpg", DateTime.Now.ToFileTime()));
            }
        }

        static void Main(string[] args)
        {
            Camera camera;
            AxisMJPEGStreamer streamer;
            try
            {
                streamer = new AxisMJPEGStreamer(args[0]);
                streamer.NewFrameHandler += StreamSaveAllFrames;
                if (args.Length == 3)
                {
                    camera = new Camera(args[0], args[1], args[2]);
                }
                else
                {
                    camera = new Camera(args[0], args[1], args[2], int.Parse(args[3]));
                }
            }
            catch (Exception)
            {
                WriteLine(Usage);
                return;
            }
            string lastException = "No exceptions";
            while (true)
            {
                Write('>');
                string[] curArgs = ReadLine().Split(' ');
                try
                {
                    switch (curArgs[0])
                    {
                        case "exit":
                            return;
                        case "help":
                            WriteLine(Help);
                            break;
                        case "center":
                            camera.Center(int.Parse(curArgs[1]), int.Parse(curArgs[2]));
                            break;
                        case "pan":
                            camera.Pan(double.Parse(curArgs[1]));
                            break;
                        case "tilt":
                            camera.Tilt(double.Parse(curArgs[1]));
                            break;
                        case "zoom":
                            camera.Zoom(int.Parse(curArgs[1]));
                            break;
                        case "rpan":
                            camera.RelativePan(double.Parse(curArgs[1]));
                            break;
                        case "rtilt":
                            camera.RelativeTilt(double.Parse(curArgs[1]));
                            break;
                        case "rzoom":
                            camera.RelativeZoom(int.Parse(curArgs[1]));
                            break;
                        case "savejpg":
                            string path;
                            if (curArgs.Length == 1)
                            {
                                path = String.Format("E:\\tmp\\pictures\\{0}.jpg", DateTime.Now.ToFileTime());
                            }
                            else
                            {
                                path = curArgs[1];
                            }
                            camera.SaveJPEG(path);
                            break;
                        case "getpan":
                            WriteLine(camera.GetPan());
                            break;
                        case "gettilt":
                            WriteLine(camera.GetTilt());
                            break;
                        case "getzoom":
                            WriteLine(camera.GetZoom());
                            break;
                        case "ex":
                            WriteLine(lastException);
                            break;
                        case "startstream":
                            streamer.StartStream(args[1], args[2]);
                            break;
                        case "stopstream":
                            streamer.StopStream();
                            break;
                        default:
                            WriteLine("Invalid command. Type \"help\" to see all valid commands.");
                            break;
                    }
                }
                catch (Exception e)
                {
                    WriteLine("Invalid arguments or something went wrong");
                    lastException = e.ToString();
                }
            }
        }
    }
}
