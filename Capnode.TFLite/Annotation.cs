/*
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

namespace Capnode.TFLite
{
    /// <summary>
    /// Image annotation
    /// </summary>
    public class Annotation
    {
        /// <summary>
        /// The coordinates of the rectangle, the values are in the range of [0, 1], each rectangle contains 4 values, corresponding to the top left corner (x0, y0) and bottom right corner (x1, y1)
        /// </summary>
        public float[] Rectangle;

        /// <summary>
        /// The text to be drawn on the top left corner of the Rectangle
        /// </summary>
        public String Label;
    }

}
