﻿/*
 * Copyright 2020 Capnode AB
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

using CommunityToolkit.Mvvm.ComponentModel;
using Google.Protobuf;
using Grpc.Core;
using NetworkCamera.Core;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Tensorflow;
using Tensorflow.Serving;
using static Tensorflow.Serving.PredictionService;

namespace NetworkCamera.Service.Inference
{
    public class InferenceServer : ObservableRecipient, IDisposable
    {
        private static readonly char[] _whitespace = new char[] { ' ', '\t' };

        private volatile PredictionServiceClient _client;
        private Channel _channel;
        private string _host;
        private string _model;
        private IDictionary<int, string> _labels;

        public InferenceServer()
        {
        }

        public float Limit { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IInferenceChannel CreateChannel()
        {
            if (string.IsNullOrEmpty(_host))
            {
                return null;
            }

            return new RemoteChannel(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources.
            }
            // Free native resources.
        }

        public async Task Start(string host, string model, string labels, float limit = 0, string certificate = null)
        {
            _host = host;
            _model = model;
            Limit = limit;
            if (!string.IsNullOrEmpty(labels))
            {
                _labels = ReadLabels(labels);
            }

            if (!string.IsNullOrEmpty(host))
            {
                try
                {
                    ChannelCredentials channelCredentials =
                        certificate == default ? ChannelCredentials.Insecure : new SslCredentials(certificate);
                    _channel = new Channel(host, channelCredentials);
                    DateTime deadline = DateTime.Now.AddSeconds(5).ToUniversalTime();
                    await _channel.ConnectAsync(deadline).ConfigureAwait(false);
                    _client = new PredictionServiceClient(_channel);
                }
                catch (Exception ex)
                {
                    string message = $"{ex.Message} ({ex.GetType()})";
                    Messenger.Send(new NotificationMessage(message), 0);
                    Log.Error(ex, message);
                    _client = null;
                }
            }
        }

        public async Task Disconnect()
        {
            if (_client != null)
            {
                await _channel.ShutdownAsync().ConfigureAwait(false);
                _client = null;
            }
        }

        public async Task<IEnumerable<Detection>> Predict(Bitmap bmp)
        {
            if (_client == null) throw new ApplicationException(nameof(_client));

            // Desired image format
            const int channels = 3;
            const int width = 300;
            const int height = 300;
            const PixelFormat format = PixelFormat.Format24bppRgb;

            var shape = new TensorShapeProto
            {
                Dim = { new []
                {
                    new TensorShapeProto.Types.Dim { Name = "", Size = 1 },
                    new TensorShapeProto.Types.Dim { Name = nameof(height), Size = height },
                    new TensorShapeProto.Types.Dim { Name = nameof(width), Size = width },
                    new TensorShapeProto.Types.Dim { Name = nameof(channels), Size = channels },
                } }
            };

            var proto = new TensorProto
            {
                TensorShape = shape,
                Dtype = Tensorflow.DataType.DtUint8,
                TensorContent = ToByteString(bmp, channels, width, height, format)
            };

            var request = new PredictRequest
            {
                ModelSpec = new ModelSpec { Name = _model }
            };
            request.Inputs.Add("data", proto);

            // Send requenst for inference
            PredictResponse response = await _client.PredictAsync(request);

            return ToDetections(response);
        }

        private static unsafe ByteString ToByteString(Bitmap source, int channels, int width, int height, PixelFormat format)
        {
            using Bitmap bitmap = ResizeBitmap(source, width, height, format);
            BitmapData bmpData = bitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                bitmap.PixelFormat);

            int size = channels * width * height;
            var byteSpan = new ReadOnlySpan<byte>(bmpData.Scan0.ToPointer(), size);
            ByteString image = ByteString.CopyFrom(byteSpan);
            bitmap.UnlockBits(bmpData);
            return image;
        }

        public static Bitmap ResizeBitmap(Bitmap source, int width, int height, PixelFormat format)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            // Do nothing if already in correct format
            if (source.Width == width
                && source.Height == height
                && source.PixelFormat == format)
            {
                return source;
            }

            // Create the new bitmap.
            // Note that Bitmap has a resize constructor, but you can't control the quality.
            var bmp = new Bitmap(width, height, format);

            using (var g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(source, new Rectangle(0, 0, width, height));
                g.Save();
            }

            if (bmp.Width != width
                || bmp.Height != height
                || bmp.PixelFormat != format)
            {
                // Not possible to convert to desired format
                throw new ArgumentOutOfRangeException(nameof(source));
            }

            return bmp;
        }

        private static IDictionary<int, string> ReadLabels(string labelFile)
        {
            if (string.IsNullOrEmpty(labelFile)) return default;

            string[] lines = File.ReadAllLines(labelFile);
            var labels = new Dictionary<int, string>();
            int linecount = 0;
            foreach (string line in lines)
            {
                string[] columns = line.Split(_whitespace, StringSplitOptions.RemoveEmptyEntries);
                if (columns.Length == 0)
                {
                    // Do nothing
                }
                else if (columns.Length > 1 && int.TryParse(columns[0], out int id))
                {
                    string label = string.Join(" ", columns.Skip(1));
                    labels[id] = label;
                }
                else
                {
                    labels[linecount] = line;
                }
                linecount++;
            }

            return labels;
        }

        private unsafe IEnumerable<Detection> ToDetections(PredictResponse response)
        {
            ReadOnlySpan<byte> output0 = response.Outputs["output0"].TensorContent.Span;
            ReadOnlySpan<float> boxes = MemoryMarshal.Cast<byte, float>(output0);
            ReadOnlySpan<byte> output1 = response.Outputs["output1"].TensorContent.Span;
            ReadOnlySpan<float> classes = MemoryMarshal.Cast<byte, float>(output1);
            ReadOnlySpan<byte> output2 = response.Outputs["output2"].TensorContent.Span;
            ReadOnlySpan<float> scores = MemoryMarshal.Cast<byte, float>(output2);
            ReadOnlySpan<byte> output3 = response.Outputs["output3"].TensorContent.Span;
            ReadOnlySpan<float> count = MemoryMarshal.Cast<byte, float>(output3);

            var detections = new List<Detection>();
            float size = count[0];
            for (int i = 0; i < size; i++)
            {
                float score = scores[i];
                if (score < Limit) continue;

                var detection = new Detection
                {
                    Label = ToLabel(classes[i]),
                    Score = score,
                    Box = Detection.ToRectangle(
                        top : boxes[4 * i],
                        left : boxes[4 * i + 1],
                        bottom : boxes[4 * i + 2],
                        right : boxes[4 * i + 3])
                };
                detections.Add(detection);
            }

            return detections;
        }

        private string ToLabel(float id)
        {
            if (!_labels.TryGetValue((int)id, out string label))
            {
                label = id.ToString(CultureInfo.InvariantCulture);
            }

            return label;
        }
    }
}
