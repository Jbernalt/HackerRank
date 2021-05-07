// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$('input.IsChecked').on('click', function(evt) {
    if ($('.IsChecked:checked').length > 4) {
       this.checked = false;
   }
});
var liveFeedHubconnection = new signalR.HubConnectionBuilder().withUrl("/LiveFeedHub").build();

liveFeedHubconnection.on("ReceiveMessage", function (message) {
    let arr = JSON.parse(message);
    let liveFeedMessage = arr.LiveFeedMessage;
    let topFiveGroups = arr.TopFiveGroups;
    let topFiveUsers = arr.TopFiveUsers;

    $("#userTableBody").empty();
    $("#groupTableBody").empty();

    for (let user of topFiveUsers) {
        let img = document.createElement("img");
        if (user.ProfileImage == "default-profile-picture.png") {
            img.src = "~/img/" + user.ProfileImage;
        } else {
            img.src = "~/profileImg/" + user.ProfileImage;
        }
        img.style = "height: 50px; border-radius: 50%";
        img.alt = "profile-image";

        let a = document.createElement("a");
        a.innerHTML = user.Username;
        let baseUrl = window.location.protocol + "//" + window.location.host + "/";
        a.href = baseUrl + "user/profile/" + user.Username;
        a.className = "nav-link px-0 py-0 my-0 mx-0";

        $("#userTableBody").append("<tr><td>"
            + img + "</td><td>"
            + a + "</td><td>"
            + user.Commits + "</td><td>"
            + user.IssuesCreated + "</td><td>"
            + user.IssuesSolved + "</td><td>"
            + user.MergeRequest + "</td><td>"
            + user.Comments + "</td><td>"
            + Number(user.DailyRating).toFixed(2) + "</td></tr>");
    }

    for (let group of topFiveGroups) {
        $("#groupTableBody").append("<tr><td>"
            + group.GroupName + "</td><td>"
            + group.CommitsDaily + "</td><td>"
            + group.IssuesCreatedDaily + "</td><td>"
            + group.IssuesSolvedDaily + "</td><td>"
            + group.MergeRequestsDaily + "</td><td>"
            + group.CommentsDaily + "</td><td>"
            + Number(group.GroupDailyRating).toFixed(2) + "</td></tr>");
    }

    let element = $("#live_feed_article ul").children();
    if (element.length >= 5) {
        element.last().remove();
    }
    let li = document.createElement("li");
    li.textContent = liveFeedMessage;
    li.classList.add("list-group-item");
    if (element.length == 0) {
        document.getElementById("live_feed").appendChild(li);
    }
    else {
        $(li).insertBefore(element.first());
    }
});

liveFeedHubconnection.start().catch(function (err) {
    return console.error(err.toString());
});

$(function () {
    $('#dropdownMenuButton').keyup(function () {
        document.getElementById("dropdownMenu").innerText = '';
        let n = $("#dropdownMenuButton").val();
        $.ajax({
            type: "POST",
            url: "/search/" + n,
            data: n,
            datatype: "json",
            success: function (data) {
                if (data == 0) {
                    let a = document.createElement("a");
                    a.innerHTML = "No users found";
                    a.classList.add("dropdown-item");
                    document.getElementById("dropdownMenu").appendChild(a);
                }
                else {
                    $.each(data, function () {
                        let a = document.createElement("a");
                        a.innerHTML = this;
                        let baseUrl = window.location.protocol + "//" + window.location.host + "/";
                        a.href = baseUrl + "user/profile/" + this;
                        a.classList.add("dropdown-item");
                        document.getElementById("dropdownMenu").appendChild(a);
                    });
                }
            }
        });
    })
});