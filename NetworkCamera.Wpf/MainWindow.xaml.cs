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

using NetworkCamera.Main;
using NetworkCamera.Setting;
using System;
using System.Windows;

namespace NetworkCamera.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.MainWindowPlacement = this.GetPlacement();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            this.SetPlacement(Properties.Settings.Default.MainWindowPlacement);
        }

        private void HelpAbout(object sender, RoutedEventArgs e)
        {
            var about = new AboutView();
            about.ShowDialog();
        }

        private void FileSettings(object sender, RoutedEventArgs e)
        {
            var view = new SettingsView();
            var vm = view.DataContext as SettingsViewModel;
            var oldSettings = vm.Model.Clone();
            if ((bool)view.ShowDialog())
            {
                var mainVm = DataContext as MainViewModel;
                mainVm.SaveAll();
            }
            else
            {
                vm.Model.Copy(oldSettings);
            }
        }
    }
}
