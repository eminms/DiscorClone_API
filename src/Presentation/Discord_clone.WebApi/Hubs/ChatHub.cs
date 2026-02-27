using Microsoft.AspNetCore.SignalR;

namespace Discord_clone.WebApi.Hubs
{
    public class ChatHub:Hub
    {
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
    }
}
