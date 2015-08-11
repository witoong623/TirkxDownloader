chrome.runtime.onInstalled.addListener(Init);
chrome.contextMenus.onClicked.addListener(showMessageBox);

function showMessageBox(info, tab) {
	var link = decodeURIComponent(info.linkUrl);
	var index = link.search(/[^/\\\?]+\.\w{3,4}(?=([\?&].*$|$))/);
	var fileName = link.substring(index);
	SendMessage(fileName,link);
}

function Init() {
	chrome.contextMenus.create({
    title: "Download now",
    id: "download_now",
    contexts: ["link"]
});
}

function SendMessage(fileName, link) {
	var xhr = new XMLHttpRequest();
	xhr.open("POST","http://localhost:6230/", false);
	xhr.setRequestHeader("Content-Type", "application/json; charset=UTF-8");
	var JSONstring = JSON.stringify({ FileName: fileName, DownloadLink: link });
	xhr.send(JSONstring);
}