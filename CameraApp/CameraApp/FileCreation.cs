using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.IO;
namespace CameraApp
{
    class FileCreation
    {
        List<String> data;//All the data that will be stored from the sensors in here.
        //These will be a list for each individual sensor
        //format hh:mm:ss->value
        List<string> list_Light;
        List<string> list_GPS;
        List<string> list_Pressure;
        List<string> list_Proximity;
        //Number of frames each second
        int frames;
        public FileCreation(List<String> light, List<String> GPS, List<String> pressure, List<String> proximity) {
            this.list_Light = light;
            this.list_GPS = GPS;
            this.list_Pressure = pressure;
            this.list_Proximity = proximity;
            data = new List<string>();
            frames = 24;
        }
        public void generateFullData() {
            int numLight=0, numPressure=0,numProximity=0,numGPS=0, frameI=0,frameO=frames*5;
            int alredyThere = 0, secondsToFrames = 0, counter = 1;
            while (numLight < list_Light.Count || numPressure < list_Pressure.Count || numProximity < list_Proximity.Count || numGPS < list_GPS.Count) {               
                while (true) {
                    if (numLight < list_Light.Count)
                    {
                        secondsToFrames = secToFrames(list_Light[numLight].Split('@')[0]);
                        if (secondsToFrames >= frameI && secondsToFrames <= frameO)
                        {
                            if (alredyThere == 0)
                            {
                                data.Add("Light: Frame " + frameI + " to " + frameO + " Value: " + list_Light[numLight].Split('@')[1]);
                                alredyThere = 1;
                            }
                            numLight++;
                        }
                        else {
                            alredyThere = 0;
                            break;
                        }

                    }
                    else break;
                }
                while (true){
                    if (numPressure < list_Pressure.Count)
                    {
                        secondsToFrames = secToFrames(list_Pressure[numPressure].Split('@')[0]);
                        if (secondsToFrames >= frameI && secondsToFrames <= frameO)
                        {
                            if (alredyThere == 0)
                            {
                                data.Add("Pressure: Frame " + frameI + " to " + frameO + " Value: " + list_Pressure[numPressure].Split('@')[1]);
                                alredyThere = 1;
                            }
                            numPressure++;
                        }
                        else
                        {
                            alredyThere = 0;
                            break;
                        }

                    }
                    else break;
                }
                while (true){
                    if (numProximity < list_Proximity.Count)
                    {
                        secondsToFrames = secToFrames(list_Proximity[numProximity].Split('@')[0]);
                        if (secondsToFrames >= frameI && secondsToFrames <= frameO)
                        {
                            if (alredyThere == 0)
                            {
                                data.Add("Proximity: Frame " + frameI + " to " + frameO + " Value: " + list_Proximity[numProximity].Split('@')[1]);
                                alredyThere = 1;
                            }
                            numProximity++;
                        }
                        else
                        {
                            alredyThere=0;
                            break;
                        }

                    }
                    else break;
                }
                while (true) {
                    if (numGPS < list_GPS.Count)
                    {
                        secondsToFrames = secToFrames(list_GPS[numGPS].Split('@')[0]);
                        if (secondsToFrames >= frameI && secondsToFrames <= frameO)
                        {
                            if (alredyThere == 0)
                            {
                                data.Add("GPS: Frame " + frameI + " to " + frameO + " Value: " + list_GPS[numGPS].Split('@')[1]);
                                alredyThere = 1;
                            }
                            numGPS++;
                        }
                        else
                        {
                            alredyThere = 0;
                            break;
                        }

                    }
                    else break;
                }
                counter++;
                frameI = frameO;
                frameO = frames * 5 * counter;
            }
        }
        public int secToFrames(string time) {
            string[] hms = time.Split(':');
            int hours = Convert.ToInt32(hms[0].TrimEnd('h'));
            int minutes = Convert.ToInt32(hms[1].TrimEnd('m'));
            int seconds = Convert.ToInt32(hms[2].TrimEnd('s'));
            return (hours * 3600 + minutes * 60 + seconds) * frames;
        }
        public void saveAsFile() {
            string path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            string date = DateTime.Now.ToString("yyMMddHHmmss");
            string filename = Path.Combine(path, "video_"+date+ ".txt");

            using (var streamWriter = new StreamWriter(filename, true))
            {
                foreach (String a in data)
                    streamWriter.WriteLine(a + "\n");
            }
            using (var streamReader = new StreamReader(filename))
            {
                string content = streamReader.ReadToEnd();
                System.Diagnostics.Debug.WriteLine(content);
            }
        }
    }
}