using System;

namespace Axis
{
    public interface IMJPEGStreamer
    {
        event Action<byte[]> NewFrameHandler;

        void StartStream(string login = "", string password = "");

        void StopStream();        
    }
}
