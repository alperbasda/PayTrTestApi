<?php

	$post = $_POST;

	$merchant_key 	= 'YYYYYYYYYYYYYY';
	$merchant_salt	= 'ZZZZZZZZZZZZZZ';
    #
    $post["trans_ids"]=str_replace("\\", "", $post["trans_ids"]);
	$hash = base64_encode( hash_hmac('sha256', $post['trans_ids'].$merchant_salt, $merchant_key, true) );
	#
	if( $hash != $post['hash'] )
		die('PAYTR notification failed: bad hash');

	$trans_ids = json_decode($post['trans_ids'],1);
	foreach($trans_ids as $trans_id)
	{
		// Actions
	}

	echo "OK";
	exit;
?>