using System;
using System.Runtime.InteropServices;
using Camera2.Enums;
using JetBrains.Annotations;

namespace Camera2.Utils {
    
    [UsedImplicitly]
	internal class WinAPI {

		public static void SetCursor(WindowsCursor cursor) {
			SetCursor(LoadCursor(IntPtr.Zero, (int)cursor));
		}

		[DllImport("user32.dll", EntryPoint = "SetCursor")]
		private static extern IntPtr SetCursor(IntPtr hCursor);

		[DllImport("user32.dll", EntryPoint = "LoadCursor")]
		private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

		[DllImport("user32.dll")]
		public static extern int MessageBox(IntPtr hWnd, String text, String caption, int options);
	}
}
