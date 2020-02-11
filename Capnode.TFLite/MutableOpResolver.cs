//----------------------------------------------------------------------------
//  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace Capnode.TFLite
{
    public class MutableOpResolver : Capnode.TFLite.UnmanagedObject, IOpResolver
    {
        private IntPtr _opResolverPtr;
        
        public MutableOpResolver()
        {
            _ptr = TfLiteInvoke.MutableOpResolverCreate(ref _opResolverPtr);
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
                TfLiteInvoke.MutableOpResolverRelease(ref _ptr);
                _opResolverPtr = IntPtr.Zero;
            }
        }
    }

    public static partial class TfLiteInvoke
    {
        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr MutableOpResolverCreate(ref IntPtr opResolver);

        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void MutableOpResolverRelease(ref IntPtr resolver);
    }
}