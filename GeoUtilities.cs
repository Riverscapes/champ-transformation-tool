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

namespace RSGIS.Utilites
{

    /// <summary>
    /// Class with static methods to do simple but common geographic stuff independent of ArcObjects.
    /// </summary>
    class GeoUtilities
    {

        /// <summary>
        /// Converts a degrees/minutes/seconds string into a decimal degrees double.
        /// </summary>
        /// <param name="dms">"degrees minutes seconds"</param>
        /// <param name="dd">variable to put the decimal degrees in</param>
        /// <returns>true if the string was successfully converted, false if not</returns>
        public static bool DmsToDeg(string dms, out double dd)
        {
            double deg, min, sec;
            dd = -999;
            string[] data = dms.Split(' ');
            if (data.Length != 3)
                return false;
            if (!double.TryParse(data[0], out deg) || deg < -180 || deg > 180)
                return false;
            if (!double.TryParse(data[1], out min) || min < 0 || min > 60)
                return false;
            if (!double.TryParse(data[2], out sec) || sec < 0 || sec > 60)
                return false;
            if (deg < 0)
                dd = deg - min / 60 - sec / 3600;
            else
                dd = deg + min / 60 + sec / 3600;
            return true;
        }

        /// <summary>
        /// Checks to see if a string is a valid geographic coordinate value in either DD or DMS.
        /// </summary>
        /// <param name="str">string to check</param>
        /// <returns>true if the string can be converted to valid coordinate, false if not</returns>
        public static bool CheckGeoCoordinate(string str)
        {
            double coord;
            return (double.TryParse(str, out coord) ||
                DmsToDeg(str, out coord)) && coord >= -180 && coord <= 180;
        }

        /// <summary>
        /// Checks to see if a string is a valid projected coordinate value. Pretty stupid, though.
        /// Only checks to see if the northing and easting are greater than 0.
        /// </summary>
        /// <param name="str">string to check</param>
        /// <returns>true if the string can be converted to valid coordinate, false if not</returns>
        public static bool CheckProjCoordinate(string str)
        {
            double coord;
            return double.TryParse(str, out coord) && coord > 0;
        }

        /// <summary>
        /// Checks to see if a string is a valid bearing value.
        /// </summary>
        /// <param name="str">string to check</param>
        /// <returns>true if the string can be converted to valid bearing, false if not</returns>
        public static bool CheckBearing(string str)
        {
            double bearing;
            return (double.TryParse(str, out bearing) ||
                DmsToDeg(str, out bearing)) && bearing >= -360 && bearing <= 360;
        }

    }

}
