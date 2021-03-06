﻿/*
 * Copyright 2020 Capnode AB
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

namespace NetworkCamera.Core
{
    public class Detection
    {
        /// <summary>
        /// The object label
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The score of the matching
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// The bounding box
        /// </summary>
        public RectangleF Box { get; set; }

        public static RectangleF ToRectangle(float left, float top, float right, float bottom)
        {
            right = Math.Max(left, right);
            bottom = Math.Max(top, bottom);
            return new RectangleF(left, top, right - left, bottom - top);
        }
    }
}
