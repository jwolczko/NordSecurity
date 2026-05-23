using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using partycli.Domain;

namespace partycli.Application.Interfaces;

public interface IServerRepository
{
    Task<IReadOnlyList<Server>> GetServersAsync(VpnServerQuery query, CancellationToken cancellationToken);
}
