using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PapagoAuto
{
    public class PapagoManger
    {
        private string m_szSessionKey;
        private bool m_bKorean;
        private string m_szSourceLang;
        private string m_szTargetLang;

        public PapagoManger(string szSessionKey)
        {
            m_szSourceLang = "ko";
            m_szTargetLang = "en";
            m_szSessionKey = szSessionKey;
        }

        public string GetLastSourceLang()
        {
            if (m_szSourceLang == "ko")
            {
                return "Korean";
            }
            else if (m_szSourceLang == "en")
            {
                return "English";
            }
            else
            {
                return "none";
            }
        }
        public string GetLastTargetLang()
        {
            if (m_szTargetLang == "ko")
            {
                return "Korean";
            }
            else if (m_szTargetLang == "en")
            {
                return "English";
            }
            else
            {
                return "none";
            }
        }


        public void SetCountry(string szText)
        {
            if (szText.Length > 0)
            {
                char firstChar = szText[0];
                if ((firstChar >= '가' && firstChar <= '힣') || (firstChar >= 'ㄱ' && firstChar <= 'ㅎ'))
                {
                    m_szSourceLang = "ko";
                    m_szTargetLang = "en";
                }
                else if ((firstChar >= 'A' && firstChar <= 'Z') || (firstChar >= 'a' && firstChar <= 'z'))
                {
                    m_szSourceLang = "en";
                    m_szTargetLang = "ko";
                }
                else
                {
                    m_szSourceLang = "ko";
                    m_szTargetLang = "en";
                }
            }
            else
            {
                m_szSourceLang = "ko";
                m_szTargetLang = "en";
            }
        }

        //https://search.naver.com/search.naver?sm=tab_hty.top&where=nexearch&ssc=tab.nx.all&query=%ED%8C%8C%ED%8C%8C%EA%B3%A0&oquery=%EB%B2%88%EC%97%AD%EA%B8%B0&tqi=iD8Ildqo1aVsss%2FEZolssssstK0-054315
        //위 URL창에서 파파고 작은 창을 통해 요청하는 API 부분을 빼온방식임.
        //개발자도구에서 Network -> Script 부분에서 가져왔음
        //세션키는 변경될 수 있으니, 키를 못가져올때마다 셀레니움을 통해 가져오는 방식도 구상중임
        //우선 사용하다가 필요하다면 만들예정

        public string GetTranslateData(string szText)
        {
            string url = $"https://m.search.naver.com/p/csearch/ocontent/util/nmtProxy.naver?_callback=window.__jindo2_callback._1657&query={szText}&passportKey={m_szSessionKey}&srcLang={m_szSourceLang}&tarLang={m_szTargetLang}";
            using (WebClient client = new WebClient())
            {
                client.Encoding = System.Text.Encoding.UTF8;
                string response = client.DownloadString(url);
                //string response = client.DownloadString(url);

                Match match = Regex.Match(response, "\"translatedText\":\"(.*?)\"");
                if (match.Success)
                {
                    string translatedText = match.Groups[1].Value;
                    return translatedText;
                }

                return "";
            }
        }
    }
}
