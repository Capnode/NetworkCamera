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
    public static partial class TfLiteInvoke
    {
        private static int TfliteErrorHandler(
           int status,
           IntPtr errMsg)
        {
            string msg = Marshal.PtrToStringAnsi(errMsg);
            throw new Exception(msg);
        }

        /// <summary>
        /// Define the functional interface for the error callback
        /// </summary>
        /// <param name="status">The status code</param>
        /// <param name="errMsg">The pointer to the error message</param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(TFCallingConvention)]
        public delegate int TfliteErrorCallback(int status, IntPtr errMsg);

        /// <summary>
        /// The Tensorflow native api calling convention
        /// </summary>
        public const CallingConvention TFCallingConvention = CallingConvention.Cdecl;

        /// <summary>
        /// The string marshal type
        /// </summary>
        public const UnmanagedType StringMarshalType = UnmanagedType.LPStr;

        /// <summary>
        /// Represent a bool value in C++
        /// </summary>
        public const UnmanagedType BoolMarshalType = UnmanagedType.U1;

        /// <summary>
        /// Represent a int value in C++
        /// </summary>
        public const UnmanagedType BoolToIntMarshalType = UnmanagedType.Bool;

        /// <summary>
        /// Static Constructor to setup tensorflow environment
        /// </summary>
        static TfLiteInvoke()
        {
        }

        /// <summary>
        /// The default error handler for tensorflow lite
        /// </summary>
        public static readonly TfliteErrorCallback TfliteErrorHandlerThrowException = (TfliteErrorCallback)TfliteErrorHandler;

        [DllImport(TfliteDll, CallingConvention = TFCallingConvention)]
        internal static extern void Memcpy(IntPtr dst, IntPtr src, int length);

        /// <summary>
        /// Get the tensorflow lite version.
        /// </summary>
        public static String Version
        {
            get
            {
                return Marshal.PtrToStringAnsi(GetLiteVersion());
            }
        }

        [DllImport(TfliteDll, CallingConvention = TFCallingConvention)]
        internal static extern IntPtr GetLiteVersion();

        /// <summary>
        /// Redirect tensorflow lite error.
        /// </summary>
        /// <param name="errorHandler">The error handler</param>
        [DllImport(TfliteDll, CallingConvention = TFCallingConvention, EntryPoint = "RedirectError")]
        private static extern void RedirectError(TfliteErrorCallback errorHandler);
    }
}
