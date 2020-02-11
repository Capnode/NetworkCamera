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
    /// The default tensor flow lite buildin op resolver.
    /// </summary>
    public class BuildinOpResolver : Capnode.TFLite.UnmanagedObject, IOpResolver
    {
        private IntPtr _opResolverPtr;
        
        /// <summary>
        /// Create a default buildin op resolver.
        /// </summary>
        public BuildinOpResolver()
        {
            _ptr = TfLiteInvoke.BuiltinOpResolverCreate(ref _opResolverPtr);
        }
        
        IntPtr IOpResolver.OpResolverPtr
        {
            get
            {
                return _opResolverPtr;
            }
        }

        /// <summary>
        /// Release all the unmanaged memory associated with this model
        /// </summary>
        protected override void DisposeObject()
        {
            if (IntPtr.Zero != _ptr)
            {
                TfLiteInvoke.BuiltinOpResolverRelease(ref _ptr);
                _opResolverPtr = IntPtr.Zero;
            }
        }
    }

    /// <summary>
    /// Class that provide access to native tensorflow lite functions
    /// </summary>
    public static partial class TfLiteInvoke
    {
        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr BuiltinOpResolverCreate(ref IntPtr opResolver);

        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void BuiltinOpResolverRelease(ref IntPtr resolver);

    }
}
