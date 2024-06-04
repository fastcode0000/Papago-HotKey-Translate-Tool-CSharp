using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PapagoAuto
{
    public class KeyboardHooker
    {
        //C# 키보드 글로벌 후킹 https://seoddong.tistory.com/39 참고

        private delegate long HookedKeyboardEventHandler(int nCode, int wParam, IntPtr lParam);


        public delegate long HookedKeyboardUserEventHandler(int iKeyWhatHappened, int vkCode);

        private const int WH_KEYBOARD_LL = 13;
        private static long m_hDllKbdHook;
        private static KBDLLHOOKSTRUCT m_KbDllHs = new KBDLLHOOKSTRUCT();
        private static IntPtr m_LastWindowHWnd;
        public static IntPtr m_CurrentWindowHWnd;

        private static HookedKeyboardEventHandler m_LlKbEh = new HookedKeyboardEventHandler(HookedKeyboardProc);

        private static HookedKeyboardUserEventHandler m_fpCallbkProc = null;

        private struct KBDLLHOOKSTRUCT
        {

            public int vkCode;
           
            public int scanCode;

            public int flags;

            public int time;

            public override string ToString()
            {
                string temp = "KBDLLHOOKSTRUCT\r\n";
                temp += "vkCode: " + vkCode.ToString() + "\r\n";
                temp += "scanCode: " + scanCode.ToString() + "\r\n";
                temp += "flags: " + flags.ToString() + "\r\n";
                temp += "time: " + time.ToString() + "\r\n";
                return temp;
            }
        }

        [DllImport(@"kernel32.dll", CharSet = CharSet.Auto)]
        private static extern void CopyMemory(ref KBDLLHOOKSTRUCT pDest, IntPtr pSource, long cb);

        [DllImport(@"user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport(@"user32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetAsyncKeyState(int vKey);

        [DllImport(@"user32.dll", CharSet = CharSet.Auto)]
        private static extern long CallNextHookEx(long hHook, long nCode, long wParam, IntPtr lParam);

        [DllImport(@"user32.dll", CharSet = CharSet.Auto)]
        private static extern long SetWindowsHookEx(int idHook, HookedKeyboardEventHandler lpfn, long hmod, int dwThreadId);

        [DllImport(@"user32.dll", CharSet = CharSet.Auto)]
        private static extern long UnhookWindowsHookEx(long hHook);


        private const int HC_ACTION = 0;
        private static long HookedKeyboardProc(int nCode, int wParam, IntPtr lParam)
        {
            long lResult = 0;

            if (nCode == HC_ACTION)
            {
                unsafe
                {
                    CopyMemory(ref m_KbDllHs, lParam, sizeof(KBDLLHOOKSTRUCT));
                }

                m_CurrentWindowHWnd = GetForegroundWindow();

                if (m_CurrentWindowHWnd != m_LastWindowHWnd)
                    m_LastWindowHWnd = m_CurrentWindowHWnd;

                if (m_fpCallbkProc != null)
                {
                    lResult = m_fpCallbkProc(m_KbDllHs.flags, m_KbDllHs.vkCode);
                }

            }
            else if (nCode < 0)
            {
                return CallNextHookEx(m_hDllKbdHook, nCode, wParam, lParam);
            }

            return lResult;
        }

        public static bool Hook(HookedKeyboardUserEventHandler callBackEventHandler)
        {
            bool bResult = true;
            m_hDllKbdHook = SetWindowsHookEx(
                (int)WH_KEYBOARD_LL,
                m_LlKbEh,
                Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]).ToInt32(),
                0);

            if (m_hDllKbdHook == 0)
            {
                bResult = false;
            }
            KeyboardHooker.m_fpCallbkProc = callBackEventHandler;

            return bResult;
        }

        public static void UnHook()
        {
            UnhookWindowsHookEx(m_hDllKbdHook);
        }

    }
}