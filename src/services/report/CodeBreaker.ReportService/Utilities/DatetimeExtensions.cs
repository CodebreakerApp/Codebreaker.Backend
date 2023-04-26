namespace System;

public static class DatetimeExtensions
{
    public static DateTime ToStartOfDay(this DateTime dateTime) =>
        new(dateTime.Year, dateTime.Month, dateTime.Day);

    public static DateTime ToEndOfDay(this DateTime dateTime) =>
        new (dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59, 999);
}
