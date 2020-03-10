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
   /// An Unmanaged Object is a disposable object with a Ptr property pointing to the unmanaged object
   /// </summary>
   public abstract class UnmanagedObject : DisposableObject
   {
      /// <summary>
      /// A pointer to the unmanaged object
      /// </summary>
      protected IntPtr _ptr;

      /// <summary>
      /// Pointer to the unmanaged object
      /// </summary>
      public virtual IntPtr Ptr
      {
         get
         {
            return _ptr;
         }
      }

      /// <summary>
      /// Implicit operator for IntPtr
      /// </summary>
      /// <param name="obj">The UnmanagedObject</param>
      /// <returns>The unmanaged pointer for this object</returns>
      public static implicit operator IntPtr(UnmanagedObject obj)
      {
         return obj == null ? IntPtr.Zero : obj._ptr;
      }
   }
}
