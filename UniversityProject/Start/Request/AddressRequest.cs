using IRepositoryAll;
using Logger;

namespace Start.Request;

static class AddressRequest
{
    public static void AddAddressRequest(this IEndpointRouteBuilder app, MyLogger logger, IConfiguration config)
    {
        app.MapDelete("/Address/{id}", async (int id, HttpContext context) =>
        {
            var service = context.RequestServices.GetService<IStudentRepository>();
            await service.DeleteAddressAsync(id);
        });
        app.MapGet("/Address/Suggest/{address}", async (string address, CancellationToken token, HttpContext context) =>
        {
            var suggest = FunctionForRequest.SuggestAddress(address, config, token).Result;
            logger.Info($"Suggest {suggest.suggestions}", "AddressRequest");
            await context.Response.WriteAsJsonAsync(suggest.suggestions, cancellationToken: token);
        });
        app.MapGet("/Address/Clean/{address}", async (string address, HttpContext context) =>
        {
            var clean = FunctionForRequest.CleanAddress(address, config).Result;
            await context.Response.WriteAsJsonAsync(clean);
        });
    }
}