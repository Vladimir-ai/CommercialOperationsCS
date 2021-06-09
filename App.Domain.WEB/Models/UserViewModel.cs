namespace App.Domain.WEB.Models
{
    public class UserViewModel
    {
        public long Id { get; set; }

        public string Name { get; set; }
        public long? BuildingId { get; set; }
        public AddressViewModel Address { get; set; }
        public string UserType { get; set; }
    }
}
