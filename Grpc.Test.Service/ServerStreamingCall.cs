using Grpc.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Epos.Service.Core.Notifications
{
    public class ServerStreamingCall<TRequest, TResponse> :
        IDisposable
        where TRequest : class
        where TResponse : class
    {
        #region Init/Uninit

        public ServerStreamingCall(ServerCallContext context, TRequest request, IServerStreamWriter<TResponse> responseStream)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            if (request is null)
                throw new ArgumentNullException(nameof(request));
            if (responseStream is null)
                throw new ArgumentNullException(nameof(responseStream));
            Context = context;
            Method = context.Method;
            Client = context.Peer;
            Register(Context.CancellationToken);
            Request = request;
            ResponseStream = responseStream;
            OnStarted();
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            if (IsDisposed)
                return;
            Complete();
            IsDisposed = true;
        }

        #endregion

        public ServerCallContext Context { get; }
        public string Method { get; }
        public string Client { get; }

        public bool IsValid =>
            Context.Status.StatusCode == StatusCode.OK &&
            !TaskEnded &&
            !IsDisposed;

        public TRequest Request { get; }

        public IServerStreamWriter<TResponse> ResponseStream { get; }

        public async Task Write(TResponse response)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));
            Console.WriteLine($"Write {Method} response to client {Client}");
            try
            {
                await ResponseStream.WriteAsync(response);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #region Task

        public Task Task => call.Task;
        public bool TaskEnded =>
            Task.IsCompleted ||
            Task.IsCanceled ||
            Task.IsFaulted;

        public bool Complete(Exception error = null)
        {
            bool ended = error is null ?
                call.TrySetResult(null) :
                call.TrySetException(error);
            if (ended)
                OnEnded(error);
            return ended;
        }

        public bool Cancel(CancellationToken cancellationToken)
        {
            bool ended = call.TrySetCanceled(cancellationToken);
            if (ended)
                OnEnded(new TaskCanceledException());
            return ended;
        }

        public void Register(CancellationToken cancellationToken) => cancellationToken.Register(() => Cancel(cancellationToken));

        private TaskCompletionSource<object> call = new TaskCompletionSource<object>();

        #endregion

        private void OnStarted() => Console.WriteLine($"{Method} client {Client} connected");

        private void OnEnded(Exception error) => Console.WriteLine($"{Method} client {Client} disconnected with status {Context.Status} and {ErrorText(error)}");

        private static string ErrorText(Exception error) => error is null ?
            "success" :
            error is OperationCanceledException ?
                "cancelled" :
                $"error:\n{error}";
    }
}
