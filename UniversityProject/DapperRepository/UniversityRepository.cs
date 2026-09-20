namespace Repository;
using UCore;
using Logger;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using IRepositoryAll;

public class UniversityRepository(IGetConnectionString getConnectionString, MyLogger logger) : IUniversityRepository
{
    private const string SqlSelectUniversityQuery = @"SELECT universityId, NameUniversity, Budget
    FROM view_university";
    private const string SqlSelectPersonalOfAdministratorQuery = @"SELECT UniversityId, PersonId, Salary, CriminalRecord,
                MilitaryID, PassportID, Serial, Number, FirstName, LastName,
                MiddleName, BirthData, AddressId, Country, City, Street, HouseNumber
    FROM view_personalOfUniversity";
    private readonly string _connectionString = getConnectionString.ReturnConnectionString();

    // private const string SqlSelectIdUniversityQuery = @"Select 
    // un.Id AS ID
    // FROM University un";

    public University Get(long id)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        List<Administrator> administrators = db.Query<Administrator, Passport, Address, Administrator>(
            SqlSelectPersonalOfAdministratorQuery + @"WHERE UniversityId = @ID", 
            (administrator, passport, address) =>
            {
                passport.Address = address;
                administrator.Passport = passport;
                return administrator;
            },
            new { ID = id }).ToList();
        University university = db.Query<University>(SqlSelectUniversityQuery + @"WHERE universityId = @ID", new { ID = id }).First();
        university.Administrators = administrators;
        return university;
    }

    public long? CheckNameInUniversity(string nameUniversity)
    {
        string sqlQuery = "SELECT ID FROM UNIVERSITY WHERE NameUniversity = @nameUniversity";
        using IDbConnection db = new SqlConnection(_connectionString);
        var check = db.Query<long?>(sqlQuery, new {  nameUniversity }).FirstOrDefault();
        return check;
    }

    public List<University> ReturnList()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        List<University> universities = db.Query<University>(SqlSelectUniversityQuery).ToList();
        var personal = db.Query<PersonalOfUniversiyDTO, Administrator, Passport, Address, PersonalOfUniversiyDTO>(
            SqlSelectPersonalOfAdministratorQuery, 
            (personalOfUniversity, administrator, passport, address) =>
            {
                passport.Address = address;
                administrator.Passport = passport;
                personalOfUniversity.Administrator = administrator;
                return personalOfUniversity;
            }, splitOn: "PersonId, PassportId, AddressId").ToList();
            
        var personalOfUniversity = personal
            .GroupBy(poF => poF.IdUniversity)
            .ToDictionary(x => x.Key, 
                x => x.Select(poF => poF.Administrator).ToList());
        foreach (var university in universities)
        {
            university.Administrators = personalOfUniversity.GetValueOrDefault(university.UniversityId);
        }
        return universities;
    }
    public long Create(UniversityDto university)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        db.Open();
        using IDbTransaction transaction = db.BeginTransaction();
        try
        {
            var sqlQuery = @"INSERT INTO UNIVERSITY(NameUniversity, Budget) VALUES(@NameUniversity, @BudgetSize);
                    SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";
            university.IdUniversity = db.QuerySingle<int>(sqlQuery, university, transaction);
            var admin = university.IdAdministrators.Select(adminId => new
            {
                IdUniversity = university.IdUniversity,
                IdAdministrators = adminId
            }).ToList();
            sqlQuery = @"INSERT INTO PersonalOfUniversity(IdUniversity, IdAdministrator) VALUES(@IdUniversity, @IdAdministrators)";
            db.Execute(sqlQuery,  admin , transaction);
            transaction.Commit();
            return university.IdUniversity;
        }
        catch (Exception ex)
        {
            logger.Error("An error occured during transaction" + ex.Message, "DapperRepository:UniversityRepository");
            transaction.Rollback();
            throw;
        }
    }

    public long Update(UniversityDto university)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        db.Open();
        using IDbTransaction transaction = db.BeginTransaction();
        try
        {
            var sqlQuery = @"UPDATE UNIVERSITY SET NameUniversity = @NameUniversity, Budget = @Budget WHERE ID = @IdUniversity";
            db.Execute(sqlQuery, university, transaction);
            sqlQuery = @"DELETE FROM PersonalOfUniversity WHERE IdUniversity = @IdUniversity";
            db.Execute(sqlQuery, university, transaction);
            var admin = university.IdAdministrators.Select(adminId => new
            {
                IdUniversity = university.IdUniversity,
                IdAdministrators = adminId
            }).ToList();
            sqlQuery = @"INSERT INTO PersonalOfUniversity(IdUniversity, IdAdministrator) VALUES(@IdUniversity, @IdAdministrators)";
            db.Execute(sqlQuery,  admin , transaction);
            transaction.Commit();
            return university.IdUniversity;
        }
        catch (Exception ex)
        {
            logger.Error("An error occured during transaction" + ex.Message, "DapperRepository:UniversityRepository");
            transaction.Rollback();
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
            string sqlQuery = @"DELETE FROM University WHERE ID = @ID";
            db.Execute(sqlQuery, new { ID = id },  transaction);
            transaction.Commit();
        }
        catch (Exception ex)
        {
            logger.Error("An error occured during transaction" + ex.Message, "DapperRepository:UniversityRepository");
            transaction.Rollback();
        }
    }
    
}