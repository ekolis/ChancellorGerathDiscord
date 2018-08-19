using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ChancellorGerath
{
	public static class ConsoleWindow
	{
		[DllImport("user32.dll")]
		private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		private static IntPtr hWnd = FindWindow(null, Program.Title);

		public static void Show()
		{
			ShowWindow(hWnd, 1);
		}
		public static void Hide()
		{
			ShowWindow(hWnd, 0);
		}
	}
}
