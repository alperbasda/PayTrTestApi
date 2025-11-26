
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Net.Mail;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

public partial class paytr_geri_donen_odemeler_callback_ornek : System.Web.UI.Page {

    // #######################  #######################
    //
    // 
    string merchant_key = "AAAAAA";
    string merchant_salt = "XXXXXXXXXXXXXXXX";
    // ###########################################################################

    protected void Page_Load(object sender, EventArgs e)
    {

       
      
        string trans_id = Request.Form["trans_id"];
        string merchant_id = Request.Form["merchant_id"];
        string hash = Request.Form["hash"];
        string processed_result = Request.Form["processed_result"];
		// #######  #######
        // 
		// 
        string Birlestir = string.Concat(merchant_id, trans_id, merchant_salt);
        HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(merchant_key));
        byte[] b = hmac.ComputeHash(Encoding.UTF8.GetBytes(Birlestir));
        string token = Convert.ToBase64String(b);

        //
        // 
        // 
		
					
        if (hash.ToString() != token)
        {
			
            Response.Write("PAYTR notification failed: bad hash");
            return;
        }
		
		    

        //

		dynamic dynJson = JsonConvert.DeserializeObject(processed_result);
		
		foreach (var item in dynJson)
		{
			//
		}
        // 
        Response.Write("OK");
    }

  
}