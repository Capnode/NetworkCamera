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
    /// Types supported by tensor
    /// </summary>
    public enum DataType
    {
        /// <summary>
        /// No type
        /// </summary>
        NoType = 0,

        /// <summary>
        /// single precision float
        /// </summary>
        Float32 = 1,

        /// <summary>
        /// Int32
        /// </summary>
        Int32 = 2,

        /// <summary>
        /// UInt8
        /// </summary>
        UInt8 = 3,

        /// <summary>
        /// Int64
        /// </summary>
        Int64 = 4,

        /// <summary>
        /// String
        /// </summary>
        String = 5,

        /// <summary>
        /// Bool
        /// </summary>
        Bool = 6,

        /// <summary>
        /// Bool
        /// </summary>
        Int16 = 7,

        /// <summary>
        /// Complex64
        /// </summary>
        Complex64
    }
}
