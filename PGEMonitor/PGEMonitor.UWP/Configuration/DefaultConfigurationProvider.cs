namespace PGEMonitor.UWP.Configuration
{
    public class DefaultConfigurationProvider : IConfigurationProvider
    {
        public string LifxHttpApiOAuth2Token => "c215acc941583cebded6698641512c0703e0f93a15792efc7a0d0bd9035a5cfb";
        public int? DefaultSensorBoardId => 7;
        public string DefaultLifxDeviceLabel => "Bench Bulb";
    }
}
