using System.Diagnostics;
using System;
namespace CameraApp
{
    public class Chronometer
    {
        Stopwatch clock = new Stopwatch();
        public Chronometer()
        {
            clock = new Stopwatch();
        }
        public void Start()
        {
            clock = new Stopwatch();
            clock.Restart();
        }
        public void Finish()
        {
            clock.Stop();
        }
        public string Time()
        {
            TimeSpan ts = TimeSpan.FromMilliseconds(clock.ElapsedMilliseconds);
            string t = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D2}ms", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
            return t;
        }

    }    
}

