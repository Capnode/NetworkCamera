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

using NetworkCamera.Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using System.IO;
using NetworkCamera.Setting;
using System.Collections.ObjectModel;
using System;
using NetworkCamera.Service.Inference;

namespace NetworkCamera.Device
{
    public class DevicesViewModel : ViewModelBase
    {
        private readonly SettingsModel _settings;
        private readonly InferenceServer _inferenceServer;
        private ITreeViewModel _selectedItem;
        private bool _isBusy;

        public DevicesViewModel(DevicesModel devices, SettingsModel settings, InferenceServer inferenceServer)
        {
            Model = devices;
            _settings = settings;
            _inferenceServer = inferenceServer;

            AddCommand = new RelayCommand(() => DoAddDevice(), () => !IsBusy);
            SelectedChangedCommand = new RelayCommand<ITreeViewModel>((vm) => DoSelectedChanged(vm), (vm) => vm != null);

            DataFromModel();
        }

        public RelayCommand<ITreeViewModel> SelectedChangedCommand { get; }
        public RelayCommand AddCommand { get; }

        public DevicesModel Model { get; }
        public ObservableCollection<DeviceViewModel> Devices { get; } = new ObservableCollection<DeviceViewModel>();

        /// <summary>
        /// Mark ongoing operation
        /// </summary>
        public bool IsBusy
        {
            get =>_isBusy;
            set => Set(ref _isBusy, value);
        }

        public ITreeViewModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                Set(ref _selectedItem, value);
            }
        }

        internal bool DoDeleteDevice(DeviceViewModel device)
        {
            if (device == null) throw new ArgumentNullException(nameof(device));
            SelectedItem = null;
            return Devices.Remove(device);
        }

        public bool Read(string fileName)
        {
            if (!File.Exists(fileName)) return false;
            using (StreamReader r = new StreamReader(fileName))
            {
                string json = r.ReadToEnd();
                Model.Copy(JsonConvert.DeserializeObject<DevicesModel>(json));
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

        private void DoSelectedChanged(ITreeViewModel vm)
        {
            vm.Refresh();
            SelectedItem = vm;
        }

        private void DoAddDevice()
        {
            var loginViewModel = new DeviceViewModel(this, new DeviceModel(), _settings, _inferenceServer);
            Devices.Add(loginViewModel);
        }

        private void DataToModel()
        {
            Model.Devices.Clear();
            foreach (DeviceViewModel device in Devices)
            {
                Model.Devices.Add(device.Model);
                device.DataToModel();
            }
        }

        private void DataFromModel()
        {
            Devices.Clear();
            foreach (DeviceModel device in Model.Devices)
            {
                var viewModel = new DeviceViewModel(this, device, _settings, _inferenceServer);
                Devices.Add(viewModel);
            }
        }
    }
}
