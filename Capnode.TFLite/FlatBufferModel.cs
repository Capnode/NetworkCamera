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
    /// An RAII object that represents a read-only tflite model, copied from disk,
    /// or mmapped. This uses flatbuffers as the serialization format.
    /// </summary>   
    public class FlatBufferModel : UnmanagedObject
    {
        private byte[] _buffer = null;
        private GCHandle _handle;

        /// <summary>
        /// Builds a model based on a file.
        /// </summary>   
        /// <param name="filename">The name of the file where the FlatBufferModel will be loaded from.</param>
        public FlatBufferModel(String filename)
        {
            _ptr = TfLiteInvoke.FlatBufferModelBuildFromFile(filename);
        }

        /// <summary>
        /// Builds a model based on a pre-loaded flatbuffer.
        /// </summary>   
        /// <param name="buffer">The buffer where the FlatBufferModel will be loaded from.</param>
        public FlatBufferModel(byte[] buffer)
        {
            _buffer = new byte[buffer.Length];
            Array.Copy(buffer, _buffer, _buffer.Length);
            _handle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
            try
            {
                _ptr = TfLiteInvoke.FlatBufferModelBuildFromBuffer(_handle.AddrOfPinnedObject(), buffer.Length);
            } catch
            {
                _handle.Free();
                _buffer = null;
                throw;
            }
        }

        /// <summary>
        /// Returns true if the model is initialized
        /// </summary>   
        public bool Initialized
        {
            get
            {
                return TfLiteInvoke.FlatBufferModelInitialized(_ptr);
            }
        }

        /// <summary>
        /// Check if the model identifier is correct.
        /// </summary>
        /// <returns>
        /// True if the model identifier is correct (otherwise false and
        /// reports an error).
        /// </returns>
        public bool CheckModelIdentifier()
        {
            return TfLiteInvoke.FlatBufferModelCheckModelIdentifier(_ptr);
        }

        /// <summary>
        /// Release all the unmanaged memory associated with this model
        /// </summary>
        protected override void DisposeObject()
        {
            if (IntPtr.Zero != _ptr)
                TfLiteInvoke.FlatBufferModelRelease(ref _ptr);
            if (_buffer != null)
            {
                _handle.Free();
                _buffer = null;
            }
        }
    }

    public static partial class TfLiteInvoke
    {
        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr FlatBufferModelBuildFromFile([MarshalAs(StringMarshalType)] String filename);

        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr FlatBufferModelBuildFromBuffer(IntPtr buffer, int bufferSize);

        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void FlatBufferModelRelease(ref IntPtr model);


        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        [return: MarshalAs(BoolMarshalType)]
        internal static extern bool FlatBufferModelInitialized(IntPtr model);

        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        [return: MarshalAs(BoolMarshalType)]
        internal static extern bool FlatBufferModelCheckModelIdentifier(IntPtr model);

    }
}
