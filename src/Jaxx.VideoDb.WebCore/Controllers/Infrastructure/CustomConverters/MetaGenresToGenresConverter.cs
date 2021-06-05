using AutoMapper;
using Jaxx.VideoDb.WebCore.Models;
using System.Collections.Generic;

namespace Jaxx.VideoDb.WebCore.Controllers.Infrastructure.CustomConverters
{
    internal class MetaGenresToGenresConverter : IValueResolver<MovieMetaResource, MovieDataResource, List<MovieDataGenreResource>>
    {
        private readonly XmlMapper.IObjectMapper _genreMapper;

        public MetaGenresToGenresConverter(XmlMapper.IObjectMapper GenreMapper)
        {
            _genreMapper = GenreMapper;
        }
        public List<MovieDataGenreResource> Resolve(MovieMetaResource source, 
            MovieDataResource destination, 
            List<MovieDataGenreResource> destMember, 
            ResolutionContext context)
        {            
            var destGenres = new List<MovieDataGenreResource>();

            foreach (var srcGenre in source.Genres)
            {
                var mappedGenre = int.Parse(_genreMapper.Map(srcGenre));
                var genreResoure = new MovieDataGenreResource { Id = mappedGenre };
                destGenres.Add(genreResoure);
            }

            return destGenres;
        }
    }
}