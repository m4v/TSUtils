//
//  Utils.cs
//
//  Author:
//       Elián Hanisch <lambdae2@gmail.com>
//
//  Copyright (c) 2014 Elián Hanisch
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using UnityEngine;

namespace TrackingStationUtils
{
    public static class Utils
    {
        /* copied from http://stackoverflow.com/questions/12181024 */
        public static string format_SI (double d, string unit = "m", string format = "0.####")
        {
            char[] incPrefixes = { 'k', 'M', 'G', 'T', 'P', 'E', 'Z', 'Y' };
            char[] decPrefixes = { 'm', '\u03bc', 'n', 'p', 'f', 'a', 'z', 'y' };

            int degree = (int)Math.Floor (Math.Log10 (Math.Abs (d)) / 3);
            double scaled = d * Math.Pow (1000, -degree);

            char? prefix = null;
            switch (Math.Sign (degree)) {
            case 1:
                prefix = incPrefixes [degree - 1];
                break;
            case -1:
                prefix = decPrefixes [-degree - 1];
                break;
            }

            return scaled.ToString (format) + prefix + unit;
        }

        public static string format_time (double d)
        {
            string s = "00:00:00.0";
            if (d < 0) {
                return s;
            }
            int[] time = KSPUtil.GetDateFromUT ((int)d);
            /* time = (seconds, minutes, hours, days, years) */
            float seconds = (float)d % 60;
            if ((time[4] == 0) && (time[3] == 0)) {
                s = String.Format ("{0:00}:{1:00}:{2:00.0}", time[2], time[1], seconds);
            } else if (time[4] == 0) {
                s = String.Format ("{0:0}d {1:00}:{2:00}:{3:00}", 
                    time [3], time [2], time [1], seconds);
            } else {
                s = String.Format ("{0:0}y {1:0}d {2:00}:{3:00}:{4:00}", 
                    time[4], time [3], time [2], time [1], seconds);
            }
            return s;
        }

        public static string format_angle (double d)
        {
            float d_ = Mathf.Abs ((float)d);
            int degrees = (int) d_;
            int arcmin = (int) (60 * (d_ - degrees));
            int arcsec = (int) (3600 * (d_ - degrees - (float) arcmin / 60));

            string s = String.Format ("{0:000}°{1:00}'{2:00}\"", degrees, arcmin, arcsec);
            if (d < 0) {
                return '-' + s;
            }
            return s;
        }
    }
}

