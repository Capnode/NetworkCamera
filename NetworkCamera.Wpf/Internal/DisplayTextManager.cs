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
 * 
 * GitHub reference: dmitryshelamov/UI-Cropping-Image
 */

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NetworkCamera.Wpf.Internal
{
    /// <summary>
    /// Display text information
    /// </summary>
    internal class DisplayTextManager
    {
        private readonly TextBlock _sizeTextBlock;
        private readonly RectangleManager _rectangleManager;
        public DisplayTextManager(Canvas canvas, RectangleManager rectangleManager)
        {
            _rectangleManager = rectangleManager;
            _sizeTextBlock = new TextBlock()
            {
#pragma warning disable CA1303 // Do not pass literals as localized parameters
                Text = "Size counter",
#pragma warning restore CA1303 // Do not pass literals as localized parameters
                FontSize = 14,
                Foreground = Brushes.White,
                Background = Brushes.Black,
                Visibility = Visibility.Hidden
            };
            canvas.Children.Add(_sizeTextBlock);
        }


        /// <summary>
        /// Manage visibility of text
        /// </summary>
        /// <param name="isVisble">Set current visibility</param>
        public void ShowText(bool isVisble)
        {
            _sizeTextBlock.Visibility = isVisble ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// Update (redraw) text label
        /// </summary>
        public void UpdateText(string text)
        {
            double offsetTop = 2;
            double offsetLeft = 5;

            var calculateTop = _rectangleManager.TopLeft.Y - _sizeTextBlock.ActualHeight - offsetTop;
            if (calculateTop < 0)
                calculateTop = offsetTop;
            
            Canvas.SetLeft(_sizeTextBlock, _rectangleManager.TopLeft.X + offsetLeft);
            Canvas.SetTop(_sizeTextBlock, calculateTop);
            _sizeTextBlock.Text = text;
        }
    }
}
