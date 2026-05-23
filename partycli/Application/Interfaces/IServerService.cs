using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using partycli.Domain;

namespace partycli.Application.Interfaces;

public interface IServerService
{
    Task<IReadOnlyList<Server>> FetchAllAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Server>> FetchByCountryAsync(int countryId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Server>> FetchByProtocolAsync(int protocolId, CancellationToken cancellationToken);

    IReadOnlyList<Server> GetLocal();
}
