namespace App.Domain.WEB.Models
{
    public class UserViewModel
    {
        public long Id { get; set; }

        public string Name { get; set; }
        public long? BuildingId { get; set; }
        public AddressViewModel Address { get; set; }
        public string UserType { get; set; }

        public float TotalBoughtValue { get; set; }
        public float TotalSoldValue { get; set; }
        public int TotalBoughtAmount { get; set; }
        public int TotalSoldAmount { get; set; }
    }
}
