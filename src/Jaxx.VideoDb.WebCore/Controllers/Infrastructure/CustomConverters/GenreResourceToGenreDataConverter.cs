using AutoMapper;
using Jaxx.VideoDb.Data.DatabaseModels;
using Jaxx.VideoDb.WebCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jaxx.VideoDb.WebCore.Controllers.Infrastructure.CustomConverters
{
    internal class GenreResourceToGenreDataConverter : IValueResolver<MovieDataResource, videodb_videodata, IEnumerable<videodb_videogenre>>
    {
        public IEnumerable<videodb_videogenre> Resolve(MovieDataResource source, videodb_videodata destination, IEnumerable<videodb_videogenre> destMember, ResolutionContext context)
        {
            var result = new List<videodb_videogenre>();
            if (source.Genres != null)
            {
                foreach (var genreResource in source.Genres)
                {
                    var genreData = new videodb_videogenre { genre_id = genreResource.Id, video_id = source.id };
                    result.Add(genreData);
                }
            }
            return result;
        }

    }
}
