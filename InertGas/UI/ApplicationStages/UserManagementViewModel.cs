using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InertGas.Application.Model;
using InertGas.Application.Themes;
using InertGas.Application.UI.Dialog;
using InertGas.Application.Utility;
using InertGas.Common.DataAccess;
using InertGas.Common.Model;
using NLog;

namespace InertGas.Application.UI.ApplicationStages
{
    internal partial class UserManagementViewModel : ApplicationStageViewModel
    {
        [ObservableProperty]
        private User selectedUser;

        [ObservableProperty]
        private string userNameOnSearch;

        [ObservableProperty]
        private AddUserDialog addUserWindow;

        [RelayCommand]
        private void AddUser()
        {
            logger_.Info($"{nameof(AddUserWindow)} showing.");

            if (AddUserWindow != null)
            {
                AddUserWindow.Show();
                return;
            }

            addUserViewModel_ ??= new AddUserViewModel(dataRepository_);
            AddUserWindow ??= new AddUserDialog()
            {
                DataContext = addUserViewModel_,
                Owner = UIUtils.GetActiveWindow()
            };

            addUserViewModel_.DialogCloseRequested += (s, e) =>
            {
                logger_.Info($"{nameof(AddUserWindow)} closing.");
                AddUserWindow.Close();
            };

            AddUserWindow.ShowDialog();
        }

        [RelayCommand]
        private void DeleteUser()
        {
            try
            {
                if (SelectedUser == null)
                    throw new ArgumentNullException(nameof(SelectedUser));

                if (SelectedUser.Id == AppModel.CurrentUser.Id)
                    throw new InvalidOperationException($"Unable to delete the current user");

                dataRepository_.DeleteUser(SelectedUser);
                AppModel.Users.Remove(SelectedUser);
                SelectedUser = AppModel.Users.FirstOrDefault();
                logger_.Info($"User ID:{SelectedUser.Id} deleted");
            }
            catch (Exception ex)
            {
                UserCommunication.ShowMessage($"{Theme.GetString(Strings.Error)}", $"Message:{ex.Message}\nStackTrace:{ex.StackTrace}", MessageType.Critical);
            }
        }

        [RelayCommand]
        private void SearchUser()
        {
            try
            {
                if (UserNameOnSearch == null || UserNameOnSearch == string.Empty)
                    throw new ArgumentNullException(nameof(UserNameOnSearch));

                var users = dataRepository_.SearchPatientByName(UserNameOnSearch).ToList();

                if (users.Count == 0)
                    throw new InvalidOperationException($"No such user");

                SelectedUser = users.First();
            }
            catch (Exception ex)
            {
                UserCommunication.ShowMessage($"{Theme.GetString(Strings.Error)}", $"Message:{ex.Message}\nStackTrace:{ex.StackTrace}", MessageType.Critical);
            }
            finally
            {
                UserNameOnSearch = null;
            }
        }

        public UserManagementViewModel(IDataRepository dataRepository) : base(ApplicationStage.UserManagement)
        {
            dataRepository_ = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
            Title = Theme.GetString(Strings.UserManagement);
        }

        private static readonly Logger logger_ = LogManager.GetCurrentClassLogger();
        private readonly IDataRepository dataRepository_;

        private AddUserViewModel addUserViewModel_;
    }
}
