﻿/*
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

using NetworkCamera.Main;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace NetworkCamera.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const uint _esContinous = 0x80000000;
        private const uint _esSystemRequired = 0x00000001;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private const uint _esDisplayRequired = 0x00000002;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint SetThreadExecutionState([In] uint esFlags);

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            NetworkCamera.Wpf.Properties.Settings.Default.Reload();
            EnsureBrowserEmulationEnabled("NetworkCamera.exe");

            // Prevent going to sleep mode
            _ = SetThreadExecutionState(_esContinous | _esSystemRequired);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Enable sleep mode
            _ = SetThreadExecutionState(_esContinous);

            ViewModelLocator.MainViewModel.SaveAll();
            NetworkCamera.Wpf.Properties.Settings.Default.Save();
            base.OnExit(e);
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs ex)
        {
            Debug.WriteLine("Exception {0} - {1} - {2}", ex.GetType(), ex.Exception.Message, ex.Exception.ToString());
            Exception iex = ex.Exception.InnerException;
            if (iex != null)
                Debug.WriteLine("Exception inner {0} - {1}", iex.GetType(), iex.Message);

            ex.Handled = true; // Continue processing
        }

        /// <summary>
        /// WebBrowser Internet Explorer 11 emulation
        /// </summary>
        public static void EnsureBrowserEmulationEnabled(string exename, bool uninstall = false)
        {
            try
            {
                using var rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true);
                if (!uninstall)
                {
                    dynamic value = rk.GetValue(exename);
                    if (value == null)
                        rk.SetValue(exename, (uint)11001, RegistryValueKind.DWord);
                }
                else
                    rk.DeleteValue(exename);
            }
            finally
            {
            }
        }
    }
}
