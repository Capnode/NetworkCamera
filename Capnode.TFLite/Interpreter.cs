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
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Capnode.TFLite
{
    /// <summary>
    /// The tensorflow lite interpreter.
    /// </summary>
    public class Interpreter : UnmanagedObject
    {

        /// <summary>
        /// Create an interpreter from a flatbuffer model
        /// </summary>
        /// <param name="flatBufferModel">The flat buffer model.</param>
        /// <param name="resolver">An instance that implements the Resolver interface which maps custom op names and builtin op codes to op registrations.</param>
        public Interpreter(FlatBufferModel flatBufferModel, IOpResolver resolver = null)
        {
            if (resolver == null)
            {
                using (BuildinOpResolver buildinResolver = new BuildinOpResolver())
                {
                    _ptr = TfLiteInvoke.InterpreterCreateFromModel(flatBufferModel.Ptr, ((IOpResolver) buildinResolver).OpResolverPtr);
                }
            } else
                _ptr = TfLiteInvoke.InterpreterCreateFromModel(flatBufferModel.Ptr, resolver.OpResolverPtr);
        }

        /// <summary>
        /// Update allocations for all tensors. This will redim dependent tensors using
        /// the input tensor dimensionality as given. This is relatively expensive.
        /// If you know that your sizes are not changing, you need not call this.
        /// </summary>
        /// <returns>Status of success or failure.</returns>
        public Status AllocateTensors()
        {
            return TfLiteInvoke.InterpreterAllocateTensors(_ptr);
        }

        /// <summary>
        /// Invoke the interpreter (run the whole graph in dependency order).
        /// </summary>
        /// <returns>Status of success or failure.</returns>
        /// <remarks>It is possible that the interpreter is not in a ready state
        /// to evaluate (i.e. if a ResizeTensor() has been performed without an
        /// AllocateTensors().
        /// </remarks>
        public Status Invoke()
        {
            return TfLiteInvoke.InterpreterInvoke(_ptr);
        }

        /// <summary>
        /// Get the number of tensors in the model.
        /// </summary>
        public int TensorSize
        {
            get
            {
                return TfLiteInvoke.InterpreterTensorSize(_ptr);
            }
        }

        /// <summary>
        /// Get the number of ops in the model.
        /// </summary>
        public int NodeSize
        {
            get
            {
                return TfLiteInvoke.InterpreterNodesSize(_ptr);
            }
        }

        /// <summary>
        /// Get a tensor data structure.
        /// </summary>
        /// <param name="index">The index of the tensor</param>
        /// <returns>The tensor in the specific index</returns>
        public Tensor GetTensor(int index)
        {
            return new Tensor(TfLiteInvoke.InterpreterGetTensor(_ptr, index), false);
        }

        /// <summary>
        /// Get an array of all the input tensors
        /// </summary>
        public Tensor[] Inputs
        {
            get
            {
                int[] inputIdx = InputIndices;
                Tensor[] inputs = new Tensor[inputIdx.Length];
                for (int i = 0; i < inputs.Length; i++)
                    inputs[i] = GetTensor(inputIdx[i]);
                return inputs;
            }
        }

        /// <summary>
        /// Get an array of all the output tensors
        /// </summary>
        public Tensor[] Outputs
        {
            get
            {
                int[] outputIdx = OutputIndices;
                Tensor[] inputs = new Tensor[outputIdx.Length];
                for (int i = 0; i < inputs.Length; i++)
                    inputs[i] = GetTensor(outputIdx[i]);
                return inputs;
            }

        }

        /// <summary>
        /// Get the list of tensor index of the inputs tensors.
        /// </summary>
        public int[] InputIndices
        {
            get
            {
                int size = TfLiteInvoke.InterpreterGetInputSize(_ptr);
                int[] input = new int[size];
                GCHandle handle = GCHandle.Alloc(input, GCHandleType.Pinned);
                TfLiteInvoke.InterpreterGetInput(_ptr, handle.AddrOfPinnedObject());
                handle.Free();
                return input;
            }
        }

        /// <summary>
        /// Get the list of tensor index of the outputs tensors.
        /// </summary>
        public int[] OutputIndices
        {
            get
            {
                int size = TfLiteInvoke.InterpreterGetOutputSize(_ptr);
                int[] output = new int[size];
                GCHandle handle = GCHandle.Alloc(output, GCHandleType.Pinned);
                int outputSize = TfLiteInvoke.InterpreterGetOutput(_ptr, handle.AddrOfPinnedObject());
                Debug.Assert(outputSize == size, "Output size do not match!");
                handle.Free();
                return output;
            }
        }

        /// <summary>
        /// Enable or disable the NN API (Android Neural Network API)
        /// </summary>
        /// <param name="enable">If true, enable the NN API (Android Neural Network API). If false, disable it.</param>
        public void UseNNAPI(bool enable)
        {
            TfLiteInvoke.InterpreterUseNNAPI(_ptr, enable);
        }

        /// <summary>
        /// Set the number of threads available to the interpreter.
        /// </summary>
        /// <param name="numThreads">The number of threads</param>
        public void SetNumThreads(int numThreads)
        {
            TfLiteInvoke.InterpreterSetNumThreads(_ptr, numThreads);
        }

        /// <summary>
        /// Release all the unmanaged memory associated with this interpreter
        /// </summary>
        protected override void DisposeObject()
        {
            if (IntPtr.Zero != _ptr)
                TfLiteInvoke.InterpreterRelease(ref _ptr);
        }

        /// <summary>
        /// Allow a delegate to look at the graph and modify the graph to handle
        /// parts of the graph themselves. After this is called, the graph may
        /// contain new nodes that replace 1 more nodes.
        /// WARNING: This is an experimental API and subject to change.
        /// </summary>
        /// <param name="tfDelegate">The delegate</param>
        /// <returns>The status</returns>
        public Status ModifyGraphWithDelegate(Capnode.TFLite.IDelegate tfDelegate)
        {
            return TfLiteInvoke.InterpreterModifyGraphWithDelegate(_ptr, tfDelegate.DelegatePtr);
        }
    }

    public static partial class TfLiteInvoke
    {
        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr InterpreterCreate();

        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr InterpreterCreateFromModel(IntPtr model, IntPtr opResolver);

        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern Status InterpreterAllocateTensors(IntPtr interpreter);

        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern Status InterpreterInvoke(IntPtr interpreter);

        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr InterpreterGetTensor(IntPtr interpreter, int index);

        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern int InterpreterTensorSize(IntPtr interpreter);

        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern int InterpreterNodesSize(IntPtr interpreter);

        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern int InterpreterGetInputSize(IntPtr interpreter);
        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void InterpreterGetInput(IntPtr interpreter, IntPtr input);

        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr InterpreterGetInputName(IntPtr interpreter, int index);

        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern int InterpreterGetOutputSize(IntPtr interpreter);
        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern int InterpreterGetOutput(IntPtr interpreter, IntPtr output);

        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr InterpreterGetOutputName(IntPtr interpreter, int index);

        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void InterpreterRelease(ref IntPtr interpreter);

        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void InterpreterUseNNAPI(
            IntPtr interpreter, 
            [MarshalAs(TfLiteInvoke.BoolMarshalType)]
            bool enable);

        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void InterpreterSetNumThreads(IntPtr interpreter, int numThreads);

        [DllImport(TfliteDll, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern Status InterpreterModifyGraphWithDelegate(IntPtr interpreter, IntPtr delegatePtr);
    }
}
