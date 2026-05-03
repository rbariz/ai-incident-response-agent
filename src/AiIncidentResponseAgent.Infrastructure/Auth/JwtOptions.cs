using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace AiIncidentResponseAgent.Infrastructure.Auth;
public sealed class JwtOptions
{
    public string Issuer { get; set; } = "AiIncidentResponseAgent";
    public string Audience { get; set; } = "OpsCenter";
    public string Secret { get; set; } = "CHANGE_ME_DEV_SECRET_AT_LEAST_32_CHARS";
    public int ExpirationMinutes { get; set; } = 120;
}
