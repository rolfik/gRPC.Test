using Epos.Service.Interface.Notifications;
using Grpc.Core;
using System;
using System.Collections.Generic;
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

        public void Dispose() => ItemChanges.ItemsChanged -= OnItemsChanged;

        #endregion

        public override Task Monitor(ItemsFilter request, ResponseStream responseStream, ServerCallContext context)
        {
            OnMonitorStarted(context, responseStream);
            return Task.CompletedTask;
        }

        private void OnMonitorStarted(ServerCallContext context, ResponseStream responseStream)
        {
            Console.WriteLine($"{nameof(Monitor)} client {context.Peer} connected");
            lock (monitorCalls)
            {
                monitorCalls.Add((context, responseStream));
            }
        }

        private void OnMonitorEnded(ServerCallContext context, int index)
        {
            Console.WriteLine($"{nameof(Monitor)} client {context.Peer} disconnected with status {context.Status}");
            monitorCalls.RemoveAt(index);
        }

        private async void OnItemsChanged(ItemsChanged change)
        {
                var monitorCalls = GetMonitorCalls();
                int count = monitorCalls?.Length ?? 0;
                Console.WriteLine($"Client count {count}");
                if (count == 0)
                    return;
                foreach (var monitorCall in monitorCalls)
                {
                    try
                    {
                        await monitorCall.responseStream.WriteAsync(change);
                    }
                    catch (RpcException e)
                    {
                        Console.WriteLine(e);
                    }
                }
        }

        private (ServerCallContext context, ResponseStream responseStream)[] GetMonitorCalls()
        {
            lock (monitorCalls)
            {
                ClearMonitorCalls();
                return monitorCalls.Count == 0 ?
                    null :
                    monitorCalls.ToArray();
            }
        }

        private void ClearMonitorCalls()
        {
            for (int i = monitorCalls.Count - 1; i >= 0; i--)
            {
                var monitorCall = monitorCalls[i];
                if (monitorCall.context.Status.StatusCode != StatusCode.OK)
                    OnMonitorEnded(monitorCall.context, i);
            }
        }

        private readonly List<(ServerCallContext context, ResponseStream responseStream)> monitorCalls = new List<(ServerCallContext, ResponseStream)>();
    }
}
