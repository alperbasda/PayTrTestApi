
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

    // ####################### DÜZENLEMESİ ZORUNLU ALANLAR #######################
    //
    // API Entegrasyon Bilgileri - Mağaza paneline giriş yaparak BİLGİ sayfasından alabilirsiniz.
    string merchant_key     = "YYYYYYYY";
    string merchant_salt    = "ZZZZZZZZ";
    // ###########################################################################

    protected void Page_Load(object sender, EventArgs e) {

        // ####### Bu kısımda herhangi bir değişiklik yapmanıza gerek yoktur. #######
        // 
        // POST değerleri ile hash oluştur.
        string trans_ids = Request.Form["trans_ids"];
        string hash = Request.Form["hash"];
        
		trans_ids = trans_ids.Replace(@"\","");

        string Birlestir = string.Concat(trans_ids, merchant_salt);
        HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(merchant_key));
        byte[] b = hmac.ComputeHash(Encoding.UTF8.GetBytes(Birlestir));
        string token = Convert.ToBase64String(b);

        //
        // Oluşturulan hash'i, paytr'dan gelen post içindeki hash ile karşılaştır (isteğin paytr'dan geldiğine ve değişmediğine emin olmak için)
        if (hash.ToString() != token) {
            Response.Write("PAYTR notification failed: bad hash");
            return;
        }

        //###########################################################################
        
        ## trans_ids: Daha önce PayTR'a ilettiğiniz transfer taleplerinden tamamlanan transferlerin trans_id bilgilerini içeren JSON 
		## (trans_id bilgisi transfer talebi yaparken PayTR'a gönderdiğiniz her işlem için eşsiz değerdir)
		## Örn: Burada trans_ids JSON verisini DECODE edip, çıktıdaki her bir trans_id ile veritabanınızdan transfer talebini tespit ederek ilgili kullanıcınıza bilgilendirme gönderebilirsiniz (email, sms vb.)
		
		dynamic dynJson = JsonConvert.DeserializeObject(trans_ids);
		foreach (var item in dynJson)
		{
			## Örn: Burada $trans_id ile veritabanınızdan transfer talebini tespit edip ilgili kullanıcınıza bilgilendirme gönderebilirsiniz (email, sms vb.)
		}
		
        // Bildirimin alındığını PayTR sistemine bildir.  
        Response.Write("OK");    
    }
}