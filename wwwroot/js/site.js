// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$('input.IsChecked').on('click', function(evt) {
    if ($('.IsChecked:checked').length > 4) {
       this.checked = false;
   }
});
var liveFeedHubconnection = new signalR.HubConnectionBuilder().withUrl("/LiveFeedHub").build();

function appendTableRow(obj) {
    let td = document.createElement("td");
    td.append(obj);
    return td;
}

liveFeedHubconnection.on("ReceiveMessage", function (message) {
    let arr = JSON.parse(message);
    let liveFeedMessage = arr.LiveFeedMessage;
    let topFiveGroups = arr.TopFiveGroups;
    let topFiveUsers = arr.TopFiveUsers;
    let topFiveUserLevels = arr.TopFiveUserLevels;

    let element = $("#live_feed_article ul").children();
    if (element.length >= 5) {
        element.last().remove();
    }
    let li = document.createElement("li");
    let s = new Date();
    li.textContent = liveFeedMessage + s.toDateString() + " " + s.toLocaleTimeString();
    li.classList.add("list-group-item");
    if (element.length == 0) {
        document.getElementById("live_feed").appendChild(li);
    }
    else {
        $(li).insertBefore(element.first());
    }

    $("#userTableBody").empty();
    $("#groupTableBody").empty();

    for (let user of topFiveUsers) {
        let img = document.createElement("img");
        if (user.ProfileImage == "default-profile-picture.png") {
            img.src = "../img/" + user.ProfileImage;
        } else {
            img.src = "../profileImg/" + user.ProfileImage;
        }
        img.style = "height: 50px; border-radius: 50%";
        img.alt = "profile-image";

        let a = document.createElement("a");
        a.innerHTML = user.Username;
        let baseUrl = window.location.protocol + "//" + window.location.host + "/";
        a.href = baseUrl + "user/profile/" + user.Username;
        a.className = "nav-link px-0 py-0 my-0 mx-0";

        let tr = document.createElement("tr");
        tr.append(appendTableRow(img));
        tr.append(appendTableRow(a));
        tr.append(appendTableRow(user.Commits));
        tr.append(appendTableRow(user.IssuesCreated));
        tr.append(appendTableRow(user.IssuesSolved));
        tr.append(appendTableRow(user.MergeRequest));
        tr.append(appendTableRow(user.Comments));
        tr.append(appendTableRow(Number(user.DailyRating).toFixed(2)));

        $("#userTableBody").append(tr);
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

    if (topFiveUserLevels.length != 0) {

        $("#levelTableBody").empty();
        for (let userLevel of topFiveUserLevels) {
            let img = document.createElement("img");
            if (userLevel.ProfileImage == "default-profile-picture.png") {
                img.src = "../img/" + userLevel.ProfileImage;
            } else {
                img.src = "../profileImg/" + userLevel.ProfileImage;
            }
            img.style = "height: 50px; border-radius: 50%";
            img.alt = "profile-image";

            let a = document.createElement("a");
            a.innerHTML = userLevel.Username;
            let baseUrl = window.location.protocol + "//" + window.location.host + "/";
            a.href = baseUrl + "user/profile/" + userLevel.Username;
            a.className = "nav-link px-0 py-0 my-0 mx-0";

            let value = Number((userLevel.CurrentExperience - userLevel.Level.XpNeeded) * 10).toFixed(2).toString() + "%";
            let divParent = $('<div class="progress position-relative">')[0];
            let divChild = $('<div class="progress-bar" style="width:' + value + '; background-color: skyblue" role="progressbar" aria-valuenow="0" aria-valuemin="0" aria-valuemax="10">')[0];
            let smallChild = $('<small class="justify-content-center d-flex position-absolute w-100 text-dark">')[0];
            smallChild.innerHTML = Number(userLevel.CurrentExperience).toFixed(2).toString() + "/" + (userLevel.Level.XpNeeded + 10).toString();
            divParent.append(divChild);
            divParent.append(smallChild);

            let tr = document.createElement("tr");
            tr.append(appendTableRow(img));
            tr.append(appendTableRow(a));
            tr.append(appendTableRow(userLevel.PrestigeLevel));
            tr.append(appendTableRow("Level " + " " + userLevel.Level.LevelId + " " + userLevel.Level.LevelName))
            tr.append(appendTableRow(divParent));

            $("#levelTableBody").append(tr);
        }
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