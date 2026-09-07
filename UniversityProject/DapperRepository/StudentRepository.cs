namespace Repository;
using UCore;
using Logger;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using static Dapper.SqlBuilder;
using IRepositoryAll;
public class StudentRepository(IGetConnectionString getConnectionString, MyLogger logger) : IStudentRepository
{
    const string SQlQuerySelect = @"
    SELECT 
        s.Id AS PersonId,
        s.SkipHours,
        s.CountOfExamsPassed, 
        s.CreditScores,
        ds.LevelDegrees,
        im.LevelId AS LevelId,
        im.Id AS MillitaryId,
        p.ID AS PassportID,
        p.Serial,
        p.Number,
        p.FirstName,
        p.LastName,
        p.MiddleName,
        p.BirthData,
        a.ID AS AddressID,
        a.Country,
        a.City,
        a.Street,
        a.HouseNumber
    FROM Student s
    INNER JOIN Passport p ON s. PassportId = p.ID
    INNER JOIN Address a ON p.AddressId = a.ID
    INNER JOIN DegreesStudy ds ON s.CourseId = ds.ID
    INNER JOIN IdMilitary im ON s.MilitaryId = im.ID";
    readonly string _connectionString = getConnectionString.ReturnConnectionString();

    public async Task<long> CreateAsync(StudentDtoForPage student, CancellationToken token)
    {
        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync(token);
        await using var transaction = await db.BeginTransactionAsync(token);
        try
        {
            var sqlQuery = @"
                    INSERT INTO Address(AddressString, Country, City, Street, HouseNumber)
                    VALUES(@address, @country, @city, @state, @houseNumber)
                    SELECT SCOPE_IDENTITY()";
            student.addressId = await db.QueryFirstOrDefaultAsync<long>(sqlQuery, student, transaction);
            sqlQuery = @"
                    INSERT INTO Passport(Serial, Number, FirstName, LastName, MiddleName, BirthData, AddressId, PlaceReceipt)
                           VALUES(@serial,
                               @number,
                               @firstName,
                               @lastName, 
                               @middleName, 
                               @dob, 
                               @addressId, 
                               @PlaceReceipt)
                               SELECT SCOPE_IDENTITY()";
            student.passportId = await db.QueryFirstOrDefaultAsync<long>(sqlQuery, student, transaction);
            sqlQuery = @"
                    INSERT INTO Student(PassportId, militaryId, CriminalRecord, CourseId, SkipHours, CountOfExamsPassed, CreditScores)
                        VALUES(@passportId,
                            1,
                            @CriminalRecord,
                            @Course,
                            @SkipHours,
                            @CountOfExamsPassed, 
                            @CreditScores)
                            SELECT SCOPE_IDENTITY()";
            student.studentId = await db.QueryFirstOrDefaultAsync<long>(sqlQuery, student, transaction); 
            await transaction.CommitAsync(token);
            return (long)student.studentId;
        }
        catch(Exception ex)
        {
            logger.Error("An error occured during transaction" + ex.Message, "DapperRepository:StudentRepository");
            throw;
        }
    }

    public async Task PrintAllAsync()
    {
        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync();
        List<Student> students = (await db.QueryAsync<Student, Passport, Address, MillitaryClass, Student>(SQlQuerySelect,
            (student, passport, address, millitaryClass) =>
            {
                passport.Address = address;
                student.Passport = passport;
                student.Millitary = millitaryClass;
                return student;
            },
            splitOn: "PassportID, AddressID"
        )).AsList();
        foreach (var student in students)
        {
            student.PrintDerivedClass(logger);
        }
    }

    public async Task<List<Student>> ReturnListAsync()
    {
        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync();
        return (await db.QueryAsync<Student, Passport, Address, MillitaryClass, Student>(SQlQuerySelect,
                (student, passport, address, millitaryClass) =>
                {
                    passport.Address = address;
                    student.Passport = passport;
                    student.Millitary = millitaryClass;
                    return student;
                },
                splitOn: "PassportID, AddressID"
                )).AsList();
    }   
    
    public async Task<Student> GetAsync(long id)
    {
        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync();
        var sqlQuery = SQlQuerySelect + " WHERE s.ID = @id";
        var students = await db.QueryAsync<Student, Passport, Address, MillitaryClass, Student>(sqlQuery,
                (student, passport, address, millitaryClass) =>
                {
                    passport.Address = address;
                    student.Passport = passport;
                    student.Millitary = millitaryClass;
                    return student;
                },
                (new { id = id }),
                splitOn: "PassportID, AddressID"
            );
        return students.FirstOrDefault();
    }

    public async Task<StudentDtoForPage> GetStudentPageAsync(long studentId, CancellationToken token)
    {
        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync(token);
        const string sqlQuerySelect = @"
    SELECT 
        s.Id AS studentId,
        s.SkipHours,
        s.CountOfExamsPassed, 
        s.CreditScores,
        s.CriminalRecord,
        s.CourseID as course,
        ds.LevelDegrees,
        im.LevelId AS MilitaryIdAvailability,
        p.ID AS passportID,
        p.Serial,
        p.Number,
        p.placeReceipt,
        p.FirstName,
        p.LastName,
        p.MiddleName,
        p.BirthData as dob,
        a.ID AS addressID,
        a.AddressString as Address,
        a.Country,
        a.City,
        a.Street as state,
        a.HouseNumber
    FROM Student s
    INNER JOIN Passport p ON s.PassportId = p.ID
    INNER JOIN Address a ON p.AddressId = a.ID
    INNER JOIN DegreesStudy ds ON s.CourseId = ds.ID
    INNER JOIN IdMilitary im ON s.MilitaryId = im.ID
    WHERE s.Id = @studentId;";
        return await db.QueryFirstOrDefaultAsync<StudentDtoForPage>(sqlQuerySelect, new {studentId});
    }

    public async Task<(List<StudentTableDTO>, long)> GetStudentTableDto(long FirstId, long countOfRow, string? SortColumn, string? SortOrder, 
        FilterDto? filter, CancellationToken token)
    {
        var builder = new SqlBuilder();
        SortOrder = SortOrder ?? "ASC";
        SortColumn = SortColumn ?? "s.Id";
        SortOrder = SortOrder == "null"? "ASC" : SortOrder;
        SortColumn = SortColumn == "null" ? "s.Id" : SortColumn;
        logger.Info($"GetStudentTableDto: FirstId:{FirstId},  count:{countOfRow}, sortColumn:{SortColumn}, sortOrder:{SortOrder}," +
                      $"filterCourse:{filter.FilterCourse}, BitrhDay: {filter.FilterDate[0]} {filter.FilterDate[1]}," +
                      $"filterSkipHours: {filter.FilterSkipHoursStart} {filter.FilterSkipHoursEnd}, filtertotalScore: {filter.FilterTotalScore}", "DapperRepository:StudentRepository");
        string sql = $@"SELECT 
        s.Id AS studentId,
        s.SkipHours,
        s.CountOfExamsPassed, 
        s.CreditScores,
        IIF(s.CountOfExamsPassed = 0, 0, CAST(s.CreditScores AS DECIMAL(18, 1)) / s.CountOfExamsPassed) AS TotalScore,
        s.CourseId as Course,
        im.LevelId AS MilitaryIdAvailability,
        p.ID AS PassportID,
        p.Serial,
        p.Number,
        CONCAT_WS(' ',p.FirstName, p.LastName, p.MiddleName) AS Fio,
        p.BirthData as Dob,
        a.ID AS AddressID,
        a.AddressString as Address,
        a.Country,
        a.City,
        a.Street as State,
        a.HouseNumber as HouseNumber
    FROM Student s
    INNER JOIN Passport p ON s.PassportId = p.ID
    INNER JOIN Address a ON p.AddressId = a.ID
    INNER JOIN DegreesStudy ds ON s.CourseId = ds.ID
    INNER JOIN IdMilitary im ON s.MilitaryId = im.ID
    /**where**/
    ORDER BY {SortColumn} {SortOrder}
    OFFSET @FirstId ROWS FETCH NEXT @countOfRow ROWS ONLY";
        var template = builder.AddTemplate(sql, new
        {
            FirstId, countOfRow, FilterBirthDayStart = filter.FilterDate[0], filter.FilterCourse,
            filter.FilterSkipHoursStart, filter.FilterSkipHoursEnd, filter.FilterTotalScore,
            FilterBirthDayEnd = filter.FilterDate[1]
        });
        sql = $@"SELECT COUNT(*)
    FROM Student s
    INNER JOIN Passport p ON s.PassportId = p.ID
    INNER JOIN Address a ON p.AddressId = a.ID
    INNER JOIN DegreesStudy ds ON s.CourseId = ds.ID
    INNER JOIN IdMilitary im ON s.MilitaryId = im.ID
    /**where**/";
        if (filter.FilterCourse is not null)
        {
            long numberOfCourse = (long)filter.FilterCourse;
            builder.Where($"s.CourseId = {numberOfCourse}");
        }

        if (filter.FilterDate[0] != "")
        {
            builder.Where("p.BirthData >= @FilterBirthDayStart AND p.BirthData <= @FilterBirthDayEnd");
        }

        if (filter.FilterSkipHoursEnd is not null && filter.FilterSkipHoursStart is not null)
        {
            builder.Where("s.SkipHours >= @FilterSkipHoursStart and  s.SkipHours <= @FilterSkipHoursEnd");
        }

        if (filter.FilterTotalScore is not null)
        {
            
        }
        var templateOfPage = builder.AddTemplate(sql, new { FirstId, countOfRow, FilterBirthDayStart = filter.FilterDate[0], filter.FilterCourse,
            filter.FilterSkipHoursStart, filter.FilterSkipHoursEnd, filter.FilterTotalScore,
            FilterBirthDayEnd = filter.FilterDate[1]});
        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync(token);
        var students = (await db.QueryAsync<StudentTableDTO>(template.RawSql, template.Parameters)).AsList();
        token.ThrowIfCancellationRequested();
        var allCount = await db.QueryFirstOrDefaultAsync<long>(templateOfPage.RawSql, template.Parameters);
        token.ThrowIfCancellationRequested();
        return (students, allCount);
    }

    public async Task<long> GetCountAsync(CancellationToken token)
    {
        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync(token);
        return await db.QueryFirstOrDefaultAsync<long>("SELECT COUNT(*) FROM Student");
    }

    public async Task<Student?> GetStudentForChatIdAsync(string chatId)
    {
        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync();
        var sqlQuery = SQlQuerySelect + " WHERE s.ChatId = @chatId";
        var student = await db.QueryAsync<Student, Passport, Address, MillitaryClass, Student>(sqlQuery,
                (student, passport, address, millitary) =>
                {
                    passport.Address = address;
                    student.Passport = passport;
                    student.Millitary = millitary;
                    return student;
                },
                (new { chatId }),
                splitOn: "PassportID, AddressID"
            );
        return student.FirstOrDefault();
    }

    public async Task<long?> CheckNameAsync(string firstName, string lastName)
    {
        string sqlQuery = @"SELECT s.ID FROM Student S 
    INNER JOIN Passport p ON s.PassportId = p.ID
    WHERE p.FirstName = @firstName AND p.LastName = @lastName";
        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync();
        long? check = await db.QueryFirstOrDefaultAsync<long?>(sqlQuery, new {  firstName, lastName });
        return check;
    }
    public async Task<long?> UpdateAsync(StudentDtoForPage student, CancellationToken token)
    {
        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync(token);
        await using var transaction = await db.BeginTransactionAsync(token);
        try
        { 
            string sqlQuery = @"UPDATE Address 
                SET AddressString = @address, Country = @country,  City = @city, Street = @state, HouseNumber = @houseNumber
                WHERE Id = @addressId";
            await db.ExecuteAsync(sqlQuery, student, transaction);
            sqlQuery = @"
                    UPDATE PASSPORT 
                    SET Serial = @serial, 
                        Number = @number,  
                        FirstName = @firstName, 
                        LastName = @lastName, 
                        MiddleName = @middleName, 
                        BirthData = @dob, 
                        PlaceReceipt = @placeReceipt
                    WHERE Id = @passportId";
            await db.ExecuteAsync(sqlQuery, student, transaction);
            sqlQuery = @"
                    UPDATE STUDENT 
                    SET
                        CriminalRecord = @criminalRecord, 
                        CourseId = @course, 
                        SkipHours = @skipHours,
                        CountOfExamsPassed = @countOfExamsPassed, 
                        CreditScores = @creditScores
                    WHERE ID = @studentId";
            await db.ExecuteAsync(sqlQuery, student, transaction);
            await transaction.CommitAsync(token);
            return student.studentId;
        }
        catch (Exception ex)
        {
            logger.Error("An error occured during transaction" + ex.Message, "DapperRepository:StudentRepository");
            throw;
        }
    }

    public async Task DeleteAsync(long ID, CancellationToken token)
    {
        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync(token);
        var sqlQuery = "DELETE FROM Student where ID = @ID;";
        await db.ExecuteAsync(sqlQuery, new{ID});
    }
    
    public async Task DeleteAddressAsync(long ID)
    {
        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync();
        var sqlQuery = "DELETE FROM Address where ID = @ID;";
        await db.ExecuteAsync(sqlQuery, new{ID});
    }
    
    public async Task DeletePassportAsync(long ID)
    {
        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync();
        var sqlQuery = "DELETE FROM Passport where ID = @ID;";
        await db.ExecuteAsync(sqlQuery, new{ID});
    }
    
}