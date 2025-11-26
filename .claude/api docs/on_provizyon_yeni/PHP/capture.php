<?php

// Mağaza bilgileri
$merchant_id   = 'MAGAZA_NO';
$merchant_key  = 'XXXXXXXXXXX';
$merchant_salt = 'YYYYYYYYYYY';

// Kullanıcı bilgileri
if( isset( $_SERVER["HTTP_CLIENT_IP"] ) ) {
    $ip = $_SERVER["HTTP_CLIENT_IP"];
} elseif( isset( $_SERVER["HTTP_X_FORWARDED_FOR"] ) ) {
    $ip = $_SERVER["HTTP_X_FORWARDED_FOR"];
} else {
    $ip = $_SERVER["REMOTE_ADDR"];
}

// İşlem bilgileri
$payment_amount = '14.55';
$reference_no   = 'AXXXXXXXXXXXXXXXX';
$merchant_oid   = 'TESTPREAUTHXXX';
$client_lang    = 'tr';
$test_mode      = 0;

// paytr_token
$hash_str    = $merchant_id . $ip . $merchant_oid . $reference_no . $payment_amount;
$paytr_token = base64_encode(hash_hmac('sha256', $hash_str . $merchant_salt, $merchant_key, TRUE));

$payload = [
    'auth_type'      => 'capture',
    'merchant_id'    => $merchant_id,
    'paytr_token'    => $paytr_token,
    'merchant_oid'   => $merchant_oid,
    'reference_no'   => $reference_no,
    'payment_amount' => $payment_amount,
    'client_ip'      => $ip,
    'client_lang'    => $client_lang,
    'test_failed'    => $test_mode
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