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
using NetworkCamera.Service;
using System;

namespace NetworkCamera.Tests.Service
{
    [TestClass()]
    public class InferenceServerTests
    {
        private const string _host ="172.25.75.141:9001";
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
        public void Startup_with_empty_host()
        {
            _dut.Startup(string.Empty);
        }

        [TestMethod()]
        public void Startup()
        {
            _dut.Startup(_host);
        }
    }
}