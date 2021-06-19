# Synology Image Viewer

This project provides a Microsoft .NET application can generate contact sheets with thumbnails. You can either use the output files on a secure virtual host on a Synology NAS or have the tool also create html files for you on external media or file shares.

On a Synology NAS virtual host you can configure some provided PHP scripts to enable security and browsing functionality. The solution has been working on DSM 6.1.6 until 6.2.4

## Prepare Synology Secure File Share

### Step 1 - Ensure SSH server enabled

In **Control Panel** select open **Terminal &amp; SNMP**, Enable SSH server, specify a custom port for instance 1033, advanced settings: Medium

### Step 2 - Create folder to secure

First make sure you created a virtual host, for instance with a name *testweb.tld*, with document root web/*testweb.tld*, and enabled PHP. 
Next in File Station under web/*testweb.tld/*secdir, create 'http' read-write access, apply to 'This folder' and 'Child files' only.

### Step 3 - Copy the scripts and configuration

|Target location|Source Script|
|--------|------|
| web/*testweb.tld/*secdir/dispatcher.php | [dispatcher.php](ImageContainerWeb/dispatcher.php) |
| web/*testweb.tld/*secdir/upload.php | [upload.php](ImageContainerWeb/upload.php) |
| web/*testweb.tld/*secdir/secure |  |
| web/*testweb.tld/*secdir/secure/passwords.txt | [secure/passwords.txt](ImageContainerWeb/secure/passwords.txt) make sure the file doesn't get a BOM |
| web/*testweb.tld/*secdir/secure/ThumbnailGenerator.exe | [secure/ThumbnailGenerator.exe](ImageContainerWeb/secure/ThumbnailGenerator.exe) |
| web/*testweb.tld/*secdir/secure/ImageWeb1 |  |
| web/*testweb.tld/*secdir/secure/ImageWeb1/Folder1/orchideen-blumen-vintage-kunst.jpg |  [secure/ImageWeb1/Folder1/orchideen-blumen-vintage-kunst.jpg](ImageContainerWeb/secure/ImageWeb1/Folder1/orchideen-blumen-vintage-kunst.jpg) | |
| web/*testweb.tld/*secdir/secure/ImageWeb1/Folder1/travel-paris-france-poster-1596965769Q8C.jpg |  [secure/ImageWeb1/Folder1/travel-paris-france-poster-1596965769Q8C.jpg](ImageContainerWeb/secure/ImageWeb1/Folder1/travel-paris-france-poster-1596965769Q8C.jpg) | |
| web/*testweb.tld/*secdir/secure/ImageWeb1/Folder2/greece-athens-travel-poster.jpg |  [secure/ImageWeb1/Folder2/greece-athens-travel-poster.jpg](ImageContainerWeb/secure/ImageWeb1/Folder2/greece-athens-travel-poster.jpg) | |
| web/*testweb.tld/*secdir/public |  |
| web/*testweb.tld/*secdir/uploads |  |



### Step 4 - Connect to the NAS and configure permissions and script

Connect to server
```
ssh fred@*servername* -p 1023
```

On Synology you are not allows to modify system configuration files, but each configuration file enabled inclusion of a web site specific configuration file.
For this we first need to identify the Guid of each website where we want the script to be active. In a terminal session to the server enter:

```
grep  "server_name\|include /usr/local" /etc/nginx/app.d/server.webstation-vhost.conf
```

This, for instance, outputs:
```
  server_name www.homecraft.nl;
  include /usr/local/etc/nginx/conf.d/c26ab877-1e0e-4232-94f6-0cf9edd41e6e/user.conf*;        
  server_name homecraft.nl;
  include /usr/local/etc/nginx/conf.d/e8ffa3d0-67b6-4dba-85d3-29daafc59efa/user.conf*;        
```

Next, start editing the configuration include file

```
sudo vi /etc/nginx/conf.d/c26fbf77-1e0e-4232-94f6-0cf9edd41e6e/user.conf
```

And copy the following:
```
if ($scheme != "https") {
    rewrite ^ https://$host$request_uri permanent;
}
location ~ ^/secdir/secure/.*$ {
     deny all;
     return 404;
}
location ~ ^/secdir/public/.*$ {
     deny all;
     return 404;
}
location ~ ^/secdir/uploads/.*$ {
     deny all;
     return 404;
}
location ~ ^/secdir/.*$ {
        root /var/services/web/*testweb.tld*;
        try_files $uri /secdir/dispatcher.php$is_args$args;
}
```
The condition redirects the non-https requests to the https site (this part can be skipped when already existing); next the sections with the 'deny all' and status 404 is to disable direct access to the content files in the secure and public directory that should only be viewed by the dispatcher script.
The last clause defined the php script to be activated on the secdir subdirectory.

Do the same for other virtual host:
```
sudo vi /etc/nginx/conf.d/c26fbf77-1e0e-4232-94f6-0cf9edd41e6e/user.conf
```
And use the same configuration as above.
Make the folder writable by php:

```
sudo chmod 777 /var/services/web/*testweb.tld*/secdir
```
sudo chmod 777 /var/services/web/homecraft.nl/secdir

To reload the configuration restart the webserver:
```
sudo synoservicecfg --restart nginx
```

## Generate thumbnails

When you run the ThumbnailGenerator .NET executable on the network share it will generated per folder
* thm_index.js
* thmh200_0.jpg, thmh200_1.jpg ... thmh200_n.jpg (default 50 thumbnails per file)
* thmv800_0.jpg, thmv800_1.jpg ... thmv800_n.jpg (default 10 thumbnails per file)

| Raw | Thumbnails Added |
|-----|------------------|
|ImageWeb1/Folder1/orchideen-blumen-vintage-kunst.jpg | ImageWeb1/Folder1/orchideen-blumen-vintage-kunst.jpg|
|ImageWeb1/Folder1/travel-paris-france-poster-1596965769Q8C.jpg|ImageWeb1/Folder1/travel-paris-france-poster-1596965769Q8C.jpg|
||ImageWeb1/Folder1/thm_index.js|
||ImageWeb1/Folder1/thmh200_0.jpg|
||ImageWeb1/Folder1/thmv800_0.jpg|
|ImageWeb1/Folder2/greece-athens-travel-poster.jpg|ImageWeb1/Folder2/greece-athens-travel-poster.jpg|
||ImageWeb1/Folder1/thm_index.js|
||ImageWeb1/Folder1/thmh200_0.jpg|
||ImageWeb1/Folder1/thmv800_0.jpg|


When 'Generate Always' is unchecked, folders that already contain thm*.jpg thumbnail files will be skipped.

When using the php script the option 'Generate Html Pages' should stay unchecked; when it is check it will generated html files, so that you don't need the php script and also can browser the html pages without any server script.