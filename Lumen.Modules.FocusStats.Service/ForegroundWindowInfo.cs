using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Lumen.Modules.FocusStats.Service {
    public class ForegroundWindowInfo {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("psapi.dll")]
        private static extern bool GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule,
            [Out] StringBuilder lpBaseName, [In][MarshalAs(UnmanagedType.U4)] int nSize);

        private const int PROCESS_QUERY_INFORMATION = 0x0400;
        private const int PROCESS_VM_READ = 0x0010;

        public static (string WindowTitle, string ExePath) GetFocusedWindowInfo() {
            IntPtr hwnd = GetForegroundWindow();

            if (hwnd == IntPtr.Zero)
                return (string.Empty, string.Empty);

            // Get window title
            int length = GetWindowTextLength(hwnd);
            StringBuilder titleBuilder = new StringBuilder(length + 1);
            GetWindowText(hwnd, titleBuilder, titleBuilder.Capacity);
            string windowTitle = titleBuilder.ToString();

            // Get process ID
            GetWindowThreadProcessId(hwnd, out uint processId);

            // Get executable path
            string exePath = string.Empty;
            try {
                using (Process process = Process.GetProcessById((int)processId)) {
                    exePath = process.MainModule?.FileName ?? string.Empty;
                }
            } catch (Exception) {
                // May fail if process is protected or doesn't exist anymore
            }

            return (windowTitle, exePath);
        }
    }
}
