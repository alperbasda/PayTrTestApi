
// Mağaza bilgileri
string  = 'MAGAZA_NO';
string  = 'XXXXXXXXXXX';
string = 'YYYYYYYYYYY';

// Kullanıcı bilgileri
string client_ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (user_ip == "" || user_ip == null){
                user_ip = Request.ServerVariables["REMOTE_ADDR"];
            }

// İşlem bilgileri
string reference_no = 'AXXXXXXXXXXXXXXXX';
string merchant_oid = 'TESTPREAUTHXXX';
string client_lang  = 'tr';
string test_mode    = '0';

$hash_str    = merchant_id . $ip . $merchant_oid . $reference_no;
string Birlestir = string.Concat(merchant_id, user_ip, merchant_oid, reference_no,merchant_salt);
HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(merchant_key));
byte[] b = hmac.ComputeHash(Encoding.UTF8.GetBytes(Birlestir));

			ViewBag.auth_type = 'cancel';
			ViewBag.MerchantId = merchant_id;
            ViewBag.Client_ip = client_ip;
            ViewBag.MerchantOid = merchant_oid;
            ViewBag.TestMode = test_mode;
			ViewBag.Client_Lang= client_lang;
			ViewBag.PaytrToken = Convert.ToBase64String(b);
			ViewBag.Referance_No = reference_no;


            return View();
        }
    }
}
