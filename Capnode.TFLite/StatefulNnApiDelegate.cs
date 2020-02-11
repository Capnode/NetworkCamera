//----------------------------------------------------------------------------
//  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace Capnode.TFLite
{
    public class StatefulNnApiDelegate : Capnode.TFLite.UnmanagedObject, IDelegate
    {
        public StatefulNnApiDelegate()
        {
            _ptr = TfLiteInvoke.StatefulNnApiDelegateCreate();
        }

        /// <summary>
        /// Pointer to the native Delegate object.
        /// </summary>
        IntPtr IDelegate.DelegatePtr
        {
            get
            {
                return TfLiteInvoke.StatefulNnApiDelegateGetDelegate();
            }
        }

        /// <summary>
        /// Release all the unmanaged memory associated with this delegate
        /// </summary>
        protected override void DisposeObject()
        {
            if (IntPtr.Zero != _ptr)
                TfLiteInvoke.StatefulNnApiDelegateRelease(ref _ptr);
        }
    }

    public static partial class TfLiteInvoke
    {
        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr StatefulNnApiDelegateCreate();

        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void StatefulNnApiDelegateRelease(ref IntPtr delegatePtr);

        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr StatefulNnApiDelegateGetDelegate();

        //[DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        //[return: MarshalAs(TfLiteInvoke.BoolMarshalType)]
        //internal static extern bool StatefulNnApiDelegateIsSupported(IntPtr delegatePtr);
    }
}