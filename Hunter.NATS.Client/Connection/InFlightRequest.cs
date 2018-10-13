using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hunter.NATS.Client
{
    // Handles in-flight requests when using the new-style request/reply behavior
    internal sealed class InFlightRequest
    {
        public InFlightRequest(CancellationToken token, int timeout)
        {
            this.Waiter = new TaskCompletionSource<Message>();
            if (token != default(CancellationToken))
            {
                token.Register(() => this.Waiter.TrySetCanceled());

                if (timeout > 0)
                {
                    var timeoutToken = new CancellationTokenSource();

                    var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                        timeoutToken.Token, token);
                    this.Token = linkedTokenSource.Token;

                    timeoutToken.Token.Register(
                        () => this.Waiter.TrySetException(new NATSTimeoutException()));
                    timeoutToken.CancelAfter(timeout);
                }
                else
                {
                    this.Token = token;
                }
            }
            else
            {
                if (timeout > 0)
                {
                    var timeoutToken = new CancellationTokenSource();
                    this.Token = timeoutToken.Token;

                    timeoutToken.Token.Register(
                        () => this.Waiter.TrySetException(new NATSTimeoutException()));
                    timeoutToken.CancelAfter(timeout);
                }
            }
        }

        public string Id { get; set; }
        public CancellationToken Token { get; private set; }
        public TaskCompletionSource<Message> Waiter { get; private set; }
    }
}
