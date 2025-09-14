namespace TrucksWeighingWebApp.Infrastructure.TimeZone
{
    public static class Tz
    {
        public static TimeZoneInfo Get(string timeZoneId)
        {
			try
			{
				return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
			}
			catch
			{
				return TimeZoneInfo.Utc;
			}
        }

		public static DateTime ToUtc(DateTime local, TimeZoneInfo tz)
		{
			return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), tz);
		}

		public static DateTime FromUtc(DateTime utc, TimeZoneInfo tz)
		{
			return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Unspecified), tz);
		}
    }
}
