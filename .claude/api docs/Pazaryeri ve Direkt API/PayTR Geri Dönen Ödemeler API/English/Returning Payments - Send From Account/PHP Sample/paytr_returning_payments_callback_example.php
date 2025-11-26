<?php

	#################### Sample data from POST #####################
	#
	// [mode] => cashout
	// -> Fixed comes like this
	#
	// [hash] => wszlFsC7nrfCPvP77kdEzzE4smGdV4FWvDibKlXIpRM =,
	// -> It will be used in control.
	#
	// [trans_id] => 12345aaabbb
	// -> The unique value you send to PayTR when requesting a return payment from the account.
	#
	// [processed_result] => [{\ "amount \": 484.48, \ "receiver \": \ "XYZ LTD STI \", \ "iban \": \ "TRXXXXXXXXXXXXXXXXXX \", \ "result \": \ "success \"}]
	// -> The values you sent to PayTR when making a return request from the payment account.
	#
	// [success_total] => 1
	// -> Number of transactions successfully transferred (number of those with result: success in processed_result)
	#
	// [failed_total] => 0
	// -> Number of transactions with errors (number of those with result: failed in processed_result)
	#
	// [transfer_total] => 484.48
	// -> Total amount of successfully transferred transactions.
	#
	// [account_balance] => 0
	// -> Sub-account balance remaining after transfers.
	#
	############################################################################

	$post = $_POST;
	#
	#
	$merchant_key 	= 'YYYYYYYYYYYYYY';
	$merchant_salt	= 'ZZZZZZZZZZZZZZ';
	###########################################################################

	#
	$hash = base64_encode( hash_hmac('sha256', $post['merchant_id'].$post['trans_id'].$merchant_salt, $merchant_key, true) );
	#
	if( $hash != $post['hash'] )
		die('PAYTR notification failed: bad hash');
	###########################################################################

	$processed_result = json_decode($post['processed_result'],1);
	foreach($processed_result as $trans)
	{
		// Here you can perform the necessary database and similar operations for each transaction.
	}

	echo "OK";
	exit;
?>

