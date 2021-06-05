using AutoMapper;
using Jaxx.VideoDb.WebCore.Models;

namespace Jaxx.VideoDb.WebCore.Controllers.Infrastructure.CustomConverters
{
    internal class MetaLengthToRuntimeConverter : IValueResolver<MovieMetaResource, MovieDataResource, int?>
    {
        public int? Resolve(MovieMetaResource source, MovieDataResource destination, int? destMember, ResolutionContext context)
        {
            int? runtime = null;
            if (source.Length != null)
            {
                int parsedLength;
                var isParsed = int.TryParse(source.Length, out parsedLength);
                if (isParsed) runtime = parsedLength;
            }
            
            return runtime;
        }
    }
}
