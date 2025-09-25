[↩ Back](./readme.md) 
# SignalR

## Wat is SignalR?

* **SignalR** is een realtime-communicatiebibliotheek voor **ASP.NET Core** waarmee server en clients bi-directioneel kunnen communiceren.
* Het zorgt automatisch voor de **beste transportlaag** (WebSockets > Server-Sent Events > Long Polling) en verbergt complexe details zoals reconnects en connection management.
* Use-cases:
  * Live dashboards (telemetrie, KPI’s)
  * Chat, collab (samen editen), notificaties
  * Multiplayer / whiteboards / cursors
  * Background jobs die progress sturen naar de UI

**Kernidee:** je definieert een **Hub** op de server met methodes en clients roepen die methodes aan en kunnen zichzelf methodes laten aanroepen door de server.

## Kernconcepten

* **Hub**: centraal communicatiemodel (C# class) met methodes die clients kunnen oproepen. De hub kan ook **clients aanroepen** via `Clients.*`.
* **Connections**: elke client heeft een ConnectionId. Groepeer met **Groups** (bijv. per document/room).
* **Transports**: WebSockets (voorkeur), fallback naar SSE of Long Polling.
* **Protocol**: standaard **JSON**, optioneel **MessagePack** (kleiner & sneller).
* **Authenticatie/Autorisatie**: integreert met ASP.NET Core Auth. Je kunt per hub of per methode autoriseren.

## Quickstart (minimal API, .NET 8)

### Server (C#)

```csharp
using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
```

**Program.cs** (minimal hosting):

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

var app = builder.Build();

app.MapHub<ChatHub>("/chat");

app.Run();
```

### Client (JavaScript, browser)

> Snelste manier: CDN script van `@microsoft/signalr` gebruiken in een simpele HTML.

```html
<!doctype html>
<html>
  <body>
    <input id="user" placeholder="name" />
    <input id="msg" placeholder="message" />
    <button id="send">Send</button>
    <ul id="log"></ul>

    <script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr/dist/browser/signalr.js"></script>
    <script>
      const connection = new signalR.HubConnectionBuilder()
        .withUrl("/chat")
        .withAutomaticReconnect()
        .build();

      connection.on("ReceiveMessage", (user, message) => {
        const li = document.createElement("li");
        li.textContent = `${user}: ${message}`;
        document.getElementById("log").appendChild(li);
      });

      async function start() {
        try { await connection.start(); }
        catch (err) { console.error(err); setTimeout(start, 2000); }
      }
      start();

      document.getElementById("send").addEventListener("click", async () => {
        const u = document.getElementById("user").value || "anon";
        const m = document.getElementById("msg").value;
        await connection.invoke("SendMessage", u, m);
      });
    </script>
  </body>
</html>
```

### Client (.NET Console)

```csharp
using Microsoft.AspNetCore.SignalR.Client;

var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:5001/chat")
    .WithAutomaticReconnect()
    .Build();

connection.On<string,string>("ReceiveMessage", (user, msg) =>
{
    Console.WriteLine($"{user}: {msg}");
});

await connection.StartAsync();
await connection.InvokeAsync("SendMessage", "cli", "hello from console");
Console.ReadLine();
```

## Groepen, Users & Targeted sends

* **Groups**: dynamisch join/leave vanuit hub of client.

```csharp
public class RoomHub : Hub
{
    public Task Join(string room) => Groups.AddToGroupAsync(Context.ConnectionId, room);
    public Task Leave(string room) => Groups.RemoveFromGroupAsync(Context.ConnectionId, room);
    public Task SendToRoom(string room, string message) =>
        Clients.Group(room).SendAsync("Receive", Context.User?.Identity?.Name ?? "anon", message);
}
```

* **Specifieke client**: `Clients.Client(connectionId)`
* **User** (met auth): `Clients.User(userId)`
* **Meerdere**: `Clients.Clients(listOfIds)` / `Clients.Users(userIds)`

## Authenticatie & Autorisatie

* **JWT/Cookies/Identity** werken zoals in gewone ASP.NET Core.
* Autoriseer op hub-niveau:

```csharp
[Authorize]
public class SecuredHub : Hub { }
```

* Of per methode:

```csharp
public class MixedHub : Hub
{
    [Authorize(Roles = "Admin")]
    public Task AdminOnly() => Task.CompletedTask;
}
```

## Debugging & Troubleshooting

* **CORS**: stel correct in bij cross-origin gebruik (`app.UseCors`).
* **Negotiate failures**: check URL’s, HTTPS, reverse proxy headers.
* **WebSockets blocked**: enterprise proxies/firewalls => fallback werkt, maar check poorten/headers.
* **Version mismatch**: client/server `@microsoft/signalr` versus server package versies.
* **Logging**: verhoog loglevel voor `Microsoft.AspNetCore.SignalR`.

JS-client logging:

```js
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/chat")
  .configureLogging(signalR.LogLevel.Information)
  .build();
```

## Patronen & Best Practices

* **Thin Hub, Fat Services**: houd domeinlogica buiten de hub; injecteer services.
* **Idempotentie**: verwacht dubbele events (retries/reconnects). Gebruik event ids.
* **Security**: autoriseer nauw (per methode/groep); valideer invoer.
* **Versioneer**: contract-wijzigingen achter vlag of nieuwe methodenaam.
* **Observability**: metrics op connected clients, messages/sec, failures.

Injectie van services in hub:

```csharp
public class NotifyHub(INotifier notifier) : Hub
{
    public Task Ping() => notifier.SendAsync("pong");
}
```


> Documenteer je **event-contract** (methodenamen, payloadvormen, versies) net zo strikt als je REST/HTTP API.
