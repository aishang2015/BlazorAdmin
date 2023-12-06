namespace BlazorAdmin.Data.Constants
{
    public record JwtConstant
    {
        public const string JwtIssue = "JwtIssuer";
        public const string JwtAudience = "JwtAudience";
        public const string JwtSigningRsaPublicKey = "JwtSigningRsaPublicKey";
        public const string JwtSigningRsaPrivateKey = "JwtSigningRsaPrivateKey";
        public const string JwtExpireMinute = "JwtExpireMinute";
    }
}
