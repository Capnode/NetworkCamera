//----------------------------------------------------------------------------
//  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace Capnode.TFLite
{

    public class InterpreterBuilder : Capnode.TFLite.UnmanagedObject
    {
        public InterpreterBuilder(FlatBufferModel flatBufferModel, IOpResolver resolver)
        {
            _ptr = TfLiteInvoke.InterpreterBuilderCreate(flatBufferModel, resolver.OpResolverPtr);
        }

        public Status Build(Interpreter interpreter)
        {
            return TfLiteInvoke.InterpreterBuilderBuild(_ptr, interpreter);
        }

        /// <summary>
        /// Release all the unmanaged memory associated with this model
        /// </summary>
        protected override void DisposeObject()
        {
            if (IntPtr.Zero != _ptr)
                TfLiteInvoke.InterpreterBuilderRelease(ref _ptr);
        }
    }

    public static partial class TfLiteInvoke
    {
        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr InterpreterBuilderCreate(IntPtr model, IntPtr opResolver);

        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void InterpreterBuilderRelease(ref IntPtr builder);

        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern Status InterpreterBuilderBuild(IntPtr builder, IntPtr interpreter);
    }
}