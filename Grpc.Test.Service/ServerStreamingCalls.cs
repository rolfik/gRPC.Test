using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Epos.Service.Core.Notifications
{
    public class ServerStreamingCalls<TRequest, TResponse> :
        IDisposable
        where TRequest : class
        where TResponse : class
    {
        public string Method { get; private set; }

        public void Add(ServerStreamingCall<TRequest, TResponse> call)
        {
            lock (calls)
            {
                var method = call.Context.Method;
                if (Method == null)
                    Method = method;
                else if (method != Method)
                    throw new ArgumentException($"Expected method {Method} instead of {method}", nameof(call));
                calls.Add(call);
            }
        }

        public IList<ServerStreamingCall<TRequest, TResponse>> List()
        {
            lock (calls)
            {
                Clear();
                return calls.Count == 0 ?
                    Array.Empty<ServerStreamingCall<TRequest, TResponse>>() :
                    calls.ToArray();
            }
        }

        public async Task Write(Func<TRequest, TResponse> getResponse)
        {
            if (getResponse == null)
                throw new ArgumentNullException(nameof(getResponse));
            var calls = List();
            int count = calls.Count;
            Console.WriteLine($"Write response to {count} client(s)");
            if (count == 0)
                return;
            foreach (var call in calls)
            {
                var response = getResponse(call.Request);
                await call.Write(response);
            }
        }

        public void Dispose()
        {
            lock (calls)
            {
                foreach (var call in calls)
                    call.Dispose();
                calls.Clear();
                Method = null;
            }
        }

        private void Clear()
        {
            for (int i = calls.Count - 1; i >= 0; i--)
            {
                var call = calls[i];
                if (!call.IsValid)
                {
                    call.Dispose();
                    calls.RemoveAt(i);
                }
            }
            if (calls.Count == 0)
                Method = null;
        }

        private readonly List<ServerStreamingCall<TRequest, TResponse>> calls = new List<ServerStreamingCall<TRequest, TResponse>>();
    }
}
