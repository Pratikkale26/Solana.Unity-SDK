using System.Threading.Tasks;
using UnityEngine;

// ReSharper disable once CheckNamespace

namespace Solana.Unity.SDK
{
    /// <summary>
    /// Default <see cref="IMwaAuthCache"/> implementation using Unity's PlayerPrefs.
    /// Tokens are stored as plain-text in PlayerPrefs.
    /// 
    /// For production games that need stronger security, implement <see cref="IMwaAuthCache"/>
    /// with a platform-specific encrypted keystore backend.
    /// </summary>
    public class PlayerPrefsAuthCache : IMwaAuthCache
    {
        private const string KeyPrefix = "mwa_auth_token_";

        /// <inheritdoc/>
        public Task<string> GetAuthToken(string walletIdentity)
        {
            var key = BuildKey(walletIdentity);
            var token = PlayerPrefs.GetString(key, null);
            return Task.FromResult(string.IsNullOrEmpty(token) ? null : token);
        }

        /// <inheritdoc/>
        public Task SetAuthToken(string walletIdentity, string token)
        {
            var key = BuildKey(walletIdentity);
            PlayerPrefs.SetString(key, token);
            PlayerPrefs.Save();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes the stored authentication token for the specified wallet identity from Unity PlayerPrefs and persists the change.
        /// </summary>
        /// <param name="walletIdentity">The wallet identity used to build the PlayerPrefs key for the token to remove.</param>
        public Task ClearAuthToken(string walletIdentity)
        {
            var key = BuildKey(walletIdentity);
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
            return Task.CompletedTask;
        }

        /// <summary>
/// Constructs the PlayerPrefs storage key for the specified wallet identity by prefixing it with the auth token key prefix.
/// </summary>
/// <param name="walletIdentity">The wallet identity used to form the storage key.</param>
/// <returns>The PlayerPrefs key used to store or retrieve the auth token for the given wallet identity.</returns>
private static string BuildKey(string walletIdentity) => KeyPrefix + walletIdentity;
    }
}
