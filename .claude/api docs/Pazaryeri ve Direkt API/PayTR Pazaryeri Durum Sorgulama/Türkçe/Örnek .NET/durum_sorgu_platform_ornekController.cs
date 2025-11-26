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
using System.IO;

namespace WebApplication1.Controllers
{
    public class durum_sorgu_platform_ornekController : Controller
    {
        public ActionResult durum_sorgu_platform_ornek()
        {
            // ####################### #######################
            //
            // 

            string merchant_id = "YYYYYY";
            string merchant_key = "YYYYYYYYYYYYYY";
            string merchant_salt = "YYYYYYYYYYYYYY";
            string merchant_oid = "";
            //

   



            //  #######################
            string Birlestir = string.Concat(merchant_id, merchant_oid, merchant_salt);
            HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(merchant_key));
            byte[] b = hmac.ComputeHash(Encoding.UTF8.GetBytes(Birlestir));
            string paytr_token = Convert.ToBase64String(b);

            // #######################



            NameValueCollection data = new NameValueCollection();
            data["merchant_id"] = merchant_id;
            data["merchant_oid"] = merchant_oid;
            data["paytr_token"] = paytr_token;
            //


            using (WebClient client = new WebClient())
            {
                client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                byte[] result = client.UploadValues("https://www.paytr.com/odeme/durum-sorgu", "POST", data);
                string ResultAuthTicket = Encoding.UTF8.GetString(result);
                dynamic json = JValue.Parse(ResultAuthTicket);

             

                if (json.status == "success")
                {
    
                    Response.Write(json.payment_amount + "-" + json.currency);
                    Response.Write(json.payment_total + "-" + json.currency);
					
					

				
				
                    foreach (var return_success in json.returns)
                    {
				//Array 
				//( 
				//[return_amount] => 1 
				//[return_date] => 2021-03-25 23:45:22 
	            //[return_type] => 
	            //[date_completed] => 2021-03-25 23:46:02 
	            //[return_auth_code] =>
	            //[return_ref_num] => 
	            //[reference_no] => 111111111111111
	            //[return_source] => 
	            //)
				
                        Response.Write(return_success);
                    }

                    foreach (var sub_payments in json.submerchant_payments)
                    {
                        Response.Write(sub_payments);
                    }
                    
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