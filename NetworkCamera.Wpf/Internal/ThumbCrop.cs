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
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace NetworkCamera.Wpf.Internal
{
    internal class ThumbCrop : Thumb
    {
        public double ThumbSize { get; }

        public ThumbCrop(double thumbSize)
        {
            ThumbSize = thumbSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return null;
        }

        /// <summary>
        /// Custom visual style of thumb
        /// </summary>
        /// <param name="drawingContext"></param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(Brushes.White, new Pen(Brushes.Black, 2), new Rect(new Size(ThumbSize, ThumbSize)));
            drawingContext.DrawRectangle(Brushes.Black, new Pen(Brushes.Black, 0), new Rect(2, 2, 6, 6));
        }

        /// <summary>
        /// Set thumb to corresponding positions
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        public void SetPosition(double x, double y)
        {
            Canvas.SetTop(this, y - ThumbSize / 2);
            Canvas.SetLeft(this, x - ThumbSize / 2);
        }
    }
}
