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
        SELECT *
        FROM view_student";
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
            splitOn: "PassportID, AddressID, MillitaryId"
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
                splitOn: "PassportID, AddressID, MillitaryId"
                )).AsList();
    }   
    
    public async Task<Student> GetAsync(long id)
    {
        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync();
        var sqlQuery = SQlQuerySelect + " WHERE PersonId = @id";
        var students = await db.QueryAsync<Student, Passport, Address, MillitaryClass, Student>(sqlQuery,
                (student, passport, address, millitaryClass) =>
                {
                    passport.Address = address;
                    student.Passport = passport;
                    student.Millitary = millitaryClass;
                    return student;
                },
                (new { id = id }),
                splitOn: "PassportID, AddressID, MillitaryId"
            );
        return students.FirstOrDefault();
    }

    public async Task<StudentDtoForPage> GetStudentPageAsync(long studentId, CancellationToken token)
    {
        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync(token);
        const string sqlQuerySelect = @"
    SELECT 
        PersonId AS studentId,
        SkipHours,
        CountOfExamsPassed, 
        CreditScores,
        CriminalRecord,
        CourseID as course,
        LevelDegrees,
        LevelId AS MilitaryIdAvailability,
        PassportID AS passportID,
        Serial,
        Number,
        placeReceipt,
        FirstName,
        LastName,
        MiddleName,
        BirthData as dob,
        AddressID AS addressID,
        AddressString as Address,
        Country,
        City,
        Street as state,
        HouseNumber
    FROM view_student
    WHERE PersonId = @studentId;";
        return await db.QueryFirstOrDefaultAsync<StudentDtoForPage>(sqlQuerySelect, new {studentId});
    }

    public async Task<(List<StudentTableDTO>, long)> GetStudentTableDto(long FirstId, long countOfRow, string? SortColumn, string? SortOrder, 
        FilterDto? filter, CancellationToken token)
    {
        var builder = new SqlBuilder();
        SortOrder = SortOrder == "null"? "ASC" : SortOrder;
        SortColumn = SortColumn == "null" ? "studentId" : SortColumn;
        logger.Info($"GetStudentTableDto: FirstId:{FirstId},  count:{countOfRow}, sortColumn:{SortColumn}, sortOrder:{SortOrder}," +
                      $"filterCourse:{filter.FilterCourse}, BitrhDay: {filter.FilterDate[0]} {filter.FilterDate[1]}," +
                      $"filterSkipHours: {filter.FilterSkipHoursStart} {filter.FilterSkipHoursEnd}, filtertotalScore: {filter.FilterTotalScore}", "DapperRepository:StudentRepository");
        string sql = $@"SELECT 
        PersonId AS studentId,
        SkipHours,
        CountOfExamsPassed, 
        CreditScores,
        IIF(CountOfExamsPassed = 0, 0, CAST(CreditScores AS DECIMAL(18, 1)) / CountOfExamsPassed) AS TotalScore,
        CourseId as Course,
        LevelId AS MilitaryIdAvailability,
        PassportID,
        Serial,
        Number,
        CONCAT_WS(' ',FirstName, LastName, MiddleName) AS Fio,
        BirthData as Dob,
        AddressID,
        AddressString as Address,
        Country,
        City,
        Street as State,
        HouseNumber as HouseNumber
    FROM view_student
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
    FROM view_student
    /**where**/";
        if (filter.FilterCourse is not null)
        {
            long numberOfCourse = (long)filter.FilterCourse;
            builder.Where($"CourseId = {numberOfCourse}");
        }

        if (filter.FilterDate[0] != "")
        {
            builder.Where("BirthData >= @FilterBirthDayStart AND BirthData <= @FilterBirthDayEnd");
        }

        if (filter.FilterSkipHoursEnd is not null && filter.FilterSkipHoursStart is not null)
        {
            builder.Where("SkipHours >= @FilterSkipHoursStart and SkipHours <= @FilterSkipHoursEnd");
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
        var sqlQuery = SQlQuerySelect + " WHERE ChatId = @chatId";
        var student = await db.QueryAsync<Student, Passport, Address, MillitaryClass, Student>(sqlQuery,
                (student, passport, address, millitary) =>
                {
                    passport.Address = address;
                    student.Passport = passport;
                    student.Millitary = millitary;
                    return student;
                },
                (new { chatId }),
                splitOn: "PassportID, AddressID, MillitaryId"
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
    public async Task<long?> UpdateAsync(StudentDtoForPage studentDto, CancellationToken token)
    {
        await using var db = new SqlConnection(_connectionString);
        await db.OpenAsync(token);
        await using var transaction = await db.BeginTransactionAsync(token);
        try
        { 
            string sqlQuery = @"UPDATE Address 
                SET AddressString = @address, Country = @country,  City = @city, Street = @state, HouseNumber = @houseNumber
                WHERE Id = @addressId";
            await db.ExecuteAsync(sqlQuery, studentDto, transaction);
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
            await db.ExecuteAsync(sqlQuery, studentDto, transaction);
            sqlQuery = @"
                    UPDATE STUDENT 
                    SET
                        CriminalRecord = @criminalRecord, 
                        CourseId = @course, 
                        SkipHours = @skipHours,
                        CountOfExamsPassed = @countOfExamsPassed, 
                        CreditScores = @creditScores
                    WHERE ID = @studentId";
            await db.ExecuteAsync(sqlQuery, studentDto, transaction);
            await transaction.CommitAsync(token);
            return studentDto.studentId;
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