using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ChancellorGerath
{
	public static class ConsoleWindow
	{
		[DllImport("kernel32.dll")]
		static extern IntPtr GetConsoleWindow();

		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		const int SW_HIDE = 0;
		const int SW_SHOW = 5;

		private static IntPtr hWnd = GetConsoleWindow();

		public static void Show()
		{
			ShowWindow(hWnd, SW_SHOW);
		}
		public static void Hide()
		{
			ShowWindow(hWnd, SW_HIDE);
		}
	}
}
