using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InertGas.Application.Model;
using InertGas.Application.Themes;
using InertGas.Application.UI.ApplicationStages;
using InertGas.Application.UI.Dialog;
using InertGas.Application.Utility;
using InertGas.Common.DataAccess;
using InertGas.Common.Model;
using InertGas.HeatingBox;
using NLog;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Security;

namespace InertGas.Application.UI
{
    internal partial class MainWindowViewModel : ObservableObject
    {
        public static ApplicationModel AppModel => ApplicationModel.Instance;

        [ObservableProperty]
        private ApplicationStageViewModel currentViewModel;

        [ObservableProperty]
        private User selectedUser;

        [ObservableProperty]
        private bool isUserLoggedIn;

        [ObservableProperty]
        private string userName = "Default";

        [ObservableProperty]
        private SecureString securePassword;

        [ObservableProperty]
        private bool isPageSwitchPlaying;

        [RelayCommand]
        private async Task LogIn()
        {
            try
            {
                SelectedUser = dataRepository_.GetUsers().ToList().FirstOrDefault(x => x.Name == UserName);

                if (SelectedUser == null)
                    throw new ArgumentNullException(nameof(SelectedUser));

                if (!SecureUtils.ValidateUserPassword(SelectedUser.Password, SecurePassword, SelectedUser.Salt))
                {
                    UserCommunication.ShowMessage(Theme.GetString(Strings.IncorrectPassword), Theme.GetString(Strings.DoubleCheckUserPasswordMessage), MessageType.Info);
                    return;
                }

                IsUserLoggedIn = true;
                SecurePassword = null;

                AppModel.CurrentUser = SelectedUser;
                AppModel.CurrentApplicationStage = ApplicationStage.MainPage;
                CurrentViewModel = viewModelMap_[AppModel.CurrentApplicationStage];
                IsPageSwitchPlaying = true;

                await InitializeHardwares();
            }
            catch (Exception ex)
            {
                UserCommunication.ShowMessage($"{Theme.GetString(Strings.Error)}", $"Message:{ex.Message}\nStackTrace:{ex.StackTrace}", MessageType.Critical);
            }
        }

        [RelayCommand]
        private void Close(object obj)
        {
            CleanUpAsync();
            var window = obj as System.Windows.Window;
            window?.Close();
        }

        [RelayCommand]
        private void SwitchToMainPage()
        {
            AppModel.CurrentApplicationStage = ApplicationStage.MainPage;
        }

        [RelayCommand]
        private void SwitchToParameterSetting()
        {
            AppModel.CurrentApplicationStage = ApplicationStage.ParameterSetting;
        }

        [RelayCommand]
        private void SwitchToDataManagement()
        {
            AppModel.CurrentApplicationStage = ApplicationStage.DataManagement;
        }

        [RelayCommand]
        private void SwitchToUserManagement()
        {
            AppModel.CurrentApplicationStage = ApplicationStage.UserManagement;
        }

        private async Task InitializeHardwares()
        {
            try
            {
                foreach (var heatingBoxConfig in appConfig_.HeatingBoxConfigs)
                {
                    var heatingBox = new HeatingBoxControl();
                    await heatingBox.Initialize(heatingBoxConfig);
                    heatingBox.TemperatureDataReceived += OnTemperatureDataReceived;
                    heatingBoxControls_.Add(heatingBox);
                    StartGetTemperatureTimer();
                }
            }
            catch (Exception ex)
            {
                UserCommunication.ShowMessage($"{Theme.GetString(Strings.Error)}", $"Message:{ex.Message}\nStackTrace:{ex.StackTrace}", MessageType.Critical);
            }
        }

        private async Task UnInitializeHardwares()
        {
            try
            {
                foreach (var heatingBox in heatingBoxControls_)
                {
                    heatingBox.TemperatureDataReceived -= OnTemperatureDataReceived;
                    StopGetTemperatureTimer();
                    await heatingBox.Uninitialize();
                }
            }
            catch (Exception ex)
            {
                UserCommunication.ShowMessage($"{Theme.GetString(Strings.Error)}", $"Message:{ex.Message}\nStackTrace:{ex.StackTrace}", MessageType.Critical);
            }
        }

        public MainWindowViewModel(ApplicationConfiguration appConfig, IDataRepository dataRepository)
        {
            appConfig_ = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
            dataRepository_ = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));

            viewModelMap_.Add(ApplicationStage.MainPage, new MainPageViewModel());
            viewModelMap_.Add(ApplicationStage.ParameterSetting, new ParameterSettingViewModel());
            viewModelMap_.Add(ApplicationStage.DataManagement, new DataManagementViewModel());
            viewModelMap_.Add(ApplicationStage.UserManagement, new UserManagementViewModel(dataRepository_));

            AppModel.ApplicationStages.Add(viewModelMap_[ApplicationStage.MainPage]);
            AppModel.ApplicationStages.Add(viewModelMap_[ApplicationStage.ParameterSetting]);
            AppModel.ApplicationStages.Add(viewModelMap_[ApplicationStage.DataManagement]);
            AppModel.ApplicationStages.Add(viewModelMap_[ApplicationStage.UserManagement]);

            dataRepository_.GetUsers().ToList()
                .ForEach(x => AppModel.Users.Add(x));

            SelectedUser = AppModel.Users.FirstOrDefault();

            AppModel.PropertyChanged += OnAppModelPropertyChanged;

            logger_.Info($"{nameof(AppModel.CurrentData.CharcoalColumnTemperature)}");
        }

        private void OnAppModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(AppModel.CurrentApplicationStage):
                    IsPageSwitchPlaying = true;
                    logger_.Info($"CurrentViewModel updated to stage {AppModel.CurrentApplicationStage}.");
                    CurrentViewModel = viewModelMap_[AppModel.CurrentApplicationStage];

                    var curStageIdx = AppModel.ApplicationStages.IndexOf(CurrentViewModel);
                    if (curStageIdx != -1)
                    {
                        foreach (var stage in AppModel.ApplicationStages)
                        {
                            stage.IsCurrent = stage == CurrentViewModel;
                            var stageIdx = AppModel.ApplicationStages.IndexOf(stage);
                            stage.IsEnabled = stageIdx <= curStageIdx;
                        }
                    }
                    break;
                case nameof(AppModel.CurrentUser):
                    logger_.Warn($"Current User {AppModel.CurrentUser}.");
                    break;
            }
        }

        public async Task CleanUpAsync()
        {
            if (isCleaningUp) return;

            isCleaningUp = true;
            logger_.Info("Cleaning up...");
            await UnInitializeHardwares();
            dataRepository_.Dispose();
        }

        public void StartGetTemperatureTimer()
        {
            if (heatingBoxControls_.Count == 0 || !heatingBoxControls_.Any(x => x.IsInitialized))
                return;

            temperatureReadingTimer_ = new(66) { Enabled = true };
            temperatureReadingTimer_.Elapsed += OnTemperatureReadingTimerElapsed;
            logger_.Info($"{nameof(temperatureReadingTimer_)} started.");
        }

        public void StopGetTemperatureTimer()
        {
            if (temperatureReadingTimer_ != null)
            {
                temperatureReadingTimer_.Elapsed -= OnTemperatureReadingTimerElapsed;
                temperatureReadingTimer_.Stop();
                temperatureReadingTimer_.Dispose();
                temperatureReadingTimer_ = null;
                logger_.Info($"{nameof(temperatureReadingTimer_)} stopped.");
            }
        }

        private async void OnTemperatureReadingTimerElapsed(object state, System.Timers.ElapsedEventArgs e)
        {
            foreach (var item in heatingBoxControls_)
            {
                await item.WriteCommand(CommandTypes.ReadTemperature);
            }
        }

        public void OnTemperatureDataReceived(object? sender, int e)
        {
            var heatingBox = sender as HeatingBoxControl;

            if(heatingBox.Id == nameof(AppModel.CurrentData.CharcoalColumnTemperature))
                AppModel.CurrentData.CharcoalColumnTemperature = e;
            else
                AppModel.CurrentData.Column4A5ATemperature = e;
        }

        private static readonly Logger logger_ = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<ApplicationStage, ApplicationStageViewModel> viewModelMap_ = new();
        private readonly ApplicationConfiguration appConfig_;
        private readonly IDataRepository dataRepository_;
        private readonly List<HeatingBoxControl> heatingBoxControls_ = new();

        private System.Timers.Timer temperatureReadingTimer_;
        private bool isCleaningUp;
    }
}
