namespace partycli.Domain;

public class VpnServerQuery
{
    public int? ProtocolId { get; set; }

    public int? CountryId { get; set; }

    public int? CityId { get; set; }

    public int? RegionId { get; set; }

    public int? SpecificServerId { get; set; }

    public int? ServerGroupId { get; set; }

    public static VpnServerQuery All()
    {
        return new VpnServerQuery();
    }

    public static VpnServerQuery ByCountry(int countryId)
    {
        return new VpnServerQuery { CountryId = countryId };
    }

    public static VpnServerQuery ByProtocol(int protocolId)
    {
        return new VpnServerQuery { ProtocolId = protocolId };
    }
}
