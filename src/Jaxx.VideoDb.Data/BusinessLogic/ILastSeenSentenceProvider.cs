using Jaxx.VideoDb.Data.BusinessModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jaxx.VideoDb.Data.BusinessLogic
{
    public interface ILastSeenSentenceProvider
    {
        /// <summary>
        /// Returns a LastSeenInformation and fills up it's LastSeenSentence porperty with a sentence
        /// which describes cirumstanced of the last view based on the data given in the LastSeenInformations.
        /// </summary>
        /// <param name="Informations"></param>
        /// <returns></returns>
        LastSeenInformation GetLastSeenSentence(LastSeenInformation Informations, DateTime referenceDate);

        string GetReadableTimeSinceLastViewHtml(DateTime LastSeenDate);
    }
}
