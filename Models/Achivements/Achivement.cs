using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackerRank.Models.Achivements
{
    public class Achivement
    {
        public int AchivementId { get; set; }
        public string AchivementName { get; set; }
        public string Description { get; set; }
        public int Level { get; set; }
        public byte[] ImageBinaryData { get; set; }
    }
}
