using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
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

                string szTitle = this.Text;
                this.Text = $"{szTitle} Processing";


                //이때 작업시작.

                //Thread.Sleep(100);


                KeyManager.PressCtrlC();

                string szData = "";
                int iAttempts = 0;
                AutoResetEvent kEvent = new AutoResetEvent(false);

                Thread kThread = new Thread(() =>
                {
                    while (iAttempts < 100)
                    {
                        if (Clipboard.ContainsText())
                        {
                            try
                            {
                                szData = Clipboard.GetText();
                                kEvent.Set();
                                break;
                            }
                            catch (Exception e)
                            {
                                Thread.Sleep(100);
                            }
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }

                        iAttempts++;
                    }
                });

                kThread.SetApartmentState(ApartmentState.STA);

                kThread.Start();

                kEvent.WaitOne();

                this.Text = szTitle;


                if (szData != null && szData != "") //여기 못들어오는 경우가 있음
                {


                    szData = szData.TrimEnd();

                    g_Papago.SetCountry(szData);
                    string szResult = g_Papago.GetTranslateData(szData);

                    szResult = szResult.TrimEnd();

                    if (szResult != null && szResult != "")
                    {
                        Clipboard.SetText(szResult);
                        KeyManager.PressCtrlV();

                        if (TransList.Items.Count > 10)
                        {
                            TransList.Items.RemoveAt(0);
                        }

                        TransList.Items.Add($"{szData} ({g_Papago.GetLastSourceLang()}) -> {szResult} ({g_Papago.GetLastTargetLang()})");
                    }
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

            string szKey = kLoader.LoadString("passportkey", "cf9caeb49d7cd3bf2b9e024ffd57dbd4250f199d"); //이 키 하루마다 바뀜.
            //일단 기본값에 넣어둠

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
