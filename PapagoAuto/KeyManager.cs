using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PapagoAuto
{
    public class KeyManager
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        public const int KEYEVENTF_KEYDOWN = 0x0000;
        public const int KEYEVENTF_KEYUP = 0x0002;
        public const int VK_CONTROL = 0x11;

        public static void PressCtrlC()
        {
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYDOWN, 0);
            keybd_event(0x43, 0, KEYEVENTF_KEYDOWN, 0);

            Thread.Sleep(50); //이거 딜레이 줘야 복사된다

            keybd_event(0x43, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);
        }

        public static void PressCtrlV()
        {
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYDOWN, 0);
            keybd_event(0x56, 0, KEYEVENTF_KEYDOWN, 0);

            Thread.Sleep(50); //이거 딜레이 줘야 복사된다

            keybd_event(0x56, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);
        }
    }
}
