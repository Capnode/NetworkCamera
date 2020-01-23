/*
 * Copyright 2019 Capnode AB
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Newtonsoft.Json;
using OpenCvSharp;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace NetworkCamera.Device.Internal
{
    internal class Filter
    {
        const int _triggerFrame = 10;
        const double _minPostMinutes = 10;

        private readonly DeviceModel _device;
        private readonly BackgroundSubtractorMOG2 _segmentor;
        private int _counter;
        private int _motion;
        private DateTime _postEventTime;

        public Filter(DeviceModel device)
        {
            _device = device;
            _segmentor = BackgroundSubtractorMOG2.Create(500, 16, true);
        }

        internal void ProcessFrame(Mat frame)
        {
            if (!DetectMotion(frame))
            {
                _motion = 0;
                return;
            }

            _motion++;
            if (!string.IsNullOrEmpty(_device.Folder))
            {
                SaveImage(frame);
            }

            if (!string.IsNullOrEmpty(_device.Notification)
                && _motion == _triggerFrame
                && DateTime.Now - _postEventTime > TimeSpan.FromMinutes(_minPostMinutes))
            {
                _postEventTime = DateTime.Now;
                Task.Run(async () => await PostEvent().ConfigureAwait(false));
            }
        }

        private bool DetectMotion(Mat frame)
        {
            //OpenCvSharp.Mat src = new OpenCvSharp.Mat(path, OpenCvSharp.ImreadModes.Grayscale);
            //// Mat src = Cv2.ImRead("lenna.png", ImreadModes.Grayscale);
            //OpenCvSharp.Mat dst = new OpenCvSharp.Mat();

            //OpenCvSharp.Cv2.Canny(src, dst, 50, 200);
            //            Cv2.PutText(frame, DateTime.Now.ToLongTimeString(), new Point(0, 100), HersheyFonts.HersheyComplexSmall, 1, Scalar.All(255));

            Mat fgmask = new Mat();
            _segmentor.Apply(frame, fgmask);
            if (fgmask.Empty()) return false;

            Cv2.Threshold(fgmask, fgmask, 25, 255, ThresholdTypes.Binary);
            int noiseSize = 9;
            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(noiseSize, noiseSize));
            Cv2.Erode(fgmask, fgmask, kernel);
            kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(noiseSize, noiseSize));
            Cv2.Dilate(fgmask, fgmask, kernel, new Point(-1, -1), 3);
            Cv2.FindContours(fgmask, out Point[][] contours, out HierarchyIndex[] hierarchies, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
            Scalar color = new Scalar(0, 0, 255);
            foreach (Point[] contour in contours)
            {
                var rect = Cv2.BoundingRect(contour);
                Cv2.Rectangle(frame, rect, color, 1);
            }

            return contours.Length > 0;
        }

        private void SaveImage(Mat frame)
        {
            if (!Directory.Exists(_device.Folder))
            {
                Directory.CreateDirectory(_device.Folder);
            }

            string path;
            do
            {
                var filename = string.Format(CultureInfo.InvariantCulture, "image{0}.bmp", _counter++);
                path = Path.Combine(_device.Folder, filename);
            }
            while (File.Exists(path));
            frame.SaveImage(path);
        }

        async Task PostEvent()
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri(_device.Notification)
            };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var param = new
            {
                value1 = _device.Name,
                value2 = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}",
                value3 = "value3"
            };
            string json = JsonConvert.SerializeObject(param);
            var uri = new Uri(_device.Notification);
            using StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage result = await client.PostAsync(uri, content).ConfigureAwait(true);
            Debug.WriteLine(json);
        }
    }
}
