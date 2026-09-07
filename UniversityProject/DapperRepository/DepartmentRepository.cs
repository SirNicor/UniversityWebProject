namespace Repository;
using UCore;
using Logger;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using IRepositoryAll;

public class DepartmentRepository(IGetConnectionString getConnectionString, MyLogger logger) : IDepartmentRepository
{
    private const string SqlSelectFacultyQuery = @"SELECT dp.Id as DepartmentId, dp.NameDepartment, fc.ID AS FacultyId, fc.NameFaculty, fc.IdUniversity AS UniversityId, un.Budget FROM Department dp
JOIN Faculty fc ON fc.Id = dp.FacultyId
JOIN University un ON un.Id = fc.IdUniversity ";

    private const string SqlSelectAdministratorOfFaculty =
        $@"SELECT AOD.DepartmentId, ad.Id as PersonId, ad.Salary, ad.CriminalRecord,
        ad.MilitaryID, ad.PassportID, p.Serial, p.Number, p.FirstName, p.LastName,
        p.MiddleName, p.BirthData, p.AddressId, a.Country, a.City, a.Street, a.HouseNumber FROM AdministrationOfDepartment AOD
        JOIN Administrator ad ON ad.Id = AOD.AdministratorId
        INNER JOIN Passport p ON ad.PassportId = p.ID
        INNER JOIN Address a ON p.AddressId = a.ID
        INNER JOIN IdMilitary im ON ad.MilitaryId = im.ID ";

    private readonly string _connectionString = getConnectionString.ReturnConnectionString();

    public Department Get(long id)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        List<Administrator> administrators = db.Query<Administrator, Passport, Address, Administrator>(
            SqlSelectAdministratorOfFaculty + @"WHERE DepartmentId  = @Id",
            (administrator, passport, address) =>
            {
                passport.Address = address;
                administrator.Passport = passport;
                return administrator;
            },
            new { Id = id }, splitOn: "PassportId, AddressId").ToList();
        Department department = db.Query<Department, Faculty, University, Department>(SqlSelectFacultyQuery + @"WHERE dp.Id = @Id",
            (department, faculty, university) =>
            {
                faculty.University = university;
                department.Faculty = faculty;
                return department;
            }, new{ Id = id}, splitOn: "FacultyId, UniversityId").First();
        department.Administrators = administrators;
        return department;
    }
    public List<Department> ReturnList()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        List<Department> departments = db.Query<Department, Faculty, University, Department>(SqlSelectFacultyQuery,
            (department, faculty, university) =>
            {
                faculty.University = university;
                department.Faculty = faculty;
                return department;
            }, splitOn: "FacultyId, UniversityId").ToList();
        List<AdministrationOfDepartmentDto> administrators = db.Query<AdministrationOfDepartmentDto, Administrator, Passport, Address, AdministrationOfDepartmentDto>(
            SqlSelectAdministratorOfFaculty,    
            (administrationOfDepartmentDto, administrator, passport, address) =>
            {
                passport.Address = address;
                administrator.Passport = passport;
                administrationOfDepartmentDto.Administrator = administrator;
                return administrationOfDepartmentDto;
            }, splitOn: "PersonId, PassportId, AddressId").ToList();
        var dictionaryAdministrators = administrators
            .GroupBy(x => x.DepartmentId)
            .ToDictionary(x => x.Key, x => x.Select(d => d.Administrator).ToList());
        foreach (var department in departments)
        {
            department.Administrators = dictionaryAdministrators.GetValueOrDefault(department.DepartmentId);
        }
        return departments;
    }
    public long Create(DepartmentDto department)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        db.Open();
        using IDbTransaction transaction = db.BeginTransaction();
        try
        {
            var sqlQuery = @"INSERT INTO Department(nameDepartment, FacultyId) VALUES(@NameDepartment, @FacultyId);
                    SELECT SCOPE_IDENTITY();";
            department.DepartmentId = db.QuerySingle<int>(sqlQuery, department, transaction);
            var admin = department.IdAdministrators.Select(adminId => new
            {
                DepartmentId = department.DepartmentId,
                AdministratorsId = adminId
            }).ToList();
            sqlQuery = @"INSERT INTO AdministrationOfDepartment(DepartmentId, AdministratorId) VALUES(@DepartmentId, @AdministratorsId)";
            db.Execute(sqlQuery,  admin , transaction);
            transaction.Commit();
            return department.DepartmentId;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public void Delete(long id)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        db.Open();
        using IDbTransaction transaction = db.BeginTransaction();
        try
        {
            string sqlQuery = @"DELETE FROM Department WHERE ID = @ID";
            db.Execute(sqlQuery, new { ID = id },  transaction);
            transaction.Commit();
        }
        catch (Exception ex)
        {
            logger.Error("An error occured during transaction" + ex.Message, "DapperRepository:DepartmentRepository");
            transaction.Rollback();
        }
    }

    public long Update(DepartmentDto department)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        db.Open();
        using IDbTransaction transaction = db.BeginTransaction();
        try
        {
            var sqlQuery = @"UPDATE Department SET NameDepartment = @NameDepartment, FacultyId = @FacultyId WHERE ID = @DepartmentId";
            db.Execute(sqlQuery, department, transaction);
            sqlQuery = @"DELETE FROM AdministrationOfDepartment WHERE DepartmentId = @DepartmentId";
            db.Execute(sqlQuery, department, transaction);
            var admin = department.IdAdministrators.Select(adminId => new
            {
                DepartmentId = department.DepartmentId,
                AdministratorsId = adminId
            }).ToList();
            sqlQuery = @"INSERT INTO AdministrationOfDepartment(DepartmentId, AdministratorId) VALUES(@DepartmentId, @AdministratorsId)";
            db.Execute(sqlQuery, admin, transaction);
            transaction.Commit();
            return department.DepartmentId;
        }
        catch (Exception ex)
        {
            logger.Error("An error occured during transaction" + ex.Message, "DapperRepository:DepartmentRepository");
            transaction.Rollback();
            throw;
        }
    }

    public long? CheckNameDepartment(string nameDepartment, long facultyId)
    {
        string sqlQuery = "SELECT ID FROM Department WHERE NameDepartment = @nameDepartment AND FacultyId = @facultyId";
        using IDbConnection db = new SqlConnection(_connectionString);
        var check = db.Query<long?>(sqlQuery, new {  nameDepartment, facultyId}).FirstOrDefault();
        return check;
    }
}