// 1. ADIM için örnek kodlar

using Newtonsoft.Json.Linq; // Bu satırda hata alırsanız, site dosyalarınızın olduğu bölümde bin isimli bir klasör oluşturup içerisine Newtonsoft.Json.dll adlı DLL dosyasını kopyalayın.
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

        // ####################### DÜZENLEMESİ ZORUNLU ALANLAR #######################
        //
        // API Entegrasyon Bilgileri - Mağaza paneline giriş yaparak BİLGİ sayfasından alabilirsiniz.
        string merchant_id      = "XXXXXX";
        string merchant_key     = "YYYYYYYYYYYYYY";
        string merchant_salt    = "ZZZZZZZZZZZZZZ";
        //
        // Mağaza sipariş no: Satış işlemi için belirlediğiniz benzersiz sipariş numarası 
        string merchant_oid     = "";
        //
        // Satıcıya yapılacak bu ödemenin takibi için benzersiz takip numarası 
        string trans_id     = "";
        //
        // Satıcıya yapılacak ödeme tutarı: Satıcıya bu sipariş için ödenecek tutarın 100 ile çarpılmış hali (Örnek: 50.99 TL için 5099)
        string submerchant_amount  = "";
        //
        // Toplam ödeme tutarı: Siparişe ait toplam ödeme tutarının 100 ile çarpılmış hali (Örnek: 50.99 TL için 5099)
        string total_amount    = ""; 
        //
        // Satıcının banka hesabı için ad soyad/ünvanı
        string transfer_name    = "";
        //
        // Satıcının banka hesabı IBAN numarası
        string transfer_iban    = "";
        //
        

        // Gönderilecek veriler oluşturuluyor
        NameValueCollection data = new NameValueCollection();
        data["merchant_id"] = merchant_id;
        data["merchant_oid"] = merchant_oid;
        data["trans_id"] = trans_id;
        data["submerchant_amount"] = submerchant_amount.ToString();
        data["total_amount"] = total_amount.ToString();
        data["transfer_name"] = transfer_name;
        data["transfer_iban"] = transfer_iban;
        //
        // Token oluşturma fonksiyonu, değiştirilmeden kullanılmalıdır.
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

            /*
                Başarılı yanıt örneği:
                {"status":"success", "merchant_amount":"5", "submerchant_amount":"92", "trans_id":"45ABT34", "reference":"12SF45" }

                Başarısız yanıt örneği:
                {"status":"error", "err_no":"010", "err_msg":"toplam transfer tutarı kalan tutardan fazla olamaz"}
            */
            if (json.status == "success") {
                //VT işlemleri vs.
                Response.Write(json);
            }else{
                Response.Write("PAYTR platform transfer request failed. reason:" + json.err_msg + "");
            }
        }
    }
}