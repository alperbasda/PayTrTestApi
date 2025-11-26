
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Net.Mail;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class transfer_sonucu_ornek : System.Web.UI.Page {

    string merchant_key     = "YYYYYYYY";
    string merchant_salt    = "ZZZZZZZZ";
    // ###########################################################################

    protected void Page_Load(object sender, EventArgs e) {

        string trans_ids = Request.Form["trans_ids"];
        string hash = Request.Form["hash"];
        
		trans_ids = trans_ids.Replace(@"\","");

        string Birlestir = string.Concat(trans_ids, merchant_salt);
        HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(merchant_key));
        byte[] b = hmac.ComputeHash(Encoding.UTF8.GetBytes(Birlestir));
        string token = Convert.ToBase64String(b);

        if (hash.ToString() != token) {
            Response.Write("PAYTR notification failed: bad hash");
            return;
        }
        //###########################################################################

		dynamic dynJson = JsonConvert.DeserializeObject(trans_ids);
		foreach (var item in dynJson)
		{
			// Actions
		}
		
        Response.Write("OK");
    }
}