using AutoMapper;
using Jaxx.VideoDb.WebCore.Models;
using System.Text;

namespace Jaxx.VideoDb.WebCore.Controllers.Infrastructure.CustomConverters
{
    internal class MetaActorsToActorStringConverter : IValueResolver<MovieMetaResource, MovieDataResource, string>
    {
        public string Resolve(MovieMetaResource source, MovieDataResource destination, string destMember, ResolutionContext context)
        {
            var actorsStringBuilder = new StringBuilder();
            foreach (var actor in source.Actors)
            {
                actorsStringBuilder.AppendLine($"{actor.ActorName}::::{actor.MetaEngine}::{actor.Reference}");
            }
            return actorsStringBuilder.ToString();
        }
    }
}