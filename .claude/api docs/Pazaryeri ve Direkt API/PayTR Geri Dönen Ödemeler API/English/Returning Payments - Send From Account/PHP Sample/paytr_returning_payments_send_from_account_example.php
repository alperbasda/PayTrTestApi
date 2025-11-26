<?php

    $merchant_id    = 'XXXXXX';
    $merchant_key   = 'XXXXXXXXYYYYYYYY';
    $merchant_salt  = 'XXXXXXXXYYYYYYYY';
    #
    #
    $trans_id="PHG".time();
    $trans_info=array();
    //Sent by multiplying the amount by 100! E.g.: 12.83*100 = 1283
    $trans_info[]=array("amount"=>"1283",
        "receiver"=>"XYZ LTD ŞTİ",
        "iban"=>"TRXXXXXXXXXXXXXXXXXXXXX");
    //...$trans_info[]=...
    #
    ############################################################################################

    $paytr_token=base64_encode(hash_hmac('sha256',$merchant_id.$trans_id.$merchant_salt, $merchant_key, true));

    $post_vals=array('trans_info'=>json_encode($trans_info),
        'trans_id'=>$trans_id,
        'paytr_token'=>$paytr_token,
        'merchant_id'=>$merchant_id
    );
    #
    ############################################################################################

    $ch = curl_init();
    curl_setopt($ch, CURLOPT_URL, "https://www.paytr.com/odeme/hesaptan-gonder");
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

    if(curl_errno($ch))
    {
        echo curl_error($ch);
        curl_close($ch);
        exit;
    }

    curl_close($ch);

    $result_raw=$result;
    $result=json_decode($result,1);

    if($result[status]=='success')
    {
        print_r($result_raw);
    }
    else//status=>error
    {
        print_r($result_raw);
    }