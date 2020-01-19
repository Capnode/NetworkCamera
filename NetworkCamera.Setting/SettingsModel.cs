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

using System;
using System.ComponentModel;

namespace NetworkCamera.Setting
{
    public class SettingsModel
    {
        [DisplayName("MyDocuments")]
        [Description("MyDocuments folder.")]
        [Browsable(true)]
        [ReadOnly(true)]
        public string MyDocuments { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        [DisplayName("ApplicationData")]
        [Description("ApplicationData folder.")]
        [Browsable(true)]
        [ReadOnly(true)]
        public string AppData { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);


        [DisplayName("CommonApplicationData")]
        [Description("CommonApplicationData folder.")]
        [Browsable(true)]
        [ReadOnly(true)]
        public string CommonApplicationData { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

        [DisplayName("ProgramFiles")]
        [Description("ProgramFiles folder.")]
        [Browsable(true)]
        [ReadOnly(true)]
        public string ProgramFiles { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        [DisplayName("BaseDirectory")]
        [Description("BaseDirectory folder.")]
        [Browsable(true)]
        [ReadOnly(true)]
        public string BaseDirectory { get; set; } = AppDomain.CurrentDomain.BaseDirectory;

        public void Copy(SettingsModel oldSettings)
        {
            if (oldSettings == null)
            {
                return;
            }

            AppData = oldSettings.AppData;
        }

        public SettingsModel Clone()
        {
            return new SettingsModel
            {
                AppData = AppData
            };
        }
    }
}
