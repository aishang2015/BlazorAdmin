namespace BlazorAdmin.Data.Constants
{
    public record JwtConstant
    {
        public const string JwtIssue = "JwtIssuer";
        public const string JwtAudience = "JwtAudience";
        public const string JwtSigningKey = "JwtSigningKey";
        public const string JwtExpireMinute = "JwtExpireMinute";
    }
}
