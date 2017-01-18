using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer3.Core.Logging;
using Raven.Abstractions.Data;

namespace Identityserver.Contrib.RavenDB
{
    public class TokenCleanup
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        private RavenDbServiceOptions options;
        private CancellationTokenSource source;
        private TimeSpan interval;

        public TokenCleanup(RavenDbServiceOptions options, int interval = 60)
        {
            if (options == null) throw new ArgumentNullException("options");
            if (interval < 1) throw new ArgumentException("interval must be more than 1 second");

            this.options = options;
            this.interval = TimeSpan.FromSeconds(interval);
        }

        public void Start()
        {
            if (source != null) throw new InvalidOperationException("Already started. Call Stop first.");

            source = new CancellationTokenSource();
            Task.Factory.StartNew(() => Start(source.Token));
        }

        public void Stop()
        {
            if (source == null) throw new InvalidOperationException("Not started. Call Start first.");

            source.Cancel();
            source = null;
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Logger.Info("CancellationRequested");
                    break;
                }

                try
                {
                    await Task.Delay(interval, cancellationToken);
                }
                catch
                {
                    Logger.Info("Task.Delay exception. exiting.");
                    break;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    Logger.Info("CancellationRequested");
                    break;
                }

                await ClearTokens();
            }
        }

        private async Task ClearTokens()
        {
            try
            {
                Logger.Info("Clearing tokens");

                await options.Store.AsyncDatabaseCommands.DeleteByIndexAsync("TokenCleanupIndex",
                    new IndexQuery
                    {
                        Query = "Expires:[* TO \"" + DateTime.UtcNow.ToString("o") + "\"]"
                    }, new BulkOperationOptions
                    {
                        AllowStale = true
                    });
            }
            catch (Exception ex)
            {
                Logger.ErrorException("Exception cleaning tokens", ex);
            }
        }
    }
}
