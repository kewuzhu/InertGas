using CommunityToolkit.Mvvm.ComponentModel;
using InertGas.Application.Model;
using NLog;
using System.ComponentModel;

namespace InertGas.Application.UI.ApplicationStages
{
    internal abstract partial class ApplicationStageViewModel : ObservableObject
    {
        public static ApplicationModel AppModel => ApplicationModel.Instance;

        public string Title { get; set; }

        public Model.ApplicationStage Stage { get; }

        [ObservableProperty]
        private bool isCurrent;

        [ObservableProperty]
        private bool isEnabled;

        public IEnumerable<object> ContextMenu { get; protected set; }

        protected ApplicationStageViewModel(Model.ApplicationStage stage)
        {
            Stage = stage;

            AppModel.PropertyChanging += OnAppModelPropertyChanging;
            AppModel.PropertyChanged += OnAppModelPropertyChanged;
            AppModel.StageTransitionCompleted += OnStageTransitionCompleted;
        }

        private void OnStageTransitionCompleted(object sender, EventArgs e)
        {
            if (!stageEntered_) return;

            OnStageEntered();
            stageEntered_ = false;
        }

        protected bool IsStageActive() => AppModel.CurrentApplicationStage == Stage;

        protected virtual void OnExitingStage()
        {
            logger_.Debug($"Exiting {Stage}.");
        }

        protected virtual void OnEnteringStage()
        {
            logger_.Debug($"Entering {Stage}.");
            stageEntered_ = true;
        }

        protected virtual void OnStageEntered()
        {
            logger_.Debug($"Entered {Stage}.");
        }

        protected virtual void HandleAppModelPropertyChangedDuringStage(string? propertyName) { }

        protected virtual void HandleAppModelPropertyChangedOutOfStage(string? propertyName) { }

        private void OnAppModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var enteringStage = e.PropertyName == nameof(ApplicationModel.CurrentApplicationStage) && IsStageActive();

            if (enteringStage)
                OnEnteringStage();

            if (e.PropertyName == nameof(ApplicationModel.CurrentApplicationStage)) return;

            // every other property changed
            if (IsStageActive())
                HandleAppModelPropertyChangedDuringStage(e.PropertyName);
            else
                HandleAppModelPropertyChangedOutOfStage(e.PropertyName);
        }

        private void OnAppModelPropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            var exitingStage = e.PropertyName == nameof(ApplicationModel.CurrentApplicationStage) && IsStageActive();
            if (exitingStage)
                OnExitingStage();

            if (e.PropertyName == nameof(ApplicationModel.CurrentApplicationStage)) return;
        }

        private static readonly Logger logger_ = LogManager.GetCurrentClassLogger();
        private bool stageEntered_;
    }
}
