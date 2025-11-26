  // ########################### İŞLEM DÖKÜMÜ ALMAK  İÇİN ÖRNEK KODLAR ##########################
  //  #                                                                                          #
  //  ################################ DÜZENLEMESİ ZORUNLU ALANLAR ###############################
  //  #
  //  ## API Entegrasyon Bilgileri - Mağaza paneline giriş yaparak BİLGİ sayfasından alabilirsiniz.

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
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;


using System.Web.Routing;

namespace WebApplication1.Controllers
{
    public class paytr_geri_donen_odemeler_listele_ornekController : Controller
    {
        public ActionResult paytr_geri_donen_odemeler_listele_ornek()
        {
            // ####################### GEREKLİ BİLGİLER #######################
            //
            // 
    
            string merchant_id = "AAAAAA";
            string merchant_key = "XXXXXXXXXXXXXXXX";
            string merchant_salt = "XXXXXXXXXXXXXXXX";
            //

            //     #######################
            string start_date = "2021-11-01 00:00:00";
            string end_date = "2021-11-29 23:59:59";
            //  Başlangıç / Bitiş tarihi. En fazla 31 gün aralık tanımlanabilir.




            //  ####################### Bu kısımda herhangi bir değişiklik yapmanıza gerek yoktur.
            string Birlestir = string.Concat(merchant_id,start_date,end_date,merchant_salt);
            HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(merchant_key));
            byte[] b = hmac.ComputeHash(Encoding.UTF8.GetBytes(Birlestir));
            string paytr_token = Convert.ToBase64String(b);

            // #######################



            NameValueCollection data = new NameValueCollection();
            data["merchant_id"] = merchant_id;
            data["start_date"] = start_date;
            data["end_date"] = end_date;
            data["paytr_token"] = paytr_token;
            //


            using (WebClient client = new WebClient())
            {
                client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                byte[] result = client.UploadValues("https://www.paytr.com/odeme/geri-donen-transfer", "POST", data);
                string ResultAuthTicket = Encoding.UTF8.GetString(result);
                dynamic json = JValue.Parse(ResultAuthTicket);

                /*
                  $result değeri içerisinde dönen yanıt örneği;

                [ref_no] => 1000001
                [date_detected] => 2020-06-10
                [date_reimbursed] => 2020-06-08
                [transfer_name] => ÖRNEK İSİM
                [transfer_iban] => TR100000000000000000000001
                [transfer_amount] => 35.18
                [transfer_currency] => TL
                [transfer_date] => 2020-06-08

                */

                if (json.status == "success")
                {
					 // VT işlemleri vs.
                    Response.Write(json);
                   
                }

               else if (json.status == "failed")
                {
					// sonuç bulunamadı
                    Response.Write("İlgili tarih araliginda islem bulunamadi");

                }
                else
                {
                    // Hata durumu
                    Response.Write(json.err_no + "-" + json.err_msg);
                }
            }

            return View();
        }
    }
}