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

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace NetworkCamera.Wpf.Internal
{
    /// <summary>
    /// One-way converter from System.Drawing.Bitmap to System.Windows.Media.ImageSource
    /// </summary>
    [ValueConversion(typeof(Bitmap), typeof(System.Windows.Media.ImageSource))]
    public class ImageConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // empty images are empty...
            if (!(value is Bitmap bitmap)) return null;

            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bitmapData = bitmap.LockBits(
                rect,
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            try
            {
                int size = (rect.Width * rect.Height) * 4;
                return BitmapSource.Create(
                    bitmap.Width,
                    bitmap.Height,
                    bitmap.HorizontalResolution,
                    bitmap.VerticalResolution,
                    System.Windows.Media.PixelFormats.Bgra32,
                    null,
                    bitmapData.Scan0,
                    size,
                    bitmapData.Stride);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
