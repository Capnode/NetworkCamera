/*
 * Copyright 2018 Capnode AB
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

using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;

namespace NetworkCamera.Setting
{
    public class SettingsViewModel
    {
        public SettingsViewModel(SettingsModel settings)
        {
            Model = settings;
        }

        public SettingsModel Model { get; }

        public bool Read(string fileName)
        {
            if (!File.Exists(fileName)) return false;
            using (StreamReader r = new StreamReader(fileName))
            {
                string json = r.ReadToEnd();
                SettingsModel settings = JsonConvert.DeserializeObject<SettingsModel>(json);
                Model.Copy(settings);
            }

            DataFromModel();
            return true;
        }

        public bool Save(string fileName)
        {
            DataToModel();

            using StreamWriter file = File.CreateText(fileName);
            JsonSerializer serializer = new JsonSerializer { Formatting = Formatting.Indented };
            serializer.Serialize(file, Model);
            return true;
        }

        private static void DataToModel()
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        private void DataFromModel()
        {
        }
    }
}
