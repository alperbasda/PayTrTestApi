
// Mağaza bilgileri
string  = 'MAGAZA_NO';
string  = 'XXXXXXXXXXX';
string = 'YYYYYYYYYYY';

// Kullanıcı bilgileri
string ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (user_ip == "" || user_ip == null){
                user_ip = Request.ServerVariables["REMOTE_ADDR"];
            }

string emailstr = "testpreauth@example.com";
string user_namestr    = 'TEST TEST';
string user_addressstr = 'TEST ADRES';
string user_phonestr   = '5555555555';
string user_ip  = $ip;
object[][] user_basket = {
            new object[] {"Örnek ürün 1", "18.00", 1}, // 1. ürün (Ürün Ad - Birim Fiyat - Adet)
            new object[] {"Örnek ürün 2", "33.25", 2}, // 2. ürün (Ürün Ad - Birim Fiyat - Adet)
            new object[] {"Örnek ürün 3", "45.42", 1}, // 3. ürün (Ürün Ad - Birim Fiyat - Adet)
            }; // Ürün ismi, Fiyat, Adet




// İşlem bilgileri
string merchant_oid      = 'TESTPREAUTH';
string currency          = 'TL';
string payment_amount    = '14.55';
string payment_type      = 'card';
string non_3d            = '1';
string non3d_test_failed = '0';
string installment_count = '1';// Ön provizyonda taksitli işlem yapılamaz. Bu nedenle 1 veya 0 gönderilmelidir.
string client_lang       = 'tr';
string test_mode         = '0'; //Test modunda işlemler şuan için desteklenmemektedir. test_mode "0" ileterek, işlemlerinizi canlı modda ve gerçek kart bilgileri ile gerçekleştiriniz.
string utoken         = '';
string ctoken         = '';

 JavaScriptSerializer ser = new JavaScriptSerializer();
  string user_basket_json = ser.Serialize(user_basket);

// paytr_token
string Birlestir = string.Concat(merchant_id, user_ip, merchant_oid, emailstr, payment_amountstr.ToString(), payment_type, installment_count, currency, test_mode, non_3d, merchant_salt);
HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(merchant_key));
byte[] b = hmac.ComputeHash(Encoding.UTF8.GetBytes(Birlestir));

			ViewBag.auth_type = 'preauth';
			ViewBag.MerchantId = merchant_id;
            ViewBag.UserIp = user_ip;
            ViewBag.MerchantOid = merchant_oid;
            ViewBag.Email = emailstr;
            ViewBag.PaymentType = payment_type;
            ViewBag.PaymentAmount = payment_amountstr.ToString();
            ViewBag.InstallmentCount = installment_count;
            ViewBag.Currency = currency;
            ViewBag.TestMode = test_mode;
            ViewBag.Non3d = non_3d;
			ViewBag.Client_Lang = client_lang;
            ViewBag.UserName = user_namestr;
            ViewBag.UserAddress = user_addressstr;
            ViewBag.UserPhone = user_phonestr;
            ViewBag.UserBasket = user_basket_json;
            ViewBag.Non3dTestFailed = non3d_test_failed;
            ViewBag.PaytrToken = Convert.ToBase64String(b);
			ViewBag.cc_owner = cc_owner;
			ViewBag.cc_number= cc_number;
			ViewBag.cc_month = cc_month;
			ViewBag.cc_year= cc_year;
			ViewBag.cvv = cvv;
			ViewBag.utoken = utoken;
			ViewBag.ctoken = ctoken;

            return View();
        }
    }
}


