using System.Collections.Generic;

namespace App.Domain.WEB.Models
{
    public class ItemViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public int TotalAmount { get; set; }
        public float TotalValue { get; set; }
        public IList<CategoryViewModel> Categories { get; set; }
    }
}