using Intertwine.Domain.Abstractions;

public class Currency : ActivatableEntity
{
    public string Code { get; set; } = string.Empty;   // PHP
    public string Symbol { get; set; } = string.Empty; // ₱
    public string Name { get; set; } = string.Empty;   // Philippine Peso

    public int DecimalPlaces { get; set; } = 2;
}