/*
 * trayicon-uia.cs
 * Copyright 2018 Raising the Floor - International
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

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace TrayIconApplication
{
    class TrayIconUIA
    {
        private const int SWP_NOOWNERZORDER = 0x200;
        private const int SWP_NOACTIVATE = 0x10;
        private const int SWP_NOSIZE = 0x1;
        private const int SWP_NOZORDER = 0x4;
        private const int SWP_HIDEWINDOW = 0x80;
        private const uint WM_CLOSE = 0x10;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(WindowEnumProc lpEnumFunc, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern bool EnumChildWindows(IntPtr hWndParent, WindowEnumProc lpEnumFunc, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        public delegate bool WindowEnumProc(IntPtr hwnd, IntPtr lparam);

        private static Process GetWindowProcess(IntPtr hwnd)
        {
            int pid;
            GetWindowThreadProcessId(hwnd, out pid);
            var p = Process.GetProcessById(pid);
            return p;
        }

        /// <summary>
        /// Gets the UWP process that owns the given window.
        /// 
        /// If the process owning the window is ApplicationFrameHost, then return the process that
        /// owns a child window with a class name of "Windows.UI.Core.CoreWindow".
        /// </summary>
        /// <param name="hwnd">The window to check.</param>
        /// <returns>The process that owns the Window.</returns>
        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
        private static Process GetUwpWindowProcess(IntPtr hwnd)
        {
            int n = 0;

            Process process = GetWindowProcess(hwnd);

            if (process != null && process.ProcessName == "ApplicationFrameHost")
            {
                Process uwpProcess = null;
                EnumChildWindows(hwnd, (hwndChild, lp) =>
                {
                    StringBuilder cls = new StringBuilder(255);
                    GetClassName(hwndChild, cls, cls.Capacity);
                    if (cls.ToString() == "Windows.UI.Core.CoreWindow")
                    {
                        Process otherProcess = GetWindowProcess(hwndChild);
                        if (otherProcess.Id != process.Id)
                        {
                            uwpProcess = otherProcess;
                        }
                    }
                    return uwpProcess == null;
                }, IntPtr.Zero);

                return uwpProcess;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Finds a Window of a UWP process.
        /// </summary>
        /// <param name="processName">The process name.</param>
        /// <returns>The window handle, or 0 if not found.</returns>
        private static IntPtr FindWindowByProcess(string processName)
        {
            IntPtr result = IntPtr.Zero;

            EnumWindows((hwnd, lp) =>
            {
                Process process = GetUwpWindowProcess(hwnd);

                if (process != null && process.ProcessName == processName)
                {
                    result = hwnd;
                    return false;
                }
                else
                {
                    return true;
                }
            }, IntPtr.Zero);

            return result;
        }

        public static void ToggleTrayIcon(string appToLock)
        {
            Console.WriteLine(":: Starting ms-settings.");
            Process.Start("ms-settings:taskbar");


            // Wait for the window to appear.
            IntPtr hwnd = IntPtr.Zero;
            for (int n = 0; n < 50; n++)
            {
                hwnd = FindWindowByProcess("SystemSettings");

                if (hwnd != IntPtr.Zero)
                {
                    Console.WriteLine("found settings window");
                    break;
                }
                Thread.Sleep(100);
            }

            RECT windowRect = new RECT();
            if (hwnd != IntPtr.Zero)
            {
                // Move the window off screen.
                GetWindowRect(hwnd, out windowRect);
                SetWindowPos(hwnd, IntPtr.Zero, -windowRect.Right, windowRect.Top, 0, 0, SWP_NOOWNERZORDER | SWP_NOACTIVATE | SWP_NOSIZE | SWP_NOZORDER);
            }

            PerformUIA(appToLock);

            if (hwnd != IntPtr.Zero)
            {
                // Put the window back, and hide it.
                SetWindowPos(hwnd, IntPtr.Zero, windowRect.Left, windowRect.Top, 0, 0, SWP_NOOWNERZORDER | SWP_NOACTIVATE | SWP_NOSIZE | SWP_NOZORDER | SWP_HIDEWINDOW);
                SendMessage(hwnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }
        }

        /// <summary>Toggles the tray icon.</summary>
        /// <param name="appToLock">The name of the icon.</param>
        private static void PerformUIA(string appToLock) { 

            Console.WriteLine(":: Initializing UI Automation.");

            // The first step in calling UIA, is getting a CUIAutomation object.
            var _automation = new CUIAutomation();

            // The properties are used from the actual constant values here:
            // "https://msdn.microsoft.com/en-us/library/windows/desktop/ee684017(v=vs.85)."
            int _propertyName = 30005;
            int _propertyAutomationId = 30011;
            int _propertyClassName = 30012;

            // The patterns are used from the actual constant values here:
            // "https://msdn.microsoft.com/en-us/library/windows/desktop/ee671195(v=vs.85).aspx"
            int _invokePattern = 10000;
            int _tooglePattern = 10015;

            Console.WriteLine(":: Getting root node.");
            // The first step in calling UIA, is getting a CUIAutomation object.
            IUIAutomationElement rootElement = _automation.GetRootElement();

            IUIAutomationCondition settingsWindow =
                _automation.CreatePropertyCondition(
                    _propertyAutomationId,
                    "Settings"
                );

            IUIAutomationCondition waitSettingsOpenning =
                _automation.CreatePropertyCondition(
                    _propertyAutomationId,
                    "SystemSettings_Taskbar_Lock_ToggleSwitch"
                );

            Console.WriteLine(":: Waiting for 'ms-settings' app to be ready.");
            IUIAutomationElement settingsOpennedMark = rootElement.FindFirst(TreeScope.TreeScope_Subtree, waitSettingsOpenning);
            while (settingsOpennedMark == null)
            {
                Thread.Sleep(200);
                settingsOpennedMark = rootElement.FindFirst(TreeScope.TreeScope_Subtree, waitSettingsOpenning);
            }

            var p = settingsOpennedMark.GetCachedParent();

            Console.WriteLine(":: 'ms-settings' app oppened sucessfully.");

            // Triggers the first link to the list of possible applications icons.
            // SystemSettings_Taskbar_SelectIconsToAppearOnTaskbar_HyperlinkButton

            IUIAutomationCondition searchTaskBarIconsLink =
                _automation.CreatePropertyCondition(
                    _propertyAutomationId,
                    "SystemSettings_Taskbar_SelectIconsToAppearOnTaskbar_HyperlinkButton"
                );

            Console.WriteLine(":: Openning 'ms-settings' app section with icon toogling.");
            IUIAutomationElement selectIconsLink = rootElement.FindFirst(TreeScope.TreeScope_Subtree, searchTaskBarIconsLink);
            if (selectIconsLink == null)
            {
                Console.WriteLine("Error: No link to icon section found.");
                return;
            }
            var invokeSelecIcons = (IUIAutomationInvokePattern)selectIconsLink.GetCurrentPattern(_invokePattern);
            invokeSelecIcons.Invoke();

            IUIAutomationCondition changedToSelectIconMode =
                _automation.CreatePropertyCondition(
                    _propertyAutomationId,
                    "SystemSettings_Notifications_ShowIconsOnTaskbar_ToggleSwitch"
                );

            Console.WriteLine(":: Waiting for 'ms-settings' to be in icon toggling section.");
            var markFound = rootElement.FindFirst(TreeScope.TreeScope_Subtree, changedToSelectIconMode);
            while (markFound == null)
            {
                Thread.Sleep(1000);
                markFound = rootElement.FindFirst(TreeScope.TreeScope_Subtree, changedToSelectIconMode);
            }

            IUIAutomationCondition searchGPIIAppInIconListName =
                _automation.CreatePropertyCondition(
                    _propertyName,
                    appToLock
                );

            IUIAutomationCondition searchGPIIAppInIconListClass =
                _automation.CreatePropertyCondition(
                    _propertyClassName,
                    "ToggleSwitch"
                );

            var searchGPIITriggerButton =
                _automation.CreateAndCondition(searchGPIIAppInIconListName, searchGPIIAppInIconListClass);

            Console.WriteLine(":: Finding GPII app icon switch.");
            Thread.Sleep(100);
            var gpiiIconLockButton = rootElement.FindFirst(TreeScope.TreeScope_Subtree, searchGPIITriggerButton);
            if (gpiiIconLockButton == null)
            {
                Console.WriteLine("Error: No application with specified name found.");
                Console.ReadKey();
                return;
            }
            var triggerLockButton = (IUIAutomationTogglePattern)gpiiIconLockButton.GetCurrentPattern(_tooglePattern);
            Console.WriteLine(":: Toogling GPII app icon switch.");
            triggerLockButton.Toggle();
        }
    }
}
