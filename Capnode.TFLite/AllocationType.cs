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
    /// Memory allocation strategies.
    /// </summary>
    public enum AllocationType
    {
        /// <summary>
        /// None
        /// </summary>
        MemNone = 0,

        /// <summary>
        ///  Read-only memory-mapped data (or data externally allocated).
        /// </summary>
        MmapRo,

        /// <summary>
        /// Arena allocated data
        /// </summary>
        ArenaRw,

        /// <summary>
        /// Arena allocated persistent data
        /// </summary>
        ArenaRwPersistent,

        /// <summary>
        /// Tensors that are allocated during evaluation
        /// </summary>
        Dynamic,
    }

}
