using System.Text.Json.Serialization;

namespace Intertwine.Services.DTOs.Discovery;

/// <summary>Describes whether a discovery resource can currently be opened.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DiscoveryAccessState
{
    Included,
    SubscriptionRequired,
    SparksRequired,
    Unavailable
}
