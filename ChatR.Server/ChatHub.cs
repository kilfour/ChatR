using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatR.Server;

[Authorize]
public class ChatHub(RoomTracker tracker) : Hub
{
    private readonly RoomTracker tracker = tracker;

    public async Task JoinRoom(string room)
    {
        var cid = Context.ConnectionId;
        var old = tracker.Get(cid);
        if (!string.IsNullOrEmpty(old) && old != room)
            await Groups.RemoveFromGroupAsync(cid, old);
        await Groups.AddToGroupAsync(cid, room);
        tracker.Set(cid, room);
    }

    public override async Task OnDisconnectedAsync(Exception? ex)
    {
        var cid = Context.ConnectionId;
        var old = tracker.Get(cid);
        if (!string.IsNullOrEmpty(old))
        {
            await Groups.RemoveFromGroupAsync(cid, old);
            tracker.Remove(cid);
        }
        await base.OnDisconnectedAsync(ex);
    }

    public Task LeaveRoom(string room)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, room);

    public Task SendToRoom(string room, string user, string message)
        => Clients.Group(room).SendAsync("ReceiveRoomMessage", room, user, message);
}
