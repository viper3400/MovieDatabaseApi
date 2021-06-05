using AutoMapper;
using Jaxx.VideoDb.Data.DatabaseModels;
using Jaxx.VideoDb.WebCore.Controllers;
using Jaxx.VideoDb.WebCore.Controllers.Infrastructure.CustomConverters;
using Jaxx.VideoDb.WebCore.Models;
using Jaxx.WebApi.Shared.Controllers.Infrastructure;
using Jaxx.WebApi.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Jaxx.VideoDb.WebCore.Infrastructure
{
    public class DefaultAutomapperProfile : Profile
    {
        public DefaultAutomapperProfile()
        {
            CreateMap<videodb_videodata, MovieDataResource>()
                .ForMember(dest => dest.VideoOwner, opt => opt.MapFrom(src => src.VideoOwner.name))
                .ForMember(dest => dest.Genres, opt => opt.MapFrom<CustomConvert>())
                .ForMember(dest => dest.MediaTypeName, opt => opt.MapFrom(src => src.VideoMediaType.name))
                .ForMember(dest => dest.LastSeenInformation, opt => opt.MapFrom<LastSeenInformationConverter>())
                .ForMember(dest => dest.IsFavorite, opt => opt.MapFrom<FavoriteConverter>())
                .ForMember(dest => dest.IsFlagged, opt => opt.MapFrom<FlaggedConverter>())
                .ForMember(dest => dest.CoverImageBase64Enc, opt => opt.MapFrom<ImgUrlToImageConverter>())
                .ForMember(dest => dest.Self, opt => opt.MapFrom(src => Link.To(
                    nameof(MovieDataController.GetMovieDataByIdAsync),
                    new GetMovieByIdParameters { MovieId = src.id })));

            CreateMap<MovieDataResource, videodb_videodata>()
                .ForMember(dest => dest.VideoGenres, opt => opt.MapFrom<GenreResourceToGenreDataConverter>())
                .ForMember(dest => dest.VideoOwner, opt => opt.Ignore())
                .ForMember(dest => dest.VideoMediaType, opt => opt.Ignore())
                .ForMember(dest => dest.SeenInformation, opt => opt.Ignore())
                .ForMember(dest => dest.UserSettings, opt => opt.Ignore())
                .ForSourceMember(src => src.CoverImageBase64Enc, opt => opt.DoNotValidate())
                .ForSourceMember(src => src.Genres, opt => opt.DoNotValidate())
                .ForSourceMember(src => src.IsFavorite, opt => opt.DoNotValidate())
                .ForSourceMember(src => src.IsFlagged, opt => opt.DoNotValidate())
                .ForSourceMember(src => src.MediaTypeName, opt => opt.DoNotValidate())
                .ForMember(dest => dest.id, opt => opt.Ignore());

            CreateMap<videodb_mediatypes, MovieDataMediaTypeResource>();

            CreateMap<videodb_genres, MovieDataGenreResource>();

            CreateMap<MovieMetaEngine.MovieMetaMovieModel, MovieMetaResource>()
                .ForMember(dest => dest.Length, opt => opt.MapFrom(src => src.Length));


            CreateMap<MovieMetaResource, MovieDataResource>()
                .ForSourceMember(src => src.Genres, opt => opt.DoNotValidate())
                .ForMember(dest => dest.Genres, opt => opt.MapFrom<MetaGenresToGenresConverter>())
                .ForMember(dest => dest.runtime, opt => opt.AllowNull())
                .ForMember(dest => dest.runtime, opt => opt.MapFrom<MetaLengthToRuntimeConverter>())
                .ForMember(dest => dest.country, opt => opt.MapFrom(src => src.ProductionCountry))
                .ForMember(dest => dest.custom2, opt => opt.MapFrom(src => src.Barcode))
                .ForMember(dest => dest.actors, opt => opt.MapFrom<MetaActorsToActorStringConverter>());

            CreateMap<homewebbridge_userseen, MovieDataSeenResource>()
                .ForMember(dest => dest.SeenDate, opt => opt.MapFrom(src => src.viewdate))
                .ForMember(dest => dest.Movie, opt => opt.MapFrom(src => src.MovieInformation))
                .ForMember(dest => dest.Self, opt => opt.MapFrom(src => Link.To(
                    nameof(MovieDataController.GetMovieDataByIdAsync),
                    new GetMovieByIdParameters { MovieId = src.vdb_videoid })));

            CreateMap<homewebbridge_inventory, InventoryResource>()
                .ForMember(dest => dest.Self, opt => opt.MapFrom(src => Link.To(
                    nameof(InventoryController.GetInventory),
                    new GetByGenericIdParameter { Id = src.id })))
                .ReverseMap();

            CreateMap<homewebbridge_inventorydata, InventoryDataResource>()
                 .ForMember(dest => dest.Self, opt => opt.MapFrom(src => Link.To(
                    nameof(InventoryController.GetInventoryRackDataForRack), new GetRackIdFromRouteParameter { RackId = src.rackid })))
                .ReverseMap();
        }
    }

    internal class FlaggedConverter : IValueResolver<videodb_videodata, MovieDataResource, bool>
    {
        public bool Resolve(videodb_videodata source, MovieDataResource destination, bool destMember, ResolutionContext context)
        {
            return source.UserSettings.FirstOrDefault()?.watchagain == 1 ? true : false;
        }
    }

    internal class FavoriteConverter : IValueResolver<videodb_videodata, MovieDataResource, bool>
    {
        public bool Resolve(videodb_videodata source, MovieDataResource destination, bool destMember, ResolutionContext context)
        {
            return source.UserSettings.FirstOrDefault()?.is_favorite == 1 ? true : false;
        }
    }

    public class CustomConvert : IValueResolver<videodb_videodata, MovieDataResource, List<MovieDataGenreResource>>
    {
        public List<MovieDataGenreResource> Resolve(videodb_videodata source, MovieDataResource destination, List<MovieDataGenreResource> destMember, ResolutionContext context)
        {
            var resultList = new List<MovieDataGenreResource>();
            foreach (var genre in source.VideoGenres)
            {
                resultList.Add(new MovieDataGenreResource { Id = genre.genre_id, Name = genre.Genre.name });
            }
            return resultList;
        }
    }

}
