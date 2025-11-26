# Python 3.6+


import base64
import hmac
import hashlib
import json
import requests
import random


merchant_id = 'XXXXXX'
merchant_key = b'XXXXXXXXYYYYYYYY'
merchant_salt = 'XXXXXXXXYYYYYYYY'


trans_id = 'PHG' + random.randint(1, 9999999).__str__()
trans_info = [
    {
        'amount': '1283', 
        'receiver': 'XYZ LTD ŞTİ',
        'iban': 'TRXXXXXXXXXXXXXXXXXXXXX'
    }
]


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

    print(res)
else:

    print(res['err_no'] + ' - ' + res['err_msg'])
