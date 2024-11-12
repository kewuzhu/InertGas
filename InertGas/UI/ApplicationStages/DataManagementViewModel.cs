using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InertGas.Application.Model;
using InertGas.Application.Themes;
using InertGas.Application.UI.Dialog;
using InertGas.Application.Utility;
using InertGas.Common.DataAccess;
using InertGas.Common.Model;
using NLog;
using System.IO;

namespace InertGas.Application.UI.ApplicationStages
{
    partial class DataManagementViewModel : ApplicationStageViewModel
    {
        [ObservableProperty]
        private DateTime selectedStartDate = DateTime.Now;

        [ObservableProperty]
        private DateTime selectedEndDate = DateTime.Now;

        [ObservableProperty]
        private CollectedData selectedData;

        [RelayCommand]
        private void RefreshData()
        {
            try
            {
                AppModel.CollectedDataSet.Clear();
                AppModel.CollectedDataSet.AddRange(dataRepository_.GetData());
            }
            catch (Exception ex)
            {
                UserCommunication.ShowMessage($"{Theme.GetString(Strings.Error)}", $"Message:{ex.Message}\nStackTrace:{ex.StackTrace}", MessageType.Critical);
            }
        }

        [RelayCommand]
        private void DeleteData()
        {
            try
            {
                if (SelectedData == null)
                    throw new ArgumentNullException(nameof(SelectedData));

                dataRepository_.DeleteData(SelectedData);
                logger_.Info($"Data ID:{SelectedData.Id} deleted");
                AppModel.CollectedDataSet.Remove(SelectedData);
            
                SelectedData = AppModel.CollectedDataSet.FirstOrDefault();
            }
            catch (Exception ex)
            {
                UserCommunication.ShowMessage($"{Theme.GetString(Strings.Error)}", $"Message:{ex.Message}\nStackTrace:{ex.StackTrace}", MessageType.Critical);
            }
        }

        [RelayCommand]
        private void SearchData()
        {
            try
            {
                var dataSet = dataRepository_.SearchDataByDate(SelectedStartDate, SelectedEndDate).ToList();

                if (dataSet.Count == 0)
                    throw new InvalidOperationException($"No such data");

                SelectedData = dataSet.First();
            }
            catch (Exception ex)
            {
                UserCommunication.ShowMessage($"{Theme.GetString(Strings.Error)}", $"Message:{ex.Message}\nStackTrace:{ex.StackTrace}", MessageType.Critical);
            }
        }

        [RelayCommand]
        private static void ExportData()
        {
            var csv = "Id,CreatedDate,VolumeFlowA,VolumeFlowB,TotalFlowB,CharcoalColumnTemperature,Column4A5ATemperature,Pressure\n";

            AppModel.CollectedDataSet.ToList().ForEach(x =>
            {
                csv += $"{x.Id},{x.CreatedDate},{x.VolumeFlowA},{x.VolumeFlowB},{x.TotalFlowB},{x.CharcoalColumnTemperature},{x.Column4A5ATemperature},{x.Pressure}\n";
            });

            CreateWorkingDirectoryIfNotExists();
            var filename = GetRecordFilename();
            File.WriteAllText(filename, csv);
            logger_.Info($"Results saved to {filename}.");
        }

        public DataManagementViewModel(IDataRepository dataRepository) : base(ApplicationStage.DataManagement)
        {
            dataRepository_ = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
            Title = Theme.GetString(Strings.DataManagement);
        }

        private static void CreateWorkingDirectoryIfNotExists()
        {
            if (!Directory.Exists(workingDirectory_))
                Directory.CreateDirectory(workingDirectory_);
        }

        private static string GetRecordFilename() =>
            Path.Combine(workingDirectory_,
            $"InertGas_data_{DateTime.Now:yyyy-MM-dd-HHmmss}.csv");

        private static readonly Logger logger_ = LogManager.GetCurrentClassLogger();
        private static readonly string workingDirectory_ = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), ApplicationConstants.DEFAULT_WORKING_DIR_NAME);
        private readonly IDataRepository dataRepository_;
    }
}
