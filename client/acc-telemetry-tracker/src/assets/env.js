(function (window) {
    window.env = window.env || {};
    console.log(window.env);

    // Environment variables
    window["env"]["apiUrl"] = "https://localhost:7112";
    window["env"]["discordClient"] = "client";
    console.log(window["env"]);
})(this);