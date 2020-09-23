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

        [Category("External")]
        [DisplayName("Inference server")]
        [Description("Tensorflow lite gRPC Inference server (Experimental). Format <ip>:<port> ie: 192.168.1.100:8501")]
        [Browsable(true)]
        [ReadOnly(false)]
        public string InferenceServer { get; set; }

        public void Copy(SettingsModel oldSettings)
        {
            if (oldSettings == null) return;

            AppData = oldSettings.AppData;
            InferenceServer = oldSettings.InferenceServer;
        }

        public SettingsModel Clone()
        {
            return new SettingsModel
            {
                AppData = AppData,
                InferenceServer = InferenceServer
            };
        }
    }
}
