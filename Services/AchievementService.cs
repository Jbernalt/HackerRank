using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;

using HackerRank.Data;
using HackerRank.Models.Achievements;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using static System.Net.WebRequestMethods;

namespace HackerRank.Services
{

    public interface IAchievementService
    {
        Task<List<Achievement>> ListAllAchievements();
        Task<Achievement> FindAchievementById(int id);
        Task<AchievementViewModel> FindAchievementModelById(int id);
        Task Create(AchievementViewModel achievementModel, IFormFile file);
        Task Edit(AchievementViewModel achievementModel, IFormFile file);
        Task<Achievement> Details(int id);
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

        public async Task<List<Achievement>> ListAllAchievements()
        {
            return await _context.Achievement.ToListAsync();
        }

        public async Task<Achievement> FindAchievementById(int id)
        {
            return await _context.Achievement.FindAsync(id);
        }

        public async Task<AchievementViewModel> FindAchievementModelById(int id)
        {
            Achievement achievement = await _context.Achievement.FindAsync(id);

            return _mapper.Map<AchievementViewModel>(achievement);
        }

        public async Task Create(AchievementViewModel achievementModel, IFormFile file)
        {
            Achievement achievement =_mapper.Map<Achievement>(achievementModel);
            achievement.Image = await SaveImage(file);

            _context.Add(achievement);
            await _context.SaveChangesAsync();
        }

        public async Task Edit(AchievementViewModel achievementModel, IFormFile file)
        {
            Achievement achievement = _mapper.Map<Achievement>(achievementModel);

            try
            {
                _context.Achievement.Update(achievement);

                if (file == null)
                {
                    _context.Entry(achievement).Property(p => p.Image).IsModified = false;
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

        public async Task<Achievement> Details(int id)
        {
            var achievement = await _context.Achievement
                .FirstOrDefaultAsync(m => m.AchievementId == id);
            return achievement;
        }

        public async Task Delete(int id)
        {
            var achievement = await _context.Achievement.FindAsync(id);
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
