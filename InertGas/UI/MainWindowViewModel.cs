using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using InertGas.Application.Model;
using InertGas.Application.Themes;
using InertGas.Application.UI.ApplicationStages;
using InertGas.Application.UI.Dialog;
using InertGas.Application.Utility;
using InertGas.Common.DataAccess;
using InertGas.Common.Model;
using NLog;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Security;
using System.Windows.Media.Media3D;

namespace InertGas.Application.UI
{
    internal partial class MainWindowViewModel : ObservableObject
    {
        public static ApplicationModel AppModel => ApplicationModel.Instance;

        public ObservableCollection<User> Users { get; } = new();

        [ObservableProperty]
        private ApplicationStageViewModel currentViewModel;

        [ObservableProperty]
        private User selectedUser;

        [ObservableProperty]
        private bool isUserLoggedIn;

        [ObservableProperty]
        private string userName;

        [ObservableProperty]
        private SecureString securePassword;

        [ObservableProperty]
        private bool isPageSwitchPlaying;

        [RelayCommand]
        private void LogIn()
        {
            if (SelectedUser == null) return;

            if (!SecureUtils.ValidateUserPassword(SelectedUser.Password, SecurePassword, SelectedUser.Salt))
            {
                UserCommunication.ShowMessage(Theme.GetString(Strings.IncorrectPassword), Theme.GetString(Strings.DoubleCheckUserPasswordMessage), MessageType.Info);
                return;
            }

            IsUserLoggedIn = true;
            SecurePassword = null;

            AppModel.CurrentApplicationStage = ApplicationStage.MainPage;
            CurrentViewModel = viewModelMap_[AppModel.CurrentApplicationStage];
            IsPageSwitchPlaying = true;
        }

        [RelayCommand]
        private void Close(object obj)
        {
            CleanUp();
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

        public MainWindowViewModel(IDataRepository dataRepository)
        {
            dataRepository_ = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));

            viewModelMap_.Add(ApplicationStage.MainPage, new MainPageViewModel());
            viewModelMap_.Add(ApplicationStage.ParameterSetting, new ParameterSettingViewModel());
            viewModelMap_.Add(ApplicationStage.DataManagement, new DataManagementViewModel());
            viewModelMap_.Add(ApplicationStage.UserManagement, new UserManagementViewModel());

            AppModel.ApplicationStages.Add(viewModelMap_[ApplicationStage.MainPage]);
            AppModel.ApplicationStages.Add(viewModelMap_[ApplicationStage.ParameterSetting]);
            AppModel.ApplicationStages.Add(viewModelMap_[ApplicationStage.DataManagement]);
            AppModel.ApplicationStages.Add(viewModelMap_[ApplicationStage.UserManagement]);

            dataRepository_.GetUsers().ToList()
                .ForEach(x => Users.Add(x));

            SelectedUser = Users.FirstOrDefault();

            AppModel.PropertyChanged += OnAppModelPropertyChanged;
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
            }
        }

        public void CleanUp()
        {
            if (isCleaningUp) return;

            isCleaningUp = true;
            logger_.Info("Cleaning up...");
            dataRepository_.Dispose();
        }

        private static readonly Logger logger_ = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<ApplicationStage, ApplicationStageViewModel> viewModelMap_ = new();
        private readonly IDataRepository dataRepository_;

        private bool isCleaningUp;
    }
}
