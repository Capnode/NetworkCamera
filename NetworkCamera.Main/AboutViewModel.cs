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

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace NetworkCamera.Main
{
    public class AboutViewModel : ObservableRecipient
    {
        public string Title { get; private set; }
        public string ProductName { get; private set; }
        public string Version { get; private set; }
        public string Copyright { get; private set; }
        public string Description { get; private set; }

        public AboutViewModel()
        {
            Title = String.Format(CultureInfo.InvariantCulture, "About {0}", AboutModel.AssemblyProduct);
            ProductName = AboutModel.AssemblyProduct;
            Version = String.Format(CultureInfo.InvariantCulture, "Version: {0}", AboutModel.AssemblyVersion);
            Copyright = AboutModel.AssemblyCopyright;
            Description = AboutModel.AssemblyDescription;
        }

        public static string GetProgramFolder()
        {
            string unc = Assembly.GetExecutingAssembly().Location;
            string folder = Path.GetDirectoryName(unc);
            return folder;
        }

        public static string GetAppDataFolder()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string company = AboutModel.AssemblyCompany.Split(' ')[0];
            string product = AboutModel.AssemblyTitle.Split('.')[0];
            string path = Path.Combine(appData, company, product);
            Directory.CreateDirectory(path);
            return path;
        }

        public static string GetUserDataFolder()
        {
            string userData = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string company = AboutModel.AssemblyCompany.Split(' ')[0];
            string product = AboutModel.AssemblyTitle.Split('.')[0];
            string path = Path.Combine(userData, company, product);
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
