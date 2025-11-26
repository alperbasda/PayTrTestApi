
	//#################### POST içerisinde gelen örnek veriler ####################
	//#
	// [mode] => cashout
	// -> Sabit bu şekilide gelir
	//#
	// [hash] => wszlFsC7nrfCPvP77kdEzzE4smGdV4FWvDibKlXIpRM=,
	// -> Kontrolde kullanaılacaktır.
	//#
	// [trans_id] => 12345aaabbb
	// -> Geri dönen ödeme hesaptan gönderme talebi yaparken PayTR'a gönderdiğiniz eşsiz değer.
	//#
	// [processed_result] => [{\"amount\":484.48,\"receiver\":\"XYZ LTD STI\",\"iban\":\"TRXXXXXXXXXXXXXXXXXX\",\"result\":\"success\"}]
	// -> Geri dönen ödeme hesaptan gönderme talebi yaparken PayTR'a gönderdiğiniz değerler.
	//#
	// [success_total] => 1
	// -> Başarıyla transfer edilen işlem sayısı (processed_result içerisinde, result:success olanların sayısı)
	//#
	// [failed_total] => 0
	// -> Hata alan işlem sayısı (processed_result içerisinde, result:failed olanların sayısı)
	//#
	// [transfer_total] => 484.48
	// -> Başarıyla tranasfer edilen işlemlerin toplam tutarı.
	//#
	// [account_balance] => 0
	// -> Transferler sonrasında kalan alt hesap bakiyeniz.
	//############################################################################



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

    // ####################### DÜZENLEMESİ ZORUNLU ALANLAR #######################
    //
    // API Entegrasyon Bilgileri - Mağaza paneline giriş yaparak BİLGİ sayfasından alabilirsiniz.
    string merchant_key = "AAAAAA";
    string merchant_salt = "XXXXXXXXXXXXXXXX";
    // ###########################################################################

    protected void Page_Load(object sender, EventArgs e)
    {

       
      
        string trans_id = Request.Form["trans_id"];
        string merchant_id = Request.Form["merchant_id"];
        string hash = Request.Form["hash"];
        string processed_result = Request.Form["processed_result"];
		// ####### Bu kısımda herhangi bir değişiklik yapmanıza gerek yoktur. #######
        // 
		// POST değerleri ile hash oluştur.
        string Birlestir = string.Concat(merchant_id, trans_id, merchant_salt);
        HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(merchant_key));
        byte[] b = hmac.ComputeHash(Encoding.UTF8.GetBytes(Birlestir));
        string token = Convert.ToBase64String(b);

        //
        // Oluşturulan hash'i, paytr'dan gelen post içindeki hash ile karşılaştır (isteğin paytr'dan geldiğine ve değişmediğine emin olmak için)
        // Bu işlemi yapmazsanız maddi zarara uğramanız olasıdır.
		
					
        if (hash.ToString() != token)
        {
			
            Response.Write("PAYTR notification failed: bad hash");
            return;
        }
		
		    

        //## trans_id bilgisi transfer talebi yaparken PayTR'a gönderdiğiniz her işlem için eşsiz değerdir.

		dynamic dynJson = JsonConvert.DeserializeObject(processed_result);
		
		foreach (var item in dynJson)
		{
			// Burada her işlem için gerekli veri tabanı vb. işlemleri yapabilirsiniz.
		}
        // Bildirimin alındığını PayTR sistemine bildir.
        Response.Write("OK");
    }

  
}