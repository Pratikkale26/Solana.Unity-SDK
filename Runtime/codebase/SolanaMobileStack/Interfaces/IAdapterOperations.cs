using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Scripting;

// ReSharper disable once CheckNamespace

[Preserve]
public interface IAdapterOperations
{
    /// <summary>
    /// Request authorization from the wallet for the calling application.
    /// </summary>
    /// <param name="identityUri">A URI that uniquely identifies the application (for example, a manifest or deeplink).</param>
    /// <param name="iconUri">A URI for the application's icon to display in the wallet UI.</param>
    /// <param name="identityName">A human-readable name for the application shown to the user.</param>
    /// <param name="rpcCluster">The RPC cluster identifier the application intends to use (for example, "mainnet" or "testnet").</param>
    /// <returns>An <see cref="AuthorizationResult"/> containing an authorization token when authorization succeeds.</returns>
    [Preserve]
    public Task<AuthorizationResult> Authorize(Uri identityUri, Uri iconUri, string identityName, string rpcCluster);

    /// <summary>
    /// Reauthorizes a previously authorized identity using an existing auth token without prompting the user.
    /// </summary>
    /// <param name="identityUri">The identity's URI used to identify the requesting application.</param>
    /// <param name="iconUri">A URI for the identity's icon to display in the wallet UI.</param>
    /// <param name="identityName">A human-readable name for the identity shown to the user.</param>
    /// <param name="authToken">A previously issued authorization token to reuse for reauthorization.</param>
    /// <returns>An <see cref="AuthorizationResult"/> containing the resulting authorization token and session details.</returns>
    [Preserve]
    public Task<AuthorizationResult> Reauthorize(Uri identityUri, Uri iconUri, string identityName, string authToken);

    /// <summary>
    /// Revoke an authorization token so the wallet forgets the associated session.
    /// </summary>
    /// <param name="authToken">The previously issued authorization token to revoke.</param>
    [Preserve]
    public Task Deauthorize(string authToken);

    /// <summary>
    /// Queries the connected wallet for supported features and protocol limits.
    /// </summary>
    /// <returns>A <see cref="WalletCapabilities"/> instance describing the wallet's supported features, limits, and constraints.</returns>
    [Preserve]
    public Task<WalletCapabilities> GetCapabilities();

    /// <summary>
    /// Requests the wallet to sign one or more serialized transactions.
    /// </summary>
    /// <param name="transactions">A collection of transactions, each serialized as a byte array.</param>
    /// <returns>A <see cref="SignedResult"/> containing the signed transactions and any associated metadata.</returns>
    [Preserve]
    public Task<SignedResult> SignTransactions(IEnumerable<byte[]> transactions);

    /// <summary>
    /// Requests the wallet to sign one or more arbitrary messages.
    /// </summary>
    /// <param name="messages">A sequence of messages to be signed, each serialized as a byte array.</param>
    /// <param name="addresses">A sequence of addresses (serialized as byte arrays) corresponding one-to-one with <paramref name="messages"/> indicating which key should sign each message.</param>
    /// <returns>A <see cref="SignedResult"/> containing the signed messages and any associated metadata.</returns>
    [Preserve]
    public Task<SignedResult> SignMessages(IEnumerable<byte[]> messages, IEnumerable<byte[]> addresses);
}