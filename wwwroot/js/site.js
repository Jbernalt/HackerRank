// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
var counterHubconnection = new signalR.HubConnectionBuilder().withUrl("/LiveFeedHub").build();

counterHubconnection.on("ReceiveMessage", function (message) {
    var li = document.createElement("li");
    li.textContent = message;
    document.getElementById("live_feed").appendChild(li);
});

counterHubconnection.start().catch(function (err) {
    return console.error(err.toString());
});