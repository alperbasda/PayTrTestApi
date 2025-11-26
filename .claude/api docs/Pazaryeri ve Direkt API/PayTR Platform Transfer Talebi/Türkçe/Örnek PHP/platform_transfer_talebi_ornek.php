<?php

    $merchant_id = 'MAGAZA_NO';
    $merchant_key = 'XXXXXXXXXXX';
    $merchant_salt = 'YYYYYYYYYYY';

    // Mağaza sipariş no: Satış işlemi için belirlediğiniz benzersiz sipariş numarası
    $merchant_oid = "";

    // Satıcıya yapılacak bu ödemenin takibi için benzersiz takip numarası
    $trans_id = time();

    // Satıcıya yapılacak ödeme tutarı: Satıcıya bu sipariş için ödenecek tutarın 100 ile çarpılmış hali (Örnek: 50.99 TL için 5099)
    $submerchant_amount = "";

    // Toplam ödeme tutarı: Siparişe ait toplam ödeme tutarının 100 ile çarpılmış hali (Örnek: 50.99 TL için 5099)
    $total_amount = "";

    // Satıcının banka hesabı için ad soyad/ünvanı
    $transfer_name = "";

    // Satıcının banka hesabı IBAN numarası
    $transfer_iban = "";

    // İsteğin sizden geldiğine ve içeriğin değişmediğine emin olmamız için oluşturacağınız değerdir
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

    //XXX: DİKKAT: lokal makinanızda "SSL certificate problem: unable to get local issuer certificate" uyarısı alırsanız eğer
    //aşağıdaki kodu açıp deneyebilirsiniz. ANCAK, güvenlik nedeniyle sunucunuzda (gerçek ortamınızda) bu kodun kapalı kalması çok önemlidir!
    //curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, 0);

    $result = @curl_exec($ch);

    if(curl_errno($ch))
        die("PAYTR platform transfer request connection error. err:".curl_error($ch));

    curl_close($ch);

    $result=json_decode($result,1);

    /*
        Başarılı yanıt örneği:
        {"status":"success", "merchant_amount":"5", "submerchant_amount":"92", "trans_id":"45ABT34", "reference":"12SF45" }

        Başarısız yanıt örneği:
        {"status":"error", "err_no":"010", "err_msg":"toplam transfer tutarı kalan tutardan fazla olamaz"}
    */

    if($result[status]=='success')
    {
        //VT işlemleri vs.
    }
    else
    {
        echo $result[err_no]." - ".$result[err_msg];
    }
    #########################################################################

?>