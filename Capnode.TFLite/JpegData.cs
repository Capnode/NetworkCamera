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

namespace Capnode.TFLite
{
    /// <summary>
    /// The Jpeg Data
    /// </summary>
    public class JpegData
    {
        /// <summary>
        /// The width of the image
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The height of the image
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The raw jpeg data
        /// </summary>
        public byte[] Raw { get; set; }
    }
}
