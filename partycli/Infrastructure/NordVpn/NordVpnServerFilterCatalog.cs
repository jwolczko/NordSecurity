using System;
using System.Collections.Generic;
using System.Linq;
using partycli.Application.Interfaces;

namespace partycli.Infrastructure.NordVpn;

public class NordVpnServerFilterCatalog : IServerFilterCatalog
{
    private readonly IReadOnlyDictionary<string, int> countries = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        ["france"] = 74
    };

    private readonly IReadOnlyDictionary<string, int> protocols = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        ["UDP"] = 3,
        ["TCP"] = 5,
        ["NordLynx"] = 35
    };

    public bool TryGetCountryId(string countryName, out int countryId)
    {
        return countries.TryGetValue(countryName, out countryId);
    }

    public bool TryGetProtocolId(string protocolName, out int protocolId)
    {
        return protocols.TryGetValue(protocolName, out protocolId);
    }

    public string GetSupportedCountries()
    {
        return string.Join(", ", countries.Keys.OrderBy(country => country));
    }

    public string GetSupportedProtocols()
    {
        return string.Join(", ", protocols.Keys.OrderBy(protocol => protocol));
    }
}
