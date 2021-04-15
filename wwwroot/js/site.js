// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

var limit = 4;
$('input.IsChecked').on('click', function(evt) {
    if ($('.IsChecked:checked').length > limit) {
       this.checked = false;
   }
});
var liveFeedHubconnection = new signalR.HubConnectionBuilder().withUrl("/LiveFeedHub").build();

liveFeedHubconnection.on("ReceiveMessage", function (message) {
    var li = document.createElement("li");
    li.textContent = message;
    li.classList.add("list-group-item");
    document.getElementById("live_feed").appendChild(li);
});

liveFeedHubconnection.start().catch(function (err) {
    return console.error(err.toString());
});