using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Helper
{
    internal class KeyboardControl
    {
        // P/Invoke für keybd_event
        [DllImport("user32.dll", SetLastError = true)]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        // Konstanten für Keydown und Keyup
        private const uint KEYEVENTF_KEYDOWN = 0x0000;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        // Virtual-Key-Codes für spezielle Tasten
        private const byte VK_CONTROL = 0x11;
        private const byte VK_SHIFT = 0x10;
        private const byte VK_ALT = 0x12;
        private const byte VK_C = 0x43; // Beispiel für "C"
        private const byte VK_V = 0x56; // Beispiel für "V"

        public static async Task SendKey(byte vk)
        {
            try
            {
                keybd_event(vk, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
                keybd_event(vk, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send key: {ex.Message}");
            }
        }

        public static void SendCtrlC()
        {
            // Strg + C
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero); // Strg drücken
            keybd_event(VK_C, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);       // C drücken
            keybd_event(VK_C, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);         // C loslassen
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);   // Strg loslassen7
        }

        public static void SendCtrlV()
        {
            // Strg + V
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero); // Strg drücken
            keybd_event(VK_V, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);       // V drücken
            keybd_event(VK_V, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);         // V loslassen
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);   // Strg loslassen
        }

        public static void SendAltF4()
        {
            // Alt + F4
            keybd_event(VK_ALT, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);     // Alt drücken
            keybd_event(0x73, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);       // F4 drücken (VK_F4 = 0x73)
            keybd_event(0x73, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);         // F4 loslassen
            keybd_event(VK_ALT, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);       // Alt loslassen
        }
    }
}
