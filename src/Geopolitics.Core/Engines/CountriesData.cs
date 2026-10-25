using Geopolitics.Core.Models;

namespace Geopolitics.Core.Engines;

public static class CountriesData
{
    public static int CalculateSoldiers(long population)
    {
        if (population <= 5_000_000) return 1;
        if (population <= 10_000_000) return 2;
        if (population <= 25_000_000) return 3;
        if (population <= 50_000_000) return 4;
        if (population <= 75_000_000) return 5;
        if (population <= 100_000_000) return 6;
        if (population <= 125_000_000) return 7;
        if (population <= 150_000_000) return 8;
        return 9;
    }

    private static readonly List<Country> RawCountries = new()
    {
        // Africa
        new Country { Id = "za", Name = "South Africa", Code = "ZAF", Flag = "🇿🇦", Continent = "Africa", Population = 60414495, X = 54.8, Y = 66.6 },
        new Country { Id = "eg", Name = "Egypt", Code = "EGY", Flag = "🇪🇬", Continent = "Africa", Population = 112716598, X = 56.5, Y = 41.6 },
        new Country { Id = "ng", Name = "Nigeria", Code = "NGA", Flag = "🇳🇬", Continent = "Africa", Population = 223804632, X = 50.2, Y = 52.0 },
        new Country { Id = "ke", Name = "Kenya", Code = "KEN", Flag = "🇰🇪", Continent = "Africa", Population = 55100586, X = 59.0, Y = 54.6 },
        new Country { Id = "cd", Name = "DR Congo", Code = "COD", Flag = "🇨🇩", Continent = "Africa", Population = 102262808, X = 54.5, Y = 54.0 },
        new Country { Id = "et", Name = "Ethiopia", Code = "ETH", Flag = "🇪🇹", Continent = "Africa", Population = 126527060, X = 59.8, Y = 50.3 },
        new Country { Id = "ma", Name = "Morocco", Code = "MAR", Flag = "🇲🇦", Continent = "Africa", Population = 37840000, X = 45.0, Y = 41.0 },
        new Country { Id = "gh", Name = "Ghana", Code = "GHA", Flag = "🇬🇭", Continent = "Africa", Population = 34121985, X = 47.3, Y = 51.7 },
        new Country { Id = "mg", Name = "Madagascar", Code = "MDG", Flag = "🇲🇬", Continent = "Africa", Population = 30325732, X = 61.6, Y = 63.5 },
        new Country { Id = "tz", Name = "Tanzania", Code = "TZA", Flag = "🇹🇿", Continent = "Africa", Population = 67438106, X = 58.2, Y = 59.9 },

        // Asia
        new Country { Id = "in", Name = "India", Code = "IND", Flag = "🇮🇳", Continent = "Asia", Population = 1428627663, X = 71.5, Y = 50.5 },
        new Country { Id = "cn", Name = "China", Code = "CHN", Flag = "🇨🇳", Continent = "Asia", Population = 1425671352, X = 75.5, Y = 28.1 },
        new Country { Id = "jp", Name = "Japan", Code = "JPN", Flag = "🇯🇵", Continent = "Asia", Population = 123294513, X = 85.5, Y = 32.5 },
        new Country { Id = "id", Name = "Indonesia", Code = "IDN", Flag = "🇮🇩", Continent = "Asia", Population = 277534122, X = 79.5, Y = 59.5 },
        new Country { Id = "pk", Name = "Pakistan", Code = "PAK", Flag = "🇵🇰", Continent = "Asia", Population = 240485658, X = 67.5, Y = 41.1 },
        new Country { Id = "bd", Name = "Bangladesh", Code = "BGD", Flag = "🇧🇩", Continent = "Asia", Population = 149200000, X = 72.8, Y = 44.5 },
        new Country { Id = "vn", Name = "Vietnam", Code = "VNM", Flag = "🇻🇳", Continent = "Asia", Population = 98858915, X = 77.8, Y = 48.9 },
        new Country { Id = "ph", Name = "Philippines", Code = "PHL", Flag = "🇵🇭", Continent = "Asia", Population = 117337368, X = 81.5, Y = 48.0 },
        new Country { Id = "kr", Name = "South Korea", Code = "KOR", Flag = "🇰🇷", Continent = "Asia", Population = 51784059, X = 81.9, Y = 34.3 },
        new Country { Id = "sa", Name = "Saudi Arabia", Code = "SAU", Flag = "🇸🇦", Continent = "Asia", Population = 36947025, X = 61.5, Y = 44.2 },
        new Country { Id = "sg", Name = "Singapore", Code = "SGP", Flag = "🇸🇬", Continent = "Asia", Population = 6014723, X = 77.2, Y = 56.5 },
        new Country { Id = "ae", Name = "UAE", Code = "ARE", Flag = "🇦🇪", Continent = "Asia", Population = 9516871, X = 63.8, Y = 41.5 },

        // Europe
        new Country { Id = "de", Name = "Germany", Code = "DEU", Flag = "🇩🇪", Continent = "Europe", Population = 83294633, X = 50.5, Y = 27.1 },
        new Country { Id = "gb", Name = "United Kingdom", Code = "GBR", Flag = "🇬🇧", Continent = "Europe", Population = 67736802, X = 47.1, Y = 24.3 },
        new Country { Id = "fr", Name = "France", Code = "FRA", Flag = "🇫🇷", Continent = "Europe", Population = 64756584, X = 48.2, Y = 29.3 },
        new Country { Id = "it", Name = "Italy", Code = "ITA", Flag = "🇮🇹", Continent = "Europe", Population = 58870762, X = 51.2, Y = 32.3 },
        new Country { Id = "es", Name = "Spain", Code = "ESP", Flag = "🇪🇸", Continent = "Europe", Population = 47519628, X = 46.9, Y = 33.1 },
        new Country { Id = "pl", Name = "Poland", Code = "POL", Flag = "POL", Continent = "Europe", Population = 40600000, X = 53.2, Y = 26.5 },
        new Country { Id = "nl", Name = "Netherlands", Code = "NLD", Flag = "🇳🇱", Continent = "Europe", Population = 17618299, X = 48.9, Y = 25.8 },
        new Country { Id = "ch", Name = "Switzerland", Code = "CHE", Flag = "🇨🇭", Continent = "Europe", Population = 8796669, X = 49.6, Y = 29.8 },
        new Country { Id = "is", Name = "Iceland", Code = "ISL", Flag = "🇮🇸", Continent = "Europe", Population = 375318, X = 44.7, Y = 15.6 },
        new Country { Id = "va", Name = "Vatican City", Code = "VAT", Flag = "🇻🇦", Continent = "Europe", Population = 764, X = 51.5, Y = 32.5 },

        // Americas
        new Country { Id = "us", Name = "United States", Code = "USA", Flag = "🇺🇸", Continent = "North America", Population = 339996563, X = 22.0, Y = 27.5 },
        new Country { Id = "ca", Name = "Canada", Code = "CAN", Flag = "🇨🇦", Continent = "North America", Population = 38781291, X = 23.7, Y = 10.8 },
        new Country { Id = "mx", Name = "Mexico", Code = "MEX", Flag = "🇲🇽", Continent = "North America", Population = 128455567, X = 18.5, Y = 39.1 },
        new Country { Id = "br", Name = "Brazil", Code = "BRA", Flag = "🇧🇷", Continent = "South America", Population = 216422446, X = 31.7, Y = 70.8 },
        new Country { Id = "ar", Name = "Argentina", Code = "ARG", Flag = "🇦🇷", Continent = "South America", Population = 45773884, X = 30.0, Y = 75.1 },
        new Country { Id = "co", Name = "Colombia", Code = "COL", Flag = "🇨🇴", Continent = "South America", Population = 52085168, X = 26.2, Y = 54.3 },
        new Country { Id = "cl", Name = "Chile", Code = "CHL", Flag = "🇨🇱", Continent = "South America", Population = 19629590, X = 28.5, Y = 83.3 },
        new Country { Id = "uy", Name = "Uruguay", Code = "URY", Flag = "🇺🇾", Continent = "South America", Population = 3423108, X = 31.8, Y = 76.5 },

        // Oceania & Eurasia
        new Country { Id = "au", Name = "Australia", Code = "AUS", Flag = "🇦🇺", Continent = "Oceania", Population = 26439111, X = 85.7, Y = 71.8 },
        new Country { Id = "nz", Name = "New Zealand", Code = "NZL", Flag = "🇳🇿", Continent = "Oceania", Population = 5228100, X = 96.4, Y = 80.4 },
        new Country { Id = "ru", Name = "Russia", Code = "RUS", Flag = "🇷🇺", Continent = "Europe/Asia", Population = 144444359, X = 73.1, Y = 13.8 },
        new Country { Id = "tr", Name = "Turkey", Code = "TUR", Flag = "🇹🇷", Continent = "Eurasia", Population = 85816199, X = 58.1, Y = 33.6 }
    };

    public static readonly IReadOnlyList<Country> AllCountries;

    static CountriesData()
    {
        var list = new List<Country>();
        foreach (var c in RawCountries)
        {
            var country = c.Clone();
            country.InitialSoldiers = CalculateSoldiers(country.Population);
            country.Soldiers = country.InitialSoldiers;
            list.Add(country);
        }
        AllCountries = list.AsReadOnly();
    }
}

