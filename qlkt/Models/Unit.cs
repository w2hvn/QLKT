namespace MilitaryRewardApp.Models
{
    public class Unit
    {
        public int UnitID { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public int? ParentID { get; set; }
    }
}
