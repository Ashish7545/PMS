using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMSAPI.Models.Authentication
{
    public class ApplicationUser: IdentityUser
    {
        public string? Location { get; set; }
    }
}
