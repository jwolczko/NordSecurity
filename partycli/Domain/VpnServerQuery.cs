namespace partycli.Domain;

public class VpnServerQuery
{
    public int? ProtocolId { get; set; }

    public int? CountryId { get; set; }

    public static VpnServerQuery All()
    {
        return new VpnServerQuery();
    }

    public static VpnServerQuery WithFilters(int? countryId, int? protocolId)
    {
        return new VpnServerQuery
        {
            CountryId = countryId,
            ProtocolId = protocolId
        };
    }
}
