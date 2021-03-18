using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HackerRank.Data;

namespace HackerRank.Services
{
    public interface IRankingService
    {

    }

    public class RankingService : IRankingService
    {
        HackerRankContext _context;

        public RankingService(HackerRankContext context)
        {
            _context = context;
        }
    }
}
