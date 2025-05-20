using System;
using System.Linq;
using System.Security.Cryptography;

namespace Chitti.Helpers;

public static class ApiKeyProtector
{
    public static string Encrypt(string apiKey)
    {
        var entropy = new byte[20];
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(entropy);
        }

        var encryptedData = ProtectedData.Protect(
            System.Text.Encoding.UTF8.GetBytes(apiKey),
            entropy,
            DataProtectionScope.CurrentUser);

        return Convert.ToBase64String(entropy.Concat(encryptedData).ToArray());
    }

    public static string Decrypt(string encryptedApiKey)
    {
        try
        {
            var data = Convert.FromBase64String(encryptedApiKey);
            var entropy = data.Take(20).ToArray();
            var encryptedData = data.Skip(20).ToArray();

            var decryptedData = ProtectedData.Unprotect(
                encryptedData,
                entropy,
                DataProtectionScope.CurrentUser);

            return System.Text.Encoding.UTF8.GetString(decryptedData);
        }
        catch
        {
            return string.Empty;
        }
    }
} 