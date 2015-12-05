# TirkxDownloader
Unofficial downloader for tirkx
##What is it?
**TirkxDownloader** is download manager program. at first, intended to be used for browsing anime on Tirkx and download it without open web browser later it was written in the way that it can download every file that URL directly point to by sending URI to program and make HTTP request that resource to server.  
The reason it can't browse anime on Tirkx directly because
1. I don't want to use Tirkx API and it is too slow and inefficiently to download every web pages and parse HTML content to list of animes.  
2. There is no reason to make this program to download file from only one website.  
##Feature
 - Download file from URI that directly point to by sending URL from web browser(currently support Chrome only)
 - Google Drive file downloading(by specific file)
 - Queueing
 - Stop downloading
 - Concurrently downloading
 - Authenticate request by let user enter username and password for specific website that require authentication.
 
##Unimplemented feature
- Google Drive folder browsing
- Mega file downloading
- Pause downloading
- Shutdown after download complete

##Is it work now?
Although this program isn't even in beta state, it can be use to downloading and queueing without any bug.  
**Please note** that Google Drive doesn't expose stream's length so file size in program usually mismatch file size on computer.

##How to install
This program doesn't release yet, the only way to install is to compile from source.  
1. Download Visual Studio Community which is free version of Visual Studio from [this site.](https://www.visualstudio.com/en-us/downloads/download-visual-studio-vs.aspx)  
2. Clone this repository to your computer.  
3. Open Visual Studio project, select Release and Build Solution(press F6), you can find compiled program in `bin/release` directory in root project directory.    
4. Go to Extensions page in Chrome, press **load unpacked extension...** and select `TirkxDownloader chrome extension` directory in root project directory.  

##License
[MIT license](https://github.com/witoong623/TirkxDownloader/blob/master/license.txt)
