using App.Domain.WEB.Utils;

namespace App.Domain.WEB.Models
{
    public class ItemFilterViewModel
    {
        public string SortOrder { get; set; }
        public string NameFilter { get; set; }

        [StringArrayInterceptor]
        public string[] CatFilter { get; set; }

        public float MinVal { get; set; } = 0;
        public float MaxVal { get; set; } = float.MaxValue;

        public int MinAmount { get; set; } = 0;
        public int MaxAmount { get; set; } = int.MaxValue;
    }
}