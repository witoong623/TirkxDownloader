var port = null;
var IsConnect = false;

chrome.runtime.onInstalled.addListener(Init);
chrome.runtime.onSuspend.addListener(Disconnect);
chrome.contextMenus.onClicked.addListener(showMessageBox);

function showMessageBox(info, tab) {
	var link = decodeURIComponent(info.linkUrl);
	var index = link.search(/[^/\\&\?]+\.\w{3,4}(?=([\?&].*$|$))/);
	var fileName = link.substring(index);

	alert("will download from " + link + " soon\n File name : " + fileName);

	try{
		if (IsConnect == false) {
			IsConnect = true;
			Connect();
		}
		SendMessage(fileName, link);
	}
	catch(err) {
		console.debug("said from catch");
		IsConnect = false;
		if (IsConnect == false) {
			IsConnect = true;
			Connect();
		}
		SendMessage(fileName, link);
	}
	
	console.debug("sended");
}

function Init() {
	chrome.contextMenus.create({
    title: "Download now",
    id: "download_now",
    contexts: ["link"]
});
}

function Connect() {
	port = chrome.extension.connectNative("com.aliveplex.tirkx_downloader");
	port.onMessage.addListener(function(msg) {
		console.log("Receive " + msg);
	});
	console.debug("connected");
}

function Disconnect() {
	port.disconnect();
	IsConnect = false;
	console.debug("disconnected");
}

function SendMessage(fileName, link) {
	port.postMessage({ FileName: fileName, DownloadLink: link });
}