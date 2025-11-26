using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections.Specialized;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace WebApplication1.Controllers
{
    class PayTR
    {
        public string amount { get; set; }
        public string receiver { get; set; }
        public string iban { get; set; }
    }

    public class paytr_geri_donen_odemeler_hesaptan_gonder_ornekController : Controller
    {
        public ActionResult paytr_geri_donen_odemeler_hesaptan_gonder_ornek()
        {
            List<PayTR> TransferInfo = new List<PayTR>();
            PayTR info = new PayTR();
            info.amount = Convert.ToString(10 * 100); //amount 100 ile çarpılarak gönderilir.
            info.receiver = "XYZ LTD ŞTİ";
            info.iban = "TRXXXXXXXXXXXXXXXXXXXXX";
           
            TransferInfo.Add(info);

            string TransInfo = Newtonsoft.Json.JsonConvert.SerializeObject(TransferInfo);

      

            // ####################### #######################
            //
            // 
            string merchant_id = "AAAAAA";
            string merchant_key = "XXXXXXXXXXXXXXXX";
            string merchant_salt = "XXXXXXXXXXXXXXXX";
            //
            // #######################
            string TransId = "ZZZZZZZ"; 
          
            //  #######################
            string Birlestir = string.Concat(merchant_id, TransId, merchant_salt);
            HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(merchant_key));
            byte[] b = hmac.ComputeHash(Encoding.UTF8.GetBytes(Birlestir));
            string paytr_token = Convert.ToBase64String(b);

            // #######################



     

            NameValueCollection data = new NameValueCollection();
            data["trans_info"] = TransInfo;
            data["trans_id"] = TransId;
            data["paytr_token"] = paytr_token;
            data["merchant_id"] = merchant_id;
        
            //


            using (WebClient client = new WebClient())
            {
                client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                byte[] result = client.UploadValues("https://www.paytr.com/odeme/hesaptan-gonder", "POST", data);
                string ResultAuthTicket = Encoding.UTF8.GetString(result);
                dynamic json = JValue.Parse(ResultAuthTicket);


                if (json.status == "success")
                {
					//status ve trans_id içerir
                    Response.Write(json);

                }
                else
                {
                    // Hata durumu
					//status=>error
                    Response.Write("Error. reason:" + json.err_no + "-" + json.err_msg);
                }
            }

            return View();
        }
    }
}