<?php
require "common.php";



$pwd = null;
$username = null;
$backLink = $_SERVER["HTTP_REFERER"];
if( isset($_COOKIE[$cookieNameUser]) && isset($_COOKIE[$cookieNamePwd]) )
{
   $username = $_COOKIE[$cookieNameUser];
   $pwd = $_COOKIE[$cookieNamePwd];
}
if( isset($_POST['username']) )
{
   $username = $_POST['username'];
   $pwd = $_POST['password'];
}
elseif( isset($_GET['continuation']) )
{
   $backLink = $_GET['continuation'];
}

if( !empty($username) )
{
   if (($handle = fopen(dirname(__FILE__) . "/secure/passwords.txt", "r")) !== FALSE) {
      while ( ($data = fgetcsv($handle) ) !== FALSE ) {
        //process          
          if( strcasecmp($data[0],$username)==0 && strcmp($pwd,$data[1])==0 )
          {
             $loggedIn = true;    
             if( !empty($_POST["remember"]) )
             {
                 setcookie($cookieNameUser, $username, time() + (86400 * 300), "/", $websiteDomain); 
                 setcookie($cookieNamePwd, $pwd, time() + (86400 * 300), "/", $websiteDomain); 
                 header("Location: /" . $secureVirtualHostName . "/upload.php");
                 die();
             }
             else if(!empty($_POST))
             {
                 setcookie($cookieNameUser, $username, 0, "/", $websiteDomain); 
                 setcookie($cookieNamePwd, $pwd, 0, "/", $websiteDomain); 
                 header("Location: /" . $secureVirtualHostName . "/upload.php");
                 die();
             }             
             break;
          }
      }
   }
}

$aantalFiles = 10;
if( $loggedIn )
{
  $msg = "";
  for($i=0; $i<$aantalFiles; $i++)
  {
    if(!empty($_FILES['uploaded_file' . $i]))
    {
      $curFile = $_FILES['uploaded_file' . $i];
      if( !empty($curFile['name']))
      {
        $sourceFileName = basename( $curFile['name']);
        $path = dirname(__FILE__) . "/uploads/" . date("Y-m-d-His") . "_". $username . "_" . $sourceFileName;
        if($msg)
        {
            $msg = $msg . "<br/>\r\n";
        }
        if(move_uploaded_file($curFile['tmp_name'], $path)) {
          $msg = $msg . "The file <b>".  $sourceFileName. "</b> has been uploaded.";
        } else{
          $msg = $msg . "<b>There was an error uploading the file, please try again!</b>";
        }
      }
    }
  }
  if($msg)
  {
    $msg = "<p>" . $msg . "</p>";

    $mailMsg = "<style>* {font-family: \"Verdana\"; font-size:11px;}</style>".
            "<h1>File upload has been attempted by " . htmlspecialchars($username) . "</h1>" . $msg .
            "<p>Look in <a href=\"file://\\\\broodroosterXL\\web\\homecraft.nl\\sharing\\uploads\">\\\\broodroosterXL\\web\\homecraft.nl\\sharing\\uploads</a></p>";
    $headers = "From: " . $fromEmailAddress . "\r\n" .
        "Reply-To: " . $fromEmailAddress . "\r\n" .
        "MIME-Version: 1.0\r\n".
        "Content-Type: text/html; charset=ISO-8859-1\r\n".
        "X-Mailer: PHP/" . phpversion();
    mail($toEmailAddress, "Files Uploaded", $mailMsg, $headers);
  }
?>
<!DOCTYPE html>
<html>
<head>
  <title>Upload your files</title>
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <link rel="stylesheet" type="text/css" href="viewer.css" />
</head>
<body><?PHP 
echo $msg
?>
  <form enctype="multipart/form-data" action="upload.php?continuation=<?PHP
echo htmlspecialchars($backLink);
    ?>" method="POST">
  <div style="float:right"><a href="<?PHP 
echo htmlspecialchars($backLink)
?>">Back...</a></div>
    <h1>Upload your files</h1>
    <p>Select the file(s) to be uploaded:</p>
    <p><table><?PHP
    for($i=0; $i<$aantalFiles; $i++)
    {
        echo "<tr><td>File " . $i . ":</td><td><input type=\"file\" name=\"uploaded_file" . $i . "\"></input></td></tr>";
    }
    ?></table></p>
  <p>And press 'Upload' to start the upload</p>
    <input type="submit" value="Upload"></input>
  </form>
</body>
</html><?PHP
}
else
{
    ?><!DOCTYPE html>
<html>
<head>
<title>Upload</title>
</head>
<body>
<div class="container">
<h1>Fred's file share</h1>

<p>Please enter you credentials to upload</b></p>
<form action="upload.php" method="POST" onsubmit="">
<input type="hidden" name="confirm" value="1"/>
<table>
<tr><td><label for="username">User Name</label>:</td><td><input id="username" name="username" type="text"/></td></tr>
<tr><td><label for="password">Password</label>:</td><td><input id="password" name="password" type="password"/></td></tr>
<tr><td></td><td><input id="remember" name="remember" type="checkbox" value="1"/> <label for="remember">Remember my password</label></td></tr>
<tr><td></td><td><input id="butSubmit" type="submit" value="Continue to upload form..."/></td></tr>
</table>
</body>
</html><?PHP
}
?>
