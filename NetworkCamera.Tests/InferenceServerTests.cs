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
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using NetworkCamera.Core;
using NetworkCamera.Service.Inference;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace NetworkCamera.Tests.Service
{
    [TestClass()]
    public class InferenceServerTests
    {
        private const string _host ="172.25.75.141:9001";
        private const string _imageFile = @"TestData/grace_hopper_300x300.bmp";
        private const string _largeImageFile = @"TestData/grace_hopper.bmp";
        private const string _modelName = InferenceServer.SsdMobilenetV2Model;
        private const string _labelFile = InferenceServer.SsdMobilenetV2Labels;

        private InferenceServer _dut;

        [TestInitialize()]
        public void Initialize()
        {
            _dut = new InferenceServer();
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ConnectWithEmptyHost()
        {
            await _dut.Connect(string.Empty, _modelName, _labelFile).ConfigureAwait(true);
        }

        [TestMethod()]
        public async Task PredictNoResize()
        {
            // Arrange
            using var bmp = new Bitmap(_imageFile);

            // Act
            await _dut.Connect(_host, _modelName, _labelFile).ConfigureAwait(true);
            IEnumerable<Detection> detections = await _dut.Predict(bmp).ConfigureAwait(true);
            await _dut.Disconnect().ConfigureAwait(true);
            Detection[] results = detections.ToArray();
            LogDetections(detections);

            // Assert
            Assert.AreEqual(20, results.Length);

            Detection result = results[0];
            Assert.AreEqual("person", result.Label);
            Assert.AreEqual(0.87890625, result.Score);
            Assert.AreEqual("{X=0,004936278,Y=0,013600767,Width=0,98389876,Height=0,98389876}", result.Box.ToString());

            result = results[1];
            Assert.AreEqual("tie", result.Label);
            Assert.AreEqual(0.5, result.Score);
            Logger.LogMessage("{0}", result.Box.ToString());
            Assert.AreEqual("{X=0,7059594,Y=0,43098423,Width=0,20436776,Height=0,13439634}", result.Box.ToString());
        }

        [TestMethod()]
        public async Task PredictResize()
        {
            // Arrange
            using var bmp = new Bitmap(_largeImageFile);

            // Act
            await _dut.Connect(_host, _modelName, _labelFile).ConfigureAwait(true);
            IEnumerable<Detection> detections = await _dut.Predict(bmp).ConfigureAwait(true);
            await _dut.Disconnect().ConfigureAwait(true);
            Detection[] results = detections.ToArray();
            LogDetections(detections);

            // Assert
            Assert.AreEqual(20, results.Length);

            Detection result = results[0];
            Assert.AreEqual("person", result.Label);
            Assert.AreEqual(0.83984375, result.Score, 0.0001);
            Assert.AreEqual("{X=0,027399272,Y=0,009679586,Width=0,95677316,Height=0,9744122}", result.Box.ToString());

            result = results[1];
            Assert.AreEqual("tie", result.Label);
            Assert.AreEqual(0.5, result.Score, 0.0001);
            Logger.LogMessage("{0}", result.Box.ToString());
            Assert.AreEqual("{X=0,7078091,Y=0,43220064,Width=0,20066833,Height=0,13196346}", result.Box.ToString());
        }

        private void LogDetections(IEnumerable<Detection> detections)
        {
            foreach (Detection item in detections)
            {
                Logger.LogMessage("{0} {1} {2}", item.Label, item.Score, item.Box);
            }
        }

    }
}