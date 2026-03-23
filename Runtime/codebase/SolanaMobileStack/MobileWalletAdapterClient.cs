using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Solana.Unity.SDK;
using UnityEngine;
using UnityEngine.Scripting;

// ReSharper disable once CheckNamespace
[Preserve]
public class MobileWalletAdapterClient : JsonRpc20Client, IAdapterOperations, IMessageReceiver
{
    private int _mNextMessageId = 1;

    /// <summary>
    /// Creates a MobileWalletAdapterClient that uses the provided message sender for JSON-RPC communication.
    /// </summary>
    /// <param name="messageSender">The transport used to send and receive JSON-RPC messages.</param>
    public MobileWalletAdapterClient(IMessageSender messageSender) : base(messageSender)
    {
    }

    /// <summary>
    /// Initiates an authorization request with the mobile wallet adapter for the specified identity.
    /// </summary>
    /// <param name="identityUri">An absolute, hierarchical URI that uniquely identifies the requesting identity; may be null.</param>
    /// <param name="iconUri">A relative URI referencing the identity's icon; may be null and must be relative if provided.</param>
    /// <param name="identityName">The display name for the requesting identity.</param>
    /// <param name="cluster">The target cluster identifier (for example, a network or environment); may be null.</param>
    /// <returns>An <see cref="AuthorizationResult"/> containing the wallet's authorization response.</returns>
    [Preserve]
    public Task<AuthorizationResult> Authorize(Uri identityUri, Uri iconUri, string identityName, string cluster)
    {
        var request = PrepareAuthRequest(identityUri, iconUri, identityName, cluster, "authorize");
        return SendRequest<AuthorizationResult>(request);
    }

    /// <summary>
    /// Requests reauthorization of the given application identity with the wallet.
    /// </summary>
    /// <param name="identityUri">The absolute, hierarchical URI that identifies the application (may be null).</param>
    /// <param name="iconUri">A relative URI for the application's icon (may be null).</param>
    /// <param name="identityName">The display name of the application.</param>
    /// <param name="authToken">The existing authorization token to refresh or revalidate.</param>
    /// <returns>An AuthorizationResult containing the wallet's reauthorization response.</returns>
    public Task<AuthorizationResult> Reauthorize(Uri identityUri, Uri iconUri, string identityName, string authToken)
    {
        var request = PrepareAuthRequest(identityUri, iconUri, identityName, null, "reauthorize");
        request.Params.AuthToken = authToken;
        return SendRequest<AuthorizationResult>(request);
    }

    /// <summary>
    /// Revokes the given auth token with the wallet app. The wallet will discard the session.
    /// Always call this before clearing local state on logout.
    /// <summary>
    /// Sends a deauthorization request to the wallet for the specified authorization token.
    /// </summary>
    /// <param name="authToken">The authorization token to revoke. Must be a token previously issued to the client.</param>
    public Task Deauthorize(string authToken)
    {
        var request = new JsonRequest
        {
            JsonRpc = "2.0",
            Method = "deauthorize",
            Params = new JsonRequest.JsonRequestParams
            {
                AuthToken = authToken
            },
            Id = NextMessageId()
        };
        return SendRequest<object>(request);
    }

    /// <summary>
    /// Queries the connected wallet for its supported capabilities and limits.
    /// Use this to adapt batch sizes and feature detection for your app.
    /// <summary>
    /// Requests the connected wallet for its capabilities.
    /// </summary>
    /// <returns>A WalletCapabilities object describing the wallet's supported features.</returns>
    public Task<WalletCapabilities> GetCapabilities()
    {
        var request = new JsonRequest
        {
            JsonRpc = "2.0",
            Method = "get_capabilities",
            Params = new JsonRequest.JsonRequestParams(),
            Id = NextMessageId()
        };
        return SendRequest<WalletCapabilities>(request);
    }

    /// <summary>
    /// Requests the wallet to sign a collection of transactions.
    /// </summary>
    /// <param name="transactions">A collection of transaction payloads as byte arrays to be signed.</param>
    /// <returns>A <see cref="SignedResult"/> containing the signed transaction payloads and any associated metadata.</returns>
    public Task<SignedResult> SignTransactions(IEnumerable<byte[]> transactions)
    {
        var request = PrepareSignTransactionsRequest(transactions);
        return SendRequest<SignedResult>(request);
    }

    public Task<SignedResult> SignMessages(IEnumerable<byte[]> messages, IEnumerable<byte[]> addresses)
    {
        var request = PrepareSignMessagesRequest(messages, addresses);
        return SendRequest<SignedResult>(request);
    }

    /// <summary>
    /// Creates a JSON-RPC 2.0 request for an authorization-style RPC (authorize or reauthorize) populated with the provided identity and parameters.
    /// </summary>
    /// <param name="uriIdentity">The identity URI to include; if non-null it must be an absolute, hierarchical URI.</param>
    /// <param name="icon">A relative URI for the identity icon; if non-null it must be a relative URI.</param>
    /// <param name="name">The display name for the identity.</param>
    /// <param name="cluster">The cluster identifier to include in the request params (may be null).</param>
    /// <param name="method">The JSON-RPC method name to set on the request (e.g., "authorize" or "reauthorize").</param>
    /// <returns>A JsonRequest configured with JSON-RPC version "2.0", the specified method, populated Params (Identity and Cluster), and a unique request Id.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="uriIdentity"/> is non-null and not absolute, or if <paramref name="icon"/> is non-null and absolute.</exception>
    private JsonRequest PrepareAuthRequest(Uri uriIdentity, Uri icon, string name, string cluster, string method)
    {
        if (uriIdentity != null && !uriIdentity.IsAbsoluteUri)
            throw new ArgumentException("If non-null, identityUri must be an absolute, hierarchical Uri");
        if (icon != null && icon.IsAbsoluteUri)
            throw new ArgumentException("If non-null, iconRelativeUri must be a relative Uri");

        return new JsonRequest
        {
            JsonRpc = "2.0",
            Method = method,
            Params = new JsonRequest.JsonRequestParams
            {
                Identity = new JsonRequest.JsonRequestIdentity
                {
                    Uri = uriIdentity,
                    Icon = icon,
                    Name = name
                },
                Cluster = cluster
            },
            Id = NextMessageId()
        };
    }

    /// <summary>
    /// Builds a JSON-RPC 2.0 request that asks the wallet to sign multiple transactions.
    /// </summary>
    /// <param name="transactions">A sequence of transaction byte arrays to be base64-encoded as the request payloads.</param>
    /// <returns>A JsonRequest configured with method "sign_transactions", Params.Payloads containing the base64-encoded transactions, and a unique request id.</returns>
    private JsonRequest PrepareSignTransactionsRequest(IEnumerable<byte[]> transactions)
    {
        return new JsonRequest
        {
            JsonRpc = "2.0",
            Method = "sign_transactions",
            Params = new JsonRequest.JsonRequestParams
            {
                Payloads = transactions.Select(Convert.ToBase64String).ToList()
            },
            Id = NextMessageId()
        };
    }

    /// <summary>
    /// Create a JSON-RPC 2.0 request for the "sign_messages" method.
    /// </summary>
    /// <param name="messages">Message payloads to be signed; each byte array is encoded as a base64 string in the request's Payloads.</param>
    /// <param name="addresses">Addresses corresponding to each message; each byte array is encoded as a base64 string in the request's Addresses.</param>
    /// <returns>A <see cref="JsonRequest"/> representing a "sign_messages" request with Payloads and Addresses as base64-encoded strings and an assigned request Id.</returns>
    private JsonRequest PrepareSignMessagesRequest(IEnumerable<byte[]> messages, IEnumerable<byte[]> addresses)
    {
        return new JsonRequest
        {
            JsonRpc = "2.0",
            Method = "sign_messages",
            Params = new JsonRequest.JsonRequestParams
            {
                Payloads = messages.Select(Convert.ToBase64String).ToList(),
                Addresses = addresses.Select(Convert.ToBase64String).ToList()
            },
            Id = NextMessageId()
        };
    }

    /// <summary>
    /// Retrieves the next JSON-RPC request identifier and advances the internal counter.
    /// </summary>
    /// <returns>The current message identifier (integer) to use for a JSON-RPC request.</returns>
    private int NextMessageId()
    {
        return _mNextMessageId++;
    }
}