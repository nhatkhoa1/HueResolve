using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HueResolve.Models.DTOs;

public class LoginResponse
{
    public string Token { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
}