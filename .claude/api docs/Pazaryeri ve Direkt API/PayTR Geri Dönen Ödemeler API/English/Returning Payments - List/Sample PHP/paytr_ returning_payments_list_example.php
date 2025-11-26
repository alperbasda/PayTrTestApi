<?php

    $merchant_id    = 'XXXXXX';
    $merchant_key   = 'XXXXXXXXYYYYYYYY';
    $merchant_salt  = 'XXXXXXXXYYYYYYYY';
    #
    #
    $start_date = "2020-05-20 00:00:00";
    $end_date = "2020-06-16 23:59:59";
    # Start / End date. A maximum of 31 days can be defined.
    #
    ############################################################################################

    $paytr_token = base64_encode(hash_hmac('sha256', $merchant_id . $start_date . $end_date . $merchant_salt, $merchant_key, true));

    $post_vals = array('merchant_id' => $merchant_id,
        'start_date' => $start_date,
        'end_date' => $end_date,
        'paytr_token' => $paytr_token
    );
    #
    ############################################################################################

    $ch = curl_init();
    curl_setopt($ch, CURLOPT_URL, "https://www.paytr.com/odeme/geri-donen-transfer");
    curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
    curl_setopt($ch, CURLOPT_POST, 1);
    curl_setopt($ch, CURLOPT_POSTFIELDS, $post_vals);
    curl_setopt($ch, CURLOPT_FRESH_CONNECT, true);
    curl_setopt($ch, CURLOPT_TIMEOUT, 90);
    curl_setopt($ch, CURLOPT_CONNECTTIMEOUT, 90);

    //ATTENTION: If you get “SSL certificate problem: unable to get local issuer certificate” warning on your local machine, you can open the following code and try it.
    //BUT, for security reasons it is very important to keep this code off on your server (in your real environment)!
    //curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, 0);

    $result = @curl_exec($ch);

    if (curl_errno($ch)) {
        echo curl_error($ch);
        curl_close($ch);
        exit;
    }

    curl_close($ch);

    $result = json_decode($result, 1);

    /*
      Response example returned in $result value;

    [ref_no] => 1000001
    [date_detected] => 2020-06-10
    [date_reimbursed] => 2020-06-08
    [transfer_name] => TEEST USER
    [transfer_iban] => TR100000000000000000000001
    [transfer_amount] => 35.18
    [transfer_currency] => TL
    [transfer_date] => 2020-06-08

    */

    if ($result[status] == 'success')
    {
        print_r($result);
    }
    elseif ($result[status] == 'failed')
    {
        echo "No transaction was found in the relevant date range.";
    }
    else
    {
        echo $result[err_no] . " - " . $result[err_msg];
    }