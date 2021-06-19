<?php

$secureVirtualHostName = "secdir";
$secureFolderTitle = "My file share";
$websiteDomain = "homecraft.nl";
$cookieNameUser = "secdiruser";
$cookieNamePwd = "secdirpwd";
$fromEmailAddress = "somebody@somewhere.org";
$toEmailAddress = "somebody@somewhere.org";

function isSecure() {
  return  (!empty($_SERVER['HTTPS']) && $_SERVER['HTTPS'] !== 'off')
         || $_SERVER['SERVER_PORT'] == 443;
}
function endsWith($haystack, $needle) {
    // search forward starting from end minus needle length characters
    return $needle === "" || (($temp = strlen($haystack) - strlen($needle)) >= 0 && strpos($haystack, $needle, $temp) !== FALSE);
}
function startsWith($haystack, $needle) {
    // search forward starting from end minus needle length characters
    return $needle === "" || strpos($haystack, $needle) === 0;
}
