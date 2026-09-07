using Microsoft.Extensions.Configuration;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Logger;
namespace Repository.Migrations;

public class StartMigrations
{
    public StartMigrations(IConfiguration appConfig, MyLogger logger)
    {
        _configuration = appConfig;
        _logger = logger;
    }

    public void Start()
    {   
        _logger.Info("Migrated start", "DapperRepository:Migrations:StartMigrations");
        var serviceProvider = new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSqlServer()          
                .WithGlobalConnectionString(_configuration.GetValue<string>("ConnectionStrings"))   
                .ScanIn(typeof(M0001AddAddress).Assembly).For.Migrations())
            .BuildServiceProvider(false);
        var scope = serviceProvider.CreateAsyncScope();     
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
        _logger.Info("Migrated up", "DapperRepository:Migrations:StartMigrations");
    }
    
    private IConfiguration _configuration;
    private MyLogger _logger;
}