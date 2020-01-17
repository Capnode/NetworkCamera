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

using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace NetworkCamera.Wpf.Internal
{
    internal partial class FilenameEditor : UserControl
    {
        public FilenameEditor()
        {
            InitializeComponent();
        }

        public string Filename
        {
            get { return (string)GetValue(FilenameProperty); }
            set
            {
                SetValue(FilenameProperty, value);
                textbox.Text = value;
            }
        }

        public static readonly DependencyProperty FilenameProperty
            = DependencyProperty.Register(
                nameof(Filename),
                typeof(string),
                typeof(FilenameEditor),
                new PropertyMetadata(null)
            );

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            string path = Filename;
            string folder;
            if (string.IsNullOrWhiteSpace(path))
            {
                folder = AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                string fullPath = Path.GetFullPath(path);
                folder = Path.GetDirectoryName(fullPath);
            }

            openFileDialog.InitialDirectory = folder;
            if ((bool)openFileDialog.ShowDialog())
            {
                Filename = openFileDialog.FileName;
            }
        }
    }
}
