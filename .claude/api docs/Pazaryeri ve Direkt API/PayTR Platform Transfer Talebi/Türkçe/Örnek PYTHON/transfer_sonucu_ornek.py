# Python 3.6+
# Django Web Framework referans alınarak hazırlanmıştır

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
    post['trans_ids'] = post['trans_ids'].replace('\\', '')
    hash_str = post['trans_ids'] + merchant_salt
    hash = base64.b64encode(hmac.new(merchant_key, hash_str.encode(), hashlib.sha256).digest())

    # Oluşturulan hash'i, paytr'dan gelen post içindeki hash ile karşılaştır
    # (isteğin paytr'dan geldiğine ve değişmediğine emin olmak için)
    # Bu işlemi güvenlik nedeniyle mutlaka yapmanız gerekiyor.
    if hash != post['hash']:
        return HttpResponse(str('PAYTR notification failed: bad hash'))

    # post['trans_ids'] içerisinde daha önce PayTR'a ilettiğiniz transfer taleplerinden tamamlanan transferlerin trans_id bilgileri JSON formatında gelir
    # trans_id bilgisi transfer talebi yaparken PayTR'a gönderdiğiniz her işlem için eşsiz değerdir
    trans_ids = json.loads(post['trans_ids'])

    for ids in trans_ids:
        # Örn: Burada trans_id ile veritabanınızdan transfer talebini tespit edip ilgili kullanıcınıza bilgilendirme gönderebilirsiniz (email, sms vb.)
        print(ids)

    # Bildirimin alındığını PayTR sistemine bildir.
    return HttpResponse(str('OK'))