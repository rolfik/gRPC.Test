using Grpc.Core;
using System;
using System.Threading.Tasks;
using static Epos.Service.Interface.Grpc.Grpc;

namespace Epos.Service.Core.Grpc
{
    /// <summary>
    /// gRPC (<see cref="https://grpc.io"/>) server
    /// </summary>
    public static class GrpcServer
    {
        public static void Start()
        {
            try
            {
                if (server != null)
                    throw new InvalidOperationException($"{nameof(Start)} was already called");
                itemChanges = new Notifications.ItemChangesService();
                server = new Server
                {
                    Ports =
                        {
                            new ServerPort(Url.Host, Url.Port, ServerCredentials.Insecure)
                        },
                    Services =
                        {
                            Interface.Notifications.ItemChangesService.BindService(itemChanges)
                        }
                };
                server.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static async Task Stop()
        {
            try
            {
                if (server is null)
                    throw new InvalidOperationException($"{nameof(Start)} was not called");
                itemChanges.Dispose();
                await server.KillAsync(); // stop serving even if there are calls in progress
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static Notifications.ItemChangesService itemChanges;
        private static Server server;
    }
}
