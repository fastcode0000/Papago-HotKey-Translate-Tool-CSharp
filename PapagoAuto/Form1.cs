using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PapagoAuto
{
    public partial class Form1 : Form
    {
        private bool bAltAndNum;
        private bool bAltOrNum;
        event KeyboardHooker.HookedKeyboardUserEventHandler HookedKeyboardNofity;

        public Form1()
        {
            InitializeComponent();
        }
        private long OnEventProcess(int iKeyWhatHappened, int vkCode)
        {
            long lResult = 0;


            if (vkCode == 49 && iKeyWhatHappened == 32) //ALT + 1
            {
                bAltAndNum = true;
                bAltOrNum = false;
                lResult = 0;
                textBox1.Text = "ALT + 1이 눌렸습니다.";
            }
            else if (bAltAndNum && iKeyWhatHappened == 128)
            {
                bAltAndNum = false;
                bAltOrNum = true;
                lResult = 0;
                textBox1.Text += Environment.NewLine + "1은 눌려있고 ALT가 떨어졌다.";
            }
            else if (bAltAndNum && vkCode == 49)
            {
                bAltAndNum = false;
                bAltOrNum = true;
                lResult = 0;
                textBox1.Text += Environment.NewLine + "ALT는 눌려있고 1이 떨어졌다.";
            }
            else if (!bAltAndNum && bAltOrNum && (vkCode == 49 || vkCode == 164))
            {
                bAltOrNum = false;
                lResult = 0;
                textBox1.Text += Environment.NewLine + "키 조합이 완료되었다.";
            }
            else
            {
                bAltAndNum = false;
                bAltOrNum = false;
                lResult = 0;
            }


            return lResult;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            HookedKeyboardNofity += new KeyboardHooker.HookedKeyboardUserEventHandler(OnEventProcess);
            KeyboardHooker.Hook(HookedKeyboardNofity);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            KeyboardHooker.UnHook();
        }
    }
}
