var request = require('request');
var crypto = require('crypto');
var express = require('express');
var app = express();

app.use(express.json());
app.use(express.urlencoded({ extended: true }));


// API - Information Integration  - You can get them from the information page after logging into the PayTR Merchant Panel.

var merchant_id = 'XXXXXX';
var merchant_key = 'XXXXXXXXYYYYYYYY';
var merchant_salt = 'XXXXXXXXYYYYYYYY';

app.get("/list", function (req, res) {

    //  Start / End date. Maximum 31 days can be defined.
    var start_date = '2020-11-01 00:00:00';
    var end_date = '2020-11-29 23:59:59';


    //  ####################### 
    var paytr_token = crypto.createHmac('sha256', merchant_key).update(merchant_id + start_date + end_date + merchant_salt).digest('base64');

    var options = {
        'method': 'POST',
        'url': 'https://www.paytr.com/odeme/geri-donen-transfer',
        'headers': {
            'Content-Type': 'application/x-www-form-urlencoded'
        },
        form: {
            'merchant_id': merchant_id,
            'start_date': start_date,
            'end_date': end_date,
            'paytr_token': paytr_token,
        }
    };

    request(options, function (error, response, body) {
        if (error) throw new Error(error);
        var res_data = JSON.parse(body);

        if (res_data.status == 'success') {

            /*

            [ref_no] => 1000001
            [date_detected] => 2020-06-10
            [date_reimbursed] => 2020-06-08
            [transfer_name] => EXP NAME
            [transfer_iban] => TR100000000000000000000001
            [transfer_amount] => 35.18
            [transfer_currency] => TL
            [transfer_date] => 2020-06-08

            */
         
            res.send(res_data);

        } else {
            
            // Error

            console.log(response.body);
            res.end(response.body);
        }

    });


});

app.get("/send", function (req, res) {

    var trans_id = '';
    var trans_info = [{
        'amount': '1283',
        'receiver': 'XYZ LTD ŞTİ',
        'iban': 'TRXXXXXXXXXXXXXXXXXXXXX'
    }];

    var paytr_token = crypto.createHmac('sha256', merchant_key).update(merchant_id + trans_id + merchant_salt).digest('base64');

    var options = {
        'method': 'POST',
        'url': 'https://www.paytr.com/odeme/hesaptan-gonder',
        'headers': {
            'Content-Type': 'application/x-www-form-urlencoded'
        },
        form: {

            'trans_info': JSON.stringify(trans_info),
            'trans_id': trans_id,
            'paytr_token': paytr_token,
            'merchant_id': merchant_id,

        }
    };

    request(options, function (error, response, body) {
        if (error) throw new Error(error);
        var res_data = JSON.parse(body);

        if (res_data.status == 'success') {

            res.send(response.body);


        } else {

            res.end(response.body);



        }

    });


});

app.post("/callback", function (req, res) {
    var callback = req.body;

    var paytr_token = crypto.createHmac('sha256', merchant_key).update(callback.merchant_id + callback.trans_id + merchant_salt).digest('base64');

    if (paytr_token != callback.hash) {
        throw new Error("PAYTR notification failed: bad hash");
    }

    var processed_result = JSON.parse(callback.processed_result);

    for (const [key, value] of Object.entries(processed_result)) {
        console.log(`${key}: ${value}`);
    }

    res.send("OK"); // Notify the PayTR system that the notification has been received.  
});


var port = 3200;
app.listen(port, function () {
    console.log("Server is running. Port:" + port);
});
