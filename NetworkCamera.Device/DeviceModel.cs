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

using NetworkCamera.Core;
using NetworkCamera.Device.Internal;
using System.ComponentModel;
using System.Drawing;

namespace NetworkCamera.Device
{
    public class DeviceModel : ModelBase
    {
        private string _format;

        public DeviceModel()
        {
            Format = nameof(Mjpeg);
        }

        [Category("Device")]
        [DisplayName("Device name")]
        [Description("Name of the device.")]
        [Browsable(true)]
        [ReadOnly(false)]
        public string Name { get; set; } = "Device";

        [Category("Device")]
        [DisplayName("Format")]
        [Description("Streaming format.")]
        [TypeConverter(typeof(DeviceNameConverter))]
        [RefreshProperties(RefreshProperties.All)]
        [Browsable(true)]
        [ReadOnly(false)]
        public string Format
        {
            get => _format;
            set
            {
                _format = value;
                Refresh();
            }
        }

        [Category("Device")]
        [DisplayName("Source")]
        [Description("Device source address")]
        [Browsable(false)]
        [ReadOnly(false)]
        public string Source { get; set; } = string.Empty;

        [Category("Device")]
        [DisplayName("Motion detection")]
        [Description("Activate motion detection")]
        [Browsable(true)]
        [ReadOnly(false)]
        public bool MotionDetection { get; set; }

        [Category("Device")]
        [DisplayName("Object detection")]
        [Description("Activate object detection")]
        [Browsable(true)]
        [ReadOnly(false)]
        public bool ObjectDetection { get; set; }

        [Category("Account")]
        [DisplayName("Login")]
        [Description("User login.")]
        [Browsable(false)]
        [ReadOnly(false)]
        public string Login { get; set; } = string.Empty;

        [Category("Account")]
        [DisplayName("Password")]
        [Description("User login password.")]
        [PasswordPropertyText(true)]
        [Browsable(false)]
        [ReadOnly(false)]
        public string Password { get; set; } = string.Empty;

        [Category("Export")]
        [DisplayName("Folder")]
        [Description("File export folder")]
        [Browsable(true)]
        [ReadOnly(false)]
        public string Folder { get; set; }

        [Category("Export")]
        [DisplayName("Notification")]
        [Description("Event notification web address")]
        [Browsable(true)]
        [ReadOnly(false)]
        public string Notification { get; set; }

        [Browsable(false)]
        [ReadOnly(false)]
        public bool Active { get; set; }

        [Browsable(false)]
        [ReadOnly(false)]
        public Rectangle Crop { get; set; } = new Rectangle();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public void Refresh()
        {
            switch (Format)
            {
                case nameof(Mjpeg):
                    SetBrowsable(nameof(Source), true);
                    SetBrowsable(nameof(Login), true);
                    SetBrowsable(nameof(Password), true);
                    break;

                case nameof(ImageDevice):
                    SetBrowsable(nameof(Source), true);
                    break;

                default:
                    SetBrowsable(nameof(Login), false);
                    SetBrowsable(nameof(Password), false);
                    break;
            }
        }
    }
}
