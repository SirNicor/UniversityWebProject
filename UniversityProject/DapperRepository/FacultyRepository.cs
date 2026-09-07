namespace Repository;
using UCore;
using Logger;
using Dapper;
using System.Data;
using System.Data.SqlClient;

using IRepositoryAll;
public class FacultyRepository(IGetConnectionString getConnectionString, MyLogger logger) : IFacultyRepository
{
    private const string SqlSelectFacultyQuery = @"SELECT fc.ID AS FacultyId, fc.NameFaculty, fc.IdUniversity AS UniversityId, un.Budget, un.Budget FROM Faculty fc 
JOIN University un ON un.Id = fc.IdUniversity ";

    private const string SqlSelectAdministratorOfFaculty =
        @"SELECT ADO.IdFaculty AS FacultyId, ad.Id as PersonId, ad.Salary, ad.CriminalRecord,
        ad.MilitaryID, ad.PassportID, p.Serial, p.Number, p.FirstName, p.LastName,
        p.MiddleName, p.BirthData, p.AddressId, a.Country, a.City, a.Street, a.HouseNumber FROM AdministrationOfFaculty ADO
        JOIN Administrator ad ON ad.Id = ADO.IdAdministrator
        INNER JOIN Passport p ON ad.PassportId = p.ID
        INNER JOIN Address a ON p.AddressId = a.ID
        INNER JOIN IdMilitary im ON ad.MilitaryId = im.ID ";

    private readonly string _connectionString = getConnectionString.ReturnConnectionString();

    public long Create(FacultyDto facutlyDto)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        db.Open();
        using IDbTransaction transaction = db.BeginTransaction();
        try
        {
            var sqlQuery = @"INSERT INTO Faculty(NameFaculty, IdUniversity) VALUES(@NameFaculty, @IdUniversity);
                    SELECT SCOPE_IDENTITY();";
            facutlyDto.IdFaculty = db.QuerySingle<int>(sqlQuery, facutlyDto, transaction);
            var admin = facutlyDto.IdAdministrators.Select(adminId => new
            {
                IdFaculty = facutlyDto.IdFaculty,
                IdAdministrators = adminId
            }).ToList();
            sqlQuery = @"INSERT INTO AdministrationOfFaculty(IdFaculty, IdAdministrator) VALUES(@IdFaculty, @IdAdministrators)";
            db.Execute(sqlQuery,  admin , transaction);
            transaction.Commit();
            return facutlyDto.IdFaculty;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public Faculty Get(long id)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        List<Administrator> administrators = db.Query<Administrator, Passport, Address, Administrator>(
            SqlSelectAdministratorOfFaculty + @"WHERE IdFaculty = @Id",
            (administrator, passport, address) =>
            {
                passport.Address = address;
                administrator.Passport = passport;
                return administrator;
            },
            new { Id = id }, splitOn: "PassportId, AddressId").ToList();
        Faculty faculty = db.Query<Faculty, University, Faculty>(SqlSelectFacultyQuery + @"WHERE fc.Id = @Id",
            (faculty, university) =>
            {
                faculty.University = university;
                return faculty;
            }, new{ Id = id}, splitOn: "UniversityId").First();
        faculty.AdministrationOfFaculty = administrators;
        return faculty;
    }

    public List<Faculty> ReturnList()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        List<Faculty> faculties = db.Query<Faculty, University, Faculty>(SqlSelectFacultyQuery,
            (faculty, university) =>
            {
                faculty.University = university;
                return faculty;
            }, splitOn: "UniversityId").ToList();
        List<AdministrationOfFacultyDto> administrators = db.Query<AdministrationOfFacultyDto, Administrator, Passport, Address, AdministrationOfFacultyDto>(
            SqlSelectAdministratorOfFaculty,    
            (administrationOfFacultyDto, administrator, passport, address) =>
            {
                passport.Address = address;
                administrator.Passport = passport;
                administrationOfFacultyDto.Administrator = administrator;
                return administrationOfFacultyDto;
            }, splitOn: "PersonId, PassportId, AddressId").ToList();
        var dictionaryAdministrators = administrators
            .GroupBy(x => x.FacultyId)
            .ToDictionary(x => x.Key, x => x.Select(poF => poF.Administrator).ToList());
        foreach (var faculty in faculties)
        {
            faculty.AdministrationOfFaculty = dictionaryAdministrators.GetValueOrDefault(faculty.FacultyId);
        }
        return faculties;
    }

    public void Delete(long ID)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        db.Open();
        using IDbTransaction transaction = db.BeginTransaction();
        try
        {
            string sqlQuery = @"DELETE FROM Faculty WHERE ID = @ID";
            db.Execute(sqlQuery, new { ID },  transaction);
            transaction.Commit();
        }
        catch (Exception ex)
        {
            logger.Error("An error occured during transaction" + ex.Message, "DapperRepository:FacultyRepository");
            transaction.Rollback();
        }
    }

    public long Update(FacultyDto faculty)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        db.Open();
        using IDbTransaction transaction = db.BeginTransaction();
        try
        {
            var sqlQuery = @"UPDATE Faculty SET NameFaculty = @NameFaculty, IdUniversity = @IdUniversity WHERE ID = @IdFaculty";
            db.Execute(sqlQuery, faculty, transaction);
            sqlQuery = @"DELETE FROM AdministrationOfFaculty WHERE IdFaculty = @IdFaculty";
            db.Execute(sqlQuery, faculty, transaction);
            var admin = faculty.IdAdministrators.Select(adminId => new
            {
                IdFaculty = faculty.IdFaculty,
                IdAdministrators = adminId
            }).ToList();
            sqlQuery = @"INSERT INTO AdministrationOfFaculty (IdFaculty, IdAdministrator) VALUES (@IdFaculty, @IdAdministrators)";
            db.Execute(sqlQuery, admin, transaction);
            transaction.Commit();
            return faculty.IdFaculty;
        }
        catch (Exception ex)
        {
            logger.Error("An error occured during transaction" + ex.Message, "DapperRepository:FacultyRepository");
            transaction.Rollback();
            throw;
        }
    }

    public long? CheckNameFaculty(string nameFaculty, long universityId)
    {
        string sqlQuery = "SELECT ID FROM Faculty WHERE NameFaculty = @namefaculty AND IdUniversity = @universityId";
        using IDbConnection db = new SqlConnection(_connectionString);
        var check = db.Query<long?>(sqlQuery, new {  nameFaculty, universityId}).FirstOrDefault();
        check = check == 0 ? null : check;
        return check;
    }
}