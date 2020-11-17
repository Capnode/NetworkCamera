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
 
using NetworkCamera.Wpf.Internal;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace NetworkCamera.Wpf
{
    /// <summary>
    /// Interaction logic for CropImage.xaml
    /// </summary>
    public partial class CropControl : UserControl
    {
        private CroppingAdorner CroppingAdorner;

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source",
            typeof(ImageSource),
            typeof(CropControl),
            new PropertyMetadata(default(ImageSource), OnSourceChanged));

        public static readonly DependencyProperty CropProperty = DependencyProperty.Register(
            "Crop",
            typeof(Rectangle),
            typeof(CropControl),
            new PropertyMetadata(default(Rectangle), OnRectChanged));

        public CropControl()
        {
            InitializeComponent();

            CanvasPanel.Loaded += CanvasPanel_Loaded;
            CanvasPanel.SizeChanged += CanvasPanel_SizeChanged;
            CanvasPanel.MouseLeftButtonDown += CanvasPanel_MouseLeftButtonDown;
        }

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public Rectangle Crop
        {
            get { return (Rectangle)GetValue(CropProperty); }
            set { SetValue(CropProperty, value); }
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is CropControl cropControl)) return;
            if (e.NewValue is ImageSource newImage)
            {
                cropControl.Image.Source = newImage;
                cropControl.AdornerCrop(cropControl.Crop);
            }
            else
            {
                cropControl.Image.Source = null;
                cropControl.CroppingAdorner.Crop = Rectangle.Empty;
            }
        }

        private static void OnRectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is CropControl cropControl)) return;
            if (cropControl.CroppingAdorner == default) return;
            if (e.NewValue is Rectangle rect)
            {
                cropControl.AdornerCrop(rect);
            }
            else
            {
                cropControl.CroppingAdorner.Crop = Rectangle.Empty;
            }
        }

        private void CanvasPanel_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement visual)
            {
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(visual);
                if (adornerLayer == null) return;
                CroppingAdorner = new CroppingAdorner(visual);
                adornerLayer.Add(CroppingAdorner);
                AdornerCrop(Crop);
                CroppingAdorner.OnRectangleSizeEvent += CroppingAdorner_OnRectangleSizeEvent;
            }
        }

        private void CanvasPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdornerCrop(Crop);
        }

        private void CanvasPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CroppingAdorner.MouseLeftButtonDownEventHandler(sender, e);
        }

        private void CroppingAdorner_OnRectangleSizeEvent(object sender, Rectangle rect)
        {
            if (Image?.Source == null) return;

            double scale = Math.Max(Image.Source.Width / CanvasPanel.ActualWidth, Image.Source.Height / CanvasPanel.ActualHeight);
            int width = (int)Math.Round(scale * rect.Width);
            int height = (int)Math.Round(scale * rect.Height);
            SetText(width, height);
            Crop = new Rectangle(
                (int)Math.Round(scale * rect.X),
                (int)Math.Round(scale * rect.Y),
                width,
                height);
        }

        private void AdornerCrop(Rectangle rect)
        {
            if (CroppingAdorner == null) return;
            if (Image.Source == null) return;

            SetText(rect.Width, rect.Height);
            double scale = Math.Max(Image.Source.Width / CanvasPanel.ActualWidth, Image.Source.Height / CanvasPanel.ActualHeight);
            CroppingAdorner.Crop = new Rectangle(
                (int)Math.Round(rect.X / scale),
                (int)Math.Round(rect.Y / scale),
                (int)Math.Round(rect.Width / scale),
                (int)Math.Round(rect.Height /scale));
        }

        private void SetText(int width, int height)
        {
            CroppingAdorner.Text = $"{width}x{height}";
        }
    }
}
