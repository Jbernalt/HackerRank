using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;

using HackerRank.Models.Achievements;

namespace HackerRank.Data
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AchievementViewModel, Achievement>().ReverseMap().ForMember(x => x.Image, opt => opt.Ignore()); ;
        }
    }
}
