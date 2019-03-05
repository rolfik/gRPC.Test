using Epos.Service.Interface.Notifications;
using Grpc.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Epos.ServiceClient.Grpc.Notifications
{
    public class ItemChangesServiceClient :
        ItemChangesService.ItemChangesServiceClient,
        IDisposable
    {
        #region Init/Uninit

        public ItemChangesServiceClient(Channel channel) :
            base(channel)
            => Monitor();

        public void Dispose()
        {
            DisposeMonitor();
            disposed = true;
        }

        private void DisposeMonitor()
        {
            if (monitor == null)
                return;
            monitor.Dispose();
            monitor = null;
        }

        private bool disposed;

        #endregion

        #region Monitor

        public bool RestartMonitorOnEndOfStream
        {
            get => restartMonitorOnEndOfStream;
            set
            {
                restartMonitorOnEndOfStream = value;
                Console.WriteLine($"Mode: {nameof(RestartMonitorOnEndOfStream)} = {value}");
            }
        }

        private async void Monitor()
        {
            while (!disposed)
            {
                EnsureMonitor();
                var change = await NextChange();
                if (change?.HasChanges == true)
                    ItemChanges.OnItemsChanged(change);
            }
        }

        private void EnsureMonitor()
        {
            if (monitor is null)
                monitor = Monitor(ItemsFilter.All);
        }

        private async Task<ItemsChanged> NextChange()
        {
            try
            {
                // new change
                if (await monitor.ResponseStream.MoveNext(CancellationToken.None))
                    return monitor.ResponseStream.Current;
                // nothing new yet
                Console.WriteLine("End of Monitor response stream");
                if (RestartMonitorOnEndOfStream)
                {
                    Console.WriteLine("Restarting Monitor");
                    DisposeMonitor();
                }
                await DelayNextChangeRetry();
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // try to start new monitoring
                DisposeMonitor();
                await DelayNextChangeRetry();
                // signal any change meaning to refresh all missed changes
                return ItemsChanged.Any;
            }
        }

        private Task DelayNextChangeRetry() => Task.Delay(nextChangeRetryDelay);

        private readonly TimeSpan nextChangeRetryDelay = TimeSpan.FromSeconds(1);
        private AsyncServerStreamingCall<ItemsChanged> monitor;
        private bool restartMonitorOnEndOfStream;

        #endregion
    }
}
