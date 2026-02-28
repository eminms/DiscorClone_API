using Discord_clone.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_clone.Application.DTOs
{
    public class UpdateRoleDto
    {
        public string TargetUserId { get; set; } = string.Empty; // Kimə rol veririk?
        public ServerRole NewRole { get; set; }                  // Hansı rolu veririk? (0=Member, 1=Moderator)
    }
}
