using System.Threading.Tasks;

// ReSharper disable once CheckNamespace

namespace Solana.Unity.SDK
{
    /// <summary>
    /// Extensible interface for persisting Mobile Wallet Adapter authorization tokens.
    /// 
    /// Implement this interface to provide custom token storage backends 
    /// (e.g. encrypted storage, cloud sync, secure keystore).
    /// 
    /// The default implementation is <see cref="PlayerPrefsAuthCache"/> which
    /// uses Unity's PlayerPrefs for simple local persistence.
    /// 
    /// Example custom implementation:
    /// <code>
    /// public class MySecureCache : IMwaAuthCache {
    ///     public Task&lt;string&gt; GetAuthToken(string walletIdentity) { ... }
    ///     public Task SetAuthToken(string walletIdentity, string token) { ... }
    ///     public Task ClearAuthToken(string walletIdentity) { ... }
    /// }
    /// // Then inject it:
    /// var adapter = new SolanaWalletAdapter(options, authCache: new MySecureCache());
    /// </code>
    /// </summary>
    public interface IMwaAuthCache
    {
        /// <summary>
        /// Retrieves the cached auth token for the given wallet identity, or null if none.
        /// </summary>
        /// <summary>
/// Retrieves the cached Mobile Wallet Adapter authorization token for the specified wallet identity.
/// </summary>
/// <param name="walletIdentity">A unique key identifying the wallet (e.g. identity URI + name).</param>
/// <returns>The cached auth token for the specified wallet identity, or <c>null</c> if no token is stored.</returns>
        Task<string> GetAuthToken(string walletIdentity);

        /// <summary>
        /// Stores an auth token for the given wallet identity.
        /// <summary>
/// Persists the authorization token for the specified wallet identity.
/// </summary>
/// <param name="walletIdentity">The identifier for the wallet whose token will be stored.</param>
/// <param name="token">The authorization token to persist for the given wallet identity.</param>
        Task SetAuthToken(string walletIdentity, string token);

        /// <summary>
        /// Clears the stored auth token for the given wallet identity.
        /// <summary>
/// Removes any stored Mobile Wallet Adapter authorization token associated with the specified wallet identity.
/// </summary>
/// <param name="walletIdentity">The unique identifier of the wallet whose cached authorization token should be removed.</param>
/// <returns>A task that completes when the cache removal operation has finished.</returns>
        Task ClearAuthToken(string walletIdentity);
    }
}
