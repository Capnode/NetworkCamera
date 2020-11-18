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
using System.Drawing;
using System.Threading;

namespace NetworkCamera.Device.Core
{
    internal class ImageDevice : IDevice
    {
        public void Main(DeviceModel device, DeviceEventHandler deviceEvent, CancellationToken token)
        {
            if (device != null) throw new ArgumentNullException(nameof(device));
            if (string.IsNullOrWhiteSpace(device.Source)) throw new ArgumentException(nameof(device.Source));
            if (deviceEvent == null) throw new ArgumentNullException(nameof(deviceEvent));
            if (token == null) throw new ArgumentNullException(nameof(token));

            try
            {
                using Bitmap bitmap = new Bitmap(Image.FromFile(device.Source));
                deviceEvent(this, new DeviceEventArgs(bitmap));
                token.WaitHandle.WaitOne();
            }
            finally
            {
                // notify client
                deviceEvent(this, new DeviceEventArgs(null));
            }
        }
    }
}
