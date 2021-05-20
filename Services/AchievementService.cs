using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using AutoMapper;

using HackerRank.Data;
using HackerRank.Models.Achievements;
using HackerRank.Responses;
using HackerRank.ViewModels;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using static HackerRank.Models.ActionTypes;

namespace HackerRank.Services
{
    public interface IAchievementService
    {
        Task<List<AchievementViewModel>> ListAllAchievements(string username);
        Task<AchievementViewModel> FindAchievementById(int id);
        Task<AchievementResponse> FindAchievementModelById(int id);
        Task Create(AchievementResponse achievementModel, IFormFile file);
        Task Edit(AchievementResponse achievementModel, IFormFile file);
        Task<AchievementViewModel> Details(int id);
        Task Delete(int id);
        Task SetShowCase(List<string> achievementIds, string username);
        Task<string> UpdateAchievementsOnUser(string username, ActionType actionType);
    }

    public class AchievementService : IAchievementService
    {
        private readonly HackerRankContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;

        public AchievementService(HackerRankContext context, IWebHostEnvironment webHostEnvironment, IMapper mapper, IImageService imageService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _mapper = mapper;
            _imageService = imageService;
        }

        public async Task<string> UpdateAchievementsOnUser(string username, ActionType actionType)
        {
            var achievements = await _context.Achievement.Where(a => a.TypeOfAction == actionType).ToArrayAsync();
            var user = await _context.Users
                .Include(u => u.UserStats)
                .Where(n => n.NormalizedUserName == username.ToUpper())
                .FirstOrDefaultAsync();

            string message = string.Empty;
            int i = 0;

            foreach (var a in achievements)
            {
                var userachievements = await _context.UserAchievement
                    .Include("Achievement")
                    .Where(ua => ua.Achievement.AchievementId == a.AchievementId && ua.User.NormalizedUserName == user.NormalizedUserName)
                    .FirstOrDefaultAsync();

                if (userachievements == null)
                {
                    UserAchievement userAchievement = new()
                    {
                        IsUnlocked = true,
                        User = user,
                        Achievement = a
                    };

                    if (user.UserStats.TotalCommits >= a.NumberOfActions && a.TypeOfAction == ActionType.Commit)
                    {
                        if (i == 0)
                        {
                            message = $"{user.UserName} achieved {userAchievement.Achievement.AchievementName}, ";
                        }
                        else if (i > 0)
                        {
                            message += $"{userAchievement.Achievement.AchievementName}, ";
                        }
                        await _context.UserAchievement.AddAsync(userAchievement);
                        i++;
                    }
                    else if (user.UserStats.TotalIssuesCreated >= a.NumberOfActions && a.TypeOfAction == ActionType.IssueOpened)
                    {
                        if (i == 0)
                        {
                            message = $"{user.UserName} achieved {userAchievement.Achievement.AchievementName}, ";
                        }
                        else if (i > 0)
                        {
                            message += $"{userAchievement.Achievement.AchievementName}, ";
                        }
                        await _context.UserAchievement.AddAsync(userAchievement);
                        i++;
                    }
                    else if (user.UserStats.TotalIssuesSolved >= a.NumberOfActions && a.TypeOfAction == ActionType.IssueSolved)
                    {
                        if (i == 0)
                        {
                            message = $"{user.UserName} achieved {userAchievement.Achievement.AchievementName}, ";
                        }
                        else if (i > 0)
                        {
                            message += $"{userAchievement.Achievement.AchievementName}, ";
                        }
                        await _context.UserAchievement.AddAsync(userAchievement);
                        i++;
                    }
                    else if (user.UserStats.TotalMergeRequests >= a.NumberOfActions && a.TypeOfAction == ActionType.MergeRequest)
                    {
                        if (i == 0)
                        {
                            message = $"{user.UserName} achieved {userAchievement.Achievement.AchievementName}, ";
                        }
                        else if (i > 0)
                        {
                            message += $"{userAchievement.Achievement.AchievementName}, ";
                        }
                        await _context.UserAchievement.AddAsync(userAchievement);
                        i++;
                    }
                    else if (user.UserStats.TotalComments >= a.NumberOfActions && a.TypeOfAction == ActionType.Comment)
                    {
                        if (i == 0)
                        {
                            message = $"{user.UserName} achieved {userAchievement.Achievement.AchievementName}, ";
                        }
                        else if (i > 0)
                        {
                            message += $"{userAchievement.Achievement.AchievementName}, ";
                        }
                        await _context.UserAchievement.AddAsync(userAchievement);
                        i++;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return message;
        }

        public async Task SetShowCase(List<string> achievementIds, string username)
        {
            var userAchievements = await _context.UserAchievement
                .Include("Achievement")
                .Include("User")
                .Where(u => u.User.NormalizedUserName == username)
                .ToListAsync();

            foreach (var achievement in userAchievements)
            {
                achievement.IsShowCase = false;
                foreach (var id in achievementIds)
                {
                    if (achievement.Achievement.AchievementId == int.Parse(id))
                    {
                        achievement.IsShowCase = true;
                    }
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task<List<AchievementViewModel>> ListAllAchievements(string username)
        {
            List<AchievementViewModel> viewModelList = new();

            var userAchievements = await _context.UserAchievement
                .Include(a => a.Achievement)
                .Where(x => x.User.NormalizedUserName == username).ToListAsync();

            var achievements = await _context.Achievement.ToListAsync();

            _mapper.Map(achievements, viewModelList);

            foreach (var a in viewModelList)
            {
                if (userAchievements.Where(x => x.Achievement.AchievementId == a.AchievementId).FirstOrDefault() != null)
                {
                    a.IsUnlocked = true;
                }
                if (userAchievements.Where(x => x.IsShowCase == true && x.Achievement.AchievementId == a.AchievementId).FirstOrDefault() != null)
                {
                    a.IsShowCase = true;
                }
            }

            return viewModelList.OrderByDescending(x => x.IsUnlocked).ToList();
        }

        public async Task<AchievementViewModel> FindAchievementById(int id)
        {
            AchievementViewModel viewModel = new();
            _mapper.Map(await _context.Achievement.FindAsync(id), viewModel);
            return viewModel;
        }

        public async Task<AchievementResponse> FindAchievementModelById(int id)
        {
            Achievement achievement = await _context.Achievement.FindAsync(id);

            return _mapper.Map<AchievementResponse>(achievement);
        }

        public async Task Create(AchievementResponse achievementModel, IFormFile file)
        {
            Achievement achievement = _mapper.Map<Achievement>(achievementModel);
            var filename = await _imageService.SaveImage(file, false);
            achievement.Image = filename ?? "default-achievement.png";

            _context.Add(achievement);
            await _context.SaveChangesAsync();
        }

        public async Task Edit(AchievementResponse achievementModel, IFormFile file)
        {
            Achievement achievement = _mapper.Map<Achievement>(achievementModel);

            try
            {
                var _tempImg = _context.Achievement.Where(a => a.AchievementId == achievement.AchievementId).Select(p => p.Image).FirstOrDefault();
                _context.Achievement.Update(achievement);

                achievement.Image = await _imageService.SaveImage(file, false) ?? _tempImg;

                if (_tempImg != achievement.Image)
                    _imageService.DeleteImage(_tempImg, false);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AchievementExists(achievement.AchievementId))
                {
                    return;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<AchievementViewModel> Details(int id)
        {
            var achievement = await _context.Achievement.FirstOrDefaultAsync(m => m.AchievementId == id);
            AchievementViewModel viewModel = new();

            return _mapper.Map(achievement, viewModel);
        }

        public async Task Delete(int id)
        {
            var achievement = await _context.Achievement.FindAsync(id);
            var ua = _context.UserAchievement.Where(i => i.Achievement.AchievementId == id).ToList();
            if (ua.Count > 0)
                _context.UserAchievement.RemoveRange(ua);

            _context.Achievement.Remove(achievement);
            await _context.SaveChangesAsync();
        }

        private bool AchievementExists(int id)
        {
            return _context.Achievement.Any(e => e.AchievementId == id);
        }
    }
}
