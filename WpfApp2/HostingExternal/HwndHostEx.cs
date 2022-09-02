using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace WpfApp2.HostingExternal
{
    // based on https://stackoverflow.com/q/30186930/426315
    public class HwndHostEx : HwndHost
    {
        private readonly IntPtr _childHandle;

        public HwndHostEx(IntPtr handle)
        {
            _childHandle = handle;
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            var childRef = new HandleRef();

            if (_childHandle != IntPtr.Zero)
            {
                var childStyle = (IntPtr)(Win32API.WindowStyles.WS_CHILD |
                                          // Child window should be have a thin-line border
                                          Win32API.WindowStyles.WS_BORDER |
                                          // the parent cannot draw over the child's area. this is needed to avoid refresh issues
                                          Win32API.WindowStyles.WS_CLIPCHILDREN |
                                          Win32API.WindowStyles.WS_VISIBLE |
                                          Win32API.WindowStyles.WS_MAXIMIZE);

                childRef = new HandleRef(this, _childHandle);
                Win32API.SetWindowLongPtr(childRef, Win32API.GWL_STYLE, childStyle);
                Win32API.SetParent(_childHandle, hwndParent.Handle);
            }

            return childRef;
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
        }
    }
}
