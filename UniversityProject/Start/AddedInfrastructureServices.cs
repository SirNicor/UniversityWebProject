using ApiTelegramBot;
using EFRepository;
using Microsoft.EntityFrameworkCore;
using Repository;
using Logger;
using UJob;
using IRepositoryAll;

namespace Start;

public static class AddedInfrastructureServices
{
    public static void AddInfrastructureServices(this IServiceCollection services, MyLogger logger, IConfiguration configuration)
    {
        services.AddDbContext<UniversityDbContext>(options => options.UseSqlServer(configuration.GetValue<string>("ConnectionStrings")));
        services.AddMiniProfiler().AddEntityFramework();
        services.AddSingleton<MyLogger>(logger);
        services.AddTransient<IGetConnectionString, GetConnectionString>();
        services.AddScoped<IScheduleUpdate, ScheduleUpdate>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IStudentRepository, EfStudentRepository>();
        services.AddScoped<IDirectionRepository, DirectionRepository>();
        services.AddScoped<IDisciplineRepository,  DisciplineRepository>();
        services.AddScoped<IFacultyRepository, FacultyRepository>();
        services.AddScoped<IUniversityRepository, UniversityRepository>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        services.AddScoped<IWorkerTeacherRepository, WorkerTeacherRepository>();
        services.AddScoped<IWorkerAdministratorRepository, WorkerAdministratorRepository>();
        services.AddScoped<IUserStateTelegramRepository, UserStateTelegramRepository>();
        services.AddTransient<IInfoCouplesAttendanceJob, InfoCouplesAttendanceJob>();
        services.AddTransient<IPrintStudentJob, PrintStudentsJob>();
        services.AddTransient<IPrintWorkersJob, PrintWorkersJob>();
        services.AddTransient<ISalaryJob, UJob.SalaryJob>();
        services.AddTransient<IScoresOfStudentsJob, ScoresOfStudentsJob>();
        services.AddScoped<IAuthorizationRepository, AuthorizationRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddTransient<ReturnListOfStudents>();
        services.AddScoped<ReturnOneStudent>();
        services.AddTransient<ReturnListAdministrator>();
        services.AddScoped<ReturnOneAdministrator>();
        services.AddTransient<ReturnListOfUniversity>();
        services.AddScoped<ReturnOneUniversity>();
        services.AddTransient<IGetToken, GetToken>();
        services.AddScoped<IStartBot, StartBot>();
        services.AddScoped<IStartFunctionalForGroup, StartFunctionalForGroup>();
        services.AddTransient<IDisciplineUpdate, DisciplineUpdate>();
        services.AddTransient<IStudentUpdate, StudentUpdate>();
        services.AddScoped<IInitializedClass, InitializedClass>();
        services.AddScoped<IRegistrationClass, RegistrationClass>();
        services.AddScoped<IRegistrationForUniversity, RegistrationForUniversity>();
        services.AddScoped<IRegistrationForDepartment, RegistrationForDepartment>();
        services.AddScoped<IRegistrationForFaculty, RegistrationForFaculty>();
        services.AddScoped<IRegistrationForDirection, RegistrationForDirection>();
        services.AddScoped<IRegistrationForLastName, RegistrationForLastName>();
        services.AddScoped<IRegistrationForFirstName, RegistrationForFirstName>();
        services.AddTransient<ICreateMessageClass, CreateMessageClass>();
        services.AddScoped<IRegistrationRepository, RegistrationRepository>();
        services.AddScoped<FunctionOfBot, FunctionOfBot>();
        services.AddScoped<CreateFileClass, CreateFileClass>();
    }
}