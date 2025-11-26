
// Mağaza bilgileri
var merchant_id = 'XXXXXX'
var merchant_key = 'XXXXXX'
var merchant_salt = 'XXXXXX'

// Kullanıcı bilgileri
var user_ip = '';
var email        = "testpreauth@example.com";
var user_name    = 'TEST TEST';
var user_address = 'TEST ADRES';
var   = user_phone = '5555555555';

var basket = JSON.stringify([
    ['Örnek Ürün 1', '18.00', 1],
    ['Örnek Ürün 2', '33.25', 2],
    ['Örnek Ürün 3', '45.42', 1] ]);// Ürün ismi, Fiyat, Adet



// İşlem bilgileri
var auth_type = 'preauth';
var merchant_oid      = 'TESTPREAUTH';
var currency          = 'TL';
var payment_amount    = '14.55';
var payment_type      = 'card';
var non_3d            = '1';
var non3d_test_failed = '0';
var installment_count = '1';// Ön provizyonda taksitli işlem yapılamaz. Bu nedenle 1 veya 0 gönderilmelidir.
var client_lang       = 'tr';
var test_mode         = '0'; //Test modunda işlemler şuan için desteklenmemektedir. test_mode "0" ileterek, işlemlerinizi canlı modda ve gerçek kart bilgileri ile gerçekleştiriniz.
var utoken         = '';
var ctoken         = '';


// paytr_token
var hashSTR = `${merchant_id}${user_ip}${merchant_oid}${email}${payment_amount}${payment_type}${installment_count}${currency}${test_mode}${non_3d}`;
    console.log('HASH STR' + hashSTR);
    var paytr_token = hashSTR + merchant_salt;
    console.log('PAYTR TOKEN' + paytr_token);
    var token = crypto.createHmac('sha256', merchant_key).update(paytr_token).digest('base64');
	
console.log('TOKEN' + token);
    context = {
		auth_type,
        merchant_id,
        user_ip,
        merchant_oid,
        email,
        payment_type,
        payment_amount,
        currency,
        test_mode,
        non_3d,
        user_name,
        user_address,
        user_basket,
		user_phone,
        client_lang,
        token,
        non3d_test_failed,
        installment_count,
		cc_owner,
		cc_number,
		cc_month,
		cc_year,
		cvv,
		utoken,
		ctoken,
		
    };

    res.render('index');
});
var callback = req.body;

    paytr_token = callback.merchant_oid + merchant_salt + callback.status + callback.total_amount;
    var token = crypto.createHmac('sha256', merchant_key).update(paytr_token).digest('base64');

  
var port = 3200;
app.listen(port, function () {
    console.log("Server is running. Port:" + port);
});