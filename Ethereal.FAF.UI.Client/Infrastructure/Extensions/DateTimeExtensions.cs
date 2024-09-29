using System;

namespace Ethereal.FAF.UI.Client.Infrastructure.Extensions
{
    internal static class DateTimeExtensions
    {
        public static DateTime Truncate(this DateTime dateTime, TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero) return dateTime; // Or could throw an ArgumentException
                                                            // Some comments suggest removing the following line.  I think the check
                                                            // for MaxValue makes sense - it's often used to represent an indefinite expiry date.
                                                            // (The check for DateTime.MinValue has no effect, because DateTime.MinValue % timeSpan
                                                            // is equal to DateTime.MinValue for any non-zero value of timeSpan.  But I think
                                                            // leaving the check in place makes the intent clearer).
                                                            // YMMV and the fact that different people have different expectations is probably
                                                            // part of the reason such a method doesn't exist in the Framework.
            if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue) return dateTime; // do not modify "guard" values
            return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
        }
    }
}
