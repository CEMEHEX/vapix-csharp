using System;
using System.Drawing.Imaging;
using System.Threading;
using static System.Console;
using Axis;
using Emgu.CV;
using Emgu.CV.Stitching;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace PanoramaBuilder
{
    class PanoramaBuilder
    {
        private const string Usage =
            "Usage: PanoramaBuilder <serverName> <login> <password> <rows> <columns> <step> <path>\n";

        static void Main(string[] args)
        {
            try
            {
                var camera = new Camera(args[0], args[1], args[2]);

                var rows = int.Parse(args[4]);
                var columns = int.Parse(args[5]);
                var step = int.Parse(args[6]);
                var path = args[7];

                var sourceImages = new Image<Bgr, byte>[rows * columns];

                WriteLine("Preparing images...");
                for (var i = 0; i < rows; ++i)
                {
                    if (i != 0)
                    {
                        camera.RelativePan(-step * columns);
                        camera.RelativeTilt(-step);

                        WriteLine("Pan to initial position");
                        WriteLine($"rTilt {-step}");
                    }
                    Thread.Sleep(2000);
                    for (var j = 0; j < columns; ++j)
                    {
                        WriteLine($"row: {i}, column: {j}");
                        try
                        {
                            var ind = i * columns + j;
                            sourceImages[ind] = new Image<Bgr, byte>(camera.GetBitmap());
                            sourceImages[ind]
                                .Bitmap.Save($"{path}\\picture{i}{j}.bmp", ImageFormat.Bmp);
                        }
                        catch (Exception e)
                        {
                            WriteLine(e.ToString());
                        }
                        camera.RelativePan(step);
                        WriteLine($"rPan {step}");
                        Thread.Sleep(2000);
                    }
                }
                WriteLine("Moving Camera to initial position...");
                camera.RelativePan(-step * columns);
                camera.RelativeTilt(step * rows);
                WriteLine("Stitching images...");
                try
                {
                    using (var stitcher = new Stitcher(true))
                    {
                        using (var vm = new VectorOfMat())
                        {
                            var result = new Mat();
                            vm.Push(sourceImages);
                            var stitchStatus = stitcher.Stitch(vm, result);
                            if (stitchStatus == Stitcher.Status.Ok)
                            {
                                try
                                {
                                    result.Bitmap.Save($"{path}\\result.bmp", ImageFormat.Bmp);
                                }
                                catch (Exception ex)
                                {
                                    WriteLine(ex.ToString());
                                }
                            }
                            else
                            {
                                WriteLine($"Stiching Error: {stitchStatus}");
                            }
                        }
                    }
                }
                finally
                {
                    foreach (var img in sourceImages)
                    {
                        img.Dispose();
                    }
                }
            }
            catch (Exception)
            {
                WriteLine(Usage);
            }
        }
    }
}