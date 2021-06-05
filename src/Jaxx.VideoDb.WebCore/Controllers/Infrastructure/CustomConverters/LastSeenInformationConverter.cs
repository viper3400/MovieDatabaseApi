using AutoMapper;
using Jaxx.VideoDb.Data.BusinessLogic;
using Jaxx.VideoDb.Data.BusinessModels;
using Jaxx.VideoDb.Data.DatabaseModels;
using Jaxx.VideoDb.WebCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jaxx.VideoDb.WebCore.Controllers.Infrastructure.CustomConverters
{
    public class LastSeenInformationConverter : IValueResolver<videodb_videodata, MovieDataResource, LastSeenInformation>
    {
        private readonly ILastSeenSentenceProvider _lastSeenSentenceProvider;

        public LastSeenInformationConverter()
        {
            _lastSeenSentenceProvider = new NodaTimeLastSeenSentenceProvider();
        }

        public LastSeenInformation Resolve(videodb_videodata source, MovieDataResource destination, LastSeenInformation destMember, ResolutionContext context)
        {
            var seenInformation = new LastSeenInformation();

            if (source.SeenInformation != null && source.SeenInformation.Count() > 0)
            {
                seenInformation.SeenCount = source.SeenInformation.Count();
                seenInformation.LastSeenDate = source.SeenInformation.OrderByDescending(entry => entry.viewdate).FirstOrDefault().viewdate;
                // Mit DateTime.Date wird nur der tatsächliche Datumsanteil berücksichtigt, eventuelle Zeitanteil spielen keine Rolle
                seenInformation.DaysSinceLastView = DateTime.Now.Date.Subtract(seenInformation.LastSeenDate.Date).Days;
                seenInformation = _lastSeenSentenceProvider.GetLastSeenSentence(seenInformation, DateTime.Now);
                seenInformation.ReadableTimeSinceLastViewHtml = _lastSeenSentenceProvider.GetReadableTimeSinceLastViewHtml(seenInformation.LastSeenDate);
                seenInformation.AllSeenDates = source.SeenInformation.OrderByDescending(s => s.viewdate).Select(s => s.viewdate).ToList();
            }
            else seenInformation = null;

            return seenInformation;
        }
    }
}
