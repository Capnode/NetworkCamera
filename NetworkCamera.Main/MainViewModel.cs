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
using System.IO;
using System.Reflection;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using NetworkCamera.Device;
using NetworkCamera.Setting;

namespace NetworkCamera.Main
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private bool _isBusy;
        private string _statusMessage;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(
            SettingsViewModel settingsViewModel,
            DevicesViewModel devicesViewModel)
        {
            SettingsViewModel = settingsViewModel;
            DevicesViewModel = devicesViewModel;

            SaveCommand = new RelayCommand(() => SaveAll(), () => !IsBusy);
            Messenger.Default.Register<NotificationMessage>(this, OnStatusMessage);

            // Set working directory
            string userData = GetUserDataFolder();
            Directory.SetCurrentDirectory(userData);

            // Read configuration
            ReadConfigAsync(userData);
        }

        public RelayCommand SaveCommand { get; }
        public SettingsViewModel SettingsViewModel { get; }
        public DevicesViewModel DevicesViewModel { get; }

        public static string Title => AboutModel.AssemblyProduct;

        /// <summary>
        /// Mark ongoing operation
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set => Set(ref _isBusy, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => Set(ref _statusMessage, value);
        }

        public void SaveAll()
        {
            string appData = GetUserDataFolder();
            SaveConfig(appData);
        }

        public static string GetAppDataFolder()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            GetAssemblyInfo(out string company, out string product);
            string path = Path.Combine(appData, company, product);
            Directory.CreateDirectory(path);
            return path;
        }

        public static string GetUserDataFolder()
        {
            string userData = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            GetAssemblyInfo(out string company, out string product);
            string path = Path.Combine(userData, company, product);
            Directory.CreateDirectory(path);
            return path;
        }

        private static void GetAssemblyInfo(out string company, out string product)
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            company = string.Empty;
            product = string.Empty;
            object[] companyAttributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
            if ((companyAttributes != null) && (companyAttributes.Length > 0))
            {
                company = ((AssemblyCompanyAttribute)companyAttributes[0]).Company.Split(' ')[0];
            }

            object[] productAttributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            if ((productAttributes != null) && (productAttributes.Length > 0))
            {
                product = ((AssemblyProductAttribute)productAttributes[0]).Product;
            }
        }

        private void OnStatusMessage(NotificationMessage message)
        {
            StatusMessage = message.Notification;
            if (string.IsNullOrWhiteSpace(message.Notification))
                return;
        }

        private void ReadConfigAsync(string appData)
        {
            string contentFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            try
            {
                IsBusy = true;

                SettingsViewModel.Read(Path.Combine(appData, "Settings.json"), Path.Combine(contentFolder, "Settings.json"));
                DevicesViewModel.Read(Path.Combine(appData, "Devices.json"), Path.Combine(contentFolder, "Devices.json"));
            }
            finally
            {
                Messenger.Default.Send(new NotificationMessage(string.Empty));
                IsBusy = false;
            }
        }

        private void SaveConfig(string appData)
        {
            try
            {
                IsBusy = true;
                if (!Directory.Exists(appData))
                {
                    Directory.CreateDirectory(appData);
                }

                SettingsViewModel.Save(Path.Combine(appData, "Settings.json"));
                DevicesViewModel.Save(Path.Combine(appData, "Devices.json"));
            }
            finally
            {
                Messenger.Default.Send(new NotificationMessage(string.Empty));
                IsBusy = false;
            }
        }
    }
}