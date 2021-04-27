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

$(function () {
    $('#dropdownMenuButton').keyup(function () {
        document.getElementById("dropdownMenu").innerText = '';
        var n = $("#dropdownMenuButton").val();
        $.ajax({
            type: "POST",
            url: "/search/" + n,
            data: n,
            datatype: "json",
            success: function (data) {
                if (data == 0) {
                    var a = document.createElement("a");
                    a.innerHTML = "No users found";
                    a.classList.add("dropdown-item");
                    document.getElementById("dropdownMenu").appendChild(a);
                }
                else {
                    $.each(data, function () {
                        var a = document.createElement("a");
                        a.innerHTML = this;
                        var baseUrl = window.location.protocol + "//" + window.location.host + "/";
                        a.href = baseUrl + "user/profile/" + this;
                        a.classList.add("dropdown-item");
                        document.getElementById("dropdownMenu").appendChild(a);
                    });
                }
            }
        });
    })
});