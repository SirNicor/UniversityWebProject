namespace Repository;
using UCore;
using Logger;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using IRepositoryAll;

public class UniversityRepository(IGetConnectionString getConnectionString, MyLogger logger) : IUniversityRepository
{
    private const string SqlSelectUniversityQuery = @"SELECT un.ID as universityId, un.NameUniversity, un.Budget, ad.Id as PersonId, ad.Salary, ad.CriminalRecord,
ad.MilitaryID, ad.PassportID, p.Serial, p.Number, p.FirstName, p.LastName,
p.MiddleName, p.BirthData, p.AddressId, a.Country, a.City, a.Street, a.HouseNumber FROM University un
JOIN Administrator ad ON ad.Id = un.Rector
INNER JOIN Passport p ON ad.PassportId = p.ID
INNER JOIN Address a ON p.AddressId = a.ID
INNER JOIN IdMilitary im ON ad.MilitaryId = im.ID ";
    private const string SqlSelectPersonalOfAdministratorQuery = @"SELECT PU.IdUniversity, ad.Id as PersonId, ad.Salary, ad.CriminalRecord,
ad.MilitaryID, ad.PassportID, p.Serial, p.Number, p.FirstName, p.LastName,
p.MiddleName, p.BirthData, p.AddressId, a.Country, a.City, a.Street, a.HouseNumber FROM PersonalOfUniversity PU
JOIN Administrator ad ON ad.Id = PU.IdAdministrator
INNER JOIN Passport p ON ad.PassportId = p.ID
INNER JOIN Address a ON p.AddressId = a.ID
INNER JOIN IdMilitary im ON ad.MilitaryId = im.ID";
    private readonly string _connectionString = getConnectionString.ReturnConnectionString();

    // private const string SqlSelectIdUniversityQuery = @"Select 
    // un.Id AS ID
    // FROM University un";

    public University Get(long id)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        List<Administrator> administrators = db.Query<Administrator, Passport, Address, Administrator>(
            SqlSelectPersonalOfAdministratorQuery + @"WHERE IdUniversity = @ID", 
            (administrator, passport, address) =>
            {
                passport.Address = address;
                administrator.Passport = passport;
                return administrator;
            },
            new { ID = id }, splitOn: "PassportId, AddressId").ToList();
        University university = db.Query<University>(SqlSelectUniversityQuery + @"WHERE un.ID = @ID", new { ID = id }).First();
        university.Administrators = administrators;
        return university;
    }

    public long? CheckNameInUniversity(string nameUniversity)
    {
        string sqlQuery = "SELECT ID FROM UNIVERSITY WHERE NameUniversity = @nameUniversity";
        using IDbConnection db = new SqlConnection(_connectionString);
        var check = db.Query<long?>(sqlQuery, new {  nameUniversity }).FirstOrDefault();
        check = check == 0 ? null : check;
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