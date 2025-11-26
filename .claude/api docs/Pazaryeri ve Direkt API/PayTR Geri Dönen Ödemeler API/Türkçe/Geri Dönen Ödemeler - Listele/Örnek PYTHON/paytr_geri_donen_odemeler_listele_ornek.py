# Python 3.6+

import base64
import hmac
import hashlib
import json
import requests

# API Entegrasyon Bilgilier - Mağaza paneline giriş yaparak BİLGİ sayfasından alabilirsiniz.
merchant_id = 'XXXXXX'
merchant_key = b'XXXXXXXXYYYYYYYY'
merchant_salt = 'XXXXXXXXYYYYYYYY'

# Gerekli Bilgiler
start_date = '2020-05-20 00:00:00'
end_date = '2020-06-16 23:59:59'
# Başlangıç / Bitiş tarihi. En fazla 31 gün aralık tanımlanabilir.

# Bu kısımda herhangi bir değişiklik yapmanıza gerek yoktur.
hash_str = merchant_id + start_date + end_date + merchant_salt
paytr_token = base64.b64encode(hmac.new(merchant_key, hash_str.encode(), hashlib.sha256).digest())

params = {
    'merchant_id': merchant_id,
    'start_date': start_date,
    'end_date': end_date,
    'paytr_token': paytr_token
}

result = requests.post('https://www.paytr.com/odeme/geri-donen-transfer', params)
res = json.loads(result.text)

"""
res değeri içerisinde;
['ref_no']              - 1000001
['date_detected']       - 2020-06-10
['date_reimbursed']     - 2020-06-08
['transfer_name']       - ÖRNEK İSİM
['transfer_iban']       - TR100000000000000000000001
['transfer_amount']     - 35.18
['transfer_currency']   - TL
['transfer_date']       - 2020-06-08
bilgileri dönmektedir.
"""

if res['status'] == 'success':
    # VT işlemleri vs.
    print(res)
elif res['status'] == 'failed':
    print('İlgili tarih araliginda islem bulunamadi')
else:
    print(res['err_no'] + ' - ' + res['err_msg'])