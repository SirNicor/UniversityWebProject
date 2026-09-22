using Dadata;
using Dadata.Model;
using Microsoft.Extensions.Configuration;

namespace Service;

public class InfoPersonGroupService : IInfoPersonGroupService
{
    private readonly string _token;
    private readonly string _secret;
    public InfoPersonGroupService(IConfiguration configuration)
    {
        _token = configuration.GetValue<string>("DaData:token");
        _secret =  configuration.GetValue<string>("DaData:secret");
    }
    public async Task<Address> AsyncCleanAddress(string address, IConfiguration configuration)
    {
        var api = new CleanClientAsync(_token, _secret);
        var result = await api.Clean<Address>(address);
        return result;
    }

    public async Task<SuggestResponse<Address>> AsyncSuggestAddress(string address, IConfiguration configuration, CancellationToken cancellationToken)
    {
        var api = new SuggestClientAsync(_token);
        var result = await api.SuggestAddress(address, 10, cancellationToken);
        return result;
    }
}