using System;

namespace MilitaryRewardApp.Models
{
    public class Soldier
    {
        public int SoldierID { get; set; }
        public string SoldierCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Rank { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public int UnitID { get; set; }
        public string UnitName { get; set; } = string.Empty; // For display
        public string Ethnicity { get; set; } = string.Empty;
        public string Religion { get; set; } = string.Empty;
        public string AcademicLevel { get; set; } = string.Empty;
        public string PoliticalTheory { get; set; } = string.Empty;
        public string SchoolsAttended { get; set; } = string.Empty;
        public string WorkHistory { get; set; } = string.Empty;
    }
}
