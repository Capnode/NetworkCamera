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

using Ookii.Dialogs.Wpf;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace NetworkCamera.Wpf.Internal
{
    internal partial class FolderEditor : UserControl
    {
        public FolderEditor()
        {
            InitializeComponent();
        }

        public string Folder
        {
            get { return (string)GetValue(FolderProperty); }
            set
            {
                SetValue(FolderProperty, value);
                textbox.Text = value;
            }
        }

        public static readonly DependencyProperty FolderProperty
            = DependencyProperty.Register(
                nameof(Folder),
                typeof(string),
                typeof(FolderEditor),
                new PropertyMetadata(null)
            );

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            string path = Folder;
            string folder;
            if (string.IsNullOrWhiteSpace(path))
            {
                folder = Directory.GetCurrentDirectory();
            }
            else
            {
                string fullPath = Path.GetFullPath(path);
                folder = Path.GetDirectoryName(fullPath);
            }

            dialog.SelectedPath = folder;
            if ((bool)dialog.ShowDialog())
            {
                Folder = dialog.SelectedPath;
            }
        }
    }
}
