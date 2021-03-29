using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;

using HackerRank.Models.Achievements;
using HackerRank.ViewModels;
using HackerRank.Models.Users;
using HackerRank.Responses;

namespace HackerRank.Data
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AchievementViewModel, Achievement>().ReverseMap().ForMember(x => x.Image, opt => opt.Ignore());
            CreateMap<UserViewModel, User>().ReverseMap();
            CreateMap<UserViewModel, HttpResponseMessage>().ReverseMap().ForMember(x => x.StatusCode, opt => opt.MapFrom(h => h.StatusCode));
        }
    }
}
