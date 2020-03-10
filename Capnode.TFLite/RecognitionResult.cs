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
using System.Collections.Generic;

namespace Capnode.TFLite
{
    public class RecognitionResult
    {
        /// <summary>
        /// Rectangles will be in the format of (x, y, width, height) in the original image coordinate
        /// </summary>
        public IReadOnlyList<float> Rectangle { get; set; }

        /// <summary>
        /// The object label
        /// </summary>
        public String Label { get; set; }

        /// <summary>
        /// The score of the matching
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// The class index
        /// </summary>
        public int Class { get; set; }
    }
}
