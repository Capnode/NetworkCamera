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

using Capnode.TFLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NetworkCamera.TFLite
{
    public partial class Network : IDisposable
    {
        private const string labelFileName = "Model/labels.txt";
        private const string modelFileName = "Model/detect.tflite";
        private const string imageFileName = "Model/dog416.png";

        private bool _isDisposed = false;
        private Interpreter _interpreter;
        private string[] _labels;
        private FlatBufferModel _model;
        private Tensor _inputTensor;
        private Tensor[] _outputTensors;

        public Network()
        {
        }

         ~Network()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        /// <summary>
        /// Load TF lite model
        /// </summary>
        /// <returns></returns>
        public bool LoadModel()
        {
            string unc = Assembly.GetExecutingAssembly().Location;
            string exePath = Path.GetDirectoryName(unc);
            string labelPath = Path.Combine(exePath, labelFileName);
            string modelPath = Path.Combine(exePath, modelFileName);

            if (!File.Exists(modelPath)) return false;
            if (!File.Exists(labelPath)) return false;

            _labels = File.ReadAllLines(labelPath);
            _model = new FlatBufferModel(modelPath);
            _interpreter = new Interpreter(_model);
            Status allocateTensorStatus = _interpreter.AllocateTensors();
            if (allocateTensorStatus == Status.Error) throw new Exception("Failed to allocate tensor");
            _inputTensor = _interpreter.Inputs[0];
            _outputTensors = _interpreter.Outputs;

            string imagePath = Path.Combine(exePath, imageFileName);
            RecognitionResult[] result = Recognize(imagePath);
            return true;
        }

        /// <summary>
        /// Perform Coco Ssd Mobilenet detection
        /// </summary>
        /// <param name="imageFile">The image file where we will ran the network through</param>
        /// <param name="scoreThreshold">If non-positive, will return all results. If positive, we will only return results with score larger than this value</param>
        /// <returns>The result of the detection.</returns>
        public RecognitionResult[] Recognize(string imageFile, float scoreThreshold = 0.0f)
        {
            int height = _inputTensor.Dims[1];
            int width = _inputTensor.Dims[2];

            NativeImageIO.ReadImageFileToTensor<byte>(imageFile, _inputTensor.DataPointer, height, width, 0.0f, 1.0f);
            _interpreter.Invoke();
            return ConvertResults(scoreThreshold);
        }

        private RecognitionResult[] ConvertResults(float scoreThreshold)
        {
            float[,,] outputLocations = _interpreter.Outputs[0].JaggedData as float[,,];
            float[] classes = _interpreter.Outputs[1].Data as float[];
            float[] scores = _interpreter.Outputs[2].Data as float[];
            int numDetections = (int)Math.Round((_interpreter.Outputs[3].Data as float[])[0]);

            // SSD Mobilenet V1 Model assumes class 0 is background class
            // in label file and class labels start from 1 to number_of_classes+1,
            // while outputClasses correspond to class index from 0 to number_of_classes
            List<RecognitionResult> results = new List<RecognitionResult>();

            int labelOffset = 1;
            for (int i = 0; i < numDetections; i++)
            {
                if (classes[i] == 0) //background class
                    continue;

                if (scoreThreshold > 0.0f && scores[i] < scoreThreshold)
                    continue;

                RecognitionResult r = new RecognitionResult();
                r.Class = (int)Math.Round(classes[i]);
                r.Label = _labels[r.Class + labelOffset];
                r.Score = scores[i];
                float x0 = outputLocations[0, i, 1];
                float y0 = outputLocations[0, i, 0];
                float x1 = outputLocations[0, i, 3];
                float y1 = outputLocations[0, i, 2];
                r.Rectangle = new float[] { x0, y0, x1, y1 };

                results.Add(r);
            }

            return results.ToArray();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _interpreter.Dispose();
                    _model.Dispose();
                }

                _isDisposed = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
