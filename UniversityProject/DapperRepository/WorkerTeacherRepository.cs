namespace Repository;
using UCore;
using Logger;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using IRepositoryAll;
public class WorkerTeacherRepository(IGetConnectionString getConnectionString, MyLogger logger)
    : IWorkerTeacherRepository
{
    private readonly string _sqlQuerySelect = @"
SELECT PersonId, Salary, CriminalRecord, MillitaryId, LevelId, PassportID, Serial, Number, FirstName, LastName, MiddleName,
BirthData, AddressID, Country, City, Street, HouseNumber
FROM view_teacher";

    private readonly string _connectionString = getConnectionString.ReturnConnectionString();

    public List<Teacher> GetForIds(List<long> ids)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var teacher = db.Query<Teacher,  MillitaryClass, Passport, Address, Teacher>(
            _sqlQuerySelect + " WHERE PersonId IN @ID",
            (teacher, millitary, passport, address) =>
            {
                teacher.Millitary = millitary;
                passport.Address = address;
                teacher.Passport = passport;
                return teacher;
            }, new{ ID = ids},
            splitOn: "MillitaryId, PassportId, AddressId").ToList();
        return teacher;
    }

    public Teacher GetForId(long id)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var teacher = db.Query<Teacher,  MillitaryClass, Passport, Address, Teacher>(
            _sqlQuerySelect + " WHERE PersonId = @ID",
            (teacher, millitary, passport, address) =>
            {
                teacher.Millitary = millitary;
                passport.Address = address;
                teacher.Passport = passport;
                return teacher;
            }, new{ ID = id},
            splitOn: "MillitaryId, PassportId, AddressId").FirstOrDefault();
        return teacher;
    }

    public void PrintAll()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var teachers = db.Query<Teacher,  MillitaryClass, Passport, Address, Teacher>(
            _sqlQuerySelect,
            (teacher, millitary, passport, address) =>
            {
                teacher.Millitary = millitary;
                passport.Address = address;
                teacher.Passport = passport;
                return teacher;
            },
            splitOn: "MillitaryId, PassportId, AddressId").ToList();
        foreach (var teacher in teachers)
        {
            teacher.PrintInfo(logger);
        }
    }
    public long Create(Teacher teacher)
    {
        var passport = teacher.Passport;
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
            teacher.PassportId = db.Query<long>(sqlQuery, passport, transaction).First();
            sqlQuery = @"
                INSERT INTO Teacher(Salary, CriminalRecord, PassportId, MilitaryId)
                    VALUES(@Salary,
                        @CriminalRecord,
                        @PassportId,
                        @MillitaryId)
                 SELECT SCOPE_IDENTITY()";
            teacher.TeacherId = db.Query<long>(sqlQuery, teacher, transaction).First();
            transaction.Commit();
            return teacher.TeacherId;
        }
        catch(Exception ex)
        {
            logger.Error("An error occured during transaction" + ex.Message, "DapperRepository:WorkerTeacherRepository");
            transaction.Rollback();
            throw;
        }
    }
    public List<Teacher> ReturnList()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var teachers = db.Query<Teacher,  MillitaryClass, Passport, Address, Teacher>(
            _sqlQuerySelect,
            (teacher, millitary, passport, address) =>
            {
                teacher.Millitary = millitary;
                passport.Address = address;
                teacher.Passport = passport;
                return teacher;
            },
            splitOn: "MillitaryId, PassportId, AddressId").ToList();
        return teachers;
    }
    public void Delete(long Id)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        db.Execute("DELETE FROM Teacher WHERE ID = @ID", new { ID = Id});
        logger.Info("Delete administrator - " + Id, "DapperRepository:WorkerTeacherRepository");
    }
    public long Update(Teacher teacher)
    {
        var passport = teacher.Passport;
        var address = passport.Address;
        using IDbConnection db = new SqlConnection(_connectionString);
        db.Open();
        using IDbTransaction transaction = db.BeginTransaction();
        try
        {
            string sqlQuery = @"SELECT PassportID FROM Teacher WHERE Id = @PersonID";
            passport.PassportId = db.Query<int>(sqlQuery, teacher, transaction).First();
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
            sqlQuery = @"UPDATE Teacher
                    SET Salary = @Salary, MilitaryId = @MillitaryId, CriminalRecord = @CriminalRecord
                    WHERE ID = @PersonId";
            db.Execute(sqlQuery, teacher, transaction);
            transaction.Commit();
            return teacher.TeacherId;
        }
        catch(Exception ex)
        {
            logger.Error("An error occured during transaction" + ex.Message, "DapperRepository:WorkerTeacherRepository");
            transaction.Rollback();
            throw;
        }
    }
}