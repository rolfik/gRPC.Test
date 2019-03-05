using Epos.ServiceClient.Grpc;
using System;
using System.Threading.Tasks;

namespace Grpc.Test.Client
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.Title = "gRPC Client";
            GrpcClient.Start();
            Console.WriteLine("gRPC client started. Press spacebar to switch mode or any other key to stop it.");
            GrpcClient.ItemChanges.RestartMonitorOnEndOfStream = false;
            while (Console.ReadKey().Key == ConsoleKey.Spacebar)
                GrpcClient.ItemChanges.RestartMonitorOnEndOfStream = !GrpcClient.ItemChanges.RestartMonitorOnEndOfStream;
            await GrpcClient.Stop();
        }
    }
}
