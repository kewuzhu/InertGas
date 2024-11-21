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

        public List<HardwareControl> HardwareControls { get; } = new();

        [ObservableProperty]
        private int charcoalColumnTemperatureThreshold = 300;

        [ObservableProperty]
        private int column4A5ATemperatureThreshold = 300;

        [ObservableProperty]
        private int volumeFlowAThreshold = 900;

        [ObservableProperty]
        private int volumeFlowBThreshold = 40;

        [ObservableProperty]
        private bool isPneumaticPumpOneInUse;

        [ObservableProperty]
        private bool isPneumaticPumpTwoInUse;

        [ObservableProperty]
        private int collectionDuration = 5; //min

        [ObservableProperty]
        private int purificationDuration = 5; //min

        [ObservableProperty]
        private int purificationHeatingDuration = 5; //min

        [ObservableProperty]
        private int excitationHeatingDuration = 3; //hour

        private static readonly Logger logger_ = LogManager.GetCurrentClassLogger();
    }
}
