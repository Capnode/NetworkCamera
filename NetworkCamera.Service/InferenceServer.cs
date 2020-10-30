/*
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

using Google.Protobuf;
using Grpc.Core;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using Tensorflow;
using Tensorflow.Serving;
using static Tensorflow.Serving.PredictionService;

namespace NetworkCamera.Service
{
    public class InferenceServer
    {
        public unsafe void Startup(string inferenceServer, string certificate = default)
        {
            if (string.IsNullOrEmpty(inferenceServer)) throw new ArgumentNullException(nameof(inferenceServer));

            ChannelCredentials channelCredentials = 
                certificate == default ? ChannelCredentials.Insecure : new SslCredentials(certificate);
            var channel = new Channel(inferenceServer, channelCredentials);
            channel.ConnectAsync(DateTime.Now.AddSeconds(10).ToUniversalTime()).Wait();
            var client = new PredictionServiceClient(channel);

            // Set image
            const string imageFile = @"TestData/grace_hopper_300x300.bmp";
            const string model = @"testdata/ssd_mobilenet_v2_coco_quant_postprocess_edgetpu.tflite";

            var bmp = new Bitmap(imageFile);
            int channels = 3;
            int width = bmp.Width;
            int height = bmp.Height;
            int size = channels * width * height;
            BitmapData bmpData = bmp.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                bmp.PixelFormat);

            ReadOnlySpan<byte> byteSpan = new ReadOnlySpan<byte>(bmpData.Scan0.ToPointer(), size);
            ByteString image = ByteString.CopyFrom(byteSpan);

            //Unlock the pixels
            bmp.UnlockBits(bmpData);

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
                Dtype = DataType.DtUint8,
                TensorContent = image
            };

            var request = new PredictRequest
            {
                ModelSpec = new ModelSpec { Name = model }
            };
            request.Inputs.Add("data", proto);
            PredictResponse response = client.Predict(request);



            //var a = Tensorflow.TensorProto.

        }
    }
}
