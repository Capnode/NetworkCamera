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

/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:NetworkCamera.ViewModel"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using NetworkCamera.Device;
using NetworkCamera.Setting;
using NetworkCamera.Service.Inference;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace NetworkCamera.Main
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "<Pending>")]
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            // Register types
            var services = new ServiceCollection();
            services.AddSingleton<InferenceServer>();
            services.AddSingleton<SettingsModel>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<AboutViewModel>();
            services.AddSingleton<DevicesViewModel>();
            services.AddSingleton<DevicesModel>();
            Ioc.Default.ConfigureServices(services.BuildServiceProvider());
        }

        public static MainViewModel MainViewModel => Ioc.Default.GetService<MainViewModel>();
        public static SettingsViewModel SettingsViewModel => Ioc.Default.GetService<SettingsViewModel>();
        public static AboutViewModel AboutViewModel => Ioc.Default.GetService<AboutViewModel>();
        public static DevicesViewModel DevicesViewModel => Ioc.Default.GetService<DevicesViewModel>();

        public static void Cleanup()
        {
        }
    }
}