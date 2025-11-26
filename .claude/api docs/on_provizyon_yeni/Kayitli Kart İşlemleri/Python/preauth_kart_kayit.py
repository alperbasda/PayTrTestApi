

# Mağaza bilgileri
merchant_id   = 'MAGAZA_NO';
merchant_key  = b'XXXXXXXXXXX';
merchant_salt = b'YYYYYYYYYYY';

# Kullanıcı bilgileri

user_ip = '';


email        = "testpreauth@example.com";
user_name    = 'TEST TEST';
user_address = 'TEST ADRES';
user_phone   = '5555555555';
user_basket = html.unescape(json.dumps([['Altis Renkli Deniz Yatağı - Mavi', '18.00', 1],
                                            ['Pharmaso Güneş Kremi 50+ Yetişkin & Bepanthol Cilt Bakım Kremi', '33,25',
                                             2],
                                            ['Bestway Çocuklar İçin Plaj Seti Beach Set ÇANTADA DENİZ TOPU-BOT-KOLLUK',
                                             '45,42', 1]])) # Ürün ismi, Fiyat, Adet

# Kart bilgileri
cc_owner  = 'TEST TEST';
card_number = '4355084355084358';
expiry_month  = '12';
expiry_year   = '99';
cvv       = '000';

 #İşlem bilgileri
merchant_oid      = 'TESTPREAUTH'.time();
currency          = 'TL';
payment_amount    = '14.55';
payment_type      = 'card';
non_3d            = '1';
non3d_test_failed = '0';
installment_count = '1';#Ön provizyonda taksitli işlem yapılamaz. Bu nedenle 1 veya 0 gönderilmelidir.
client_lang       = 'tr';
test_mode         = '0'; #Test modunda işlemler şuan için desteklenmemektedir. test_mode "0" ileterek, işlemlerinizi canlı modda ve gerçek kart bilgileri ile gerçekleştiriniz.
store_card         = '1';

# paytr_token
hash_str = merchant_id + user_ip + merchant_oid + email + payment_amount + payment_type + installment_count + currency + test_mode + non_3d
paytr_token = base64.b64encode(hmac.new(merchant_key, hash_str.encode() + merchant_salt, hashlib.sha256).digest())

 context = {
        'auth_type' : 'preauth',
        'merchant_id': merchant_id,
        'user_ip': user_ip,
        'merchant_oid': merchant_oid,
        'email': email,
        'payment_type': payment_type,
        'payment_amount': payment_amount,
        'currency': currency,
        'test_mode': test_mode,
        'non_3d': non_3d,
        'user_name': user_name,
        'user_address': user_address,
        'user_phone': user_phone,
        'user_basket': user_basket,
        'client_lang': client_lang,
        'paytr_token': paytr_token.decode(),
        'non3d_test_failed': non3d_test_failed,
        'installment_count': installment_count,
        'cc_owner' : cc_owner,
        'card_number': cc_number,
        'expiry_month' : cc_month,
        'expiry_year' : cc_year,
        'cvv' : cvv,
        'store_card' : store_card
    }
 return render(request, 'home.html', context)