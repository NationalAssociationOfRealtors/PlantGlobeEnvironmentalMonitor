namespace PGEMonitor.UWP.Calculators
{
    /// <summary>
    /// Defines methods for dew point calculation.
    /// </summary>
    public interface IDewPointCalculator
    {
        /// <summary>
        /// Calculates dew point temperature based on the room humidity and temperature levels.
        /// </summary>
        /// <param name="humidity">Humidity in range from 0.00 to 100.00.</param>
        /// <param name="temperature">Temperature in degrees of Celsius.</param>
        /// <returns>Dew point temperature in degrees of Celsius.</returns>
        double Calculate(double humidity, double temperature);
    }
}
