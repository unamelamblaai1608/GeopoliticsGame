using System.Text.Json.Serialization;

namespace Geopolitics.Core.Models;

public class Country
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("flag")]
    public string Flag { get; set; } = string.Empty;

    [JsonPropertyName("continent")]
    public string Continent { get; set; } = string.Empty;

    [JsonPropertyName("population")]
    public long Population { get; set; }

    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }

    [JsonPropertyName("initialSoldiers")]
    public int InitialSoldiers { get; set; }

    [JsonPropertyName("soldiers")]
    public int Soldiers { get; set; }

    [JsonPropertyName("ownerId")]
    public string? OwnerId { get; set; }

    [JsonPropertyName("startingOwnerId")]
    public string? StartingOwnerId { get; set; }

    public Country Clone()
    {
        return new Country
        {
            Id = Id,
            Name = Name,
            Code = Code,
            Flag = Flag,
            Continent = Continent,
            Population = Population,
            X = X,
            Y = Y,
            InitialSoldiers = InitialSoldiers,
            Soldiers = Soldiers,
            OwnerId = OwnerId,
            StartingOwnerId = StartingOwnerId
        };
    }
}

