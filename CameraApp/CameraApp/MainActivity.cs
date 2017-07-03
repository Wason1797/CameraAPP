using Android.App;
using Android.Widget;
using Android.OS;
using Android.Media;
using Android.Hardware;
using Android.Locations;
using System.Collections.Generic;
using System.Threading;
using Java.IO;
using Android.Runtime;
using System;
using System.IO;
using Android.Content.PM;

namespace CameraApp
{
    [Activity(Label = "CameraApp", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
    ScreenOrientation = ScreenOrientation.SensorLandscape)]
    public class MainActivity : Activity, ILocationListener, ISensorEventListener
    {
        private MediaRecorder recorder;
        //Codigo de Sensores
        SensorManager sensorManagerLight;
        Sensor sensor_Light;
        SensorManager sensorManagerPressure;
        Sensor sensor_Pressure;
        SensorManager sensorManagerProximity;
        Sensor sensor_Proximity;
        LocationManager locationManager;
        Chronometer chronometer;
        Thread crono;
        List<string> list_Light;
        List<string> list_GPS;
        List<string> list_Pressure;
        List<string> list_Proximity;
        Thread thread_Light;
        Thread thread_Pressure;
        Thread thread_Proximity;
        Thread thread_GPS;
        //Times
        int time_Light;
        int time_Pressure;
        int time_Proximity;
        string text;

        bool captureGPS;
        int value_Interval_Time;


        public void StartThreadChrono()
        {
            chronometer.Start();
        }

        public void StartThreadGPS()
        {
            while (true)
            {
                Thread.Sleep(value_Interval_Time);
                captureGPS = true;
                Thread.Sleep(100);
                captureGPS = false;
            }
        }
        public void StartThreadLight()
        {
            while (true)
            {
                Thread.Sleep(time_Light);
                sensorManagerLight.RegisterListener(this, sensor_Light, SensorDelay.Fastest);
                Thread.Sleep(10000 - time_Light);
            }
        }
        public void StartThreadProximity()
        {
            while (true)
            {
                Thread.Sleep(time_Proximity);
                sensorManagerProximity.RegisterListener(this, sensor_Proximity, SensorDelay.Fastest);
                Thread.Sleep(10000 - time_Proximity);
            }
        }
        public void StartThreadPressure()
        {
            while (true)
            {
                Thread.Sleep(time_Pressure);
                sensorManagerPressure.RegisterListener(this, sensor_Pressure, SensorDelay.Fastest);
                Thread.Sleep(10000 - time_Pressure);
            }
        }
        public void InitializeComponents()
        {
            //Lists.... Para guardar en las listas
            list_Light = new List<string>();
            list_GPS = new List<string>();
            list_Pressure = new List<string>();
            list_Proximity = new List<string>();
            //Sensor of Pressure - Barometer
            thread_Pressure = new Thread(new ThreadStart(StartThreadPressure));
            sensorManagerPressure = (SensorManager)GetSystemService(SensorService);
            sensor_Pressure = sensorManagerPressure.GetDefaultSensor(SensorType.Pressure);
            sensorManagerPressure.UnregisterListener(this);
            //Sensor of Light
            thread_Light = new Thread(new ThreadStart(StartThreadLight));
            sensorManagerLight = (SensorManager)GetSystemService(SensorService);
            sensor_Light = sensorManagerLight.GetDefaultSensor(SensorType.Light);
            sensorManagerLight.UnregisterListener(this);
            //Sensor of Proximity
            thread_Proximity = new Thread(new ThreadStart(StartThreadProximity));
            sensorManagerProximity = (SensorManager)GetSystemService(SensorService);
            sensor_Proximity = sensorManagerProximity.GetDefaultSensor(SensorType.Pressure);
            sensorManagerProximity.UnregisterListener(this);
            //Sensor de GPS
            locationManager = (LocationManager)GetSystemService(LocationService);
            locationManager.RemoveUpdates(this);
            captureGPS = false;
            value_Interval_Time = 250;
            //Cronometro
            chronometer = new Chronometer();
            crono = new Thread(new ThreadStart(StartThreadChrono));

            thread_GPS = new Thread(new ThreadStart(StartThreadGPS));

        }
        public void StartCapturing()
        {
            int seconds = 10;
            int lapsus = seconds / 4;
            time_Light = lapsus * 1000;
            time_Pressure = lapsus * 1000 * 2;
            time_Proximity = lapsus * 1000 * 3;
            crono.Start();
            thread_GPS.Start();
            thread_Light.Start();
            thread_Pressure.Start();
            thread_Proximity.Start();
            locationManager.RequestLocationUpdates(LocationManager.GpsProvider, 0, 0, this);
        }
        public void FinishCapturing()
        {
            chronometer.Finish();
            SaveDataInternal();
            crono.Abort();            
            thread_Light.Abort();
            thread_Pressure.Abort();
            thread_Proximity.Abort();
            locationManager.RemoveUpdates(this);
        }
        public void SaveDataInternal()
        {
            try
            {
                FileCreation a = new FileCreation(list_Light, list_GPS, list_Pressure, list_Proximity);
                a.generateFullData();
                a.saveAsFile();
                Toast t = Toast.MakeText(this, "Saved Data", ToastLength.Long);
                t.Show();
            }
            catch (System.IO.IOException e)
            {
                Toast t = Toast.MakeText(this, "Failed To Save Data", ToastLength.Long);
                t.Show();
            }
        }
        public void ShowData()
        {
            //Aqui se muestra la información por el momento se imprime solo en consola
            text = "DATA LIGHT";
            foreach (string aux in list_Light) text += "\n" + aux;
            text += "\nDATA GPS";
            foreach (string aux in list_GPS) text += "\n" + aux;
            text += "\nDATA PRESSURE";
            foreach (string aux in list_Pressure) text += "\n" + aux;
            text += "\nDATA PROXIMITY";
            foreach (string aux in list_Proximity) text += "\n" + aux;
            System.Diagnostics.Debug.WriteLine(text);
            //screen.Text = text;
        }
        //.............................Main Principal......................................
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            // Set our view from the "main" layout resource
            InitializeComponents();
            SetContentView (Resource.Layout.Main);
            string date;
            string path="";
            var record = FindViewById<Button>(Resource.Id.recordBtn);
            var stop = FindViewById<Button>(Resource.Id.stopBtn);
            var play = FindViewById<Button>(Resource.Id.playBtn);
            var video = FindViewById<VideoView>(Resource.Id.videoView);      
            bool isPlaying=false;
            CamcorderProfile cpHigh = CamcorderProfile.Get(CamcorderQuality.High);
            record.Click += delegate
            {
                video.StopPlayback();
                isPlaying = true;
                recorder = new MediaRecorder();
                recorder.SetVideoSource(VideoSource.Camera);
                recorder.SetAudioSource(AudioSource.Mic);
                //recorder.SetProfile(new CamcorderProfile());
                //recorder.SetOutputFormat(OutputFormat.Mpeg4);
                //recorder.SetVideoEncoder(VideoEncoder.H264);
                //ecorder.SetAudioEncoder(AudioEncoder.Default);
                date = DateTime.Now.ToString("yyMMddHHmmss");
                path = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath + "video_" + date + ".mp4";
                recorder.SetProfile(cpHigh);
                recorder.SetPreviewDisplay(video.Holder.Surface);
                recorder.SetOutputFile(path);
                recorder.Prepare();
                StartCapturing();
                captureGPS = false;
                recorder.Start();
                record.Enabled = false;
            };
            stop.Click += delegate
            {
                
                if (video.IsPlaying)
                {
                    video.StopPlayback();
                    video.ClearAnimation();
                }
                if (recorder != null&&isPlaying)
                {
                    recorder.Stop();
                    recorder.Release();
                    FinishCapturing();
                    ShowData();
                }
                isPlaying = false;
                
                InitializeComponents();
                record.Enabled = true;
            };
            play.Click += delegate 
            {
                
                Android.Net.Uri uri = Android.Net.Uri.Parse(path);
                video.SetVideoURI(uri);
                video.Start();
            };
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (recorder != null)
            {
                recorder.Release();
                //recorder.Dispose();
                recorder = null;
            }
        }
        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy) { }
        public void OnSensorChanged(SensorEvent e)
        {
            if (e.Sensor.Type.Equals(SensorType.Light))
            {
                list_Light.Add(chronometer.Time() + "@" + e.Values[0]);
                //            screen.Text += "\nCaptured Light Data";
                sensorManagerLight.UnregisterListener(this);
            }
            if (e.Sensor.Type.Equals(SensorType.Pressure))
            {
                list_Pressure.Add(chronometer.Time() + "@" + e.Values[0]);
                //          screen.Text += "\nCaptured Pressure Data";
                sensorManagerPressure.UnregisterListener(this);
            }
            if (e.Sensor.Type.Equals(SensorType.Proximity))
            {
                list_Proximity.Add(chronometer.Time() + "@" + e.Values[0]);
                //        screen.Text += "\nCaptured Proximity Data";
                sensorManagerProximity.UnregisterListener(this);
            }
        }
        public void OnLocationChanged(Location location)
        {
            if (captureGPS)
            {
                list_GPS.Add(chronometer.Time() + "@" + Decimal.Round((decimal)location.Latitude, 4) + "-->" + Decimal.Round((decimal)location.Longitude, 4));

            }
        }
        public void OnProviderDisabled(string provider) { }
        public void OnProviderEnabled(string provider) { }
        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras) { }
    }
}

