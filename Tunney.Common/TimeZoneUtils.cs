using System;
using System.Globalization;

namespace Tunney
{
    [Serializable]
    public class TimeZoneUtils
    {
        public const string TIMEZONE_PST = @"Pacific Standard Time";
        public const string TIMEZONE_EST = @"Eastern Standard Time";

        public static TimeSpan GetUTCOffset(TimeZoneInfo _timeZoneInfo, DateTime _dataSampleStamp)
        {
            TimeSpan offset = _timeZoneInfo.BaseUtcOffset;

            //.NET TimeZoneInfo.BaseUtcOffset class does NOT take DST into account.  WTF????
            if (_timeZoneInfo.SupportsDaylightSavingTime)
            {
                DateTime stamp = TimeZoneInfo.ConvertTime(_dataSampleStamp, _timeZoneInfo);                

                

                foreach (TimeZoneInfo.AdjustmentRule ar in _timeZoneInfo.GetAdjustmentRules())
                {
                    if (stamp >= ar.DateStart && stamp <= ar.DateEnd)
                    {
                        DateTime transitionStart = GetTransitionInfoDateTime(ar.DaylightTransitionStart, _dataSampleStamp);
                        DateTime transitionEnd = GetTransitionInfoDateTime(ar.DaylightTransitionEnd, _dataSampleStamp);
                        if (stamp >= transitionStart && stamp < transitionEnd)
                        {
                            offset += ar.DaylightDelta;
                        }                        
                    }
                }
            }
            return offset;
        }

        /// <summary>
        /// Don't judge me on the following code.  It is ripped right from MSDN :)
        /// http://msdn.microsoft.com/en-us/library/system.timezoneinfo.transitiontime.isfixeddaterule.aspx        /// 
        /// </summary>
        /// <param name="_stamp"></param>        
        /// <returns></returns>
        protected static DateTime GetTransitionInfoDateTime(TimeZoneInfo.TransitionTime _transition, DateTime _stamp)
        {
            // For non-fixed date rules, get local calendar
            Calendar cal = CultureInfo.CurrentCulture.Calendar;

            // Get first day of week for transition
            // For example, the 3rd week starts no earlier than the 15th of the month
            int startOfWeek = _transition.Week * 7 - 6;

            // What day of the week does the month start on?
            int firstDayOfWeek = (int)cal.GetDayOfWeek(new DateTime(_stamp.Year, _transition.Month, 1));

            // Determine how much start date has to be adjusted
            int transitionDay;
            int changeDayOfWeek = (int)_transition.DayOfWeek;

            if (firstDayOfWeek <= changeDayOfWeek)
            {
                transitionDay = startOfWeek + (changeDayOfWeek - firstDayOfWeek);
            }
            else
            {
                transitionDay = startOfWeek + (7 - firstDayOfWeek + changeDayOfWeek);
            }

            // Adjust for months with no fifth week
            if (transitionDay > cal.GetDaysInMonth(_stamp.Year, _transition.Month)) transitionDay -= 7;

            DateTime retval = new DateTime(_stamp.Year, _transition.Month, transitionDay, _transition.TimeOfDay.Hour, _transition.TimeOfDay.Minute, _transition.TimeOfDay.Second);
            return retval;
        }   

        public static DateTimeOffset ConvertToDateTimeOffset(DateTime _originalValue, TimeZoneInfo _timeZoneInfo)
        {
            DateTimeOffset retval = new DateTimeOffset(_originalValue, _timeZoneInfo.BaseUtcOffset);
            return retval;
        }

        //public static TimeSpan GetTimeZoneInfoDelta(TimeZoneInfo _timeZoneA, TimeZoneInfo _timeZoneB)
        //{
        //    if (_timeZoneA.StandardName.Equals(_timeZoneB.StandardName)) return TimeSpan.Zero; //Quick escape!

        //    TimeSpan offsetA = GetUTCOffset(_timeZoneA);
        //    TimeSpan offsetB = GetUTCOffset(_timeZoneB);

        //    //So if A is PST and B is EST, we should see a offset of -3 hours (-8 - (-5) = -3)

        //    return (offsetA - offsetB);
        //}
    }
}