using System.Globalization;

namespace RM.CommonService.Helpers
{
    public class UtcToIstDateTime
    {
        static TimeZoneInfo istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        public static DateTime UtcStringToIst(string utcDateString)
        {
            DateTime utcDateTime = DateTime.ParseExact(utcDateString, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            DateTime istDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, istTimeZone);
            return istDateTime;
        }

        public static DateTime UtcStringToIst(DateTime utcDate)
        {
            DateTime istDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDate, istTimeZone);
            return istDateTime;
        }
    }
}
