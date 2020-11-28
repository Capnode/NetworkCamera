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

using System;
using System.ComponentModel;

namespace NetworkCamera.Setting
{
    public class SettingsModel
    {
        private const string _inferenceLimit = "0.5";

        [Category("Program")]
        [DisplayName("ApplicationData")]
        [Description("ApplicationData folder.")]
        [Browsable(true)]
        [ReadOnly(true)]
        public string AppData { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        [Category("Program")]
        [DisplayName("ProgramFiles")]
        [Description("ProgramFiles folder.")]
        [Browsable(true)]
        [ReadOnly(true)]
        public string ProgramFiles { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        [Category("Inference")]
        [DisplayName("Inference server")]
        [Description("Tensorflow gRPC Inference server. Format <ip>:<port> ie: 192.168.1.100:8501")]
        [Browsable(true)]
        [ReadOnly(false)]
        public string InferenceServer { get; set; }

        [Category("Inference")]
        [DisplayName("Inference model")]
        [Description("Tensorflow gRPC Inference model name.")]
        [Browsable(true)]
        [ReadOnly(false)]
        public string InferenceModel { get; set; }

        [Category("Inference")]
        [DisplayName("Inference labels")]
        [Description("Tensorflow gRPC Inference labels file.")]
        [Browsable(true)]
        [ReadOnly(false)]
        public string InferenceLabels { get; set; }

        [Category("Inference")]
        [DisplayName("Inference limit")]
        [Description("Inference limit threshold, value in range 0.0 to 1.0.")]
        [Browsable(true)]
        [ReadOnly(false)]
        public string InferenceLimit { get; set; } = _inferenceLimit;

        public void Copy(SettingsModel oldSettings)
        {
            if (oldSettings == null) return;

            AppData = oldSettings.AppData;
            InferenceServer = oldSettings.InferenceServer;
            InferenceModel = oldSettings.InferenceModel;
            InferenceLabels = oldSettings.InferenceLabels;
            InferenceLimit = oldSettings.InferenceLimit;
        }

        public SettingsModel Clone()
        {
            return new SettingsModel
            {
                AppData = AppData,
                InferenceServer = InferenceServer,
                InferenceModel = InferenceModel,
                InferenceLabels = InferenceLabels,
                InferenceLimit = InferenceLimit
            };
        }
    }
}
