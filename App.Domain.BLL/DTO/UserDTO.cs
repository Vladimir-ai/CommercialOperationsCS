
namespace App.Domain.BLL.DTO
{
    public class UserDto
    {
        public long Id { get; set; }

        public string Name { get; set; }
        public long BuildingId { get; set; }
        public AddressDto Address { get; set; }
        public string UserType { get; set; }
    }
}
