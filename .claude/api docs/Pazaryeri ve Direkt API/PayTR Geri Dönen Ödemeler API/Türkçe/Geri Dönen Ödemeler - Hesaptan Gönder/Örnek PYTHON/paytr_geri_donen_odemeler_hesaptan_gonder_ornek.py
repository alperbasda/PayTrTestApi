# Python 3.6+
# İŞLEM DÖKÜMÜ ALMAK  İÇİN ÖRNEK KODLAR

import base64
import hmac
import hashlib
import json
import requests
import random

# API Entegrasyon Bilgilier - Mağaza paneline giriş yaparak BİLGİ sayfasından alabilirsiniz.
merchant_id = 'XXXXXX'
merchant_key = b'XXXXXXXXYYYYYYYY'
merchant_salt = 'XXXXXXXXYYYYYYYY'

# Gerekli Bilgiler
trans_id = 'PHG' + random.randint(1, 9999999).__str__()
trans_info = [
    {
        'amount': '1283',  # amount 100 ile çarpılarak gönderilir!!
        'receiver': 'XYZ LTD ŞTİ',
        'iban': 'TRXXXXXXXXXXXXXXXXXXXXX'
    }
]

# Bu kısımda herhangi bir değişiklik yapmanıza gerek yoktur.
hash_str = merchant_id + trans_id + merchant_salt
paytr_token = base64.b64encode(hmac.new(merchant_key, hash_str.encode(), hashlib.sha256).digest())

params = {
    'trans_info': json.dumps(trans_info),
    'trans_id': trans_id,
    'merchant_id': merchant_id,
    'paytr_token': paytr_token
}

result = requests.post('https://www.paytr.com/odeme/hesaptan-gonder', params)
res = json.loads(result.text)

if res['status'] == 'success':
    # status ve trans_id içerir
    print(res)
else:
    # status = error
    # status ve err_no - err_msg içerir
    print(res['err_no'] + ' - ' + res['err_msg'])
