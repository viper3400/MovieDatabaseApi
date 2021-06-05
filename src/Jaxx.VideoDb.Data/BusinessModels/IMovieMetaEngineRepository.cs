using System.Collections.Generic;

namespace Jaxx.VideoDb.Data.BusinessModels
{
    public interface IMovieMetaEngineRepository
    {
        IEnumerable<MovieMetaEngineDefinition> MovieMetaEngines { get; }
    }
}