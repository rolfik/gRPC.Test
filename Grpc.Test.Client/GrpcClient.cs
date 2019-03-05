using Epos.ServiceClient.Grpc.Notifications;
using Grpc.Core;
using System;
using System.Threading.Tasks;
using static Epos.Service.Interface.Grpc.Grpc;

namespace Epos.ServiceClient.Grpc
{
    /// <summary>
    /// gRPC (<see cref="https://grpc.io"/>) client
    /// </summary>
    public static class GrpcClient
    {
        public static ItemChangesServiceClient ItemChanges { get; private set; }

        public static void Start()
        {
            try
            {
                if (channel != null)
                    throw new InvalidOperationException($"{nameof(Start)} was already called");
                channel = new Channel(Url.Host, Url.Port, ChannelCredentials.Insecure);
                ItemChanges = new ItemChangesServiceClient(channel);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static async Task Stop()
        {
            try
            {
                if (channel is null)
                    throw new InvalidOperationException($"{nameof(Start)} was not called");
                ItemChanges.Dispose();
                await channel.ShutdownAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static Channel channel;
    }
}
