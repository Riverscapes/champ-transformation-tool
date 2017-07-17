//CHaMP Transformation Tool
//Copyright (C) 2012  Joseph Wheaton

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;

namespace RSGIS.Utilites
{

    /// <summary>
    /// Class with static methods to do simple but common trigonometric stuff.
    /// </summary>
    class TrigUtilities
    {

        /// <summary>
        /// Convert degress to radians.
        /// </summary>
        /// <param name="degrees">degree value to convert</param>
        /// <returns>radians value</returns>
        public static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        /// <summary>
        /// Convert radians to degrees.
        /// </summary>
        /// <param name="radians">radians value to convert</param>
        /// <returns>degrees value</returns>
        public static double RadiansToDegrees(double radians)
        {
            return radians * 180 / Math.PI;
        }

        /// <summary>
        /// Convert an angle from the trig value (east = 0, counter-clockwise) to the geo value
        /// (north = 0, clockwise).
        /// </summary>
        /// <param name="degrees">the trig value in degrees to convert</param>
        /// <returns>the geo value in degrees</returns>
        public static double TrigToGeo(double degrees)
        {
            double deg = 450 - degrees;
            while (deg > 360)
                deg -= 360;
            return deg;
        }

        /// <summary>
        /// Converts an angle from radians in the trig world to degrees in the geo world.
        /// </summary>
        /// <param name="radians">radians value to convert</param>
        /// <returns>degrees value with north = 0</returns>
        public static double TrigRadiansToGeoDegrees(double radians)
        {
            return TrigToGeo(RadiansToDegrees(radians));
        }

        /// <summary>
        /// Gets the reverse of a bearing.
        /// </summary>
        /// <param name="bearing">bearing to get the reverse of</param>
        /// <returns>reverse bearing</returns>
        public static double ReverseBearing(double bearing)
        {
            double reverseBearing = bearing + 180;
            while (reverseBearing > 360)
                reverseBearing -= 360;
            return reverseBearing;
        }

    }
}
