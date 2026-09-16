namespace Intertwine.Services.DTOs.Currencies;

/// <summary>
/// Describes an active currency available for Spark packages.
/// </summary>
public class CurrencyDto
{
    public string Code { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DecimalPlaces { get; set; }
}
