using System;

namespace App.Domain.WEB.Models
{
    public class AddressViewModel
    {
        public string Country { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string Building { get; set; }

        public override string ToString()
        {
            return $"{Country}, {City}, {Street}, {Building}";
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(AddressViewModel))
                return false;
            var aa = (AddressViewModel)obj;
            
            return Country.Equals(aa.Country) && 
                City.Equals(aa.City) && 
                Street.Equals(aa.Street) && 
                Building.Equals(aa.Building);
        }

        public static AddressViewModel FromString(string str)
        {
            var addressArray = str.Split(",", 4, StringSplitOptions.RemoveEmptyEntries);

            if (addressArray.Length != 4)
                throw new ArgumentException("Incorrect string length");

            return new AddressViewModel
            {
                Country = addressArray[0].Trim(),
                City = addressArray[1].Trim(),
                Street = addressArray[2].Trim(),
                Building = addressArray[3].Trim()
            };
        }
    }
}