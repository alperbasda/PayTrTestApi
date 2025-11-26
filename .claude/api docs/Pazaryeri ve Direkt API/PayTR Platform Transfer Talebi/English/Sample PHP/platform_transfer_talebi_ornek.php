<?php

    $merchant_id = 'MERCHANT_ID';
    $merchant_key = 'XXXXXXXXXXX';
    $merchant_salt = 'YYYYYYYYYYY';
    
    $merchant_oid = "";

    $trans_id = time();

    $submerchant_amount = "";

    $total_amount = "";

    $transfer_name = "";

    $transfer_iban = "";

    $hash_str = $merchant_id . $merchant_oid . $trans_id . $submerchant_amount . $total_amount . $transfer_name . $transfer_iban;
    $token = base64_encode(hash_hmac('sha256',$hash_str.$merchant_salt,$merchant_key,true));

    $post_vals=array(
            'merchant_id'=>$merchant_id,
            'merchant_oid'=>$merchant_oid,
            'trans_id'=>$trans_id,
            'submerchant_amount'=>$submerchant_amount,
            'total_amount'=>$total_amount,
            'transfer_name'=>$transfer_name,
            'transfer_iban'=>$transfer_iban,
            'paytr_token'=>$token
        );

    $ch=curl_init();
    curl_setopt($ch, CURLOPT_URL, "https://www.paytr.com/odeme/platform/transfer");
    curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
    curl_setopt($ch, CURLOPT_POST, 1) ;
    curl_setopt($ch, CURLOPT_POSTFIELDS, $post_vals);
    curl_setopt($ch, CURLOPT_FRESH_CONNECT, true);
    curl_setopt($ch, CURLOPT_TIMEOUT, 20);

    $result = @curl_exec($ch);

    if(curl_errno($ch))
        die("PAYTR platform transfer request connection error. err:".curl_error($ch));

    curl_close($ch);

    $result=json_decode($result,1);

    if($result[status]=='success')
    {
        //DB actions
    }
    else
    {
        echo $result[err_no]." - ".$result[err_msg];
    }
    #########################################################################

?>