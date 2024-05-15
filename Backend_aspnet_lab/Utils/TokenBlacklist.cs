using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend_aspnet_lab.Utils
{
    public static class TokenBlackList
    {
        private static readonly HashSet<string> BlacklistedTokens = new HashSet<string>();

        public static void AddToBlacklist(string token)
        {
            lock (BlacklistedTokens)
            {
                BlacklistedTokens.Add(token);
            }
        }

        public static bool TokenBlacklisted(string token)
        {
            lock (BlacklistedTokens)
            {
                return BlacklistedTokens.Contains(token);
            }
        }
    }

}
