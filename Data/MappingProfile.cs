using AutoMapper;

using HackerRank.Models.Achievements;
using HackerRank.Models.Groups;
using HackerRank.Models.Users;
using HackerRank.Responses;
using HackerRank.ViewModels;

namespace HackerRank.Data
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AchievementResponse, Achievement>().ReverseMap().ForMember(x => x.Image, opt => opt.Ignore());
            CreateMap<AchievementViewModel, Achievement>().ReverseMap();
            CreateMap<UserViewModel, User>().ReverseMap();
            CreateMap<TopFiveViewModel, GroupStats>().ReverseMap();
        }
    }
}
