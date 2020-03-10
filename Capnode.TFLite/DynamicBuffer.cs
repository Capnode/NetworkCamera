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
using System.Runtime.InteropServices;
using System.Text;

namespace Capnode.TFLite
{
    /// <summary>
    /// DynamicBuffer holds temporary buffer that will be used to create a dynamic tensor. 
    /// </summary>
    public class DynamicBuffer : UnmanagedObject
    {
        /// <summary>
        /// Create a new dynamic buffer.
        /// </summary>
        public DynamicBuffer()
        {
            _ptr = TfLiteInvoke.DynamicBufferCreate();
        }

        /// <summary>
        /// Add string to dynamic buffer by resizing the buffer and copying the data.
        /// </summary>
        /// <param name="str">The string to add to the dynamic buffer</param>
        public void AddString(String str)
        {
            byte[] rawString = Encoding.ASCII.GetBytes(str);
            GCHandle handle = GCHandle.Alloc(rawString, GCHandleType.Pinned);
            TfLiteInvoke.DynamicBufferAddString(_ptr, handle.AddrOfPinnedObject(), rawString.Length);
            handle.Free();
        }

        /// <summary>
        /// Fill content into a string tensor.
        /// </summary>
        /// <param name="tensor">The string tensor</param>
        public void WriteToTensor(Tensor tensor, IntArray newShape = null)
        {
            TfLiteInvoke.DynamicBufferWriteToTensor(_ptr, tensor, newShape ?? IntPtr.Zero);
        }
        
        /// <summary>
        /// Release all the unmanaged memory associated with this model
        /// </summary>
        protected override void DisposeObject()
        {
            if (IntPtr.Zero != _ptr)
            {
                TfLiteInvoke.DynamicBufferRelease(ref _ptr);
            }
        }
    }

    public static partial class TfLiteInvoke
    {
        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr DynamicBufferCreate();

        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void DynamicBufferRelease(ref IntPtr buffer);

        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void DynamicBufferAddString(IntPtr buffer, IntPtr str, int len);

        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void DynamicBufferWriteToTensor(IntPtr buffer, IntPtr tensor, IntPtr newShape);

    }
}
