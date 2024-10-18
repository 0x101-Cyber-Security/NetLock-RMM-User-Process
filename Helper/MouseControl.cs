using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Helper
{
    internal class MouseControl
    {
        // P/Invoke für SetCursorPos
        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);

        // P/Invoke für mouse_event
        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        // Konstanten für die Mausereignisse
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

        public static void MoveMouse(int x, int y)
        {
            // Setze den Mauszeiger an die angegebenen Koordinaten
            SetCursorPos(x, y);
        }

        public static void ClickMouse()
        {
            // Simuliere einen Mausklick
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0); // Linke Maustaste drücken
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);   // Linke Maustaste loslassen
        }
    }
}
