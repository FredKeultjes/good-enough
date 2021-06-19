<?php
//error_reporting(-1);
//ini_set('display_errors', 'On');
require "common.php";


define('CHUNK_SIZE', 1024*1024); // Size (in bytes) of tiles chunk

// Read a file and display its content chunk by chunk
function readfile_chunked($filename, $retbytes = TRUE) {
    $buffer = '';
    $cnt    = 0;
    $handle = fopen($filename, 'rb');

    if ($handle === false) {
        return false;
    }

    while (!feof($handle)) {
        $buffer = fread($handle, CHUNK_SIZE);
        echo $buffer;
        ob_flush();
        flush();

        if ($retbytes) {
            $cnt += strlen($buffer);
        }
    }

    $status = fclose($handle);

    if ($retbytes && $status) {
        return $cnt; // return num. bytes delivered like readfile() does.
    }

    return $status;
}

function downloadFile($downloadPath, $username)
{
  $pathLog = dirname(__FILE__) . "/downloads-" . date("Y-m") . ".log";
  file_put_contents($pathLog, date("Y-m-d H:i:s") . "\t". $_SERVER['REMOTE_ADDR'] . "\t" . $username . "\t" . $_SERVER['HTTP_USER_AGENT']. "\t" . substr($downloadPath,strlen(dirname(__FILE__)) ) . "\t". filesize($downloadPath) . "\r\n", FILE_APPEND | LOCK_EX);

  $ext = strtolower(pathinfo($downloadPath, PATHINFO_EXTENSION));

  if( $ext === "html" )
  {
    header('Content-Type: text/html');      
  }
  elseif( $ext === "css" )
  {
    header('Content-Type: text/css');      
  }
  elseif( $ext === "jpg" )
  {
    header('Content-Type: image/jpeg'); 
  }
  else {      
    header('Content-Description: File Transfer');
    header('Content-Type: application/octet-stream');
    header('Content-Disposition: attachment; filename="'.basename($downloadPath).'"');
    header('Expires: 0');
    header('Cache-Control: must-revalidate');
    header('Pragma: public');
  }
  header('Content-Length: ' . filesize($downloadPath));
  readfile_chunked($downloadPath);
  die();
}

function downloadCss()
{
  header('Content-Description: File Transfer');
  header('Content-Type: text/css');
  header('Content-Disposition: attachment; filename="viewer.css"');
  header('Expires: 600');
  header('Pragma: public');
  header('Content-Length: ' . filesize($downloadPath));

  echo "*,body,td {font-family:Georgia,Times New Roman} ".
       ".wrap {margin: 0 auto; width: 90%;} ".
       "h1 {font-size:26px} h2 {font-size:22px}" .
       ".subs {font-size:14px} " .
       ".supr {font-size:14px} " .
       "h1 .subs {display:inline; padding-left:20px; font-size: 15px; font-weight:normal} " .
       ".section {border: 1px solid #f0e0e0; padding:10px} " .
       ".prev200 {width:200px;height:200px; display:inline-block; background-repeat:no-repeat;}".
       ".prev800 {width:800px;display:inline-block; background-repeat:no-repeat;}".
       ".clear {clear:both;}".
       
       "body.imgviewer table {margin:0 auto;}".
       "body.imgviewer td {vertical-align:middle;height:100%;}".
       "body.imgviewer td.prev a, body.imgviewer td.next a {vertical-align:middle; height:100%; display:table-cell; padding:10px;}".
       "body.imgviewer td.prev a img, body.imgviewer td.next a img {min-width:100px;}".
       "body.imgviewer td.prev a:hover, body.imgviewer td.next a:hover {background-color:#fff0f0;}".
       "body.imgviewer td.prev div.supr, body.imgviewer td.next div.supr {width:200px; text-align:center;margin:0 0 5px 0;}".
       "body.imgviewer td.prev div.subs, body.imgviewer td.next div.subs {width:200px; text-align:center;margin:5px 0 0 0;}".
       "body.imgviewer td.main a img{width:800px;}".
           
       "body.index div.img {float:left;margin:10px; width:220px; height:220px;;} ".
       "body.index div.img .subs{width:200px; text-align:center;} ".
       "body.index li span {padding-left:10px;} ";
    
  
  die();
}

function fileSizeFormatted($filePath)
{
    $bytes = filesize($filePath);
    if ($bytes >= 1073741824)
    {
        $bytes = number_format($bytes / 1073741824, 2) . ' GB';
    }
    elseif ($bytes >= 1048576)
    {
        $bytes = number_format($bytes / 1048576, 2) . ' MB';
    }
    elseif ($bytes >= 1024)
    {
        $bytes = number_format($bytes / 1024, 2) . ' KB';
    }
    elseif ($bytes > 1)
    {
        $bytes = $bytes . ' bytes';
    }
    elseif ($bytes == 1)
    {
        $bytes = $bytes . ' byte';
    }
    else
    {
        $bytes = '0 bytes';
    }

    return $bytes;
}

function isImageExt($ext)
{
    return $ext==="jpg" || $ext==="jpeg" || $ext==="gif" || $ext==="png" || $ext==="tif" || $ext==="tiff";
}
function isVideoExt($ext)
{
    return $ext==="mp4";
}
function isAudioExt($ext)
{
    return $ext==="mp3";
}
function isFileNameToDiscard($fileName)
{
    return startsWith($fileName, "thmh200_") || startsWith($fileName, "thmv200_") || startsWith($fileName, "thmv800_") || $fileName === "thm_index.js" || $fileName == "passwords.txt" || $fileName === "thm_index.js" || $fileName == "indexbody.html";
}

function generateIndex($filePath)
{
    $numberOfImagesPerRow = 5;
    $dirPath = "";
    if( endsWith($filePath,"/index.html") )
    {
        $dirPath = substr($filePath,0,strlen($filePath)-11);
    }
    else
    {
        if( !endsWith($filePath, "/"))
        {
            header("Location: " . basename($filePath) . "/index.html");
            die();
        }
        $dirPath = $filePath;
    }
    $pageName = basename($dirPath);
    $parentDirPath = dirname($dirPath);
    $parentDirName = basename($parentDirPath);
    
    echo "<!DOCTYPE html><html><head><meta http-equiv=\"content-type\" content=\"text/html; charset=UTF-8\" /><title>" . htmlspecialchars($pageName) . "</title><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"><link rel=\"stylesheet\" type=\"text/css\" href=\"viewer.css\" /><script src=\"thm_index.js\"></script></head><body class=\"index\"><div class=\"wrap\">";
    
    if( $parentDirName !== "secure")
    {
        echo "<a href=\"../index.html\">To Parent Directory: <b>" . htmlspecialchars($parentDirName). "</b></a>";
    }
    echo "<div style=\"float:right\"><a href=\"/". $GLOBALS["secureVirtualHostName"] . "/upload.php\">Upload...</a></div>";
    echo "<h1>" . htmlspecialchars($pageName) . "</h1>";
    
    $indexbodyPath = $dirPath . "/indexbody.html";
    if( file_exists($indexbodyPath) )
    {
        echo file_get_contents($indexbodyPath); 
    }
    
    $files = scandir($dirPath);
    $imageFiles = array();
    $audioFiles = array();
    $videoFiles = array();
    $subDirs = array();
    $otherFiles = array();
    foreach($files as $curFileName)
    {
        $curPath = $dirPath . "/". $curFileName;
        $ext = strtolower(pathinfo($curFileName, PATHINFO_EXTENSION));
        if(isImageExt($ext))
        {
            array_push($imageFiles, $curPath);
        }
        elseif(isAudioExt($ext))
        {
            array_push($audioFiles, $curPath);
        }
        //elseif(isVideoExt($ext))
        //{
        //    array_push($videoFiles, $curPath);
        //}
        elseif(substr($curFileName,0,1)!="." && substr($curFileName,0,1)!="@")
        {
            if( is_dir($curPath))
            {
                array_push($subDirs, $curPath);
            }
            else
            {
                array_push($otherFiles, $curPath);
            }
        }
    }
    
    if(count($subDirs))
    {
        echo "<div class=\"container\"><h2>Subdirectories</h2><div class=\"section\"><ul>";
        foreach($subDirs as $curPath)
        {
            $temp = basename($curPath);
            echo "<li><a href=\"" . htmlspecialchars($temp . "/index.html") . "\">" . htmlspecialchars($temp) . "</a></li>";
        }
        echo "</ul></div></div>";
    }
    if(count($otherFiles))
    {
        $first = true;
        foreach($otherFiles as $curPath)
        {
            $temp = basename($curPath);
            if( !isFileNameToDiscard($temp) )
            {
                if( $first )
                {
                    echo "<div class=\"container\"><h2>Files</h2><div class=\"section\"><ul>";
                    $first = false;
                }
                echo "<li><a href=\"" . htmlspecialchars($temp) . "\">" . htmlspecialchars($temp) . "</a><span>" . fileSizeFormatted($curPath) . "</span></li>";
            }
        }
        if( !$first )
        {
            echo "</ul></div></div>";
        }
    }
    
    if(count($audioFiles))
    {
        $first = true;
        foreach($audioFiles as $curPath)
        {
            $temp = basename($curPath);
            if( !isFileNameToDiscard($temp) )
            {
                if( $first )
                {
                    echo "<div class=\"container\"><h2>Audiobestanden</h2><div class=\"section\">";
                    $first = false;
                }
                else
                {
                    echo "<hr/>";
                }
                echo "<div class=\"video\"><h3>" . htmlspecialchars($temp) . "</h3><audio height=\"300\" controls><source src=\"" . htmlspecialchars($temp) . "\"/></audio>&nbsp;<a href=\"" . htmlspecialchars($temp) . "\">Download...</a> <span>(" . fileSizeFormatted($curPath) . ")</span></div>";
            }
        }
        if( !$first )
        {
            echo "</div></div>";
        }
    }
    if(count($videoFiles))
    {
        $first = true;
        foreach($videoFiles as $curPath)
        {
            $temp = basename($curPath);
            if( !isFileNameToDiscard($temp) )
            {
                if( $first )
                {
                    echo "<div class=\"container\"><h2>Videos</h2><div class=\"section\">";
                    $first = false;
                }
                echo "<div class=\"video\"><h3>" . htmlspecialchars($temp) . "</h3><video height=\"300\" controls><source src=\"" . htmlspecialchars($temp) . "\"/></video>&nbsp;<a href=\"" . htmlspecialchars($temp) . "\">Download...</a> <span>(" . fileSizeFormatted($curPath) . ")</span></div>";
            }
        }
        if( !$first )
        {
            echo "</div></div>";
        }
    }
    if(count($imageFiles))
    {
        echo "<div class=\"container\"><h2>Images</h2><div class=\"section\">";
        $index = 0;
        foreach($imageFiles as $curPath)
        {
            $temp = basename($curPath);
            if(!isFileNameToDiscard($temp))
            {
                echo "<div class=\"img\"><a href=\"" . htmlspecialchars($temp) . ".prv.html\">".
                     "<script>cur = thm200index[\"" . $temp .
                     "\"];document.write('<span class=\"prev200\" style=\"margin:'+((200-cur.h)/2)+'px '+((200-cur.w)/2)+'px;background-image:url(' + cur.sheet + ');background-position:'+(-cur.x)+'px '+(-cur.y)+'px;width:'+cur.w+'px;height:'+cur.h+'px\"></span>');</script>".
                        "</a><div class=\"subs\">" 
                        . htmlspecialchars($temp) . ", " . fileSizeFormatted($curPath) . "</div></div>"; 
                $index++;
            }
        }
        echo "<div class=\"clear\"></div>";
        
        echo "</div></div>";
    }
    echo "</div></body></html>";
}



function generateImageViewer($downloadPath)
{
    $filePath = "";
    if( endsWith($downloadPath,".prv.html") )
    {
        $filePath = substr($downloadPath,0,strlen($downloadPath)-9);
    }
    else
    {
        $filePath = $downloadPath;
    }
    $fileName = basename($filePath);
    $dirPath = dirname($filePath);
    $dirName = basename($dirPath);
    
    echo "<!DOCTYPE html><html><head><meta http-equiv=\"content-type\" content=\"text/html; charset=UTF-8\" /><title>" . htmlspecialchars($fileName) . "</title><link rel=\"stylesheet\" type=\"text/css\" href=\"viewer.css\" /><script src=\"thm_index.js\"></script></head><body class=\"imgviewer\"><div class=\"wrap\">";
    
    echo "<a href=\"index.html\">To Index of: <b>" . htmlspecialchars($dirName) . "</b></a>";
    echo "<div class=\"container\"><h1>" . htmlspecialchars($dirName . ": " . $fileName) . "<div class=\"subs\">" . fileSizeFormatted($filePath) . "</div></h1>";
    
    $files = scandir($dirPath);
    $imageFiles = array();
    $prevFilePath = $null;
    $nextFilePath = $null;
    $found = false;
    foreach($files as $curFileName)
    {
        $curPath = $dirPath . "/". $curFileName;
        $ext = strtolower(pathinfo($curFileName, PATHINFO_EXTENSION));
        if(isImageExt($ext) && !isFileNameToDiscard($curFileName))
        {
            if( !$found )
            {
                if( $curPath == $filePath )
                {
                    $found = true;
                }
                else
                {
                    $prevFilePath = $curPath;
                }
            }
            else
            {
                $nextFilePath = $curPath;
                break;
            }
        }
        
    }
    
    echo "<div class=\"section\"><table><tbody><tr><td class=\"prev\">";
    if( $prevFilePath )
    {
        $prevFileName = basename($prevFilePath);
        echo "<a href=\"" . htmlspecialchars($prevFileName) . ".prv.html\"><div class=\"supr\">Previous</div><script>cur = thm200index[\"" . $prevFileName .
                     "\"];document.write('<span class=\"prev200\" style=\"margin:'+((200-cur.h)/2)+'px '+((200-cur.w)/2)+'px;background-image:url(' + cur.sheet + ');background-position:'+(-cur.x)+'px '+(-cur.y)+'px;width:'+cur.w+'px;height:'+cur.h+'px;\"></span>');</script><div class=\"subs\">" . htmlspecialchars($prevFileName). "</div></a>";
    }
    echo "</td><td class=\"main\"><div class=\"img\"><a href=\"" . htmlspecialchars($fileName) . "\" title=\"" . 
            htmlspecialchars($fileName) . " (" . fileSizeFormatted($filePath) . ")\"><script>cur = thm800index[\"" . $fileName .
                     "\"];document.write('<span class=\"prev800\" style=\"background-image:url(' + cur.sheet + ');background-position:'+(-cur.x)+'px '+(-cur.y)+'px; height:'+cur.h+'px;\"></span>');</script></a></div></td><td class=\"next\">";
    if( $nextFilePath )
    {
        $nextFileName = basename($nextFilePath);
        echo "<a href=\"" . htmlspecialchars($nextFileName) . ".prv.html\"><div class=\"supr\">Next</div><script>cur = thm200index[\"" . $nextFileName .
                     "\"];document.write('<span class=\"prev200\" style=\"margin:'+((200-cur.h)/2)+'px '+((200-cur.w)/2)+'px;background-image:url(' + cur.sheet + ');background-position:'+(-cur.x)+'px '+(-cur.y)+'px;width:'+cur.w+'px;height:'+cur.h+'px;\"></span>');</script><div class=\"subs\">". htmlspecialchars($nextFileName) ."</div></a>";
    }
    echo "</td></tr></table>";
    echo "</div></div></div><hr><p><a href=\"/" . $GLOBALS["secureVirtualHostName"] . "/upload.php\">Upload...</a></p></body></html>";
}

if( !isSecure() )
{
    echo "site not secure";
    die();
}

$ip = $_SERVER['REMOTE_ADDR'];

// get relative url (including querystring)
$url = urldecode($_SERVER['REQUEST_URI']);
$p = strpos( $url, "?");
if( $p>0 )
{
   $url = substr( $url, 0, $p);
}

if( !preg_match("@^/" . $secureVirtualHostName . "/.+$@i", $url) || strpos($url,"..")!==false )
{
    header("HTTP/1.0 404 Not Found");
    header("Status: 404 Not Found");
    echo "<h1>404 File not found</h1>";
    die();
}
if(endsWith($url,"viewer.css"))
{
    downloadCss();
    die();
}
$url = substr($url,strlen($secureVirtualHostName)+2);
if( $url === "passwords.txt" )
{
    header("HTTP/1.0 404 Not Found");
    header("Status: 404 Not Found");
    echo "<h1>404 File not found</h1>";
    die();
}

$pwd = null;
$username = null;
if( isset($_COOKIE[$cookieNameUser]) && isset($_COOKIE[$cookieNamePwd]) )
{
   $username = $_COOKIE[$cookieNameUser];
   $pwd = $_COOKIE[$cookieNamePwd];
}

if( !empty($_POST) )
{
   $username = $_POST['username'];
   $pwd = $_POST['password'];
}
$loggedIn = false;

if( !empty($username) )
{
   if (($handle = fopen(dirname(__FILE__) . "/secure/passwords.txt", "r")) !== FALSE) {
      while ( ($data = fgetcsv($handle) ) !== FALSE ) {
        //process          
          if( strcasecmp(trim($data[0]),$username)==0 && strcmp($pwd,$data[1])==0 )
          {
             $loggedIn = true;
             if( !empty($_POST["remember"]) )
             {
                 setcookie($cookieNameUser, $username, time() + (86400 * 300), "/", $websiteDomain); 
                 setcookie($cookieNamePwd, $pwd, time() + (86400 * 300), "/", $websiteDomain); 
                 header("Location: /" . $secureVirtualHostName . "/" . $url);
                 die();
             }
             else if(!empty($_POST))
             {
                 setcookie($cookieNameUser, $username, 0, "/", $websiteDomain); 
                 setcookie($cookieNamePwd, $pwd, 0, "/", $websiteDomain); 
                 header("Location: /" . $secureVirtualHostName . "/" . $url);
                 die();
             }
             if( !startsWith($url . "/", $data[2]) )
             {
                header("HTTP/1.0 403 Forbidden");
                header("Status: 403 Forbidden");
                echo "<h1>403 Forbidden</h1>";
                die();
             }
             break;
          }
      }
   }
}
$downloadPath = dirname(__FILE__) . "/public/" . $url;
if( file_exists($downloadPath) )
{
   downloadFile($downloadPath, $username);
   die();
}
$downloadPath = dirname(__FILE__) . "/secure/" . $url;
if( $loggedIn )
{
    if( is_dir($downloadPath) )
    {
        if( endsWith($url,"/"))
        {
            header("Location: /" . $secureVirtualHostName . "/" . $url . "index.html");            
        }
        else
        {
            header("Location: /" . $secureVirtualHostName . "/" . $url . "/index.html");
        }        
        die();
    }
    elseif( file_exists($downloadPath) )
    {   
        downloadFile($downloadPath, $username);
        die();
    }
    elseif( (endsWith($downloadPath,"/index.html") && strlen($url)>10) )
    {
        generateIndex($downloadPath);
        die();
    }
    elseif(endsWith($downloadPath,".prv.html"))
    {
        generateImageViewer($downloadPath);
        die();
    }
    else
    {
        header("HTTP/1.0 404 Not Found");
        header("Status: 404 Not Found");
        echo "<h1>404 File not found</h1>";
        die();
    }
}
?><!DOCTYPE html>
<html>
<head>
<title>Download</title>
</head>
<body>
<div class="container">
<h1><?php
echo htmlspecialchars($secureFolderTitle);
?></h1>

<p>Please enter you credentials to download <b><?php
echo htmlspecialchars($url);
?></b></p>
<form action="" method="POST" onsubmit="">
<input type="hidden" name="confirm" value="1"/>
<table>
<tr><td><label for="username">User Name</label>:</td><td><input id="username" name="username" type="text"/></td></tr>
<tr><td><label for="password">Password</label>:</td><td><input id="password" name="password" type="password"/></td></tr>
<tr><td></td><td><input id="remember" name="remember" type="checkbox" value="1"/> <label for="remember">Remember my password</label></td></tr>
<tr><td></td><td><input id="butSubmit" type="submit" value="Download File"/></td></tr>
</table>
</body>
</html>

