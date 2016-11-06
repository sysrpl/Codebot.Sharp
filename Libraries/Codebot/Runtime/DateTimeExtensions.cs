using System;

namespace Codebot
{
	public static class DateTimeExtensions
	{
		private static readonly DateTime BaseDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		private static readonly long BaseLong = BaseDate.Ticks;


		public static long ToLong(this DateTime value)
		{
			return ((value.ToUniversalTime().Ticks - BaseLong) / 10000);
		}

		public static DateTime ToDateTime(this long value)
		{
			return BaseDate.AddTicks(value * 10000).ToLocalTime();
		}
	}
}
