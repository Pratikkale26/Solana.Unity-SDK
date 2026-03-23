using System;
using System.Threading.Tasks;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Wallet;

// ReSharper disable once CheckNamespace

namespace Solana.Unity.SDK
{
    [Serializable]
    public class SolanaWalletAdapterOptions
    {
        public SolanaMobileWalletAdapterOptions solanaMobileWalletAdapterOptions;
        public SolanaWalletAdapterWebGLOptions solanaWalletAdapterWebGLOptions;
        public PhantomWalletOptions phantomWalletOptions;
    }

    /// <summary>
    /// Cross-platform Solana wallet adapter.
    /// Automatically routes to the correct platform-specific implementation:
    /// Android → <see cref="SolanaMobileWalletAdapter"/> (MWA)
    /// WebGL   → <see cref="SolanaWalletAdapterWebGL"/>
    /// iOS     → <see cref="PhantomDeepLink"/>
    /// </summary>
    public class SolanaWalletAdapter : WalletBase
    {
        private readonly WalletBase _internalWallet;

        /// <summary>Convenience accessor to the Android MWA wallet. Null on non-Android platforms.</summary>
        private SolanaMobileWalletAdapter MobileAdapter =>
            _internalWallet as SolanaMobileWalletAdapter;

        // ─── Events ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Fired when the wallet is explicitly disconnected via <see cref="DisconnectWallet"/>.
        /// Subscribe to update your game UI (e.g. hide wallet address, show connect button).
        /// </summary>
        public event Action OnWalletDisconnected;

        /// <summary>
        /// Fired when a silent reconnect succeeds using a cached auth token.
        /// Subscribe to update your game UI (e.g. restore wallet state without re-prompting user).
        /// </summary>
        public event Action OnWalletReconnected;

        // ─── Constructor ─────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a cross-platform Solana wallet adapter.
        /// </summary>
        /// <param name="options">Platform-specific options.</param>
        /// <param name="rpcCluster">The Solana RPC cluster (default: DevNet).</param>
        /// <param name="customRpcUri">Override the RPC endpoint.</param>
        /// <param name="customStreamingRpcUri">Override the streaming RPC endpoint.</param>
        /// <param name="autoConnectOnStartup">Auto-connect when the adapter is created.</param>
        /// <param name="authCache">
        /// Custom auth token cache for Android MWA. Defaults to <see cref="PlayerPrefsAuthCache"/>.
        /// Inject a custom <see cref="IMwaAuthCache"/> for encrypted or cloud-synced token storage.
        /// <summary>
        /// Creates a cross-platform Solana wallet adapter that selects and initializes a platform-specific internal implementation and configures RPC/connection settings.
        /// </summary>
        /// <param name="options">Platform-specific adapter configuration (Android MWA, WebGL, or iOS) used to construct the underlying adapter.</param>
        /// <param name="rpcCluster">The RPC cluster to target (for example, DevNet or MainNet).</param>
        /// <param name="customRpcUri">An optional custom RPC URI to override the cluster default.</param>
        /// <param name="customStreamingRpcUri">An optional custom streaming RPC URI to override the cluster default.</param>
        /// <param name="autoConnectOnStartup">If true, attempts to auto-connect the adapter on construction/startup.</param>
        /// <param name="authCache">Android MWA-only authentication cache; passed to the mobile adapter when running on Android, ignored on other platforms.</param>
        public SolanaWalletAdapter(
            SolanaWalletAdapterOptions options,
            RpcCluster rpcCluster = RpcCluster.DevNet,
            string customRpcUri = null,
            string customStreamingRpcUri = null,
            bool autoConnectOnStartup = false,
            IMwaAuthCache authCache = null
        ) : base(rpcCluster, customRpcUri, customStreamingRpcUri, autoConnectOnStartup)
        {
#if UNITY_ANDROID
#pragma warning disable CS0618
            var mobileAdapter = new SolanaMobileWalletAdapter(
                options.solanaMobileWalletAdapterOptions,
                rpcCluster, customRpcUri, customStreamingRpcUri, autoConnectOnStartup,
                authCache);

            // Bubble up events from mobile adapter
            mobileAdapter.OnWalletDisconnected += () => OnWalletDisconnected?.Invoke();
            mobileAdapter.OnWalletReconnected += () => OnWalletReconnected?.Invoke();

            _internalWallet = mobileAdapter;
#elif UNITY_WEBGL
#pragma warning disable CS0618
            _internalWallet = new SolanaWalletAdapterWebGL(
                options.solanaWalletAdapterWebGLOptions, rpcCluster, customRpcUri, customStreamingRpcUri, autoConnectOnStartup);
#elif UNITY_IOS
#pragma warning disable CS0618
            _internalWallet = new PhantomDeepLink(
                options.phantomWalletOptions, rpcCluster, customRpcUri, customStreamingRpcUri, autoConnectOnStartup);
#endif
        }

        /// <summary>
        /// Attempts to authenticate and obtain an Account using the configured internal wallet adapter.
        /// </summary>
        /// <param name="password">Optional password to use for authentication if the internal adapter requires it.</param>
        /// <returns>The authenticated <see cref="Account"/> returned by the internal adapter, or <c>null</c> if the adapter returns null.</returns>
        /// <exception cref="NotImplementedException">Thrown when no internal wallet adapter is available.</exception>

        protected override Task<Account> _Login(string password = null)
        {
            if (_internalWallet != null)
                return _internalWallet.Login(password);
            throw new NotImplementedException();
        }

        protected override Task<Transaction> _SignTransaction(Transaction transaction)
        {
            if (_internalWallet != null)
                return _internalWallet.SignTransaction(transaction);
            throw new NotImplementedException();
        }

        protected override Task<Transaction[]> _SignAllTransactions(Transaction[] transactions)
        {
            if (_internalWallet != null)
                return _internalWallet.SignAllTransactions(transactions);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Signs the provided message using the active wallet.
        /// </summary>
        /// <param name="message">Raw message bytes to sign.</param>
        /// <returns>The signature as a byte array.</returns>
        /// <exception cref="NotImplementedException">Thrown when no internal wallet implementation is available to perform signing.</exception>
        public override Task<byte[]> SignMessage(byte[] message)
        {
            if (_internalWallet != null)
                return _internalWallet.SignMessage(message);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new account using an optional BIP-39 mnemonic and optional password.
        /// </summary>
        /// <param name="mnemonic">Optional BIP-39 mnemonic phrase to derive the account; if null, an implementation may generate a new mnemonic.</param>
        /// <param name="password">Optional password used to protect or encrypt the created account; pass null for no password.</param>
        /// <returns>A task that yields the created <see cref="Account"/>.</returns>
        /// <exception cref="NotImplementedException">Thrown because account creation is not implemented in this adapter.</exception>
        protected override Task<Account> _CreateAccount(string mnemonic = null, string password = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Logs out of this wallet adapter and also logs out the underlying platform-specific wallet if one is attached.
        /// </summary>
        /// <remarks>
        /// Performs adapter-level logout logic and, when an internal platform adapter is present, invokes its logout routine.
        /// </remarks>
        public override void Logout()
        {
            base.Logout();
            _internalWallet?.Logout();
        }

        // ─── New MWA APIs ────────────────────────────────────────────────────────

        /// <summary>
        /// Explicitly disconnects the wallet:
        /// 1. Sends Deauthorize to the wallet app (revokes token)
        /// 2. Clears cached auth state
        /// 3. Fires <see cref="OnWalletDisconnected"/>
        /// 
        /// Use for "Sign Out" buttons in your game UI.
        /// Only available on Android (no-op on other platforms).
        /// <summary>
        /// Disconnects the connected mobile wallet on Android; on other platforms, performs a regular logout.
        /// </summary>
        /// <returns>A Task that completes when the disconnect or logout operation has finished.</returns>
        public Task DisconnectWallet()
        {
            if (MobileAdapter != null)
                return MobileAdapter.DisconnectWallet();

            // On non-Android platforms, fall back to regular logout
            Logout();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Attempts a silent reconnect using a cached auth token.
        /// If no valid token exists, falls back to a full Authorize flow (user prompted).
        /// Fires <see cref="OnWalletReconnected"/> on silent success.
        /// Only meaningful on Android. Other platforms perform a normal Login.
        /// <summary>
        /// Attempts to reconnect a previously connected mobile wallet; on non-mobile platforms initiates the standard login flow.
        /// </summary>
        /// <returns>The Account for the reconnected or newly authenticated wallet.</returns>
        public Task<Account> ReconnectWallet()
        {
            if (MobileAdapter != null)
                return MobileAdapter.ReconnectWallet();
            return Login();
        }

        /// <summary>
        /// Queries the wallet for its supported capabilities and limits.
        /// Returns null on non-Android platforms or if the wallet does not support this endpoint.
        /// <summary>
        /// Retrieves the capabilities of the connected mobile wallet if a mobile adapter is available.
        /// </summary>
        /// <returns>`WalletCapabilities` describing features supported by the connected mobile wallet, or `null` when no mobile adapter is present.</returns>
        public Task<WalletCapabilities> GetCapabilities()
        {
            if (MobileAdapter != null)
                return MobileAdapter.GetCapabilities();
            return Task.FromResult<WalletCapabilities>(null);
        }
    }
}