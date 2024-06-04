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

            if (nCode == HC_ACTION) //LowLevelKeyboardProc
            {
                //visusl studio 2008 express 버전에서는 빌드 옵션에서 안전하지 않은 코드 허용에 체크
                unsafe
                {
                    //도대체 어디서 뭘 카피해놓는다는건지 이거 원..
                    CopyMemory(ref m_KbDllHs, lParam, sizeof(KBDLLHOOKSTRUCT));
                }

                //전역 후킹을 하기 위해서 현재 활성화 된 윈도우의 핸들값을 찾는다.
                //그래서 이 윈도우에서 발생하는 이벤트를 후킹해야 전역후킹이 가능해진다.
                m_CurrentWindowHWnd = GetForegroundWindow();

                //후킹하려는 윈도우의 핸들을 방금 찾아낸 핸들로 바꾼다.
                if (m_CurrentWindowHWnd != m_LastWindowHWnd)
                    m_LastWindowHWnd = m_CurrentWindowHWnd;

                // 이벤트 발생
                if (m_fpCallbkProc != null)
                {
                    lResult = m_fpCallbkProc(m_KbDllHs.flags, m_KbDllHs.vkCode);
                }

            }
            else if (nCode < 0) //나머지는 그냥 통과시킨다.
            {
                #region MSDN Documentation on return conditions
                // "If nCode is less than zero, the hook procedure must pass the message to the 
                // CallNextHookEx function without further processing and should return the value 
                // returned by CallNextHookEx. "
                // ...
                // "If nCode is greater than or equal to zero, and the hook procedure did not 
                // process the message, it is highly recommended that you call CallNextHookEx 
                // and return the value it returns;"
                #endregion
                return CallNextHookEx(m_hDllKbdHook, nCode, wParam, lParam);
            }

            //
            //lResult 값이 0이면 후킹 후 이벤트를 시스템으로 흘려보내고
            //0이 아니면 후킹도 하고 이벤트도 시스템으로 보내지 않는다.
            return lResult;
        }

        // 후킹 시작
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
            // 외부에서 KeyboardHooker의 이벤트를 받을 수 있도록 이벤트 핸들러를 할당함
            KeyboardHooker.m_fpCallbkProc = callBackEventHandler;

            return bResult;
        }

        // 후킹 중지
        public static void UnHook()
        {
            //프로그램 종료 시점에서 호출해주자.
            UnhookWindowsHookEx(m_hDllKbdHook);
        }

    }//end of class(KeyboardHooker)
}