using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_clone.Domain.Enums
{
    public enum ServerRole
    {
        Member = 0,     // Adi üzv (Sadəcə yazışa bilər)
        Moderator = 1,  // Köməkçi (Mesaj silə bilər, adam ata bilər)
        Admin = 2       // Sahib (Hər şeyə icazəsi var)
    }
}
