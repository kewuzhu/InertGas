namespace InertGas.Common.Model
{
    public interface ISystemHardware
    {
        string Id { get; }

        bool IsInitialized { get; }
    }
}
