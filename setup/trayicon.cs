/*
 * trayicon.cs
 * This gets invoked by the installer to make the GPII tray icon remain visible.
 * Icons can't be modified until they're created by the application, so it will keep trying for 5 minutes.
 *
 * This must be built targetting the "Any CPU" platform in order for it to work on both 32 and 64-bit systems
 * in order for it to run natively and access Explorer.
 *
 * Copyright 2017 Raising the Floor - International
 *
 * Licensed under the New BSD license. You may not use this file except in
 * compliance with this License.
 *
 * The R&D leading to these results received funding from the
 * Department of Education - Grant H421A150005 (GPII-APCP). However,
 * these results do not necessarily represent the policy of the
 * Department of Education, and you should not assume endorsement by the
 * Federal Government.
 *
 * You may obtain a copy of the License at
 * https://github.com/GPII/universal/blob/master/LICENSE.txt
 */

namespace TrayIconApplication
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Runtime.InteropServices;
    using Microsoft.Win32;
    using System.Collections.Generic;

    public class TrayIcon
    {
        static void Usage()
        {
            Console.WriteLine("\nShow or hide notification icons for app.exe");
            Console.WriteLine("Usage: {0} [-show|-hide] [-nowait] app.exe Icon", Process.GetCurrentProcess().ProcessName);
            Console.WriteLine("   app.exe  The executable name that provides the icon.");
            Console.WriteLine("   Icon     The icon name.");
            Console.WriteLine("   -show    Keep the icon visible (default).");
            Console.WriteLine("   -hide    Hide the icon.");
            Console.WriteLine("   -nowait  Quit immediately if the icon hasn't been registered,");
            Console.WriteLine("             otherwise keep retrying for 5 minutes.");
            Console.Write("\nPress a key");
            Console.ReadKey();
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Usage();
                return;
            }

            // If this process created a new console window, then hide it.
            bool newConsole = Console.CursorLeft == 0 && Console.CursorTop == 0;
            if (newConsole)
            {
                ShowWindow(GetConsoleWindow(), 0);
            }

            bool show = true;
            bool wait = true;

            while (args[0][0] == '-')
            {
                switch (args[0])
                {
                    case "-hide":
                        show = false;
                        break;
                    case "-show":
                        show = true;
                        break;
                    case "-nowait":
                        show = true;
                        break;
                    default:
                        Usage();
                        return;
                }

                args = args.Skip(1).ToArray();
            }

            if (args.Length < 2)
            {
                Usage();
                return;
            }

            string exe = args[0].ToLowerInvariant();
            string iconName = string.Join(" ", args.Skip(1).ToArray());

            // Wait for the process to start. The icon needs to be shown in order to configure it.
            bool running = false;
            int attempts = 100;

            bool? iconState = null;
            do
            {
                iconState = TrayIcon.SetPreference(exe, show, true);

                if (iconState == false && IsWindows10)
                {
                    // See if it worked
                    iconState = TrayIcon.SetPreference(exe, show, false);
                    if (iconState == false)
                    {
                        TrayIconUIA.ToggleTrayIcon(iconName);
                        break;
                    }
                }
                Thread.Sleep(1000);
            } while (iconState == null && --attempts > 0);

        }

        /// <summary>
        /// Check if the newer version of ITrayNotify should be used, ITrayNotifyNew, which is available
        /// to Windows 8 or above. Otherwise (for Windows 7) ITrayNotifyOld should be used.
        /// </summary>
        private static bool UseNew
        {
            get { return Environment.OSVersion.Version >= new Version(6, 2); }
        }

        private static bool IsWindows10
        {
            get
            {
                return Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows NT\CurrentVersion")
                    .GetValue("ProductName").ToString().Contains("Windows 10");
            }
        }

        /// <summary>
        /// Sets the visibility preference of an item.
        /// </summary>
        /// <param name="exeName">The executable name.</param>
        /// <param name="alwaysShow">true to always show the icon.</param>
        /// <param name="update">true to apply the setting.</param>
        /// <returns>
        /// true a matching icon was already in the desired state, false if not, or null if there is no icon.
        /// </returns>
        public static bool? SetPreference(string exeName, bool alwaysShow, bool update)
        {
            bool? result = null;
            CTrayNotify trayNotify = null;
            INotificationCB callback = null;
            Thread t = null;
            try
            {
                uint newValue = (bool)alwaysShow ? NOTIFYITEM.AlwaysShow : NOTIFYITEM.Hide;
                trayNotify = new CTrayNotify();
                
                callback = new NotificationCallback(item =>
                {
                    Console.WriteLine(item.pszExeName + "=" + item.dwUserPref);
                    if (item.pszExeName.IndexOf(exeName, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        NOTIFYITEM_OUT itemOut = new NOTIFYITEM_OUT(item);

                        bool alreadySet = newValue == item.dwUserPref;
                        result = alreadySet;

                        if (!alreadySet && update)
                        {
                            itemOut.dwUserPref = newValue;

                            if (TrayIcon.UseNew)
                            {
                                ((ITrayNotifyNew)trayNotify).SetPreference(itemOut);
                            }
                            else
                            {
                                ((ITrayNotifyOld)trayNotify).SetPreference(itemOut);
                            }
                        }
                    }
                });
                if (TrayIcon.UseNew)
                {
                    // Windows 8+
                    ulong handle;
                    ((ITrayNotifyNew)trayNotify).RegisterCallback(callback, out handle);
                    ((ITrayNotifyNew)trayNotify).UnregisterCallback(handle);
                }
                else
                {
                    // Windows 7
                    ((ITrayNotifyOld)trayNotify).RegisterCallback(callback);
                    ((ITrayNotifyOld)trayNotify).RegisterCallback(null);
                }
            }
            finally
            {
                if (trayNotify != null)
                {
                    //Marshal.ReleaseComObject(trayNotify);
                    //trayNotify = null;
                }
            }
            return result;
        }

        /// <summary>
        /// An icon in the notification area.
        /// </summary>
        /// <see cref="http://www.geoffchappell.com/studies/windows/shell/explorer/interfaces/notifyitem.htm"/>
        public struct NOTIFYITEM
        {
            public const uint Hide = 0;
            public const uint AlwaysHide = 1;
            public const uint AlwaysShow = 2;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszExeName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszIconText;
            public IntPtr hIcon;
            public IntPtr hWnd;
            public uint dwUserPref;
            public uint uID;
            public Guid guidItem;
            public uint uID2;
        };

        /// <summary>
        /// An icon in the notification area - this is the same as NOTIFYITEM, but the strings are pointers
        /// to an allocated block. The built-in marshalling doesn't seem to work.
        /// </summary>
        /// <see cref="http://www.geoffchappell.com/studies/windows/shell/explorer/interfaces/notifyitem.htm"/>
        public struct NOTIFYITEM_OUT
        {
            public IntPtr pszExeName;
            public IntPtr pszIconText;
            public IntPtr hIcon;
            public IntPtr hWnd;
            public uint dwUserPref;
            public uint uID;
            public Guid guidItem;
            public uint uID2;

            public NOTIFYITEM_OUT(NOTIFYITEM notifyItem)
            {
                this.pszExeName = Marshal.StringToCoTaskMemAuto(notifyItem.pszExeName);
                this.pszIconText = Marshal.StringToCoTaskMemAuto(notifyItem.pszIconText);
                this.hIcon = notifyItem.hIcon;
                this.hWnd = notifyItem.hWnd;
                this.dwUserPref = notifyItem.dwUserPref;
                this.uID = notifyItem.uID;
                this.guidItem = notifyItem.guidItem;
                //this.guidItem = IntPtr.Zero;
                this.uID2 = notifyItem.uID2;
            }
        };

        /// <summary>
        /// Provides a callback function for the tray icons.
        /// </summary>
        /// <see cref="http://www.geoffchappell.com/studies/windows/shell/explorer/interfaces/inotificationcb/index.htm"/>
        [ComImport, Guid("D782CCBA-AFB0-43F1-94DB-FDA3779EACCB"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface INotificationCB
        {
            void Notify(uint nEvent, [In] ref NOTIFYITEM notifyItem);
        }

        /// <summary>
        /// This interface is implemented by EXPLORER to support the notification area of the taskbar.
        /// For Windows 7
        /// </summary>
        /// <see cref="http://www.geoffchappell.com/studies/windows/shell/explorer/interfaces/itraynotify/index.htm"/>
        [ComImport, Guid("FB852B2C-6BAD-4605-9551-F15F87830935"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface ITrayNotifyOld
        {
            void RegisterCallback([MarshalAs(UnmanagedType.Interface)] INotificationCB pcb);
            void SetPreference([In] NOTIFYITEM_OUT notifyItem);
            void EnableAutoTray(bool enable);
        }

        /// <summary>
        /// This interface is implemented by EXPLORER to support the notification area of the taskbar.
        /// For Windows 8 and above.
        /// </summary>
        /// <see cref="http://www.geoffchappell.com/studies/windows/shell/explorer/interfaces/itraynotify/index.htm"/>
        /// <see cref="https://hianz.wordpress.com/2013/09/03/new-windows-tray-notification-manager-is-here/"/>
        [ComImport, Guid("D133CE13-3537-48BA-93A7-AFCD5D2053B4"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface ITrayNotifyNew
        {
            void RegisterCallback([MarshalAs(UnmanagedType.Interface)] INotificationCB pcb, out ulong handle);
            void UnregisterCallback(ulong handle);
            void SetPreference([In] ref NOTIFYITEM_OUT notifyItem);
            void EnableAutoTray(bool enable);
            void DoAction(bool enable);
        }

        [ComImport, Guid("25DEAD04-1EAC-4911-9E3A-AD0A4AB560FD")]
        class CTrayNotify { }

        /// <summary>
        /// An implementation of INotificationCB that calls another callback when Notify is invoked.
        /// </summary>
        class NotificationCallback : INotificationCB
        {
            private Action<NOTIFYITEM> callback;

            /// <summary>Creates a new instance of NotificationCallback.</summary>
            /// <param name="callback">Invoked when this callback is invoked.</param>
            public NotificationCallback(Action<NOTIFYITEM> callback)
            {
                this.callback = callback;
            }

            /// <summary>
            /// Called by explorer when RegisterCallback has been called, for each notification item.
            /// </summary>
            /// <param name="nEvent"></param>
            /// <param name="notifyItem">The item.</param>
            public void Notify([In] uint nEvent, [In] ref NOTIFYITEM notifyItem)
            {
                this.callback.Invoke(notifyItem);
            }
        }

        [DllImport("user32.dll")]
        private static extern int ShowWindow(int Handle, int showState);
        [DllImport("kernel32.dll")]
        public static extern int GetConsoleWindow();

    }
}
