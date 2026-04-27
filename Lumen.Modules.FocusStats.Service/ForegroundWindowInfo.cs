using System.Runtime.InteropServices;
using System.Text;

namespace Lumen.Modules.FocusStats.Service {
    public static class ForegroundWindowInfo {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool QueryFullProcessImageName(IntPtr hProcess, uint dwFlags, [Out, MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpExeName, ref uint lpdwSize);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool CloseHandle(IntPtr handle);

        private const int PROCESS_QUERY_INFORMATION = 0x0400;

        public static (string WindowTitle, string ExePath) GetFocusedWindowInfo<T>(ILogger<T> logger) {
            IntPtr hwnd = GetForegroundWindow();

            if (hwnd == IntPtr.Zero)
                return (string.Empty, string.Empty);

            // Get window title
            int length = GetWindowTextLength(hwnd);
            StringBuilder titleBuilder = new(length + 1);
            GetWindowText(hwnd, titleBuilder, titleBuilder.Capacity);
            string windowTitle = titleBuilder.ToString();

            // Get process ID
            GetWindowThreadProcessId(hwnd, out uint processId);

            nint hProc = OpenProcess(PROCESS_QUERY_INFORMATION, false, processId);
            uint size = 1024;
            StringBuilder exeBuilder = new((int)size);
            if (!QueryFullProcessImageName(hProc, 0, exeBuilder, ref size)) {
                CloseHandle(hProc);
            }

            var exePath = exeBuilder.ToString();

            return (windowTitle, exePath);
        }
    }
}
