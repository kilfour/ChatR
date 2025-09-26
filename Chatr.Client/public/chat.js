// const apiUrl = "http://localhost:5067";
const apiUrl = "https://unhappy-cariotta-pequivents-b0afd5e9.koyeb.app";

async function handleResponse(response) {
    if (!response.ok) {
        var json = await response.json();
        app.ports.onErrorMessage.send(json.error);
        return false;
    }
    const data = await response.json();
    token = data.token;
    localStorage.setItem("jwt", token);
    return true;
}

async function login(username, password) {
    const response = await fetch(`${apiUrl}/api/login`, {
        method: "POST",
        headers: { "content-type": "application/json" },
        body: JSON.stringify({ username, password })
    });
    return handleResponse(response);
}

async function register(username, password) {
    const response = await fetch(`${apiUrl}/api/register`, {
        method: "POST",
        headers: { "content-type": "application/json" },
        body: JSON.stringify({ username, password })
    });
    return handleResponse(response);
}


var app = Elm.Main.init();
var connection = null;
(async () => {
    let token = localStorage.getItem("jwt") || "";
    if (token) {
        await initializeConnection();
        app.ports.onLoggedIn.send(true);
    }

    app.ports.register.subscribe(async ([user, pass]) => {
        var success = await register(user, pass);
        if (success) {
            await initializeConnection();
            app.ports.onLoggedIn.send(success);
        }
    });

    app.ports.login.subscribe(async ([user, pass]) => {
        var success = await login(user, pass);
        if (success) {
            await initializeConnection();
            app.ports.onLoggedIn.send(success);
        }
    });

    app.ports.logout.subscribe(async () => {
        await connection.stop();
        localStorage.removeItem("jwt");
    });

    app.ports.joinRoom.subscribe(room => {
        connection.invoke("JoinRoom", room).catch(console.error);
    });

    app.ports.sendToRoom.subscribe(([room, user, msg]) => {
        connection.invoke("SendToRoom", room, user, msg).catch(console.error);
    });

})();

async function initializeConnection() {
    connection = new signalR.HubConnectionBuilder()
        .withUrl(`${apiUrl}/chat`, {
            accessTokenFactory: () => localStorage.getItem("jwt") || "",
            withCredentials: false
        })
        .withAutomaticReconnect()
        .build();
    await connection.start();
    await connection.invoke("JoinRoom", "Alpha");
    connection.on("ReceiveRoomMessage", (room, user, message) => {
        app.ports.onRoomMessage.send(`${room}-${user}: ${message}`);
    });
}