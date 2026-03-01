using Microsoft.AspNetCore.SignalR;

namespace Discord_clone.WebApi.Hubs
{
    public class ChatHub : Hub
    {
        private readonly PresenceTracker _tracker;

        // Constructor: PresenceTracker-i bura daxil edirik (Dependency Injection)
        public ChatHub(PresenceTracker tracker)
        {
            _tracker = tracker;
        }

        // ==========================================
        // 1. ONLAYN / OFLAYN İZLƏMƏ (YENİ ƏLAVƏLƏR)
        // ==========================================

        public override async Task OnConnectedAsync()
        {
            // Frontend-dən qoşulanda göndərilən istifadəçi adını (və ya ID-ni) götürürük
            var username = Context.GetHttpContext()?.Request.Query["username"];

            if (!string.IsNullOrEmpty(username))
            {
                var isOnline = await _tracker.UserConnected(username, Context.ConnectionId);

                if (isOnline)
                {
                    // Digər bütün istifadəçilərə bu adamın onlayn olduğunu xəbər veririk
                    await Clients.Others.SendAsync("UserIsOnline", username);
                }

                // Səhifəni təzə açan bu adama hazırda onlayn olanların tam siyahısını göndəririk
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
                    // Digər istifadəçilərə bu adamın çıxdığını (oflayn olduğunu) xəbər veririk
                    await Clients.Others.SendAsync("UserIsOffline", username);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }


        // ==========================================
        // 2. KANAL VƏ MESAJLAŞMA (SƏNİN KODLARIN)
        // ==========================================

        // 1. İstifadəçi kanala girəndə onu o kanalın "Otağına" (Group) əlavə edirik
        public async Task JoinChannel(string channelId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, channelId);
        }

        // 2. İstifadəçi başqa kanala keçəndə köhnə "Otaqdan" çıxarırıq
        public async Task LeaveChannel(string channelId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, channelId);
        }

        // 3. Mesaj göndəriləndə sadəcə o "Otaqdakı" adamlara paylayırıq
        public async Task SendMessage(string channelId, string username, string avatarUrl, string message)
        {
            // "ReceiveMessage" -> Bu adı front-end-də JavaScript dinləyəcək
            await Clients.Group(channelId).SendAsync("ReceiveMessage", username, avatarUrl, message);
        }

        // İki istifadəçi üçün unikal DM otağı adı yaradan kiçik funksiya
        private string GetDirectChatRoomName(string user1, string user2)
        {
            // ID-ləri əlifba sırası ilə düzürük ki, həmişə eyni otaq adı alınsın
            return string.Compare(user1, user2) < 0 ? $"{user1}_{user2}" : $"{user2}_{user1}";
        }

        // DM otağına qoşulmaq
        public async Task JoinDirectChat(string myId, string otherUserId)
        {
            var roomName = GetDirectChatRoomName(myId, otherUserId);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        }
    }
}
