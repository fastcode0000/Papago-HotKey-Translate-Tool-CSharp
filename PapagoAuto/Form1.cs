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

        private PapagoManger g_Papago = null;

        public Form1()
        {
            InitializeComponent();
        }
        private long OnEventProcess(int iKeyWhatHappened, int vkCode)
        {
            long lResult = 0;

            if (vkCode == '1' && iKeyWhatHappened == 32)
            {
                bAltAndNum = true;
                bAltOrNum = false;
                lResult = 0;
            }
            else if (bAltAndNum && iKeyWhatHappened == 128)
            {
                bAltAndNum = false;
                bAltOrNum = true;
                lResult = 0;
            }
            else if (bAltAndNum && vkCode == '1')
            {
                bAltAndNum = false;
                bAltOrNum = true;
                lResult = 0;
            }
            else if (!bAltAndNum && bAltOrNum && (vkCode == '1' || vkCode == 164))
            {
                bAltOrNum = false;
                lResult = 0;

                //이때 작업시작.



                KeyManager.PressCtrlC(); //현재 창에서 복사를함.

                string szData = Clipboard.GetText();
                if (szData != null && szData != "")
                {
                    g_Papago.SetCountry(szData);
                    string szResult = g_Papago.GetTranslateData(szData);
                    Clipboard.SetText(szResult);
                    KeyManager.PressCtrlV();

                    if (TransList.Items.Count > 10)
                    {
                        TransList.Items.RemoveAt(0);
                    }

                    TransList.Items.Add($"{szData} -> {szResult}");
                }
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
            INILoader kLoader = new INILoader();
            kLoader.SetFileName("C:\\Users\\Admin\\Desktop\\NaverTranslate.ini");

            kLoader.SetTitle("common");

            string szKey = kLoader.LoadString("passportkey", "");

            if (szKey == "")
            {
                MessageBox.Show("SessionKey is null","PapagoAuto");
                Environment.Exit(0);
                return;
            }

            g_Papago = new PapagoManger(szKey);
            HookedKeyboardNofity += new KeyboardHooker.HookedKeyboardUserEventHandler(OnEventProcess);
            KeyboardHooker.Hook(HookedKeyboardNofity);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            KeyboardHooker.UnHook();
        }
    }
}
