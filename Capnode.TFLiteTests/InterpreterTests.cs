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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace Capnode.TFLite.Tests
{
    [TestClass()]
    public class InterpreterTests
    {
        private const string labelFileName = "labels.txt";
        private const string modelFileName = "detect.tflite";
        private const string imageFileName = "dog416.png";

        private Interpreter _interpreter;
        private string[] _labels;
        private FlatBufferModel _model;
        private Tensor _inputTensor;
        private Tensor[] _outputTensors;
        
        [TestMethod()]
        public void InterpreterTest()
        {
            string unc = Assembly.GetExecutingAssembly().Location;
            string exePath = Path.GetDirectoryName(unc);
            string labelPath = Path.Combine(exePath, labelFileName);
            string modelPath = Path.Combine(exePath, modelFileName);
            Assert.IsTrue(File.Exists(modelPath));
            Assert.IsTrue(File.Exists(labelPath));

            _labels = File.ReadAllLines(labelPath);
            _model = new FlatBufferModel(modelPath);
            _interpreter = new Interpreter(_model);
            Status allocateTensorStatus = _interpreter.AllocateTensors();
            Assert.AreEqual(Status.Ok, allocateTensorStatus);

            _inputTensor = _interpreter.Inputs[0];
            _outputTensors = _interpreter.Outputs;

            string imagePath = Path.Combine(exePath, imageFileName);

            int height = _inputTensor.Dims[1];
            int width = _inputTensor.Dims[2];

            NativeImageIO.ReadImageFileToTensor<byte>(imagePath, _inputTensor.DataPointer, height, width, 0.0f, 1.0f);
            _interpreter.Invoke();

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

                RecognitionResult r = new RecognitionResult();
                r.Class = (int)Math.Round(classes[i]);
                r.Label = _labels[r.Class + labelOffset];
                r.Score = scores[i];
                float x0 = outputLocations[0, i, 1];
                float y0 = outputLocations[0, i, 0];
                float x1 = outputLocations[0, i, 3];
                float y1 = outputLocations[0, i, 2];
                r.Rectangle = new float[] { x0, y0, x1, y1 };
                Logger.LogMessage($"{r.Class} {r.Label} {r.Score} ({r.Rectangle[0]:0.0000} {r.Rectangle[1]:0.0000} {r.Rectangle[2]:0.0000} {r.Rectangle[3]:0.0000})");
                results.Add(r);
            }

            Assert.AreEqual(results[0].Class, 1);
            Assert.AreEqual(results[0].Label, "bicycle");
            Assert.AreEqual(results[0].Score, 0.7578, 0.0001);
            Assert.AreEqual(results[0].Rectangle[0], 0.1573, 0.0001);
            Assert.AreEqual(results[0].Rectangle[1], 0.2084, 0.0001);
            Assert.AreEqual(results[0].Rectangle[2], 0.7916, 0.0001);
            Assert.AreEqual(results[0].Rectangle[3], 0.7674, 0.0001);

            Assert.AreEqual(results[1].Class, 2);
            Assert.AreEqual(results[1].Label, "car");
            Assert.AreEqual(results[1].Score, 0.7188, 0.0001);
            Assert.AreEqual(results[1].Rectangle[0], 0.6037, 0.0001);
            Assert.AreEqual(results[1].Rectangle[1], 0.1343, 0.0001);
            Assert.AreEqual(results[1].Rectangle[2], 0.8968, 0.0001);
            Assert.AreEqual(results[1].Rectangle[3], 0.3006, 0.0001);

            Assert.AreEqual(results[2].Class, 17);
            Assert.AreEqual(results[2].Score, 0.5977, 0.0001);
            Assert.AreEqual(results[2].Label, "dog");
            Assert.AreEqual(results[2].Rectangle[0], 0.1736, 0.0001);
            Assert.AreEqual(results[2].Rectangle[1], 0.3698, 0.0001);
            Assert.AreEqual(results[2].Rectangle[2], 0.4267, 0.0001);
            Assert.AreEqual(results[2].Rectangle[3], 0.9337, 0.0001);
        }
    }
}