using System.Security.Cryptography;

namespace WebIde.Web.Services;

// Generates and hashes Personal Access Tokens. Shared by the mint UI
// (ApiTokenController) and the auth handler (PersonalAccessTokenAuthenticationHandler)
// so the format and hashing stay in one place.
public static class ApiTokenService
{
    public const string TokenPrefix = "webide_pat_";
    private const int SecretBytes = 32;          // 256 bits of entropy
    private const int DisplayPrefixLength = 18;  // stored plaintext for the token list

    public sealed record GeneratedToken(string Raw, string Hash, string DisplayPrefix);

    // Called once at creation. The Raw value is shown to the user and never stored.
    public static GeneratedToken Generate()
    {
        var bytes  = RandomNumberGenerator.GetBytes(SecretBytes);
        var secret = Base64UrlEncode(bytes);
        var raw    = TokenPrefix + secret;
        return new GeneratedToken(
            Raw:           raw,
            Hash:          Hash(raw),
            DisplayPrefix: raw[..Math.Min(DisplayPrefixLength, raw.Length)]);
    }

    // SHA-256 hex of the full raw token. Deterministic — the same raw token always
    // hashes to the same value, so the auth handler can look it up by hash.
    public static string Hash(string rawToken)
    {
        var digest = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexStringLower(digest);
    }

    public static bool LooksLikeToken(string? value) =>
        value is not null && value.StartsWith(TokenPrefix, StringComparison.Ordinal);

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
