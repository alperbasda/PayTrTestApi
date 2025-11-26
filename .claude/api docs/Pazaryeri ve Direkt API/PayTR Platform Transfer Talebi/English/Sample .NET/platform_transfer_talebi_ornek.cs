using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class platform_transfer_talebi_ornek : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e) {

        string merchant_id      = "XXXXXX";
        string merchant_key     = "YYYYYYYYYYYYYY";
        string merchant_salt    = "ZZZZZZZZZZZZZZ";
        string merchant_oid     = "";
        string trans_id     = "";
        string submerchant_amount  = "";
        string total_amount    = "";
        string transfer_name    = "";
        string transfer_iban    = "";

        NameValueCollection data = new NameValueCollection();
        data["merchant_id"] = merchant_id;
        data["merchant_oid"] = merchant_oid;
        data["trans_id"] = trans_id;
        data["submerchant_amount"] = submerchant_amount.ToString();
        data["total_amount"] = total_amount.ToString();
        data["transfer_name"] = transfer_name;
        data["transfer_iban"] = transfer_iban;
        //
        string Birlestir = string.Concat(merchant_id, merchant_oid, trans_id, submerchant_amount.ToString(), total_amount.ToString(), transfer_name, transfer_iban, merchant_salt);
        HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(merchant_key));
        byte[] b = hmac.ComputeHash(Encoding.UTF8.GetBytes(Birlestir));
        data["paytr_token"] = Convert.ToBase64String(b);
        //

        using (WebClient client = new WebClient()) {
            client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            byte[] result = client.UploadValues("https://www.paytr.com/odeme/platform/transfer", "POST", data);
            string ResultAuthTicket = Encoding.UTF8.GetString(result);
            dynamic json = JValue.Parse(ResultAuthTicket);

            if (json.status == "success") {
                // DB Actions.
                Response.Write(json);
            }else{
                Response.Write("PAYTR platform transfer request failed. reason:" + json.err_msg + "");
            }
        }
    }
}