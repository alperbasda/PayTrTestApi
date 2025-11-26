# Mağaza bilgileri
merchant_id   = 'MAGAZA_NO';
merchant_key  = b'XXXXXXXXXXX';
merchant_salt = b'YYYYYYYYYYY';

# Kullanıcı bilgileri
user_ip ='';

# İşlem bilgileri
reference_no = 'AXXXXXXXXXXXXXXXX';
merchant_oid = 'TESTPREAUTHXXX';
client_lang  = 'tr';
test_mode    = 0;


 hash_str = merchant_id + user_ip + merchant_oid +  reference_no
paytr_token = base64.b64encode(hmac.new(merchant_key, hash_str.encode() + merchant_salt, hashlib.sha256).digest())

 context = {
 
        'auth_type':'capture',
        'merchant_id': merchant_id,
        'client_ip': user_ip,
        'merchant_oid': merchant_oid,
        'test_failed': test_mode,
        'reference_no' :reference_no,
        'paytr_token': paytr_token.decode(),
        'client_lang': client_lang
    } 



 return render(request, 'home.html', context)