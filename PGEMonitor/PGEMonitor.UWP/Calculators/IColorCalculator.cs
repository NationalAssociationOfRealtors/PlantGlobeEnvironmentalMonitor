using Windows.UI;

namespace PGEMonitor.UWP.Calculators
{
    /// <summary>
    /// Calculates colors based on input parameters.
    /// </summary>
    public interface IColorCalculator
    {
        /// <summary>
        /// Calculates color in the linear gradient between start color and end color.
        /// </summary>
        /// <param name="startColor">Starting color of gradient.</param>
        /// <param name="endColor">Ending color of gradient.</param>
        /// <param name="percentage">Offset from start color in percentage.</param>
        /// <returns>Gradient point color.</returns>
        Color GetLinearGradientColor(Color startColor, Color endColor, double percentage);

        /// <summary>
        /// Calculates color that matches current value of dew point temperature.
        /// </summary>
        /// <param name="dewPoint">Value of dew point temperature.</param>
        /// <returns>Matching color.</returns>
        Color CalculateDewPointColor(double dewPoint);
    }
}
