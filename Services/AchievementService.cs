using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;

using HackerRank.Data;
using HackerRank.Models.Achievements;
using HackerRank.ViewModels;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HackerRank.Services
{
    public interface IAchievementService
    {
        Task<List<AchievementViewModel>> ListAllAchievements(ClaimsPrincipal user);
        Task<AchievementViewModel> FindAchievementById(int id);
        Task<AchievementInputModel> FindAchievementModelById(int id);
        Task Create(AchievementInputModel achievementModel, IFormFile file);
        Task Edit(AchievementInputModel achievementModel, IFormFile file);
        Task<AchievementViewModel> Details(int id);
        Task Delete(int id);
        Task<string> SaveImage(IFormFile file);
    }

    public class AchievementService : IAchievementService
    {
        private readonly HackerRankContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMapper _mapper;

        public AchievementService (HackerRankContext context, IWebHostEnvironment webHostEnvironment, IMapper mapper)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _mapper = mapper;
        }

        public async Task<List<AchievementViewModel>> ListAllAchievements(ClaimsPrincipal user)
        {
            List<AchievementViewModel> viewModelList = new();

            if (user.Identity.IsAuthenticated)
            {
                var userAchievements = await _context.UserAchievement
                    .Include(a => a.Achievement)
                    .Where(x => x.User.UserName == user.Identity.Name).ToListAsync();

                var achievements = await _context.Achievement.ToListAsync();

                _mapper.Map(achievements, viewModelList);

                foreach (var a in viewModelList)
                {
                    if (userAchievements.Where(x => x.Achievement.AchievementId == a.AchievementId).FirstOrDefault() != null)
                    {
                        a.IsUnlocked = true;
                    }
                }

                return viewModelList.OrderByDescending(x => x.IsUnlocked).ToList();
            }
            else
            {
                _mapper.Map(await _context.Achievement.ToListAsync(), viewModelList);
                return viewModelList;
            }
        }

        public async Task<AchievementViewModel> FindAchievementById(int id)
        {
            AchievementViewModel viewModel = new();
            _mapper.Map(await _context.Achievement.FindAsync(id), viewModel);
            return viewModel;
        }

        public async Task<AchievementInputModel> FindAchievementModelById(int id)
        {
            Achievement achievement = await _context.Achievement.FindAsync(id);

            return _mapper.Map<AchievementInputModel>(achievement);
        }

        public async Task Create(AchievementInputModel achievementModel, IFormFile file)
        {
            Achievement achievement =_mapper.Map<Achievement>(achievementModel);
            achievement.Image = await SaveImage(file);

            _context.Add(achievement);
            await _context.SaveChangesAsync();
        }

        public async Task Edit(AchievementInputModel achievementModel, IFormFile file)
        {
            Achievement achievement = _mapper.Map<Achievement>(achievementModel);

            try
            {
                _context.Achievement.Update(achievement);

                if (file == null)
                {
                    _context.Entry(achievement).Property(p => p.Image).IsModified = false;
                }
                else
                {
                    achievement.Image = await SaveImage(file);
                }

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

        public async Task<string> SaveImage(IFormFile file)
        {
            string uniqueFileName = null;

            if (file != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "achievementImg");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }
            return uniqueFileName;
        }
    }
}
