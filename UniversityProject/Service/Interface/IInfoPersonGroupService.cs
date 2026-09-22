using Dadata.Model;
using Microsoft.Extensions.Configuration;

namespace Service;

public interface IInfoPersonGroupService
{
    public Task<Address> AsyncCleanAddress(string address, IConfiguration configuration);

    public Task<SuggestResponse<Address>> AsyncSuggestAddress(string address, IConfiguration configuration,
        CancellationToken cancellationToken);
}