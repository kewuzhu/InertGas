using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InertGas.Application.Model;
using InertGas.Application.Themes;
using InertGas.Application.Utility;
using InertGas.Common.DataAccess;
using InertGas.Common.Model;
using InertGas.Common.UI;
using NLog;
using System.Net;
using System.Security.Cryptography;

namespace InertGas.Application.UI.Dialog
{
    internal partial class AddUserViewModel : DialogViewModelBase
    {
        public static ApplicationModel AppModel => ApplicationModel.Instance;

        public event EventHandler DialogCloseRequested;

        [ObservableProperty]
        private UserRole selectedUserRole;

        [ObservableProperty]
        private string userName;

        [ObservableProperty]
        private string password;

        [RelayCommand]
        private void Close()
        {
            DialogResult = false;
            DialogCloseRequested?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void Add()
        {
            try
            {
                if (UserName == String.Empty || UserName == null || Password == String.Empty || Password == null)
                    throw new ArgumentNullException($"{nameof(UserName)} or {nameof(Password)}");

                var salt = new byte[8];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }

                var securePassword = new NetworkCredential("", Password).SecurePassword;
                var user = new User()
                {
                    Role = SelectedUserRole,
                    Name = UserName,
                    Password = SecureUtils.HashPassword(securePassword, salt),
                    CreatedDate = DateTime.Now,
                    Salt = salt
                };

                dataRepository_.UpsertUser(user);
                AppModel.Users.Add(user);
                logger_.Info($"User ID:{user.Id} added");

                UserName = null;
                Password = null;
            }
            catch (Exception ex) 
            {
                UserCommunication.ShowMessage($"{Theme.GetString(Strings.Error)}", $"Message:{ex.Message}\nStackTrace:{ex.StackTrace}", MessageType.Critical);
            }
        }

        public AddUserViewModel(IDataRepository dataRepository)
        {
            dataRepository_ = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
        }

        private static readonly Logger logger_ = LogManager.GetCurrentClassLogger();
        private readonly IDataRepository dataRepository_;
    }
}
