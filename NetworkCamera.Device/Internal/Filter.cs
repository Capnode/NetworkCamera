﻿/*
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

using NetworkCamera.Core;
using NetworkCamera.Service.Inference;
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

        private bool _disposed = false;
        private readonly DeviceModel _device;
        private readonly InferenceServer _inferenceServer;
        private readonly BackgroundSubtractorMOG2 _segmentor;
        private int _index;
        private DateTime _postEventTime;

        public Filter(DeviceModel device, InferenceServer inferenceServer)
        {
            _device = device;
            _inferenceServer = inferenceServer;
            _segmentor = BackgroundSubtractorMOG2.Create(500, 16, true);
        }

        ~Filter()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        internal async Task ProcessFrame(Mat frame)
        {
            IEnumerable<Rect> motion = Enumerable.Empty<Rect>();
            if (_device.MotionDetection)
            {
                motion = DetectMotion(frame);
                if (!motion.Any()) return;
            }

            IEnumerable<Classification> classifications = Enumerable.Empty<Classification>();
            if (_device.ObjectDetection)
            {
                classifications = await PredictFrameAsync(frame).ConfigureAwait(true);
                if (!classifications.Any()) return;
            }

            if (!motion.Any() && !classifications.Any()) return;

            DrawMotion(frame, motion);
            DrawClassification(frame, classifications);

            if (!string.IsNullOrEmpty(_device.Folder))
            {
                SaveImage(frame);
            }

            if (!string.IsNullOrEmpty(_device.Notification)
                && DateTime.Now - _postEventTime > TimeSpan.FromMinutes(_minPostMinutes))
            {
                _postEventTime = DateTime.Now;
                await PostEvent().ConfigureAwait(true);
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

        private async Task<IEnumerable<Classification>> PredictFrameAsync(Mat frame)
        {
            Mat crop = frame.Clone();
            System.Drawing.Bitmap bitmap = crop.ToBitmap();
            var results = await _inferenceServer.Predict(bitmap).ConfigureAwait(true);
            var classifications = new List<Classification>();
            foreach (var result in results)
            {
                int x0 = (int)(frame.Width * result.Box.X);
                int y0 = (int)(frame.Height * result.Box.Y);
                int width = (int)(frame.Width * result.Box.Width);
                int height = (int)(frame.Height * result.Box.Height);
//                Log.Verbose($"{result.Label} {result.Score:0.####} W:{width} H:{height}");

                var classification = new Classification
                {
                    Label = result.Label,
                    Score = result.Score,
                    Rectangle = new Rect(x0, y0, width, height),
                };
               classifications.Add(classification);
            }

            return classifications;
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
