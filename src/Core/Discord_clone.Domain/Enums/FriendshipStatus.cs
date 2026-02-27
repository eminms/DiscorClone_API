using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_clone.Domain.Enums
{
    public enum FriendshipStatus
    {
        Pending = 0,   // Gözləmədə (İstək göndərilib)
        Accepted = 1,  // Qəbul edilib (Artıq dostdurlar)
        Blocked = 2    // Bloklanıb
    }
}
