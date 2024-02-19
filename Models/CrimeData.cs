using Newtonsoft.Json;

namespace WPCTest.Models;

/// <summary>
/// Represents a summary of crimes for a specific category.
/// </summary>
public class CrimeCategorySummary
{
    public string? Category { get; set; }
    public List<CrimeData> Crimes { get; set; } = [];
}

public class CrimeData
{
    public string? Category { get; set; }
    [JsonProperty(PropertyName = "location_type")]
    public string? LocationType { get; set; }
    public Location? Location { get; set; }
    public string? Month { get; set; }
    [JsonProperty(PropertyName = "outcome_status")]
    public OutcomeStatus? OutcomeStatus { get; set; }
}

public class Location
{
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
    public Street? Street { get; set; }
}

public class Street
{
    public int? Id { get; set; }
    public string? Name { get; set; }
}

public class OutcomeStatus
{
    public string? Category { get; set; }
    public string? Date { get; set; }
}
