using System;

namespace Epos.Service.Interface.Grpc
{
    /// <summary>
    /// gRPC (<see cref="https://grpc.io"/>) service helper
    /// </summary>
    public static class Grpc
    {
        public static readonly Uri Url = new Uri("http://localhost:9007");
    }
}
