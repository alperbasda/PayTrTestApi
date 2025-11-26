<?php

// Mağaza bilgileri
 $merchant_id = '';
 $merchant_key = '';
 $merchant_salt = '';

// Kullanıcı bilgileri
if ( isset($_SERVER["HTTP_CLIENT_IP"]) ) {
    $ip = $_SERVER["HTTP_CLIENT_IP"];
} elseif ( isset($_SERVER["HTTP_X_FORWARDED_FOR"]) ) {
    $ip = $_SERVER["HTTP_X_FORWARDED_FOR"];
} else {
    $ip = $_SERVER["REMOTE_ADDR"];
}

$user_name    = 'TEST TEST';
$email        = "testpreauth@example.com";
$user_address = 'TEST ADRES';
$user_phone   = '5555555555';
$user_ip      = $ip;
$user_basket  = htmlentities(json_encode([
    [ 'Test Product 1', '18.00', 1 ],
    [ 'Test Product 2', '33,25', 2 ],
    [ 'Test Product 3', '45,42', 1 ]
])); // Ürün ismi, Fiyat, Adet

// Kart bilgileri
$cc_owner  = 'PAYTR TEST';
$card_number = '9792030394440796';
$expiry_month  = '12';
$expiry_year   = '30';
$cvv       = '000';


// İşlem bilgileri
$merchant_oid      = 'TESTPREAUTH'.time();
$currency          = 'TL';
$payment_amount    = '10';
$payment_type      = 'card';
$non_3d            = 0;
$non3d_test_failed = 0;
$installment_count = 1;// Ön provizyonda taksitli işlem yapılamaz. Bu nedenle 1 veya 0 gönderilmelidir.
$client_lang       = 'tr';
$test_mode         = 1; //Test modunda işlemler şuan için desteklenmemektedir. test_mode "0" ileterek, işlemlerinizi canlı modda ve gerçek kart bilgileri ile gerçekleştiriniz.
$store_card = 1;
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
    'merchant_ok_url'=>$merchant_ok_url,
    'merchant_fail_url'=>$merchant_fail_url,
    'payment_amount'    => $payment_amount,
    'payment_type'      => $payment_type,
    'installment_count' => $installment_count,
    'test_mode'         => $test_mode,
    'client_lang'       => $client_lang,
	'expiry_month'       => $expiry_month,
	'expiry_year'       => $expiry_year,
	'expiry_year'       => $expiry_year,
	'cvv'       		=> $cvv,
	'card_number'       => $card_number,
	'cc_owner'       	=> $cc_owner,
	'store_card'       	=> $store_card
	
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

echo $result;