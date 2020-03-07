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

using Capnode.TFLite;
using NetworkCamera.TFLite;
using Newtonsoft.Json;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace NetworkCamera.Device.Internal
{
    internal class Filter : IDisposable
    {
        private const double _minPostMinutes = 10;
        private const float _minConfidence = 0.6F;

        private bool _disposed = false;
        private readonly DeviceModel _device;
        private readonly BackgroundSubtractorMOG2 _segmentor;
        private readonly Detector _detector;
        private int _index;
        private DateTime _postEventTime;

        public Filter(DeviceModel device)
        {
            _device = device;
            _segmentor = BackgroundSubtractorMOG2.Create(500, 16, true);
            _detector = new Detector();
            _detector.LoadModel();
        }

        ~Filter()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        internal void ProcessFrame(Mat frame)
        {
            IEnumerable<Rect> motion = Enumerable.Empty<Rect>();
            if (_device.MotionDetection)
            {
                motion = DetectMotion(frame);
                if (!motion.Any()) return;
            }

            IEnumerable<Classification> classification = Enumerable.Empty<Classification>();
            if (_device.ItemClassification)
            {
                classification = ClassifyFrame(frame, new Rect(0, 0, frame.Width, frame.Height));
                if (!classification.Any()) return;
            }

            if (!motion.Any() && !classification.Any()) return;

            DrawMotion(frame, motion);
            DrawClassification(frame, classification);

            if (!string.IsNullOrEmpty(_device.Folder))
            {
                SaveImage(frame);
            }

            if (!string.IsNullOrEmpty(_device.Notification)
                && DateTime.Now - _postEventTime > TimeSpan.FromMinutes(_minPostMinutes))
            {
                _postEventTime = DateTime.Now;
                Task.Run(async () => await PostEvent().ConfigureAwait(false));
            }
        }

        private IEnumerable<Rect> DetectMotion(Mat frame)
        {
            Mat fgmask = new Mat();
            _segmentor.Apply(frame, fgmask);
            if (fgmask.Empty()) yield break;

            Cv2.Threshold(fgmask, fgmask, 25, 255, ThresholdTypes.Binary);
            int noiseSize = 9;
            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(noiseSize, noiseSize));
            Cv2.Erode(fgmask, fgmask, kernel);
            kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(noiseSize, noiseSize));
            Cv2.Dilate(fgmask, fgmask, kernel, new Point(-1, -1), 3);
            Cv2.FindContours(fgmask, out Point[][] contours, out HierarchyIndex[] hierarchies, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
            foreach (Point[] contour in contours)
            {
                yield return Cv2.BoundingRect(contour);
            }
        }

        private IEnumerable<Classification> ClassifyFrame(Mat frame, Rect rect)
        {
            Mat crop = frame.Clone(rect);
            System.Drawing.Bitmap bitmap = crop.ToBitmap();
            RecognitionResult[] results = _detector.Recognize(bitmap, _minConfidence);
            foreach (RecognitionResult result in results)
            {
                int x0 = (int)(frame.Width * result.Rectangle[0]);
                int y0 = (int)(frame.Height * result.Rectangle[1]);
                int x1 = (int)(frame.Width * result.Rectangle[2]);
                int y1 = (int)(frame.Height * result.Rectangle[3]);
                Log.Verbose($"{result.Label} {result.Score:0.####} W:{x1 - x0} H:{y1 - y0}");

                yield return new Classification
                {
                    Label = result.Label,
                    Score = result.Score,
                    Rectangle = new Rect(rect.Left + x0, rect.Top + y0, x1 - x0, y1 - y0),
                };
            }
        }

        private static void DrawMotion(Mat frame, IEnumerable<Rect> motions)
        {
            Scalar color = new Scalar(0, 255, 255);
            foreach (Rect rect in motions)
            {
                Cv2.Rectangle(frame, rect, color, 2);
            }
        }

        private static void DrawClassification(Mat frame, IEnumerable<Classification> classifications)
        {
            Scalar color = new Scalar(0, 0, 255);
            foreach (Classification classification in classifications)
            {
                var rect = classification.Rectangle;
                Cv2.Rectangle(frame, rect, color, 2);
                DrawTextInBoxAbove(frame, rect, classification.Label);
            }
        }

        private static void DrawTextInBoxAbove(Mat frame, Rect rect, string label)
        {
            Scalar fontColor = Scalar.All(0);
            Scalar borderColor = new Scalar(0, 0, 255);
            HersheyFonts fontFace = HersheyFonts.HersheySimplex;
            double fontScale = 1;
            int thickness = 2;
            Size size = Cv2.GetTextSize(label, fontFace, fontScale, thickness, out int baseline);
            baseline += thickness;
            Cv2.Rectangle(
                frame,
                new Rect(rect.Left, rect.Top - size.Height - baseline , size.Width + thickness, size.Height + baseline),
                borderColor,
                thickness);
            Cv2.PutText(frame, label, new Point(rect.Left, rect.Top - baseline), fontFace, fontScale, fontColor, thickness, LineTypes.AntiAlias);
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
                var filename = string.Format(CultureInfo.InvariantCulture, "image{0}.bmp", _index++);
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
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _segmentor.Dispose();
                    _detector.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
