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
                .AfterMap((dto, user) => user.NormalizedEmail = dto.Email?.ToUpper())
                .ReverseMap()
                .ForMember(x => x.SecurePassword, x => x.MapFrom(p => p.PasswordHash));

            CreateMap<BlogUser, BlogUserDto>().ReverseMap();
            CreateMap<Blog, BlogDto>();
        }
    }
}
