<?php

// Mağaza bilgileri
    $merchant_id = '';
        $merchant_key = '';
        $merchant_salt = ''; 

// Kullanıcı bilgileri

$ip="::1";

$email        = "testpreauth@example.com";
$user_name    = 'TEST TEST';
$user_address = 'TEST ADRES';
$user_phone   = '5555555555';
$user_ip      = $ip;
$user_basket  = htmlentities(json_encode([
    [ 'Test Product 1', '18.00', 1 ],
    [ 'Test Product 2', '33,25', 2 ],
    [ 'Test Product 3', '45,42', 1 ]
])); // Ürün ismi, Fiyat, Adet

// Kart bilgileri
$cc_owner  = '';
$cc_number = '';
$cc_month  = '';
$cc_year   = '';
$cvv       = '';

// İşlem bilgileri
$merchant_oid      = 'TESTPREAUT2H2217223224813322323283822222';
$currency          = 'TL';
$payment_amount    = '1';
$payment_type      = 'card';
$non_3d            = 0;
$non3d_test_failed = 0;
$card_type         = 'axess'; // advantage, axess, combo, bonus, cardfinans, maximum, paraf, world, saglamkart
$installment_count = 0;
$client_lang       = 'tr';
$test_mode         = 0;
$merchant_ok_url="https://basarili.com";
$merchant_fail_url="https://basarisiz.com";


// paytr_token
$hash_str    = $merchant_id . $user_ip . $merchant_oid . $email . $payment_amount . $payment_type . $installment_count . $currency . $test_mode . $non_3d;
$paytr_token = base64_encode(hash_hmac('sha256', $hash_str . $merchant_salt, $merchant_key, TRUE));
    

$payload = [
    'auth_type'         => 'preauth',
    'merchant_id'       => $merchant_id,
    'paytr_token'       => $paytr_token,
    'non_3d'            => $non_3d,
    'non3d_test_failed' => $non3d_test_failed,
    'email'             => $email,
    'user_name'         => $user_name,
    'user_address'      => $user_address,
    'user_phone'        => $user_phone,
    'user_ip'           => $user_ip,
    'user_basket'       => $user_basket,
    'merchant_oid'      => $merchant_oid,
    'currency'          => $currency,
    'payment_amount'    => $payment_amount,
    'payment_type'      => $payment_type,
    'cc_owner'          => $cc_owner,
    'card_number'       => $cc_number,
    'expiry_month'      => $cc_month,
    'expiry_year'       => $cc_year,
    'cvv'               => $cvv,
    'card_type'         => $card_type,
    'installment_count' => $installment_count,
    'test_mode'         => $test_mode,
    'client_lang'       => $client_lang,
    'merchant_ok_url' =>$merchant_ok_url,
    'merchant_fail_url'=>$merchant_fail_url
];

$ch = curl_init();
curl_setopt($ch, CURLOPT_URL, 'https://www.paytr.com/odeme/auth');
curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
curl_setopt($ch, CURLOPT_POST, 1);
curl_setopt($ch, CURLOPT_POSTFIELDS, $payload);
curl_setopt($ch, CURLOPT_FRESH_CONNECT, TRUE);
curl_setopt($ch, CURLOPT_TIMEOUT, 20);

$result = @curl_exec($ch);
curl_close($ch);
$result = json_decode($result, 1);

//Aşağıdaki alanı aktif etmeniz durumunda bu alanı yorum satırı yapabilirsiniz.
print_r($result);

//Doğrudan 3D sayfasına yönlendirme sağlamak için aşağıdaki alanı kullanabilirsiniz. 
// if ($result['status'] == 'success') {
//     if ($result['html_content']) {
//         echo base64_decode($result['html_content']);
//     }
//     else {
//         echo json_encode($result);
//     }
// }
// else {
//     echo json_encode($result);
// }
exit;