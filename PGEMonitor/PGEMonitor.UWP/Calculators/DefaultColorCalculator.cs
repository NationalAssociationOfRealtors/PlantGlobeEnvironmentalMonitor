using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace PGEMonitor.UWP.Calculators
{
    /// <summary>
    /// Default implementation of color calculator.
    /// </summary>
    public class DefaultColorCalculator : IColorCalculator
    {
        /// <summary>
        /// Color that matches dry level of humidity.
        /// </summary>
        Color _dryColor = Color.FromArgb(255, 226, 95, 38);

        /// <summary>
        /// Color that matches normal level of humidity.
        /// </summary>
        Color _normalColor = Color.FromArgb(255, 63, 173, 20);

        /// <summary>
        /// Color that matches moist level of humidity.
        /// </summary>
        Color _moistColor = Color.FromArgb(255, 6, 143, 216);

        /// <summary>
        /// Maximum value of dew point temperature that is considered as dry.
        /// </summary>
        const int DRY_MAX = 0;

        /// <summary>
        /// Minimum value of dew point temperature that is considered as normal.
        /// </summary>
        const int NORMAL_MIN = 10;

        /// <summary>
        /// Maximum value of dew point temperature that is considered as normal.
        /// </summary>
        const int NORMAL_MAX = 16;

        /// <summary>
        /// Minimum value of dew point temperature that is considered as moist.
        /// </summary>
        const int MOIST_MIN = 24;

        /// <summary>
        /// Calculates color in the linear gradient between start color and end color.
        /// </summary>
        /// <param name="startColor">Starting color of gradient.</param>
        /// <param name="endColor">Ending color of gradient.</param>
        /// <param name="percentage">Offset from start color in percentage.</param>
        /// <returns>Gradient point color.</returns>
        public Color GetLinearGradientColor(Color startColor, Color endColor, double percentage)
        {
            byte red = Convert.ToByte(startColor.R + (endColor.R - startColor.R) * percentage / 100);
            byte green = Convert.ToByte(startColor.G + (endColor.G - startColor.G) * percentage / 100);
            byte blue = Convert.ToByte(startColor.B + (endColor.B - startColor.B) * percentage / 100);
            byte alpha = Convert.ToByte(startColor.A + (endColor.A - startColor.A) * percentage / 100);

            return Color.FromArgb(alpha, red, green, blue);
        }

        /// <summary>
        /// Calculates color that matches current value of dew point temperature.
        /// </summary>
        /// <param name="dewPoint">Value of dew point temperature.</param>
        /// <returns>Matching color.</returns>
        public Color CalculateDewPointColor(double dewPoint)
        {
            if (dewPoint <= DRY_MAX) // Very DRY
            {
                return _dryColor;
            }
            else if (dewPoint > DRY_MAX && dewPoint < NORMAL_MIN) // Moving from DRY to NORMAL
            {
                double totalDistance = NORMAL_MIN - DRY_MAX;
                double distance = dewPoint - DRY_MAX;
                double percentage = (distance / totalDistance) * 100;
                return GetLinearGradientColor(_dryColor, _normalColor, percentage);
            }
            else if (dewPoint >= NORMAL_MIN && dewPoint <= NORMAL_MAX) // NORMAL
            {
                return _normalColor;
            }
            else if (dewPoint > NORMAL_MAX && dewPoint < MOIST_MIN) // Moving from NORMAL to MOIST
            {
                double totalDistance = MOIST_MIN - NORMAL_MAX;
                double distance = dewPoint - NORMAL_MAX;
                double percentage = (distance / totalDistance) * 100;
                return GetLinearGradientColor(_normalColor, _moistColor, percentage);
            }
            else if (dewPoint >= MOIST_MIN) // Wet T-Shirt competition
            {
                return _moistColor;
            }
            else // You messed up my code, didn't you? Feeling guilty now?
            {
                throw new Exception("How did we get here?");
            }
        }
    }
}
