using DotNetty.Codecs.STAN.Packets;
using Hunter.STAN.Client;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


public partial class STANClientFactory
{
    private STANClient _client;
    private readonly STANOptions _clientOptions;
    private readonly static SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);

    public STANClientFactory(
        IOptions<STANOptions> clientOptions)
    {
        if (clientOptions == null)
        {
            throw new ArgumentNullException(nameof(clientOptions));
        }
        _clientOptions = clientOptions.Value;
    }

    public STANClientFactory(
        STANOptions clientOptions)
    {
        if (clientOptions == null)
        {
            throw new ArgumentNullException(nameof(clientOptions));
        }
        _clientOptions = clientOptions;
    }

    public async Task ConnectionAsync(CancellationToken token = default)
    {
        await _semaphoreSlim.WaitAsync(token);

        while (true)
        {
            try
            {
                if (_client == null)
                {
                    _client = new STANClient(_clientOptions);
                    await _client.ContentcAsync();
                }
                break;
            }
            catch (Exception ex)
            {
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }


    /// <summary>
    /// 同步发送
    /// </summary>
    /// <param name="subject">主体</param>
    /// <param name="data">数据</param>
    /// <returns></returns>
    public Task<PubAckPacket> PublishWaitAckAsync(string subject, byte[] data)
    {
        return _client.PublishWaitAckAsync(subject, data);
    }
}

