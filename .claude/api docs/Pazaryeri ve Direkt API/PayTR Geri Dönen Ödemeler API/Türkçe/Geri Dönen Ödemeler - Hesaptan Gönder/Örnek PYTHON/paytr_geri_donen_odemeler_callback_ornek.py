# Python 3.6+
# Django Web Framework referans alınarak hazırlanmıştır
# POST içerisinde gelen örnek veriler

"""
[mode] : cashout
-> Sabit bu şekilide gelir

[hash] : wszlFsC7nrfCPvP77kdEzzE4smGdV4FWvDibKlXIpRM=,
-> Kontrolde kullanaılacaktır.

[trans_id] : 12345aaabbb
-> Geri dönen ödeme hesaptan gönderme talebi yaparken PayTR'a gönderdiğiniz eşsiz değer.

[processed_result] : [{\"amount\":484.48,\"receiver\":\"XYZ LTD STI\",\"iban\":\"TRXXXXXXXXXXXXXXXXXX\",\"result\":\"success\"}]
-> Geri dönen ödeme hesaptan gönderme talebi yaparken PayTR'a gönderdiğiniz değerler.

[success_total] : 1
-> Başarıyla transfer edilen işlem sayısı (processed_result içerisinde, result:success olanların sayısı)

[failed_total] : 0
-> Hata alan işlem sayısı (processed_result içerisinde, result:failed olanların sayısı)

[transfer_total] : 484.48
-> Başarıyla tranasfer edilen işlemlerin toplam tutarı.

[account_balance] : 0
-> Transferler sonrasında kalan alt hesap bakiyeniz.
"""

import base64
import hashlib
import hmac
import json

from django.shortcuts import render, HttpResponse
from django.views.decorators.csrf import csrf_exempt

@csrf_exempt
def callback(request):
    if request.method != 'POST':
        return HttpResponse(str(''))

    post = request.POST

    # API Entegrasyon Bilgileri - Mağaza paneline giriş yaparak BİLGİ sayfasından alabilirsiniz.
    merchant_key = b'YYYYYYYYYYYYYY'
    merchant_salt = 'ZZZZZZZZZZZZZZ'

    # Bu kısımda herhangi bir değişiklik yapmanıza gerek yoktur.
    # POST değerleri ile hash oluştur.
    hash_str = post['merchant_id'] + post['trans_id'] + merchant_salt
    hash = base64.b64encode(hmac.new(merchant_key, hash_str.encode(), hashlib.sha256).digest())

    # Oluşturulan hash'i, paytr'dan gelen post içindeki hash ile karşılaştır
    # (isteğin paytr'dan geldiğine ve değişmediğine emin olmak için)
    # Bu işlemi güvenlik nedeniyle mutlaka yapmanız gerekiyor.
    if hash != post['hash']:
        return HttpResponse(str('PAYTR notification failed: bad hash'))

    # trans_id bilgisi transfer talebi yaparken PayTR'a gönderdiğiniz her işlem için eşsiz değerdir.
    processed_result = json.loads(post['processed_result'])

    for trans in processed_result:
        # Burada her işlem için gerekli veri tabanı vb. işlemleri yapabilirsiniz.
        print(trans)

    # Bildirimin alındığını PayTR sistemine bildir.
    return HttpResponse(str('OK'))