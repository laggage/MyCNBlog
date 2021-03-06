﻿using System;
using AutoMapper;
using MyCNBlog.Core.Models;
using MyCNBlog.Core.Models.Dtos;

namespace MyCNBlog.Api.Extensions
{
    public class Map : Profile
    {
        public Map()
        {
            CreateMap<UserLoginDto, BlogUser>()
                .ForMember(x => x.NormalizedUserName, x => x.MapFrom(p => p.UserName.ToUpper()));

            CreateMap<UserRegisterDto, BlogUser>()
                .ForMember(x => x.NormalizedUserName, x => x.MapFrom(p => p.UserName.ToUpper()))
                .ForMember(x => x.RegisterDate, m => m.MapFrom(s => DateTime.Now))
                .AfterMap((dto, user) => user.NormalizedEmail = dto.Email?.ToUpper())
                .ReverseMap()
                .ForMember(x => x.SecurePassword, x => x.MapFrom(p => p.PasswordHash));

            CreateMap<BlogUser, BlogUserDto>().ReverseMap();
            CreateMap<Blog, BlogDto>().
                ForMember(
                    m => m.Blogger,
                    x => x.MapFrom(p => p.User))
                .ReverseMap();
            CreateMap<Blog, BlogAddDto>()
                .ForMember(x => x.IsOpened, m => m.Ignore())
                .ReverseMap();

            CreateMap<Post, PostDto>().ReverseMap();
            CreateMap<PostAddDto, Post>().ReverseMap();

            CreateMap<Tag, TagDto>().ReverseMap();

            CreateMap<CommentAddDto, PostComment>()
                .ReverseMap();
            CreateMap<PostComment, CommentDto>()
                .ReverseMap();
            CreateMap<BlogUser, CommentUserDto>();
        }
    }
}
