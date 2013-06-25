﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Dev2.Activities
{
    public class ModalChecker
    {
        public static Boolean IsWaitingForUserInput(Process process)
        {
            if (process == null)
                throw new Exception("No process found matching the search criteria");
            // for thread safety
            if(process.HasExited) return false;
            ModalChecker checker = new ModalChecker(process);
            return checker.WaitingForUserInput;
        }

        #region Native Windows Stuff
        private const int WS_EX_DLGMODALFRAME = 0x00000001;
        private const int GWL_EXSTYLE = (-20);
        public delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);
        private delegate int EnumWindowsProc(IntPtr hWnd, int lParam);
        [DllImport("user32")]
        private extern static int EnumWindows(EnumWindowsProc lpEnumFunc, int lParam);
        [DllImport("user32", CharSet = CharSet.Auto)]
        private extern static uint GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32")]
        private extern static uint GetWindowThreadProcessId(IntPtr hWnd, out IntPtr lpdwProcessId);
        [DllImport("user32")]
        static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);
        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr handle);

        [DllImport("user32", CharSet = CharSet.Unicode)]
        static extern int GetWindowText(
            IntPtr handle,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder caption,
            int count);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetWindowTextLength(IntPtr handle);
        #endregion

        private Process _process;
        private Boolean _waiting;

        private ModalChecker(Process process)
        {
            _process = process;
            _waiting = false;
        }

        private Boolean WaitingForUserInput
        {
            get
            {
                WindowEnum(_process.MainWindowHandle,0);
                if(!_waiting)
                {
                    _waiting = ThreadWindows(_process.MainWindowHandle);
                }
                return _waiting;
            }            
        }

        private static bool ThreadWindows(IntPtr handle)
        {
            if(IsWindowVisible(handle))
            {
                var length = GetWindowTextLength(handle);
                var caption = new StringBuilder(length + 1);
                GetWindowText(handle, caption, caption.Capacity);
                return true;
            }
            return false;

        }

        private int WindowEnum(IntPtr hWnd, int lParam)
        {
//            if (hWnd == _process.MainWindowHandle)
//                return 1;
            IntPtr processId;
          
            GetWindowThreadProcessId(hWnd, out processId);
            if (processId.ToInt32() != _process.Id)
                return 1;
            uint style = GetWindowLong(hWnd, GWL_EXSTYLE);
            if ((style & WS_EX_DLGMODALFRAME) != 0)
            {
                _waiting = true;
                return 0; // stop searching further
            }
            return 1;
        }
    }
}