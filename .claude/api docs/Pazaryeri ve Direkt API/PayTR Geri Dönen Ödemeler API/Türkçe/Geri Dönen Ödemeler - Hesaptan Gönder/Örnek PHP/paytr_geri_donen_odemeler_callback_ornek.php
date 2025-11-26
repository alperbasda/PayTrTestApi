<?php

	#################### POST içerisinde gelen örnek veriler ####################
	#
	// [mode] => cashout
	// -> Sabit bu şekilide gelir
	#
	// [hash] => wszlFsC7nrfCPvP77kdEzzE4smGdV4FWvDibKlXIpRM=,
	// -> Kontrolde kullanaılacaktır.
	#
	// [trans_id] => 12345aaabbb
	// -> Geri dönen ödeme hesaptan gönderme talebi yaparken PayTR'a gönderdiğiniz eşsiz değer.
	#
	// [processed_result] => [{\"amount\":484.48,\"receiver\":\"XYZ LTD STI\",\"iban\":\"TRXXXXXXXXXXXXXXXXXX\",\"result\":\"success\"}]
	// -> Geri dönen ödeme hesaptan gönderme talebi yaparken PayTR'a gönderdiğiniz değerler.
	#
	// [success_total] => 1
	// -> Başarıyla transfer edilen işlem sayısı (processed_result içerisinde, result:success olanların sayısı)
	#
	// [failed_total] => 0
	// -> Hata alan işlem sayısı (processed_result içerisinde, result:failed olanların sayısı)
	#
	// [transfer_total] => 484.48
	// -> Başarıyla tranasfer edilen işlemlerin toplam tutarı.
	#
	// [account_balance] => 0
	// -> Transferler sonrasında kalan alt hesap bakiyeniz.
	############################################################################

	$post = $_POST;

	####################### DÜZENLEMESİ ZORUNLU ALANLAR #######################
	#
	## API Entegrasyon Bilgileri - Mağaza paneline giriş yaparak BİLGİ sayfasından alabilirsiniz.
	$merchant_key 	= 'YYYYYYYYYYYYYY';
	$merchant_salt	= 'ZZZZZZZZZZZZZZ';
	###########################################################################

	####### Bu kısımda herhangi bir değişiklik yapmanıza gerek yoktur. #######
	#
	## POST değerleri ile hash oluştur.
	$hash = base64_encode( hash_hmac('sha256', $post['merchant_id'].$post['trans_id'].$merchant_salt, $merchant_key, true) );
	#
	## Oluşturulan hash'i, PayTR'dan gelen post içindeki hash ile karşılaştır (isteğin PayTR'dan geldiğine ve değişmediğine emin olmak için)
	## Bu işlemi güvenlik nedeniyle mutlaka yapmanız gerekiyor.
	if( $hash != $post['hash'] )
		die('PAYTR notification failed: bad hash');
	###########################################################################

	## trans_id bilgisi transfer talebi yaparken PayTR'a gönderdiğiniz her işlem için eşsiz değerdir.
	$processed_result = json_decode($post['processed_result'],1);
	foreach($processed_result as $trans)
	{
		// Burada her işlem için gerekli veri tabanı vb. işlemleri yapabilirsiniz.
	}

	## Bildirimin alındığını PayTR sistemine bildir.
	echo "OK";
	exit;
?>

