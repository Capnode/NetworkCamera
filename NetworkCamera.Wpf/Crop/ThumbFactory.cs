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
using System.Windows.Input;

namespace NetworkCamera.Wpf.Crop
{
    internal class ThumbFactory
    {
        /// <summary>
        /// Available thumbs positions
        /// </summary>
        public enum ThumbPosition
        {
            TopLeft,
            TopMiddle,
            TopRight,
            RightMiddle,
            BottomRight,
            BottomMiddle,
            BottomLeft,
            LeftMiddle
        }

        /// <summary>
        /// Thumb factory
        /// </summary>
        /// <param name="thumbPosition">Thumb positions</param>
        /// <param name="canvas">Parent UI element that we will attach thumb as child</param>
        /// <param name="size">Size of thumb</param>
        /// <returns></returns>
        public static ThumbCrop CreateThumb(ThumbPosition thumbPosition, Canvas canvas, double size)
        {
            ThumbCrop customThumb = new ThumbCrop(size)
            {
                Cursor = GetCursor(thumbPosition),
                Visibility = Visibility.Hidden
            };
            canvas.Children.Add(customThumb);
            return customThumb;
        }

        /// <summary>
        /// Display proper cursor to corresponding thumb
        /// </summary>
        /// <param name="thumbPosition">Thumb position</param>
        /// <returns></returns>
        private static Cursor GetCursor(ThumbPosition thumbPosition)
        {
            return thumbPosition switch
            {
                (ThumbPosition.TopLeft) => Cursors.SizeNWSE,
                (ThumbPosition.TopMiddle) => Cursors.SizeNS,
                (ThumbPosition.TopRight) => Cursors.SizeNESW,
                (ThumbPosition.RightMiddle) => Cursors.SizeWE,
                (ThumbPosition.BottomRight) => Cursors.SizeNWSE,
                (ThumbPosition.BottomMiddle) => Cursors.SizeNS,
                (ThumbPosition.BottomLeft) => Cursors.SizeNESW,
                (ThumbPosition.LeftMiddle) => Cursors.SizeWE,
                _ => null,
            };
        }
    }
}
