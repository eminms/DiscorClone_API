using Microsoft.AspNetCore.SignalR;

namespace Discord_clone.WebApi.Hubs
{
    public class ChatHub : Hub
    {
        private readonly PresenceTracker _tracker;

        public ChatHub(PresenceTracker tracker)
        {
            _tracker = tracker;
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.GetHttpContext()?.Request.Query["username"];

            if (!string.IsNullOrEmpty(username))
            {
                // 🔥 YENİ: İstifadəçi girən kimi onun ÖZ ADINA BİR QURUP yaradırıq (Bildirişlər bura gələcək)
                await Groups.AddToGroupAsync(Context.ConnectionId, username);

                var isOnline = await _tracker.UserConnected(username, Context.ConnectionId);

                if (isOnline)
                {
                    await Clients.Others.SendAsync("UserIsOnline", username);
                }

                var currentUsers = await _tracker.GetOnlineUsers();
                await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var username = Context.GetHttpContext()?.Request.Query["username"];

            if (!string.IsNullOrEmpty(username))
            {
                var isOffline = await _tracker.UserDisconnected(username, Context.ConnectionId);

                if (isOffline)
                {
                    await Clients.Others.SendAsync("UserIsOffline", username);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinChannel(string channelId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, channelId);
        }

        public async Task LeaveChannel(string channelId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, channelId);
        }

        public async Task SendMessage(string channelId, string username, string avatarUrl, string message)
        {
            await Clients.Group(channelId).SendAsync("ReceiveMessage", username, avatarUrl, message);
        }

        private string GetDirectChatRoomName(string user1, string user2)
        {
            return string.Compare(user1, user2) < 0 ? $"{user1}_{user2}" : $"{user2}_{user1}";
        }

        public async Task JoinDirectChat(string myId, string otherUserId)
        {
            var roomName = GetDirectChatRoomName(myId, otherUserId);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        }

        // 🔥 YENİ FUNKSİYA: Təkcə Şəxsi Mesajlaşma üçün
        public async Task SendDirectMessage(string senderName, string receiverName, string avatarUrl, string message)
        {
            var roomName = GetDirectChatRoomName(senderName, receiverName);

            // 1. Mesajı DM otağına göndəririk (Bəlkə alıcı artıq bizim otaqdadır)
            await Clients.Group(roomName).SendAsync("ReceiveMessage", senderName, avatarUrl, message);

            // 2. Alıcının birbaşa "ÖZÜNƏ" xəbərdarlıq göndəririk! (O, Lounge-da və ya başqa yerdə olsa belə çatacaq)
            await Clients.Group(receiverName).SendAsync("ReceiveDMNotification", senderName);
        }
    }
}