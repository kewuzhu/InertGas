using CommunityToolkit.Mvvm.ComponentModel;
using InertGas.Application.UI.ApplicationStages;
using InertGas.Common.Model;
using InertGas.Common.Utility;
using InertGas.HeatingBox;
using NLog;
using System.Collections.ObjectModel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace InertGas.Application.Model
{
    internal partial class ApplicationModel : ObservableObject
    {
        public event EventHandler StageTransitionCompleted;

        private static readonly Lazy<ApplicationModel> lazy = new(() => new ApplicationModel());

        public static ApplicationModel Instance => lazy.Value;

        public DateTime StartUpTime { get; } = DateTime.Now;

        public Language Language { get; set; }

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

        public CollectedData CurrentData { get; set; }

        public ObservableCollectionWithRangeSupport<CollectedData> CollectedDataSet { get; } = new();

        public readonly List<HeatingBoxControl> HeatingBoxControls = new();

        private static readonly Logger logger_ = LogManager.GetCurrentClassLogger();
    }
}
