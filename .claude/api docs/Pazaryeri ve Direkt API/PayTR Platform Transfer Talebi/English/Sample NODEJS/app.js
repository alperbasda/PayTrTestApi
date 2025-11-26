var request = require('request');
var crypto = require('crypto');
var express = require('express');
var app = express();

app.use(express.json());
app.use(express.urlencoded({ extended: true }));

var merchant_id = 'MERCHANT_NO';
var merchant_key = 'XXXXXXXXXXX';
var merchant_salt = 'YYYYYYYYYYY';



app.get("/send", function (req, res) {

    // The unique order id you set for the transaction.
    var merchant_oid = '';
 //  Unique tracking number for tracking this payment to the seller
    var trans_id = '';
   // Unique tracking number for tracking this payment to the seller 
    var submerchant_amount = '';
    //Total payment amount: The total payment amount of the order multiplied by 100.
    var total_amount = '';
    // Name, surname / title for the seller's bank account
    var transfer_name = '';
    // Seller's IBAN bank account number
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
                Success:
                {"status":"success", "merchant_amount":"5", "submerchant_amount":"92", "trans_id":"45ABT34", "reference":"12SF45" }
        
                Failed:
                {"status":"error", "err_no":"010", "err_msg":"the total transfer amount cannot be more than the remaining amount"}
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

    var paytr_token = crypto.createHmac('sha256', merchant_key).update(trans_ids + merchant_salt).digest('base64');

    if (paytr_token != callback.hash) {
        throw new Error("PAYTR notification failed: bad hash");
    }

    var processed_result = JSON.parse(trans_ids);
    console.log(processed_result);

    res.send("OK");
});


var port = 3200;
app.listen(port, function () {
    console.log("Server is running. Port:" + port);
});
