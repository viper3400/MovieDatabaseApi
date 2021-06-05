using System;
using System.Collections.Generic;
using System.Text;

namespace Jaxx.VideoDb.Data.BusinessModels
{
    public class DefaultMovieMetaEngineRepository : IMovieMetaEngineRepository
    {
        public IEnumerable<MovieMetaEngineDefinition> MovieMetaEngines { get; private set; }
        public DefaultMovieMetaEngineRepository()
        {
            MovieMetaEngines = new List<MovieMetaEngineDefinition>()
            {
                new MovieMetaEngineDefinition { TypeName = MovieMetaEngineType.Ofdb, TypeAccessor = "OfdbParser.OfdbMovieMetaSearch", FriendlyName = "Ofdb" },
                new MovieMetaEngineDefinition { TypeName = MovieMetaEngineType.TheMovieDb, TypeAccessor = "TheMovieDbApi.TheMovieDbApiHttpClient", FriendlyName = "TheMovieDb" }
            };
        }

    }
}
