using System.Text.Json.Serialization;

namespace ASP.Data.Entities
{
    public class AccessToken
    {
        public String  Jti { get; set; } = null!;
        public Guid    Sub { get; set; } //UserAccessId
        public String? Exp { get; set; }
        public String? Iat { get; set; }
        public String? Nbf { get; set; }
        public String? Aud { get; set; } //Role / RoleId
        public String? Iss { get; set; }

        [JsonIgnore]
        public UserAccess UserAccess { get; set; } = null!;
    }
}
