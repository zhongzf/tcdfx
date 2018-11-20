﻿/***************************************************************************************************
 * FileName:             NativeComponent.cs
 * Date:                 20180918
 * Copyright:            Copyright © 2017-2018 Thomas Corwin, et al. All Rights Reserved.
 * License:              https://github.com/tacdevel/tcdfx/blob/master/LICENSE.md
 **************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TCD.InteropServices
{
    /// <summary>
    /// The base implementation for an object that has a native handle represented as an <see cref="IntPtr"/> object.
    /// </summary>
    public abstract class NativeComponent : Disposable, IEquatable<NativeComponent>
    {
        private static List<NativeComponent> componentCache = new List<NativeComponent>();
        private static List<IntPtr> handleCache = new List<IntPtr>();
        private readonly int index;

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeComponent"/> class with the specified handle.
        /// </summary>
        /// <param name="handle"></param>
        protected NativeComponent(IntPtr handle)
        {
            index = componentCache.Count;
            Handle = handle;
        }

        /// <summary>
        /// Gets the handle for this <see cref="NativeComponent"/>.
        /// </summary>
        public IntPtr Handle
        {
            get => handleCache[index];
            protected internal set
            {
                if (index > 0)
                    if (componentCache.Contains(this) || handleCache.Contains(value))
                        throw new DuplicateHandleException();

                componentCache.Add(this);
                handleCache.Add(value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="NativeComponent"/> is invalid.
        /// </summary>
        public abstract bool IsInvalid { get; }

        /// <summary>
        /// Initializes this <see cref="NativeComponent"/>.
        /// </summary>
        protected virtual void InitializeComponent() { }

        /// <summary>
        /// Initializes this <see cref="NativeComponent"/> object's events.
        /// </summary>
        protected virtual void InitializeEvents() { }

        /// <summary>
        /// Indicates whether this <see cref="NativeComponent"/> and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with this <see cref="NativeComponent"/>.</param>
        /// <returns>true if obj and this <see cref="NativeComponent"/> are the same type and represent the same value; otherwise, false.</returns>
        public override bool Equals(object obj) => !(obj is NativeComponent) ? false : Equals((NativeComponent)obj);

        /// <summary>
        /// Indicates whether this and another <see cref="NativeComponent"/> object are equal.
        /// </summary>
        /// <param name="component">The component to compare with this <see cref="NativeComponent"/>.</param>
        /// <returns>true if obj and this <see cref="NativeComponent"/> represent the same value; otherwise, false.</returns>
        public bool Equals(NativeComponent component) => index == component.index;

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for this <see cref="NativeComponent"/>.</returns>
        public override int GetHashCode() => unchecked(this.GenerateHashCode(Handle));

        /// <summary>
        /// Returns a string that represents this <see cref="NativeComponent"/>.
        /// </summary>
        /// <returns>A string that represents this <see cref="NativeComponent"/></returns>
        public override string ToString() => handleCache[index].ToInt64().ToString();

        /// <summary>
        /// Performs tasks associated with releasing unmanaged resources.
        /// </summary>
        protected override void ReleaseUnmanagedResources() { }

        /// <summary>
        /// Performs tasks associated with releasing managed resources.
        /// </summary>
        protected override void ReleaseManagedResources()
        {
            if (!IsInvalid && componentCache.Contains(this))
            {
                componentCache.RemoveAt(index);
                handleCache[index] = IntPtr.Zero;
                handleCache.RemoveAt(index);
            }
        }
    }

    /// <summary>
    /// The base implementation for an object that has a native handle represented as the
    /// specified type of <see cref="SafeHandle"/>.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="SafeHandle"/>.</typeparam>
    public abstract class NativeComponent<T> : Disposable, IEquatable<NativeComponent<T>>
        where T : SafeHandle
    {
        private static readonly List<NativeComponent<T>> componentCache = new List<NativeComponent<T>>();
        private static readonly List<T> handleCache = new List<T>();
        private readonly int index;

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeComponent{T}"/> class with the specified handle.
        /// </summary>
        /// <param name="handle"></param>
        protected NativeComponent(T handle)
        {
            index = componentCache.Count;
            Handle = handle;
        }

        /// <summary>
        /// Gets the handle for this <see cref="NativeComponent{T}"/>.
        /// </summary>
        public T Handle
        {
            get => handleCache[index];
            protected internal set
            {
                if (index > 0)
                    if (componentCache.Contains(this) || handleCache.Contains(value))
                        throw new DuplicateHandleException();

                if (value.IsInvalid || value.IsClosed)
                    throw new InvalidHandleException();

                componentCache.Add(this);
                handleCache.Add(value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="NativeComponent{T}"/> is invalid.
        /// </summary>
        public bool IsInvalid => Handle.IsInvalid || Handle.IsClosed;

        /// <summary>
        /// Initializes this <see cref="NativeComponent{T}"/>.
        /// </summary>
        protected virtual void InitializeComponent() { }

        /// <summary>
        /// Initializes this <see cref="NativeComponent{T}"/> object's events.
        /// </summary>
        protected virtual void InitializeEvents() { }

        /// <summary>
        /// Indicates whether this <see cref="NativeComponent{T}"/> and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with this <see cref="NativeComponent{T}"/>.</param>
        /// <returns>true if obj and this <see cref="NativeComponent{T}"/> are the same type and represent the same value; otherwise, false.</returns>
        public override bool Equals(object obj) => !(obj is NativeComponent<T>) ? false : Equals((NativeComponent<T>)obj);

        /// <summary>
        /// Indicates whether this and another <see cref="NativeComponent{T}"/> object are equal.
        /// </summary>
        /// <param name="component">The component to compare with this <see cref="NativeComponent{T}"/>.</param>
        /// <returns>true if obj and this <see cref="NativeComponent{T}"/> represent the same value; otherwise, false.</returns>
        public bool Equals(NativeComponent<T> component) => index == component.index;

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for this <see cref="NativeComponent{T}"/>.</returns>
        public override int GetHashCode() => unchecked(this.GenerateHashCode(Handle));

        /// <summary>
        /// Returns a string that represents this <see cref="NativeComponent{T}"/>.
        /// </summary>
        /// <returns>A string that represents this <see cref="NativeComponent{T}"/></returns>
        public override string ToString() => handleCache[index].DangerousGetHandle().ToInt64().ToString();

        /// <summary>
        /// Performs tasks associated with releasing unmanaged resources.
        /// </summary>
        protected override void ReleaseUnmanagedResources() { }

        /// <summary>
        /// Performs tasks associated with releasing managed resources.
        /// </summary>
        protected override void ReleaseManagedResources()
        {
            if (!IsInvalid && componentCache.Contains(this))
            {
                componentCache.RemoveAt(index);
                handleCache[index].Dispose();
                handleCache.RemoveAt(index);
            }
        }
    }
}