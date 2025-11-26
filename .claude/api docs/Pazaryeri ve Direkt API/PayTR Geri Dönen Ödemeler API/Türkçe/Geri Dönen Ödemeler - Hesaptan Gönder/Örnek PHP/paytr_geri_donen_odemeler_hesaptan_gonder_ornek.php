<?php

    ########################### İŞLEM DÖKÜMÜ ALMAK  İÇİN ÖRNEK KODLAR ##########################
    #                                                                                          #
    ################################ DÜZENLEMESİ ZORUNLU ALANLAR ###############################
    #
    ## API Entegrasyon Bilgileri - Mağaza paneline giriş yaparak BİLGİ sayfasından alabilirsiniz.

    $merchant_id    = 'XXXXXX';
    $merchant_key   = 'XXXXXXXXYYYYYYYY';
    $merchant_salt  = 'XXXXXXXXYYYYYYYY';

    ## Gerekli Bilgiler
    #
    $trans_id="PHG".time();
    $trans_info=array();
    //amount 100 ile çarpılarak gönderilir!!
    $trans_info[]=array("amount"=>"1283",
        "receiver"=>"XYZ LTD ŞTİ",
        "iban"=>"TRXXXXXXXXXXXXXXXXXXXXX");
    //...$trans_info[]=...
    #
    ############################################################################################

    ################ Bu kısımda herhangi bir değişiklik yapmanıza gerek yoktur. ################

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

    //XXX: DİKKAT: lokal makinanızda "SSL certificate problem: unable to get local issuer certificate" uyarısı alırsanız eğer
    //aşağıdaki kodu açıp deneyebilirsiniz. ANCAK, güvenlik nedeniyle sunucunuzda (gerçek ortamınızda) bu kodun kapalı kalması çok önemlidir!
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
        //status ve trans_id içerir
        print_r($result_raw);
    }
    else//status=>error
    {
        //status ve err_no - err_msg içerir
        print_r($result_raw);
    }