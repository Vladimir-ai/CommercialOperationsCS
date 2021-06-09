namespace App.Domain.WEB.Models
{
    public class ItemStatsViewModel
    {
        public ItemViewModel Item { get; set; }
        public AddressViewModel MostPopularCityToBuy { get; set; }
        public AddressViewModel MostPopularCityToSell { get; set; }
    }
}