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

using Serilog;
using System;
using System.Linq;
using System.Threading;

namespace NetworkCamera.Device.Core
{
    internal class DeviceFactory
    {
#pragma warning disable CA1822 // Mark members as static
        public DeviceModel Run(DeviceModel deviceModel, DeviceEventHandler deviceEvent, CancellationToken token)
#pragma warning restore CA1822 // Mark members as static
        {
            if (deviceModel == null) throw new ArgumentNullException(nameof(deviceModel));

            IDevice device = CreateProvider((string)deviceModel.Format);
            if (device == null)
            {
                deviceModel.Active = false;
                return deviceModel;
            }

            device.Main(deviceModel, deviceEvent, token);
            return deviceModel;
        }

        private static IDevice CreateProvider(string name)
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IDevice).IsAssignableFrom(p) && !p.IsInterface)
                .FirstOrDefault(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (type == null)
            {
                Log.Warning($"Provider {name} not found");
                return null;
            }

            IDevice provider = (IDevice)Activator.CreateInstance(type);
            if (provider == null) throw new ApplicationException($"Can not create provider {name}");
            return provider;
        }
    }
}
