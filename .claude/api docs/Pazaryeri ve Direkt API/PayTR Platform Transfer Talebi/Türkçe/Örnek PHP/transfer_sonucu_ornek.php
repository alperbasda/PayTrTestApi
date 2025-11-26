<?php

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
    $post["trans_ids"]=str_replace("\\", "", $post["trans_ids"]);
	$hash = base64_encode( hash_hmac('sha256', $post['trans_ids'].$merchant_salt, $merchant_key, true) );
	#
	## Oluşturulan hash'i, PayTR'dan gelen post içindeki hash ile karşılaştır (isteğin PayTR'dan geldiğine ve değişmediğine emin olmak için)
	## Bu işlemi güvenlik nedeniyle mutlaka yapmanız gerekiyor.
	if( $hash != $post['hash'] )
		die('PAYTR notification failed: bad hash');
	###########################################################################

	## $post['trans_ids'] içerisinde daha önce PayTR'a ilettiğiniz transfer taleplerinden tamamlanan transferlerin trans_id bilgileri JSON formatında gelir
	## trans_id bilgisi transfer talebi yaparken PayTR'a gönderdiğiniz her işlem için eşsiz değerdir
	$trans_ids = json_decode($post['trans_ids'],1);
	foreach($trans_ids as $trans_id)
	{
		## Örn: Burada $trans_id ile veritabanınızdan transfer talebini tespit edip ilgili kullanıcınıza bilgilendirme gönderebilirsiniz (email, sms vb.)
	}

	## Bildirimin alındığını PayTR sistemine bildir.
	echo "OK";
	exit;
?>