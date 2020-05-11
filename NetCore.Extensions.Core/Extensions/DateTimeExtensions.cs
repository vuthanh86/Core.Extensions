using System;
using System.Globalization;

namespace NetCore.Extensions.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime Next(this DateTime d, DayOfWeek dayOfWeek)
        {
            while (d.DayOfWeek != dayOfWeek)
            {
                d = d.AddDays(1);
            }
            return d;
        }

        public static float CalculateAge(this DateTime birthday, DateTime toDate)
        {
            var year = toDate.Year - birthday.Year;
            var getAtDays = toDate.DayOfYear;
            var dobDays = birthday.DayOfYear;

            if (birthday.Year % 4 == 0 && dobDays > (31 + 28) && toDate.Year % 4 != 0)
            {
                dobDays -= 1;
            }

            if (toDate.Year % 4 == 0 && getAtDays > (31 + 28) && birthday.Year % 4 != 0)
            {
                getAtDays -= 1;
            }

            var days = getAtDays - dobDays;
            return year + days * 1f / 365;
        }

        public static string ToShortDateString(this DateTime dateTime) => dateTime.ToString("d");

        public static string ToShortTimeString(this DateTime dateTime) => dateTime.ToString("t");

        public static string ToLongDateString(this DateTime dateTime) => dateTime.ToString("D");

        public static string ToLongTimeString(this DateTime dateTime) => dateTime.ToString("T");

        public static string ToString(this DateTime? dateTime, string format)
        {
            return dateTime?.ToString(format);
        }

        public static int UnixTimeStamp(this DateTime date)
        {
            return (int)(date.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public static string ToShortVnDateString(this DateTime date)
        {
            CultureInfo.CurrentCulture = new CultureInfo("vi-VN", false);
            return date.ToString("dd/MM/yyyy");
        }

        public static string ToLongVnDateString(this DateTime date)
        {
            CultureInfo.CurrentCulture = new CultureInfo("vi-VN", false);
            return date.ToString("dddd, dd MMMM yyyy");
        }
    }
}
