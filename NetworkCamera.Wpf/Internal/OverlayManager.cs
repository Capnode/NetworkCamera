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
using System.Windows.Shapes;

namespace NetworkCamera.Wpf.Internal
{
    /// <summary>
    /// Class that response for adding shadow area outside of cropping rectangle)
    /// </summary>
    internal class OverlayManager
    {
        private readonly Canvas _canvas;
        private readonly RectangleManager _rectangleManager;

        private readonly Path _pathOverlay;
        private GeometryGroup _geometryGroup;

        public OverlayManager(Canvas canvas, RectangleManager rectangleManager)
        {
            _canvas = canvas;
            _rectangleManager = rectangleManager;

            _pathOverlay = new Path
            {
                Fill = Brushes.Black,
                Opacity = 0.5
            };

            _canvas.Children.Add(_pathOverlay);
        }

        /// <summary>
        /// Update (redraw) overlay
        /// </summary>
        public void UpdateOverlay()
        {
            _geometryGroup = new GeometryGroup();
            RectangleGeometry geometry1 =
                new RectangleGeometry(new Rect(new Size(_canvas.ActualWidth, _canvas.ActualHeight)));
            RectangleGeometry geometry2 = new RectangleGeometry(new Rect(_rectangleManager.TopLeft.X,
                _rectangleManager.TopLeft.Y, _rectangleManager.RectangleWidth, _rectangleManager.RectangleHeight));
            _geometryGroup.Children.Add(geometry1);
            _geometryGroup.Children.Add(geometry2);
            _pathOverlay.Data = _geometryGroup;
        }
    }
}
