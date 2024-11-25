using InertGas.Application.Model;
using NLog;

namespace InertGas.Application.AutomationAction
{
    internal abstract class AutomationAction
    {
        public static ApplicationModel AppModel => ApplicationModel.Instance;

        public event EventHandler Completed;

        public event EventHandler<Exception> ErrorOccurred;

        public event EventHandler Interrupted;

        public WorkingPhases CurrentPhase { get; set; }

        public AutomationAction NextAction { get; set; }

        public async Task Execute(CancellationTokenSource cancellationTokenSource)
        {
            LogInfo($"begin executing {this}.");

            try
            {
                await InternalExecute(cancellationTokenSource);
                OnCompleted();
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is TaskCanceledException)
            {
                OnInterrupted();
            }
            catch (Exception e)
            {
                OnErrorOccurred(e);
            }
        }

        protected abstract Task InternalExecute(CancellationTokenSource cancellationTokenSource);

        protected virtual void OnCompleted()
        {
            LogInfo($"completed {this} successfully.");
            Completed?.Invoke(this, null);
        }

        protected virtual void OnErrorOccurred(Exception ex)
        {
            LogError(ex, $"encountered an error while executing {this}.");
            ErrorOccurred?.Invoke(this, ex);
        }

        protected virtual void OnInterrupted()
        {
            LogWarning($"interrupted while executing {this}.");
            Interrupted?.Invoke(this, null);
        }

        private void LogInfo(string message)
        {
            logger_.Info($"{this} {message}");
        }

        private void LogWarning(string message)
        {
            logger_.Warn($"{this} {message}");
        }

        private void LogError(Exception ex, string message)
        {
            logger_.Error(ex, $"{this} {message}");
        }

        protected static readonly Logger logger_ = LogManager.GetCurrentClassLogger();
        protected readonly List<Func<CancellationToken, Task>> internalActions_ = new();
    }
}
