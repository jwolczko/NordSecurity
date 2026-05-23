namespace partycli.Application.Interfaces;

public interface IServerFilterCatalog
{
    bool TryGetCountryId(string countryName, out int countryId);

    bool TryGetProtocolId(string protocolName, out int protocolId);

    string GetSupportedCountries();

    string GetSupportedProtocols();
}
