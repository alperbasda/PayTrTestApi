

# Mağaza bilgileri
merchant_id   = 'MAGAZA_NO';
merchant_key  = b'XXXXXXXXXXX';
merchant_salt = b'YYYYYYYYYYY';

# Kullanıcı bilgileri
user_ip='';

// İşlem bilgileri

payment_amount = '14.55';
reference_no   = 'AXXXXXXXXXXXXXXXX';
merchant_oid   = 'TESTPREAUTHXXX';
client_lang    = 'tr';
test_mode      = '0';

# paytr_token

 hash_str = merchant_id + user_ip + merchant_oid +  reference_no + payment_amount
paytr_token = base64.b64encode(hmac.new(merchant_key, hash_str.encode() + merchant_salt, hashlib.sha256).digest())

 context = {
 
        'auth_type':'capture',
        'merchant_id': merchant_id,
        'client_ip': user_ip,
        'merchant_oid': merchant_oid,
        'payment_amount': payment_amount,
        'reference_no':reference_no,
        'test_failed': test_mode,
        'paytr_token': paytr_token.decode(),
        'client_lang': client_lang
    }


 return render(request, 'home.html', context)