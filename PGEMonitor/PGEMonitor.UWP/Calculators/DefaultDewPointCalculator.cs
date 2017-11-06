using System;

namespace PGEMonitor.UWP.Calculators
{
    /// <summary>
    /// Default implementation of dew point calculation as dictated by his holiness mr. Akram of the CRT Labs.
    /// </summary>
    public class DefaultDewPointCalculator : IDewPointCalculator
    {
        /// <summary>
        /// Calculates dew point temperature based on the room humidity and temperature levels.
        /// </summary>
        /// <param name="humidity">Humidity in range from 0.00 to 100.00.</param>
        /// <param name="temperature">Temperature in degrees of Celsius.</param>
        /// <returns>Dew point temperature in degrees of Celsius.</returns>
        public double Calculate(double humidity, double temperature)
        {
            double m = temperature > 0 ?
                17.62 : 22.46;

            double tn = temperature > 0 ?
                243.12 : 272.62;

            double ln = Math.Log(humidity / 100);

            double fract = (m * temperature) / (tn + temperature);

            return tn * (ln + fract) / (m - ln - fract);
        }
    }
}
