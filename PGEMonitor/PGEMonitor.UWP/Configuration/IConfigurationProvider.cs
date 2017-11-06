namespace PGEMonitor.UWP.Configuration
{
    public interface IConfigurationProvider
    {
        int? DefaultSensorBoardId { get; }

        string LifxHttpApiOAuth2Token { get; }

        string DefaultLifxDeviceLabel { get; }
    }
}
