﻿using System;
using System.Linq;
using System.Web;
using AutoMapper;
using OurMemory.Domain.DtoModel;
using OurMemory.Domain.DtoModel.ViewModel;
using OurMemory.Domain.Entities;
using OurMemory.Service.Model;

namespace OurMemory.AutomapperProfiles
{
    public class VeteranProfile : Profile
    {
        protected override void Configure()
        {
            base.Configure();

            Mapper.CreateMap<Veteran, VeteranViewModel>()
               .ForMember(dest => dest.FullName, opt => opt.MapFrom(x => x.FirstName
                                                                         + " " + x.LastName
                                                                         + " " + x.MiddleName))
               .ForMember(dest => dest.UserId, opt => opt.MapFrom(x => x.User.Id))
               .ForMember(dest => dest.Called, opt => opt.MapFrom(x => x.Called.HasValue ? x.Called.Value.ToString("yyyy-MM-dd") : ""))
               .ForMember(dest => dest.DateBirth, opt => opt.MapFrom(x => x.DateBirth.HasValue ? x.DateBirth.Value.ToString("yyyy-MM-dd") : ""))
               .ForMember(dest => dest.DateDeath, opt => opt.MapFrom(x => x.DateDeath.HasValue ? x.DateDeath.Value.ToString("yyyy-MM-dd") : ""))
               .AfterMap((veteranImages, veteranBindingImages) =>
               {
                   for (int i = 0; i < veteranImages.Images.Count; i++)
                   {
                       if (veteranImages.Images.ToList()[i].ImageOriginal != null)
                           veteranBindingImages.Images.ToList()[i].ImageOriginal = veteranImages.Images.ToList()[i].ImageOriginal.Insert(0, GetDomain);

                       if (veteranImages.Images.ToList()[i].ThumbnailImage != null)
                           veteranBindingImages.Images.ToList()[i].ThumbnailImage = veteranImages.Images.ToList()[i].ThumbnailImage.Insert(0, GetDomain);
                   }

               });


            Mapper.CreateMap<Veteran, Veteran>().ForMember(dest => dest.User, opt => opt.Ignore());


            Mapper.CreateMap<VeteranBindingModel, Veteran>();

            Mapper.CreateMap<VeteranMapping, VeteranBindingModel>();

            Mapper.CreateMap<Veteran, VeteranMapping>()
                .ForMember(dest => dest.UrlImages, opt => opt.MapFrom(src => string.Join(", ", src.Images
                                                    .Select(x => GetDomain + x.ImageOriginal))));


            Mapper.CreateMap<VeteranBindingModel, Veteran>()
                .AfterMap((veteranBindingImages, veteranImages) =>
            {
                for (int i = 0; i < veteranImages.Images.Count; i++)
                {
                    if (veteranBindingImages.Images.ToList()[i].ImageOriginal != null)
                        veteranImages.Images.ToList()[i].ImageOriginal = veteranBindingImages.Images.ToList()[i].ImageOriginal.Replace(GetDomain, "");

                    if (veteranBindingImages.Images.ToList()[i].ThumbnailImage != null)
                        veteranImages.Images.ToList()[i].ThumbnailImage = veteranBindingImages.Images.ToList()[i].ThumbnailImage.Replace(GetDomain, "");
                }

            });


        }


        private static string GetDomain
        {
            get { return HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority; }
        }
    }
}