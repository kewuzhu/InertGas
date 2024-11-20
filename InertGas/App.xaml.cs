using InertGas.Application.Model;
using InertGas.Application.Themes;
using InertGas.Application.UI;
using InertGas.Application.UI.Dialog;
using InertGas.Application.Utility;
using InertGas.Common.DataAccess;
using InertGas.Common.Model;
using InertGas.Common.Utility;
using InertGas.DataBase;
using InertGas.FlowMeter;
using InertGas.HeatingBox;
using InertGas.Plc;
using NLog;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using static InertGas.Application.ApplicationConstants;

namespace InertGas.Application
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private static readonly string LOG_DIRECTORY = "C://InertGas//SessionLogs";
        private static readonly string APP_LOG_FILE_NAME = "application.log";
        private static readonly string ROOT_DIRECTORY = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                InitializeLogging();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                };

                appConfig_ = JsonSerializer.Deserialize<ApplicationConfiguration>(File.ReadAllText(Path.Combine(CONFIG_DIRECTORY, APP_CONFIG_FILE_NAME)), options);
                appConfig_.WorkingDirectory = Path.Combine(ROOT_DIRECTORY, appConfig_.WorkingDirectory);

                LogUtils.InitializeExtendedLogging(appConfig_.FileLoggerLogLevel, appLogTargetName_, appConfig_.ConsoleLoggerLogLevel);

                Theme.AddStringsDictionary(appConfig_.Language);

                var repoInitTask = InitializeDataRepository(appConfig_.DataRepoConfig);

                await Task.WhenAll(repoInitTask);

                InitializeDefaultUser();

                appModel_.CollectedDataSet.AddRange(dataRepository_.GetData());

                var splashScreen = new UI.SplashScreen();
                splashScreen.Show();

                await InitializeHardwares();

                await Task.Delay(1800);
                splashScreen.Hide();

                mainWindowViewModel_ = new MainWindowViewModel(appConfig_, dataRepository_);
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
            var adminUser = new User()
            {
                Role = UserRole.Administrator,
                Name = "Admin",
                Password = SecureUtils.HashPassword(securePassword, salt),
                CreatedDate = DateTime.Now,
                Salt = salt
            };
            dataRepository_.UpsertUser(adminUser);

            salt = new byte[8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var defaultUser = new User()
            {
                Role = UserRole.Normal,
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
                await UnInitializeHardwares();
                mainWindowViewModel_.CleanUp();
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

        private async Task InitializeHardwares()
        {
            try
            {
                var HardwareControl = new HardwareControl();

                for (int i = 0; i < appConfig_.SystemHardwareConfigs.FlowMeterConfigs.Count; i++)
                {
                    var flowMeterConfig = appConfig_.SystemHardwareConfigs.FlowMeterConfigs[i];
                    logger_.Info($"Initialize Flow Meter {i + 1}, Name:{flowMeterConfig.Id}, ComPort:{flowMeterConfig.SerialConfiguration.SerialPort}");
                    var flowMeter = new FlowMeterControl();
                    //await flowMeter.Initialize(flowMeterConfig);
                    flowMeter.VolumeFlowReceived += OnFlowMeterDataReceived;
                    HardwareControl = new HardwareControl() { HardwareType = HardwareTypes.FlowMeter, Number = i + 1, Hardware = flowMeter, IsEnabled = true, IsOn = false };
                    appModel_.HardwareControls.Add(HardwareControl);
                    flowMeter.StartGetFlowDataTimer();
                }

                for (int i = 0; i < appConfig_.SystemHardwareConfigs.HeatingBoxConfigs.Count; i++)
                {
                    var heatingBoxConfig = appConfig_.SystemHardwareConfigs.HeatingBoxConfigs[i];
                    logger_.Info($"Initialize heating box {i + 1}, Name:{heatingBoxConfig.Id}, ComPort:{heatingBoxConfig.SerialConfiguration.SerialPort}");
                    var heatingBox = new HeatingBoxControl();
                    //await heatingBox.Initialize(heatingBoxConfig);
                    heatingBox.TemperatureDataReceived += OnTemperatureDataReceived;
                    HardwareControl = new HardwareControl() { HardwareType = HardwareTypes.HeatingBox, Number = i + 1, Hardware = heatingBox, IsEnabled = false, IsOn = false };
                    appModel_.HardwareControls.Add(HardwareControl);
                    heatingBox.StartGetTemperatureTimer();
                }

                var plcConfig = appConfig_.SystemHardwareConfigs.PlcConfig;
                logger_.Info($"Initialize PLC, IpAddress:{plcConfig.IpAddress}, Port:{plcConfig.Port}");
                var plcControl = new PlcControl();
                //plcControl.Initialize(plcConfig);
                plcControl.PressureDataReceived += OnPressureDataReceived;
                foreach (var plcValve in appConfig_.SystemHardwareConfigs.PlcConfig.PlcValves)
                {
                    HardwareControl = new HardwareControl() { HardwareType = HardwareTypes.Plc, Number = 1, Hardware = plcControl, IsEnabled = false, IsOn = false, PlcValve = plcValve };
                    appModel_.HardwareControls.Add(HardwareControl);
                    logger_.Info($"Plc valve added: ControlType:{plcValve.ControlType}, Number:{plcValve.Number}, Address:{plcValve.Address}");
                }
                plcControl.StartGetPressureTimer();
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
                var flowMeters = appModel_.HardwareControls
                    .Where(x => x.HardwareType == HardwareTypes.FlowMeter)
                    .Select(x => x.Hardware as FlowMeterControl)
                    .ToList();

                foreach (var flowMeter in flowMeters)
                {
                    flowMeter.VolumeFlowReceived -= OnFlowMeterDataReceived;
                    flowMeter.StopGetFlowDataTimer();
                    await flowMeter.Uninitialize();
                    logger_.Info($"Flowmeter Id:{flowMeter.Id} uninitialized.");
                }

                var heatingBoxes = appModel_.HardwareControls
                    .Where(x => x.HardwareType == HardwareTypes.HeatingBox)
                    .Select(x => x.Hardware as HeatingBoxControl)
                    .ToList();

                foreach (var heatingBox in heatingBoxes)
                {
                    heatingBox.TemperatureDataReceived -= OnTemperatureDataReceived;
                    heatingBox.StopGetTemperatureTimer();
                    await heatingBox.Uninitialize();
                    logger_.Info($"HeatingBox Id:{heatingBox.Id} uninitialized.");
                }

                var plcControl = appModel_.HardwareControls
                    .Where(x => x.HardwareType == HardwareTypes.Plc)
                    .Select(x => x.Hardware as PlcControl)
                    .ToList()
                    .First();

                plcControl.PressureDataReceived -= OnPressureDataReceived;
                plcControl.StopGetPressureTimer();
                plcControl.Uninitialize();
                logger_.Info($"Plc uninitialized.");
            }
            catch (Exception ex)
            {
                UserCommunication.ShowMessage($"{Theme.GetString(Strings.Error)}", $"Message:{ex.Message}\nStackTrace:{ex.StackTrace}", MessageType.Critical);
            }
        }

        private void OnTemperatureDataReceived(object? sender, int e)
        {
            var heatingBox = sender as HeatingBoxControl;

            logger_.Info($"Heating box id:{heatingBox.Id} temperatrue:{e}");

            if (heatingBox.Id == nameof(appModel_.CurrentData.CharcoalColumnTemperature))
                appModel_.CurrentData.CharcoalColumnTemperature = e;
            else
                appModel_.CurrentData.Column4A5ATemperature = e;
        }

        private void OnFlowMeterDataReceived(object? sender, List<string> e)
        {
            var flowMeter = sender as FlowMeterControl;

            logger_.Info($"Flowmeter id:{flowMeter.Id} flow data:{string.Join(",", e)}");

            if (flowMeter.Id == nameof(appModel_.CurrentData.VolumeFlowA))
            {
                appModel_.CurrentData.PressureA = e.First();
                appModel_.CurrentData.VolumeFlowA = e.Skip(1).First();
            }
            else
            {
                appModel_.CurrentData.PressureB = e.First();
                appModel_.CurrentData.VolumeFlowB = e.Skip(1).First();
                appModel_.CurrentData.TotalFlowB = e.Last();
            }
        }

        private void OnPressureDataReceived(object? sender, string e)
        {
            var plcControl = sender as PlcControl;

            logger_.Info($"Pressure:{e}");
        }

        private static readonly ApplicationModel appModel_ = ApplicationModel.Instance;
        private static readonly Logger logger_ = LogManager.GetCurrentClassLogger();

        private ApplicationConfiguration appConfig_;
        private IDataRepository dataRepository_;
        private string logDirectory_;
        private string appLogTargetName_;
        private MainWindowViewModel mainWindowViewModel_;
        private bool shuttingDown_;
    }
}