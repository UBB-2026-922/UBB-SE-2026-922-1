// <copyright file="SystemBrowser.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the SystemBrowser class.
// </summary>

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityModel.OidcClient.Browser;

namespace BankApp.Desktop.Utilities;

/// <summary>
///     Implementation used to invoke the system default browser.
/// </summary>
public class SystemBrowser : IBrowser
{
    private readonly string? _path;
    private readonly int _port;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SystemBrowser" /> class.
    /// </summary>
    /// <param name="port">The port number.</param>
    /// <param name="path">The _path.</param>
    /// <returns>The result of the operation.</returns>
    public SystemBrowser(int? port = null, string? path = null)
    {
        _path = path;
        _port = port ?? GetRandomUnusedPort();
    }

    /// <inheritdoc />
    /// <param name="options">The options value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    /// <returns>The result of the operation.</returns>
    public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
    {
        using var listener = new LoopbackHttpListener(_port, _path);
        OpenBrowser(options.StartUrl);
        try
        {
            string result = await listener.WaitForCallbackAsync();
            if (string.IsNullOrWhiteSpace(result))
            {
                return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = "Empty response." };
            }

            return new BrowserResult { Response = result, ResultType = BrowserResultType.Success };
        }
        catch (TaskCanceledException exception)
        {
            return new BrowserResult { ResultType = BrowserResultType.Timeout, Error = exception.Message };
        }
        catch (Exception exception)
        {
            return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = exception.Message };
        }
    }

    private static void OpenBrowser(string browserAddress)
    {
        Process.Start(
            new ProcessStartInfo
            {
                FileName = browserAddress,
                UseShellExecute = true,
            });
    }

    private int GetRandomUnusedPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}