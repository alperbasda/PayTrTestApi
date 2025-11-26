<?php

// Mağaza bilgileri
string merchant_id   = 'MAGAZA_NO';
string merchant_key  = 'XXXXXXXXXXX';
string merchant_salt = 'YYYYYYYYYYY';

// Kullanıcı bilgileri
string client_ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (user_ip == "" || user_ip == null){
                user_ip = Request.ServerVariables["REMOTE_ADDR"];
            }

// İşlem bilgileri
string payment_amountstr = '14.55';
string reference_no   = 'AXXXXXXXXXXXXXXXX';
string merchant_oid   = 'TESTPREAUTHXXX';
string client_lang    = 'tr';
string test_mode      = 0;

// paytr_token

string Birlestir = string.Concat(merchant_id, client_ip, merchant_oid, reference_no,payment_amount,merchant_salt);
HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(merchant_key));
byte[] b = hmac.ComputeHash(Encoding.UTF8.GetBytes(Birlestir));


			ViewBag.auth_type = 'capture';
			ViewBag.MerchantId = merchant_id;
            ViewBag.Client_ip = client_ip;
            ViewBag.MerchantOid = merchant_oid;
            ViewBag.TestMode = test_mode;
			ViewBag.Client_Lang= client_lang;
			ViewBag.PaytrToken = Convert.ToBase64String(b);
			ViewBag.Referance_No = reference_no;
			ViewBag.Payment_Amountstr = payment_amountstr;

            return View();
        }
    }
}
