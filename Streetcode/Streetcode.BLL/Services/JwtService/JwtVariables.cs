using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.Services.JwtService;
public class JwtVariables
{
   public string Secret { get; set; } = null!;
   public int ExpirationInMinutes { get; set; }
   public string Issuer { get; set; } = null!;
   public string Audience { get; set; } = null!;
}