var request = require('request');
var crypto = require('crypto');
var express = require('express');
var app = express();

app.use(express.json());
app.use(express.urlencoded({ extended: true }));

var merchant_id = 'MAGAZA_NO';
var merchant_key = 'XXXXXXXXXXX';
var merchant_salt = 'YYYYYYYYYYY';



app.get("/send", function (req, res) {

    // Mağaza sipariş no: Satış işlemi için belirlediğiniz benzersiz sipariş numarası
    var merchant_oid = '';
    // Eşsiz transfer numarası
    var trans_id = '';
    // Satıcıya yapılacak ödeme tutarı: Satıcıya bu sipariş için ödenecek tutarın 100 ile çarpılmış hali (Örnek: 50.99 TL için 5099)
    var submerchant_amount = '';
    // Toplam ödeme tutarı: Siparişe ait toplam ödeme tutarının 100 ile çarpılmış hali (Örnek: 50.99 TL için 5099)
    var total_amount = '';
    // Satıcının banka hesabı için ad soyad/ünvanı
    var transfer_name = '';
    // Satıcının banka hesabı IBAN numarası
    var transfer_iban = '';

    var hash_str = merchant_id + merchant_oid + trans_id + submerchant_amount + total_amount + transfer_name + transfer_iban;
    var paytr_token = crypto.createHmac('sha256', merchant_key).update(hash_str + merchant_salt).digest('base64');

    var options = {
        'method': 'POST',
        'url': 'https://www.paytr.com/odeme/platform/transfer',
        'headers': {
            'Content-Type': 'application/x-www-form-urlencoded'
        },
        form: {
            'merchant_id': merchant_id,
            'merchant_oid': merchant_oid,
            'trans_id': trans_id,
            'submerchant_amount': submerchant_amount,
            'total_amount': total_amount,
            'transfer_name': transfer_name,
            'transfer_iban': transfer_iban,
            'paytr_token': paytr_token,
        }
    };

    request(options, function (error, response, body) {
        if (error) throw new Error(error);
        var res_data = JSON.parse(body);

        if (res_data.status == 'success') {

            /*
                Başarılı yanıt örneği:
                {"status":"success", "merchant_amount":"5", "submerchant_amount":"92", "trans_id":"45ABT34", "reference":"12SF45" }
        
                Başarısız yanıt örneği:
                {"status":"error", "err_no":"010", "err_msg":"toplam transfer tutarı kalan tutardan fazla olamaz"}
            */

            res.send(res_data);

        } else {
            res.end(response.body);
        }

    });


});



app.post("/callback", function (req, res) {

    var callback = req.body;
    var trans_ids = callback.trans_ids;


    var trans_ids = trans_ids.replace('\\', '');

    // POST değerleri ile hash oluştur.
    var paytr_token = crypto.createHmac('sha256', merchant_key).update(trans_ids + merchant_salt).digest('base64');

    // Oluşturulan hash'i, paytr'dan gelen post içindeki hash ile karşılaştır (isteğin paytr'dan geldiğine ve değişmediğine emin olmak için)
    if (paytr_token != callback.hash) {
        throw new Error("PAYTR notification failed: bad hash");
    }

    // ## trans_ids: Daha önce PayTR'a ilettiğiniz transfer taleplerinden tamamlanan transferlerin trans_id bilgilerini içeren JSON 
    // ## (trans_id bilgisi transfer talebi yaparken PayTR'a gönderdiğiniz her işlem için eşsiz değerdir)
    // ## Örn: Burada trans_ids JSON verisini DECODE edip, çıktıdaki her bir trans_id ile veritabanınızdan transfer talebini tespit ederek ilgili kullanıcınıza bilgilendirme gönderebilirsiniz (email, sms vb.)
    var processed_result = JSON.parse(trans_ids);

    console.log(processed_result);


    // Bildirimin alındığını PayTR sistemine bildir.  
    res.send("OK");
});


var port = 3200;
app.listen(port, function () {
    console.log("Server is running. Port:" + port);
});
