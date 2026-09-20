namespace Repository;
using UCore;
using Logger;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using IRepositoryAll;
public class WorkerAdministratorRepository(IGetConnectionString getConnectionString, MyLogger logger)
    : IWorkerAdministratorRepository
{
    readonly string _connectionString = getConnectionString.ReturnConnectionString();
    private const string SqlQuerySelect = @"
    SELECT PersonId, Salary, CriminalRecord, MillitaryId, LevelId, PassportID, Serial, Number,
    FirstName, LastName, MiddleName, BirthData, AddressID, Country, City, Street, HouseNumber
        FROM view_administrator";

    public void PrintAll()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        List<Administrator> administrators = db.Query<Administrator, MillitaryClass, Passport, Address, Administrator>(SqlQuerySelect,
            (administrator, millitary, passport, address) =>
            {
                administrator.Millitary = millitary;
                passport.Address = address;
                administrator.Passport = passport;
                return administrator;
            }, 
            splitOn: "MillitaryId, PassportId, AddressId").ToList();
        foreach (Administrator admin in administrators)
        {
            admin.PrintDerivedClass(logger);
        }
    }
    public List<Administrator> ReturnListAdministrator()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        return db.Query<Administrator, MillitaryClass, Passport, Address, Administrator>(
            SqlQuerySelect,
            (Administrator, millitary, Passport, Address) =>
            {
                Administrator.Millitary = millitary;
                Passport.Address = Address;
                Administrator.Passport = Passport;
                return Administrator;
            }, 
            splitOn: "MillitaryId, PassportId, AddressId").ToList();
    }

    public Administrator Get(long id)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var administrator = db.Query<Administrator, MillitaryClass, Passport, Address, Administrator>(
            SqlQuerySelect + " WHERE PersonId = @ID",
            (Administrator, millitary, Passport, Address) =>
            {
                Administrator.Millitary = millitary;
                Passport.Address = Address;
                Administrator.Passport = Passport;
                return Administrator;
            }, new{ ID = id},
            splitOn: "MillitaryId, PassportId, AddressId").FirstOrDefault();
        logger.Info($"Return administrator - {administrator.Passport.Serial}, Number: {administrator.Passport.Number}", "DapperRepository:WorkerAdministratorRepository");
        return administrator;
    }   
    
    public long Create(Administrator worker)
    {
        var passport = worker.Passport;
        var address = passport.Address;
        using IDbConnection db = new SqlConnection(_connectionString);
        db.Open();
        using IDbTransaction transaction = db.BeginTransaction();
        try
        {
            var sqlQuery = @"
                INSERT INTO Address(Country, City, Street, HouseNumber)
                VALUES(@Country, @City, @Street, @HouseNumber)
                SELECT SCOPE_IDENTITY()";
            passport.AddressId = db.Query<long>(sqlQuery, address, transaction).First();
            sqlQuery = @"
                INSERT INTO Passport(Serial, Number, FirstName, LastName, MiddleName, BirthData, AddressId, PlaceReceipt)
                       VALUES(@Serial,
                           @Number,
                           @FirstName,
                           @LastName, 
                           @MiddleName, 
                           @BirthData, 
                           @AddressId, 
                           @PlaceReceipt)
                  SELECT SCOPE_IDENTITY()"; 
            worker.PassportId = db.Query<long>(sqlQuery, passport, transaction).First();
            sqlQuery = $@"
                INSERT INTO Administrator(Salary, CriminalRecord, PassportId, MilitaryId, Post)
                    VALUES(@Salary,
                        @CriminalRecord,
                        @PassportId,
                        @MillitaryId,
                        @Post)
                 SELECT SCOPE_IDENTITY()";
            worker.AdministratorId = db.Query<long>(sqlQuery, worker, transaction).First();
            transaction.Commit();
            return worker.AdministratorId;
        }
        catch(Exception ex)
        {
            logger.Error("An error occured during transaction" + ex.Message, "DapperRepository:WorkerAdministratorRepository");
            transaction.Rollback();
            throw;
        }
    }

    public long Update(Administrator administrator)
    {
        var passport = administrator.Passport;
        var address = passport.Address;
        using IDbConnection db = new SqlConnection(_connectionString);
        db.Open();
        using IDbTransaction transaction = db.BeginTransaction();
        try
        {
            string sqlQuery = @"SELECT PassportID FROM Administrator WHERE Id = @PersonID";
            passport.PassportId = db.Query<int>(sqlQuery, administrator, transaction).First();
            sqlQuery = @"SELECT AddressId FROM Passport WHERE Id = @PassportID";
            address.AddressId = db.Query<int>(sqlQuery, passport, transaction).First();
            sqlQuery = @"UPDATE Address 
                SET Country = @Country,  City = @City, Street = @Street, HouseNumber = @HouseNumber
                WHERE ID = @AddressId";
            db.Execute(sqlQuery, address , transaction);
            sqlQuery = @"
                    UPDATE PASSPORT 
                    SET Serial = @Serial, 
                        Number = @Number,  
                        FirstName = @FirstName, 
                        LastName = @LastName, 
                        MiddleName = @MiddleName, 
                        BirthData = @BirthData, 
                        PlaceReceipt = @PlaceReceipt
                    WHERE ID = @PassportID";
            db.Execute(sqlQuery, passport, transaction);
            sqlQuery = @"UPDATE Administrator
                    SET Salary = @Salary, MilitaryId = @MillitaryId, CriminalRecord = @CriminalRecord
                    WHERE ID = @PersonId";
            db.Execute(sqlQuery, administrator, transaction);
            transaction.Commit();
            return administrator.AdministratorId;
        }
        catch(Exception ex)
        {
            logger.Error("An error occured during transaction" + ex.Message, "DapperRepository:WorkerAdministratorRepository");
            transaction.Rollback();
            throw;
        }
    }
    public void Delete(long Id)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        db.Execute("DELETE FROM Administrator WHERE ID = @ID", new { ID = Id});
    }
    
}