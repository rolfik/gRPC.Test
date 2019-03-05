using Epos.Service.Core.Grpc;
using Epos.Service.Interface.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grpc.Test.Service
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.Title = "gRPC Service";
            GrpcServer.Start();
            Console.WriteLine("gRPC server started.\nPress spacebar to notify item changes or any other key to stop it.");
            while (Console.ReadKey().Key == ConsoleKey.Spacebar)
                RaiseItemsChanged();
            await GrpcServer.Stop();
        }

        private static void RaiseItemsChanged()
        {
            var change = new ItemsChanged()
            {
                Name = "Item",
                AddedIds = { NextIds() }
            };
            change.UpdatedIds.Add(NextIds().Except(change.AddedIds));
            change.RemovedIds.Add(NextIds().Except(change.AddedIds).Except(change.UpdatedIds));
            ItemChanges.OnItemsChanged(change);
        }

        private static IEnumerable<string> NextIds() => Enumerable.
            Range(1, 5).
            Select(i => random.Next(1, 99).ToString()).
            Distinct();

        private static readonly Random random = new Random();
    }
}
