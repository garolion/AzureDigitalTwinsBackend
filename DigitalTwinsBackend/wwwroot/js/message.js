"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/messageHub").build();

connection.on("ReceiveMessage", function (message) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var li = document.createElement("li");
    li.textContent = msg;
    document.getElementById("messagesList").appendChild(li);
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});

function Reset() {

    document.getElementById("infoMessages").innerHTML = "";

    $.post("/Home/ResetMessages");

    //$.ajax({
    //    type: 'POST',
    //    url: "/Home/ResetMessages"
    //});
}
