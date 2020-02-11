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

namespace Capnode.TFLite
{
    /// <summary>
    /// An abstract class that wrap around a disposable object
    /// </summary>
    public abstract class DisposableObject : IDisposable
    {
        ///<summary> Track whether Dispose has been called. </summary>
        private bool _disposed;

        /// <summary>
        /// The dispose function that implements IDisposable interface
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Returns true if the object has been disposed.
        /// </summary>
        public bool Disposed
        {
            get { return _disposed; }
        }

        ///<summary> 
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// If disposing equals true, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources
        /// can be disposed.
        /// If disposing equals false, the method has been called by the
        /// runtime from inside the finalizer and you should not reference
        /// other objects. Only unmanaged resources can be disposed.
        ///</summary>
        /// <param name="disposing">
        /// If disposing equals false, the method has been called by the
        /// runtime from inside the finalizer and you should not reference
        /// other objects. Only unmanaged resources can be disposed.
        /// </param>
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                _disposed = true;

                // If disposing equals true, release all managed resources as well
                if (disposing)
                {
                    ReleaseManagedResources();
                }

                //release unmanaged resource.
                DisposeObject();
            }
        }

        /// <summary>
        /// Release the managed resources. This function will be called during the disposal of the current object.
        /// override ride this function if you need to call the Dispose() function on any managed IDisposable object created by the current object
        /// </summary>
        protected virtual void ReleaseManagedResources()
        {
        }

        /// <summary>
        /// Release the unmanaged resources
        /// </summary>
        protected abstract void DisposeObject();

        /// <summary>
        /// Destructor
        /// </summary>
        ~DisposableObject()
        {
            Dispose(false);
        }
    }
}
