using CommunityToolkit.Mvvm.ComponentModel;
using InertGas.Application.UI.ApplicationStages;
using InertGas.Common.Model;
using InertGas.Common.Utility;
using NLog;
using System.Collections.ObjectModel;

namespace InertGas.Application.Model
{
    internal partial class ApplicationModel : ObservableObject
    {
        public event EventHandler StageTransitionCompleted;

        private static readonly Lazy<ApplicationModel> lazy = new(() => new ApplicationModel());

        public static ApplicationModel Instance => lazy.Value;

        public DateTime StartUpTime { get; } = DateTime.Now;

        private ApplicationStage currentApplicationStage_;
        public ApplicationStage CurrentApplicationStage
        {
            get => currentApplicationStage_;
            set
            {
                var prevStage = currentApplicationStage_;
                if (prevStage == value) return;

                logger_.Debug($"Begin stage transition {prevStage} ---> {value}.");
                if (SetProperty(ref currentApplicationStage_, value))
                {
                    StageTransitionCompleted?.Invoke(this, null);
                    logger_.Debug($"End stage transition {prevStage} ---> {value}.");
                }
            }
        }

        public ObservableCollection<ApplicationStageViewModel> ApplicationStages { get; } = new();

        public User CurrentUser { get; set; }

        public ObservableCollectionWithRangeSupport<User> Users { get; } = new();

        [ObservableProperty]
        private CurrentData currentData = new();

        public ObservableCollectionWithRangeSupport<CollectedData> CollectedDataSet { get; } = new();

        public List<ValveControl> ValveControls { get; } = new();

        private static readonly Logger logger_ = LogManager.GetCurrentClassLogger();
    }
}
