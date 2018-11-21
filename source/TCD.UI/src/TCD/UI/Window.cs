﻿/***************************************************************************************************
 * FileName:             Window.cs
 * Date:                 20180921
 * Copyright:            Copyright © 2017-2018 Thomas Corwin, et al. All Rights Reserved.
 * License:              https://github.com/tacdevel/tcdfx/blob/master/LICENSE.md
 **************************************************************************************************/

using System;
using System.IO;
using TCD.Drawing;
using TCD.InteropServices;
using TCD.Native;
using TCD.SafeHandles;
using TCD.UI.Controls;

namespace TCD.UI
{
    /// <summary>
    /// Represents a native window that makes up an application's user interface.
    /// </summary>
    public partial class Window : Control
    {
        private bool isMargined, fullscreen, borderless = false;
        private Size size;
        private string title;

        /// <summary>
        /// Initializes a new instance of the <see cref="Window"/> class, with the options of specifying
        /// the window's width, height, title, and whether or not it has a <see cref="Controls.Containers.Menu"/>.
        /// </summary>
        /// <param name="title">The title at the top of the window.</param>
        /// <param name="width">The width of the window.</param>
        /// <param name="height">The height of the window.</param>
        /// <param name="hasMenu">Whether or not the window will have a menu.</param>
        public Window(string title = "", int width = 600, int height = 400, bool hasMenu = false) : base(new SafeControlHandle(Libui.NewWindow(title, width, height, hasMenu)))
        {
            this.title = title;
            Console.Title = title;
            size = new Size(width, height);
            HasMenu = hasMenu;
            IsMargined = isMargined;
            Fullscreen = fullscreen;
            Borderless = borderless;
            InitializeEvents();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Window"/> class, with the options of specifying
        /// the window's size, title, and whether or not it has a <see cref="Controls.Containers.Menu"/>.
        /// </summary>
        /// <param name="title">The title at the top of the window.</param>
        /// <param name="size">The size of the window.</param>
        /// <param name="hasMenu">Whether or not the window will have a menu.</param>
        public Window(string title, Size size, bool hasMenu = false) : this(title, size.Width, size.Height, hasMenu) { }

        /// <summary>
        /// Occurs when the <see cref="Window"/> is closing.
        /// </summary>
        public event NativeEventHandler<Window, CloseEventArgs> Closing;

        /// <summary>
        /// Occurs when the <see cref="Size"/> property value changes.
        /// </summary>
        public event NativeEventHandler<Window> SizeChanged;

        /// <summary>
        /// Gets whether or not this <see cref="Window"/> has a menu.
        /// </summary>
        public bool HasMenu { get; }

        /// <summary>
        /// Gets or sets the title of this <see cref="Window"/>.
        /// </summary>
        public string Title
        {
            get
            {
                if (IsInvalid) throw new InvalidHandleException();
                title = Libui.WindowTitle(Handle);
                return title;
            }
            set
            {
                if (title == value) return;
                if (IsInvalid) throw new InvalidHandleException();
                Libui.WindowSetTitle(Handle, value);
                title = value;
            }
        }

        /// <summary>
        /// Gets or sets the content size of this <see cref="Window"/>.
        /// </summary>
        public Size Size
        {
            get
            {
                if (IsInvalid) throw new InvalidHandleException();
                Libui.WindowContentSize(Handle, out int w, out int h);
                size = new Size(w, h);
                return size;
            }
            set
            {
                if (size == value) return;
                if (IsInvalid) throw new InvalidHandleException();
                Libui.WindowSetContentSize(Handle, value.Width, value.Height);
                size = value;
            }
        }

        /// <summary>
        /// Gets the content width of this <see cref="Window"/>.
        /// </summary>
        public int Width => Size.Width;

        /// <summary>
        /// Gets the content height of this <see cref="Window"/>.
        /// </summary>
        public int Height => Size.Height;

        /// <summary>
        /// Gets or sets whether or not this <see cref="Window"/> fills the entire screen.
        /// </summary>
        public bool Fullscreen
        {
            get
            {
                if (IsInvalid) throw new InvalidHandleException();
                fullscreen = Libui.WindowFullscreen(Handle);
                return fullscreen;
            }
            set
            {
                if (fullscreen == value) return;
                if (IsInvalid) throw new InvalidHandleException();
                Libui.WindowSetFullscreen(Handle, value);
                fullscreen = value;
            }
        }

        /// <summary>
        /// Gets or sets whether or not this <see cref="Window"/> has borders.
        /// </summary>
        public bool Borderless
        {
            get
            {
                if (IsInvalid) throw new InvalidHandleException();
                borderless = Libui.WindowBorderless(Handle);
                return borderless;
            }
            set
            {
                if (borderless == value) return;
                if (IsInvalid) throw new InvalidHandleException();
                Libui.WindowSetBorderless(Handle, value);
                borderless = value;
            }
        }

        /// <summary>
        /// Sets the child <see cref="Control"/> of this <see cref="Window"/>.
        /// </summary>
        public Control Child
        {
            set
            {
                if (Handle != null)
                {
                    if (value == null) throw new ArgumentNullException(nameof(value));
                    if (IsInvalid) throw new InvalidHandleException();
                    Libui.WindowSetChild(Handle, value.Handle);
                }
            }
        }

        /// <summary>
        /// Gets or sets whether this <see cref="Window"/> has margins between its child control and its border.
        /// </summary>
        public bool IsMargined
        {
            get
            {
                if (IsInvalid) throw new InvalidHandleException();
                isMargined = Libui.WindowMargined(Handle);
                return isMargined;
            }
            set
            {
                if (isMargined == value) return;
                if (IsInvalid) throw new InvalidHandleException();
                Libui.WindowSetMargined(Handle, value);
                isMargined = value;
            }
        }

        /// <summary>
        /// Closes the <see cref="Window"/>.
        /// </summary>
        public void Close()
        {
            Hide();
            Dispose();
        }

        #region Dialogs
        /// <summary>
        /// Displays a dialog allowing a user to select a file to save to.
        /// </summary>
        /// <param name="path">The file's path selected by the user to save to.</param>
        /// <returns><see langword="true"/> if the file can be saved to, else <see langword="false"/>.</returns>
        public bool ShowSaveFileDialog(out string path) => ShowSaveFileDialog(this, out path);

        /// <summary>
        /// Displays a dialog allowing a user to select a file to save to.
        /// </summary>
        /// <param name="writeStream">The file selected by the user as a writable stream.</param>
        /// <returns><see langword="true"/> if the file can be saved to, else <see langword="false"/>.</returns>
        public bool ShowSaveFileDialog(out Stream writeStream) => ShowSaveFileDialog(this, out writeStream);

        /// <summary>
        /// Displays a dialog allowing a user to select a file to save to.
        /// </summary>
        /// <param name="path">The file's path selected by the user to save to.</param>
        /// <param name="w">The dialog's parent window.</param>
        /// <returns><see langword="true"/> if the file can be saved to, else <see langword="false"/>.</returns>
        public static bool ShowSaveFileDialog(Window w, out string path)
        {
            if (w == null) w = Application.MainWindow;
            if (w.IsInvalid) throw new InvalidHandleException();

            path = Libui.SaveFile(w.Handle);
            return string.IsNullOrEmpty(path) ? false : true;
        }

        /// <summary>
        /// Displays a dialog allowing a user to select a file to save to.
        /// </summary>
        /// <param name="writeStream">The file selected by the user as a writable stream.</param>
        /// <param name="w">The dialog's parent window.</param>
        /// <returns><see langword="true"/> if the file can be saved to, else <see langword="false"/>.</returns>
        public static bool ShowSaveFileDialog(Window w, out Stream writeStream)
        {
            if (ShowSaveFileDialog(w, out string path))
            {
                writeStream = File.OpenWrite(path);
                return true;
            }
            else
            {
                writeStream = null;
                return false;
            }
        }

        /// <summary>
        /// Displays a dialog allowing a user to select a file to open.
        /// </summary>
        /// <param name="path">The file's path selected by the user.</param>
        /// <returns><see langword="true"/> if the file exists, else <see langword="false"/>.</returns>
        public bool ShowOpenFileDialog(out string path) => ShowOpenFileDialog(this, out path);

        /// <summary>
        /// Displays a dialog allowing a user to select a file to open.
        /// </summary>
        /// <param name="readStream">The file selected by the user as a readable stream.</param>
        /// <returns><see langword="true"/> if the file exists, else <see langword="false"/>.</returns>
        public bool ShowOpenFileDialog(out Stream readStream) => ShowOpenFileDialog(this, out readStream);

        /// <summary>
        /// Displays a dialog allowing a user to select a file to open.
        /// </summary>
        /// <param name="path">The file's path selected by the user.</param>
        /// <param name="w">The dialog's parent window.</param>
        /// <returns><see langword="true"/> if the file exists, else <see langword="false"/>.</returns>
        public static bool ShowOpenFileDialog(Window w, out string path)
        {
            if (w == null) w = Application.MainWindow;
            if (w.IsInvalid) throw new InvalidHandleException();

            path = Libui.OpenFile(w.Handle);
            return string.IsNullOrEmpty(path) ? false : true;
        }

        /// <summary>
        /// Displays a dialog allowing a user to select a file to open.
        /// </summary>
        /// <param name="readStream">The file selected by the user as a readable stream.</param>
        /// <param name="w">The dialog's parent window.</param>
        /// <returns><see langword="true"/> if the file exists, else <see langword="false"/>.</returns>
        public static bool ShowOpenFileDialog(Window w, out Stream readStream)
        {
            if (ShowOpenFileDialog(w, out string path))
            {
                readStream = File.OpenRead(path);
                return true;
            }
            else
            {
                readStream = null;
                return false;
            }
        }

        /// <summary>
        /// Displays a dialog showing a message, or optionally, an error.
        /// </summary>
        /// <param name="title">The title of the message dialog.</param>
        /// <param name="description">The description of the message dialog.</param>
        /// <param name="isError">Whether the message is displayed as an error.</param>
        public void ShowMessageBox(string title, string description = null, bool isError = false) => ShowMessageBox(this, title, description, isError);

        /// <summary>
        /// Displays a dialog showing a message, or optionally, an error.
        /// </summary>
        /// <param name="w">The dialog's parent window.</param>
        /// <param name="title">The title of the message dialog.</param>
        /// <param name="description">The description of the message dialog.</param>
        /// <param name="isError">Whether the message is displayed as an error.</param>
        public static void ShowMessageBox(Window w, string title, string description = null, bool isError = false)
        {
            if (w == null) w = Application.MainWindow;
            if (w.IsInvalid) throw new InvalidHandleException();

            if (isError)
                Libui.MsgBoxError(w.Handle, title, description);
            else
                Libui.MsgBox(w.Handle, title, description);
        }
        #endregion

        /// <summary>
        /// Raises the <see cref="Closing"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The object containing the event data.</param>
        protected virtual void OnClosing(Window sender, CloseEventArgs e) => Closing?.Invoke(sender, e);

        /// <summary>
        /// Raises the <see cref="SizeChanged"/> event.
        /// </summary>
        protected virtual void OnSizeChanged(Window sender) => SizeChanged?.Invoke(sender);

        /// <summary>
        /// Initializes this <see cref="Window"/> object's events.
        /// </summary>
        protected sealed override void InitializeEvents()
        {
            if (IsInvalid) throw new InvalidHandleException();

            Libui.WindowOnClosing(Handle, (window, data) =>
            {
                CloseEventArgs e = new CloseEventArgs();
                OnClosing(this, e);
                if (e.Close)
                {
                    if (this != Application.MainWindow)
                        Close();
                    else
                        Application.Current.Shutdown();
                }
                return e.Close;
            }, IntPtr.Zero);

            Libui.WindowOnContentSizeChanged(Handle, (window, data) => OnSizeChanged(this), IntPtr.Zero);
        }
    }
}