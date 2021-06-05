using AutoMapper;
using Jaxx.VideoDb.WebCore.Controllers.Infrastructure.CustomConverters;
using Jaxx.WebApi.Shared.Controllers.Infrastructure;
using Jaxx.WebApi.Shared.Models;
using Jaxx.WebGallery.Controllers;
using Jaxx.WebGallery.CustomConverters;
using Jaxx.WebGallery.DataModels;
using Jaxx.WebGallery.ResourceModels;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Jaxx.WebGallery
{
    public class WebGalleryMappingProfile : Profile
    {
        public WebGalleryMappingProfile()
        {
            CreateMap<GalleryAlbum, GalleryAlbumResource>()
                .ForMember(dest => dest.ThumbImageBase64Enc, opt => opt.MapFrom<GenericObjectToByteResolver>())
                .ForMember(dest => dest.Self, opt => opt.MapFrom(src => Link.To(nameof(GalleryController.GetAlbumByIdAsync), new GetByGenericIdParameter { Id = src.Id })))
                .ReverseMap();

            //CreateMap<GalleryImage, GalleryImageResource>().ForMember(dest => dest.Self, opt => opt.Ignore());
            //CreateMap<GalleryImage, GalleryImageResource>().ForMember(dest => dest.Self, opt => opt.Ignore());

            CreateMap<GalleryImage, GalleryImageResource>()
                .ForMember(dest => dest.ImageBase64Enc, opt => opt.MapFrom<GenericObjectToByteResolver>())
                .ForMember(dest => dest.Self, opt => opt.MapFrom(src => Link.To(
                    nameof(GalleryController.GetAlbumByIdAsync),
                    new GetByGenericIdParameter { Id = src.Id })));
            //.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            //.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            //.ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            //.ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date))
            //.ForMember(dest => dest.AlbumId, opt => opt.MapFrom(src => src.AlbumId))
            //.ForSourceMember(src => src.LocalPath, opt => opt.DoNotValidate())
            //.ForSourceMember(src => src.RemotePath, opt => opt.DoNotValidate())
            //.ForSourceMember(src => src.Permissions, opt => opt.DoNotValidate());
        }
    }
}
