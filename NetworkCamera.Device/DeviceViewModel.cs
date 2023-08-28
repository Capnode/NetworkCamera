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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkCamera.Core;
using NetworkCamera.Device.Core;
using NetworkCamera.Service.Inference;
using NetworkCamera.Setting;
using Serilog;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NetworkCamera.Device
{
    public class DeviceViewModel : ObservableRecipient, ITreeViewModel, IDisposable
    {
        private bool _isDisposed; // To detect redundant calls
        private readonly DevicesViewModel _parent;
        private CancellationTokenSource _cancel;
        private DeviceModel _model;
        private readonly InferenceServer _inferenceServer;
        private DeviceFactory _factory;
        private bool _checkAll;
        private Filter _filter;
        private DateTime _timestamp;
        private bool _isSelected;
        private Bitmap _bitmap;
        private Bitmap _croppedBitmap;

        public DeviceViewModel(
            DevicesViewModel devicesViewModel,
            DeviceModel deviceModel,
            SettingsModel settings,
            InferenceServer inferenceServer)
        {
            _parent = devicesViewModel ?? throw new ArgumentNullException(nameof(devicesViewModel));
            Model = deviceModel ?? throw new ArgumentNullException(nameof(deviceModel));
            if (settings == null) throw new ArgumentNullException(nameof(devicesViewModel));
            _inferenceServer = inferenceServer ?? throw new ArgumentNullException(nameof(inferenceServer));

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

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool Active
        {
            get => Model.Active;
            set
            {
                Model.Active = value;
                OnPropertyChanged();

                // Run in UI thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    StartCommand.NotifyCanExecuteChanged();
                    StopCommand.NotifyCanExecuteChanged();
                    DeleteCommand.NotifyCanExecuteChanged();

                    // Notify MainView about changed device
                    Messenger.Send(new DeviceMessage(), 0);
                });
            }
        }

        public DeviceModel Model
        {
            get => _model;
            set => SetProperty(ref _model, value);
        }

        public bool CheckAll
        {
            get => _checkAll;
            set => SetProperty(ref _checkAll, value);
        }

        public Bitmap Bitmap
        {
            get => _bitmap;
            set => SetProperty(ref _bitmap, value);
        }

        public Bitmap CroppedBitmap
        {
            get => _croppedBitmap;
            set => SetProperty(ref _croppedBitmap, value);
        }

        public Rectangle Crop
        {
            get => Model.Crop;
            set
            {
                Model.Crop = value;
                OnPropertyChanged();
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
            try
            {
                await RunModel().ConfigureAwait(false);
                Messenger.Send(new NotificationMessage(string.Empty), 0);
            }
            catch (AggregateException aex)
            {
                string message = string.Empty;
                foreach (var ex in aex.InnerExceptions)
                {
                    message = $"Device {Model.Name}: {ex.Message} ({ex.GetType()})";
                    Log.Error(ex, message);
                }

                if (aex.InnerExceptions.Count > 1)
                {
                    message = $"Device {Model.Name}: {aex.GetType()} See log for details";
                }

                Messenger.Send(new NotificationMessage(message), 0);
                Model.Active = false;
                DataFromModel();
            }
            catch (Exception ex)
            {
                string message = $"Device {Model.Name}: {ex.Message} ({ex.GetType()})";
                Log.Error(ex, message);
                Messenger.Send(new NotificationMessage(message), 0);
                Model.Active = false;
                DataFromModel();
            }
        }

        private async Task RunModel()
        {
            DataToModel();
            DeviceModel model = Model;
            _cancel = new CancellationTokenSource();
            _filter = new Filter(model, _inferenceServer);

            while (!_cancel.Token.IsCancellationRequested && model.Active)
            {
                Log.Verbose($"{model.Format} start {model.Source}");
                _factory = new DeviceFactory();
                _cancel = new CancellationTokenSource();
                await Task.Run(() => model = _factory
                    .Run(model, DeviceEvent, _cancel.Token), _cancel.Token)
                    .ConfigureAwait(false);
                _factory = null;

                // Update view
                Model = null;
                Model = model;
                DataFromModel();
            }

            _filter.Dispose();
            _filter = null;
            _cancel = null;
            Log.Verbose($"{Model.Format} stop {model.Source}");
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
                StartTaskAsync().ConfigureAwait(false);
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
                StartTaskAsync().ConfigureAwait(false);
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
            DateTime entryTime = DateTime.Now;
            TimeSpan elapsed = _timestamp == default ? TimeSpan.Zero : entryTime - _timestamp;
            _timestamp = entryTime;

            Bitmap bitmap = e.Bitmap;
            if (bitmap == null)
            {
                Bitmap = null;
                return;
            }

            Rectangle crop = Crop;
            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            crop.Intersect(rect);
            if (crop.Width == 0 || crop.Height == 0)
            {
                crop = rect;
            }

            BitmapData cropData = bitmap.LockBits(crop, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            OpenCvSharp.Mat frame = new OpenCvSharp.Mat(crop.Height, crop.Width, OpenCvSharp.MatType.CV_8UC3, cropData.Scan0, cropData.Stride);
            Debug.Assert(_filter != null);
            _filter.ProcessFrame(frame).Wait();
            if (Model.OnscreenInfo)
            {
                TimeSpan processingTime = DateTime.Now - entryTime;
                string message = $"Frame {(int)elapsed.TotalMilliseconds} ms\nFilter {(int)processingTime.Milliseconds} ms";
                Filter.DrawText(frame, 10, 0, message);
            }

            frame.Dispose();
            bitmap.UnlockBits(cropData);
            Bitmap = bitmap.Clone(rect, bitmap.PixelFormat);
            CroppedBitmap = bitmap.Clone(crop, bitmap.PixelFormat);
        }
    }
}
