using Epos.Service.Interface.Notifications;
using Grpc.Core;
using System;
using System.Threading.Tasks;
using ResponseStream = Grpc.Core.IServerStreamWriter<Epos.Service.Interface.Notifications.ItemsChanged>;

namespace Epos.Service.Core.Notifications
{
    /// <summary>
    /// gRPC item changes notifications
    /// </summary>
    public class ItemChangesService :
        Interface.Notifications.ItemChangesService.ItemChangesServiceBase,
        IDisposable
    {
        #region Init/Uninit

        public ItemChangesService() =>
            ItemChanges.ItemsChanged += OnItemsChanged;

        public void Dispose()
        {
            ItemChanges.ItemsChanged -= OnItemsChanged;
            monitorCalls.Dispose();
        }

        #endregion

        public override Task Monitor(ItemsFilter request, ResponseStream responseStream, ServerCallContext context)
        {
            var call = new ServerStreamingCall<ItemsFilter, ItemsChanged>(context, request, responseStream);
            lock (monitorCalls)
                monitorCalls.Add(call);
            return call.Task;
        }

        private async void OnItemsChanged(ItemsChanged change) => await monitorCalls.Write(i => change);

        private readonly ServerStreamingCalls<ItemsFilter, ItemsChanged> monitorCalls = new ServerStreamingCalls<ItemsFilter, ItemsChanged>();
    }
}
