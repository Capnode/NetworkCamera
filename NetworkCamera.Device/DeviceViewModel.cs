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

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using NetworkCamera.Core;
using NetworkCamera.Device.Internal;
using NetworkCamera.Device.Properties;
using NetworkCamera.Setting;
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkCamera.Device
{
    public class DeviceViewModel : ViewModelBase, ITreeViewModel, IDisposable
    {
        private bool _isDisposed = false; // To detect redundant calls
        private readonly DevicesViewModel _parent;
        private CancellationTokenSource _cancel;
        private DeviceModel _model;
        private DeviceFactory _factory;
        private bool _checkAll;
        private IList _selectedItems;
        private Bitmap _bitmap;
        private Filter _filter;

        public DeviceViewModel(DevicesViewModel devicesViewModel, DeviceModel deviceModel, SettingsModel settings)
        {
            _parent = devicesViewModel ?? throw new ArgumentNullException(nameof(devicesViewModel));
            Model = deviceModel ?? throw new ArgumentNullException(nameof(deviceModel));
            if (settings == null) throw new ArgumentNullException(nameof(devicesViewModel));

            DeleteCommand = new RelayCommand(() => _parent?.DoDeleteDevice(this), () => !Active);
            ActiveCommand = new RelayCommand(() => DoActiveCommand(Model.Active));
            StartCommand = new RelayCommand(() => DoStartCommand(), () => !Active);
            StopCommand = new RelayCommand(() => DoStopCommand(), () => Active);

            DataFromModel();
            DoActiveCommand(Active);
        }

        ~DeviceViewModel()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        public RelayCommand DeleteCommand { get; }
        public RelayCommand ActiveCommand { get; }
        public RelayCommand StartCommand { get; }
        public RelayCommand StopCommand { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "<Pending>")]
        public IList SelectedItems
        {
            get { return _selectedItems; }
            set
            {
                _selectedItems = value;
                string message = string.Empty;
                if (_selectedItems?.Count > 0)
                {
                    message = string.Format(CultureInfo.InvariantCulture, Resources.SelectedCount, _selectedItems.Count);
                }

                Messenger.Default.Send(new NotificationMessage(message));
            }
        }

        public bool Active
        {
            get => Model.Active;
            set
            {
                Model.Active = value;
                RaisePropertyChanged(() => Active);

                StartCommand.RaiseCanExecuteChanged();
                StopCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }

        public DeviceModel Model
        {
            get => _model;
            set => Set(ref _model, value);
        }

        public bool CheckAll
        {
            get => _checkAll;
            set => Set(ref _checkAll, value);
        }

        public Bitmap Bitmap
        {
            get => _bitmap;
            set => Set(ref _bitmap, value);
        }

        public Rectangle Crop
        {
            get => Model.Crop;
            set
            {
                Model.Crop = value;
                base.RaisePropertyChanged(() => Crop);
            }
        }

        public void Refresh()
        {
            Model.Refresh();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        internal void DataToModel()
        {
        }

        internal void DataFromModel()
        {
            Active = Model.Active;
            Crop = Model.Crop;
        }

        internal async Task StartTaskAsync()
        {
            DataToModel();
            DeviceModel model = Model;
            _cancel = new CancellationTokenSource();
            _filter = new Filter(model);

            while (!_cancel.Token.IsCancellationRequested && model.Active)
            {
                Debug.WriteLine($"{model.Format} start {model.Source}");
                try
                {
                    _factory = new DeviceFactory();
                    _cancel = new CancellationTokenSource();
                    await Task.Run(() => model = _factory.Run(model, DeviceEvent, _cancel.Token), _cancel.Token)
                        .ConfigureAwait(true);
                    _factory = null;
                }
                catch (AppDomainUnloadedException)
                {
                    Debug.WriteLine($"Device {model.Name} canceled by user");
                    _factory = null;
                    model.Active = false;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{ex.GetType()}: {ex.Message}");
                    _factory = null;
                    model.Active = false;
                }

                // Update view
                Model = null;
                Model = model;
                DataFromModel();
            }

            _filter.Dispose();
            _filter = null;
            _cancel = null;
             Debug.WriteLine($"{Model.Format} stop {model.Source}");
        }

        private void StopTask()
        {
            if (_cancel != null)
            {
                _cancel.Cancel();
            }
        }

        private void DoActiveCommand(bool value)
        {
            if (value)
            {
                StartTaskAsync().ConfigureAwait(true);
            }
            else
            {
                StopTask();
            }
        }

        private void DoStartCommand()
        {
            try
            {
                _parent.IsBusy = true;
                Active = true;
                StartTaskAsync().ConfigureAwait(true);
            }
            finally
            {
                _parent.IsBusy = false;
            }
        }

        private void DoStopCommand()
        {
            try
            {
                _parent.IsBusy = true;
                StopTask();
                Active = false;
            }
            finally
            {
                _parent.IsBusy = false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _cancel?.Dispose();
                    _filter?.Dispose();
                }

                _isDisposed = true;
            }
        }

        private void DeviceEvent(IDevice sender, DeviceEventArgs e)
        {
            Bitmap bitmap = e.Bitmap;
            if (bitmap == null)
            {
                Bitmap = null;
                return;
            }

            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            Rectangle crop = Crop;
            crop.Intersect(rect);
            BitmapData bData = bitmap.LockBits(crop, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            OpenCvSharp.Mat frame = new OpenCvSharp.Mat(crop.Height, crop.Width, OpenCvSharp.MatType.CV_8UC3, bData.Scan0, bData.Stride);
            Debug.Assert(_filter != null);
            _filter.ProcessFrame(frame);
            frame.Dispose();
            bitmap.UnlockBits(bData);
            Bitmap = bitmap.Clone(rect, bitmap.PixelFormat);
        }
    }
}
