var port = null;

chrome.runtime.onInstalled.addListener(Init);
chrome.contextMenus.onClicked.addListener(showMessageBox)

function showMessageBox(info, tab) {
	var link = decodeURIComponent(info.linkUrl);
	var index = link.search(/[^/\\&\?]+\.\w{3,4}(?=([\?&].*$|$))/);
	var fileName = link.substring(index);

	if (info.menuItemId == "download_now") {
		alert("will download from " + link + " soon\n File name : " + fileName);
		Connect();
		SendMessage(fileName, link);
	}
	else if (info.menuItemId == "queue_download") {
		alert(link + " will be queued\n File name : " + fileName);
	}
	else
	{
		return;
	}
}

function Init() {
	chrome.contextMenus.create({
    title: "Download now",
    id: "download_now",
    contexts: ["link"]
});
	chrome.contextMenus.create({
    title: "Queue download",
    id: "queue_download",
    contexts: ["link"]
});

}

function Connect() {
	port = chrome.extension.connectNative("com.aliveplex.tirkx_downloader");
	port.onMessage.addListener(function(msg) {
		console.log("Receive " + msg);
	});
}

function SendMessage(fileName, link) {
	port.postMessage({ FileName: fileName, DownloadLink: link });
}