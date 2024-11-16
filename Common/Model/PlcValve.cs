using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace InertGas.Common.Model
{
    public partial class PlcValve : ObservableObject
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [ObservableProperty]
        private PlcControlTypes controlType;

        [ObservableProperty]
        private int number;

        [ObservableProperty]
        private int address;
    }
}
