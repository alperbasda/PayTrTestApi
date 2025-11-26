# Python 3.6+

import base64
import hashlib
import hmac
import json
import requests
import random


merchant_id = 'MAGAZA_NO'
merchant_key = b'XXXXXXXXXXX'
merchant_salt = 'YYYYYYYYYYY'


merchant_oid = ''


trans_id = random.randint(1, 9999999).__str__()
submerchant_amount = ''
total_amount = ''
transfer_name = ''
transfer_iban = ''


hash_str = merchant_id + merchant_oid + trans_id + submerchant_amount + total_amount + transfer_name + transfer_iban + merchant_salt
paytr_token = base64.b64encode(hmac.new(merchant_key, hash_str.encode(), hashlib.sha256).digest())

params = {
    'merchant_id': merchant_id,
    'merchant_oid': merchant_oid,
    'trans_id': trans_id,
    'submerchant_amount': submerchant_amount,
    'total_amount': total_amount,
    'transfer_name': transfer_name,
    'transfer_iban': transfer_iban,
    'paytr_token': paytr_token
}

result = requests.post('https://www.paytr.com/odeme/platform/transfer', params)
res = json.loads(result.text)

"""

{"status":"success", "merchant_amount":"5", "submerchant_amount":"92", "trans_id":"45ABT34", "reference":"12SF45" }


{"status":"error", "err_no":"010", "err_msg":"toplam transfer tutarı kalan tutardan fazla olamaz"}
"""

if res['status'] == 'success':
 
    print(res)
else:
    print(res['err_no'] + ' - ' + res['err_msg'])