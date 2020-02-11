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

namespace Capnode.TFLite
{
    /// <summary>
    /// A tensorflow integer array
    /// </summary>
    public class IntArray : Capnode.TFLite.UnmanagedObject
    {
        private bool _needDispose;

        /// <summary>
        /// Create an int array of the given size
        /// </summary>
        /// <param name="size">The size of the IntArray</param>
        public IntArray(int size)
        {
            _needDispose = true;
            _ptr = TfLiteInvoke.IntArrayCreate(size);
        }

        internal IntArray(IntPtr ptr, bool needDispose)
        {
            _ptr = ptr;
            _needDispose = needDispose;
        }

        /// <summary>
        /// Get a copy of the data in this integer array
        /// </summary>
        public int[] Data
        {
            get
            {
                int size = TfLiteInvoke.IntArrayGetSize(_ptr);
                int[] d = new int[size];
                IntPtr dataPtr = TfLiteInvoke.IntArrayGetData(_ptr);
                Marshal.Copy(dataPtr, d, 0, size);
                return d;
            }
        }

        /// <summary>
        /// Release all the unmanaged memory associated with this IntArray
        /// </summary>
        protected override void DisposeObject()
        {

            if (IntPtr.Zero != _ptr)
            {
                if (_needDispose)
                    TfLiteInvoke.IntArrayRelease(ref _ptr);
                else
                    _ptr = IntPtr.Zero;
            }
        }
    }

    public static partial class TfLiteInvoke
    {
        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr IntArrayCreate(int size);

        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern int IntArrayGetSize(IntPtr v);

        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr IntArrayGetData(IntPtr v);

        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void IntArrayRelease(ref IntPtr v);
    }
}
