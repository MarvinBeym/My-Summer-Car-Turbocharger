using System;
namespace SatsumaTurboCharger
{
    public class hallo
    {
        public hallo()
        {


        }
        public static void test()
        {
            string url = "https://api.github.com/users/DonnerPlays";
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            HttpWebRequest webRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;
            if (webRequest != null)
            {
                webRequest.Method = "GET";
                webRequest.UserAgent = "Anything";
                webRequest.ServicePoint.Expect100Continue = false;

                try
                {
                    using (StreamReader responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream()))
                    {
                        string reader = responseReader.ReadToEnd();
                        var jsonobj = JsonConvert.DeserializeObject(reader);
                    }
                }
                catch (Exception ex)
                {
                    return;
                }
            }
        }
    }

}
