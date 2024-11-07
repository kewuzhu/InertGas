using InertGas.Application.Model;
using InertGas.Application.Themes;
using InertGas.Application.UI;
using InertGas.Application.UI.Dialog;
using InertGas.Application.Utility;
using InertGas.Common.DataAccess;
using InertGas.Common.Model;
using InertGas.Common.Utility;
using InertGas.DataBase;
using InertGas.HeatingBox;
using NLog;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;

namespace InertGas.Application
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private static readonly string APP_CONFIG_FILE_PATH = ".//res//appconfig.json";
        private static readonly string LOG_DIRECTORY = "C://InertGas//SessionLogs";
        private static readonly string APP_LOG_FILE_NAME = "application.log";
        private static readonly string ROOT_DIRECTORY = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                InitializeLogging();

                var appConfig = JsonSerializer.Deserialize<ApplicationConfiguration>(File.ReadAllText(APP_CONFIG_FILE_PATH));
                appConfig.WorkingDirectory = Path.Combine(ROOT_DIRECTORY, appConfig.WorkingDirectory);

                LogUtils.InitializeExtendedLogging(appConfig.FileLoggerLogLevel, appLogTargetName_, appConfig.ConsoleLoggerLogLevel);

                Theme.AddStringsDictionary(appConfig.Language);

                var repoInitTask = InitializeDataRepository(appConfig.DataRepoConfig);

                await Task.WhenAll(repoInitTask);

                InitializeDefaultUser();

                mainWindowViewModel_ = new MainWindowViewModel(appConfig, dataRepository_);
                MainWindow = new MainWindow { DataContext = mainWindowViewModel_ };

                MainWindow.Show();
            }
            catch (Exception ex)
            {
                UserCommunication.ShowMessage($"{Theme.GetString(Strings.Error)}", $"Message:{ex.Message}\nStackTrace:{ex.StackTrace}", MessageType.Critical);
            }
            
        }

        private void InitializeLogging()
        {
            var appAssemblyName = typeof(App).Assembly.GetName();
            var appVersion = appAssemblyName.Version;

            logDirectory_ = Path.Combine(
                LOG_DIRECTORY,
                $"{appAssemblyName.Name}-{appVersion.Major}.{appVersion.Minor}.{appVersion.Build}",
                $"{DateTime.Now:yyMMddHHmm}");

            appLogTargetName_ = LogUtils.InitializeLogging(logDirectory_, APP_LOG_FILE_NAME);

            logger_.Info($"{appAssemblyName.Name} {appVersion} is starting...");
        }

        private async Task InitializeDataRepository(DataRepositoryConfiguration config)
        {
            logger_.Info("Initializing database...");
            var repo = new RavenDBRepository();
            await repo.Initialize(config);
            dataRepository_ = repo;
            logger_.Info("Database initialized successfully.");
        }

        private void InitializeDefaultUser()
        {
            var users = dataRepository_.GetUsers();
            if (users.Any()) return;

            var salt = new byte[8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var securePassword = new NetworkCredential("", "123").SecurePassword;
            var defaultUser = new User()
            {
                Role = UserRole.Administrator,
                Name = "Default",
                Password = SecureUtils.HashPassword(securePassword, salt),
                CreatedDate = DateTime.Now,
                Salt = salt
            };

            dataRepository_.UpsertUser(defaultUser);
        }

        private async void OnApplicationExit(object sender, ExitEventArgs e) => await TerminateApplication();

        private async Task TerminateApplication(int exitCode = (int)ApplicationExitCode.Success)
        {
            if (shuttingDown_) return;

            try
            {
                shuttingDown_ = true;
                logger_.Info("Shutting down...");

                mainWindowViewModel_.CleanUpAsync();
                Current.Shutdown(exitCode);
            }
            catch (Exception e)
            {
                logger_.Info(e, "Secondary exception in TerminateApplication.");
                KillCurrentProcess();
            }
        }

        private static void KillCurrentProcess()
        {
            logger_.Info("Killing current process...");
            Process.GetCurrentProcess().Kill();
        }

        private static readonly ApplicationModel appModel_ = ApplicationModel.Instance;
        private static readonly Logger logger_ = LogManager.GetCurrentClassLogger();

        private IDataRepository dataRepository_;
        private string logDirectory_;
        private string appLogTargetName_;
        private MainWindowViewModel mainWindowViewModel_;
        private bool shuttingDown_;
        private HeatingBoxControl heatingBox_;
    }
}