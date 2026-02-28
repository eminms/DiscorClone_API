using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_clone.Application.DTOs
{
    public class SendDirectMessageDto
    {
        // Kimə göndəririk? (Kanal yoxdur, birbaşa adamın ID-si var)
        public string ReceiverId { get; set; } = string.Empty;

        // Nə yazırıq?
        public string Content { get; set; } = string.Empty;
    }
}
