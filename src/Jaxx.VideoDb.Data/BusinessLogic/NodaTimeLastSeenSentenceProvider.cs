using Jaxx.VideoDb.Data.BusinessModels;
using System;
using System.Collections.Generic;
using System.Text;
using NodaTime;

namespace Jaxx.VideoDb.Data.BusinessLogic
{
    public class NodaTimeLastSeenSentenceProvider : ILastSeenSentenceProvider
    {
        public LastSeenInformation GetLastSeenSentence(LastSeenInformation informations, DateTime referenceDate)
        {

            var daysSinceLastView = referenceDate.Date.Subtract(informations.LastSeenDate).Days;

            System.Text.StringBuilder lastSeenSentence = new System.Text.StringBuilder();
            lastSeenSentence.Append(String.Format("Du hast diesen Film bereits {0} Mal gesehen, zuletzt ", informations.SeenCount));

            var nodaNowDate = new LocalDate(referenceDate.Year, referenceDate.Month, referenceDate.Day);
            var nodaLastSeenDate = new LocalDate(informations.LastSeenDate.Year, informations.LastSeenDate.Month, informations.LastSeenDate.Day);

            var nodaPeriod = Period.Between(nodaLastSeenDate, nodaNowDate,
                                       PeriodUnits.Years | PeriodUnits.Days);


            if (nodaPeriod.Years == 0)
            {
                switch (nodaPeriod.Days)
                {
                    case 0:
                        lastSeenSentence.Append("heute.");
                        break;
                    case 1:
                        lastSeenSentence.Append("gestern.");
                        break;
                    default:
                        lastSeenSentence.Append(String.Format("am {0}. Das war vor {1} Tagen.",
                            informations.LastSeenDate.ToLongDateString(),
                            nodaPeriod.Days));
                        break;
                }
            }
            else
            {
                lastSeenSentence.Append($"am {informations.LastSeenDate.ToLongDateString()}. Das war vor {nodaPeriod.Years} Jahr(en) und {nodaPeriod.Days} Tag(en).");
            }

            informations.LastSeenSentence = lastSeenSentence.ToString();
            return informations;
        }

        public string GetReadableTimeSinceLastViewHtml(DateTime LastSeenDate)
        {
            var now = DateTime.Now;
            var nodaNowDate = new LocalDate(now.Year, now.Month, now.Day);
            var nodaLastSeenDate = new LocalDate(LastSeenDate.Year, LastSeenDate.Month, LastSeenDate.Day);

            var nodaPeriod = Period.Between(nodaLastSeenDate, nodaNowDate, PeriodUnits.Years | PeriodUnits.Days);
            var nodaPeriodYMD = Period.Between(nodaLastSeenDate, nodaNowDate, PeriodUnits.YearMonthDay);

            if (nodaPeriod.Years == 0 && nodaPeriod.Days < 91)
            {
                return $"{nodaPeriod.Days.ToString()}d";
            }
            else if (nodaPeriod.Years == 0 && nodaPeriod.Days >= 91)
            {                
                return $"{nodaPeriodYMD.Months}M";
            }
            else if (nodaPeriodYMD.Years > 0 && nodaPeriodYMD.Months < 3)
            {
                return $"{nodaPeriodYMD.Years}Y";
            }
            else if (nodaPeriodYMD.Years > 0 && nodaPeriodYMD.Months < 6)
            {
                return $"{nodaPeriodYMD.Years}&frac14;Y";
            }
            else if (nodaPeriodYMD.Years > 0 && nodaPeriodYMD.Months < 9)
            {
                return $"{nodaPeriodYMD.Years}&frac12;Y";
            }
            else if (nodaPeriodYMD.Years > 0 && nodaPeriodYMD.Months <= 12)
            {
                return $"{nodaPeriodYMD.Years}&frac34;Y";
            }
            else throw new NotSupportedException();
        }
    }
}
