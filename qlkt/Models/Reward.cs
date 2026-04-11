using System;

namespace QLKT.Models
{
    public class Reward
    {
        public int RewardID { get; set; }
        public int SoldierID { get; set; }
        public string SoldierName { get; set; } = string.Empty; // For display
        public string DecisionNumber { get; set; } = string.Empty;
        public string RewardType { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime DateSigned { get; set; }
    }
}
