using IRepositoryAll;
using Logger;
using Microsoft.EntityFrameworkCore;
using UCore;
using static Microsoft.EntityFrameworkCore.EF;
using System.Linq.Dynamic.Core;
using Ucore;

namespace EFRepository;

public class EfStudentRepository(MyLogger logger, UniversityDbContext db) : IStudentRepository
{
    private Dictionary<string, string> _columnMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "id", "StudentId" },
        { "dob", "Passport.BirthData" },      
        { "serial", "Passport.Serial" },      
        { "number", "Passport.Number" },
        { "address", "Passport.Address.AddressString" }, 
        { "course", "Course" },
        { "skiphours", "SkipHours" },
    };
    public async Task PrintAllAsync()
    {
        var students = await(db.Students
            .Select(s => new Student()
            {
                Passport = new Passport()
                {
                    Address = new Address()
                    {
                        AddressId = s.Passport.Address.AddressId,
                        Country = s.Passport.Address.Country,
                        City = s.Passport.Address.City,
                        Street = s.Passport.Address.Street,
                        HouseNumber = s.Passport.Address.HouseNumber,
                        AddressString = s.Passport.Address.AddressString
                    },
                    Serial = s.Passport.Serial,
                    Number = s.Passport.Number,
                    FirstName =  s.Passport.FirstName,
                    LastName =  s.Passport.LastName,
                    MiddleName =  s.Passport.MiddleName,
                    BirthData = s.Passport.BirthData,
                    AddressId = s.Passport.AddressId,
                    PlaceReceipt = s.Passport.PlaceReceipt,
                    PassportId = s.Passport.PassportId,
                },
                StudentId = s.StudentId,
                ChatId =  s.ChatId,
                CountOfExamsPassed = s.CountOfExamsPassed,
                Course = s.Course,
                CreditScores = s.CreditScores,
                CriminalRecord = s.CriminalRecord,
                MillitaryId = s.MillitaryId,
                Millitary = s.Millitary,
                PassportId = s.Passport.PassportId,
                SkipHours = s.SkipHours,
            }).ToListAsync());
        foreach (var st in students)
        {
            st.PrintDerivedClass(logger);
        }
    }

    public async Task<List<Student>> ReturnListAsync()
    {
        var students = await(db.Students
            .Select(s => new Student()
            {
                Passport = new Passport()
                {
                    Address = new Address()
                    {
                        AddressId = s.Passport.Address.AddressId,
                        Country = s.Passport.Address.Country,
                        City = s.Passport.Address.City,
                        Street = s.Passport.Address.Street,
                        HouseNumber = s.Passport.Address.HouseNumber,
                        AddressString = s.Passport.Address.AddressString
                    },
                    Serial = s.Passport.Serial,
                    Number = s.Passport.Number,
                    FirstName =  s.Passport.FirstName,
                    LastName =  s.Passport.LastName,
                    MiddleName =  s.Passport.MiddleName,
                    BirthData = s.Passport.BirthData,
                    AddressId = s.Passport.AddressId,
                    PlaceReceipt = s.Passport.PlaceReceipt,
                    PassportId = s.Passport.PassportId,
                },
                StudentId = s.StudentId,
                ChatId =  s.ChatId,
                CountOfExamsPassed = s.CountOfExamsPassed,
                Course = s.Course,
                CreditScores = s.CreditScores,
                CriminalRecord = s.CriminalRecord,
                MillitaryId = s.MillitaryId,
                Millitary = s.Millitary,
                PassportId = s.Passport.PassportId,
                SkipHours = s.SkipHours,
            }).ToListAsync());
        return students;
    }

    public async Task<long> CreateAsync(StudentDtoForPage student, CancellationToken token)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(token);
        var insertDate = ConvertEF.ConvertStudentToInsert(student);
        var studentRow = insertDate.Student;
        studentRow.Passport = insertDate.Passport;
        studentRow.Passport.Address = insertDate.Address;
        studentRow.Millitary = null;
        studentRow.MillitaryId = 1;
        db.Students.Add(studentRow);
        await db.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
        return (long)studentRow.StudentId;
    }

    public async Task<long?> UpdateAsync(StudentDtoForPage student, CancellationToken token)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(token);
        var insertDate = ConvertEF.ConvertStudentToInsert(student);
        var studentRow = insertDate.Student;
        studentRow.Passport = insertDate.Passport;
        studentRow.Passport.Address = insertDate.Address;
        db.Students.Update(studentRow);
        await db.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
        return studentRow.StudentId;
    }

    public async Task DeleteAsync(long id, CancellationToken token)
    {
        db.Students.Remove(await db.Students.FindAsync(id, token));
        await db.SaveChangesAsync(token);
    }

    public async Task DeleteAddressAsync(long id)
    {
        db.Addresses.Remove(await db.Addresses.FindAsync(id));
        await db.SaveChangesAsync();
    }

    public async Task DeletePassportAsync(long id)
    {
        db.Passports.Remove(await db.Passports.FindAsync(id));
        await db.SaveChangesAsync();
    }

    public async Task<Student> GetAsync(long id)
    {
        var student = await(db.Students
            .Where(s => s.StudentId == id)
            .Select(s => new Student()
            {
                Passport = new Passport()
                {
                    Address = new Address()
                    {
                        AddressId = s.Passport.Address.AddressId,
                        Country = s.Passport.Address.Country,
                        City = s.Passport.Address.City,
                        Street = s.Passport.Address.Street,
                        HouseNumber = s.Passport.Address.HouseNumber,
                        AddressString = s.Passport.Address.AddressString
                    },
                    Serial = s.Passport.Serial,
                    Number = s.Passport.Number,
                    FirstName =  s.Passport.FirstName,
                    LastName =  s.Passport.LastName,
                    MiddleName =  s.Passport.MiddleName,
                    BirthData = s.Passport.BirthData,
                    AddressId = s.Passport.AddressId,
                    PlaceReceipt = s.Passport.PlaceReceipt,
                    PassportId = s.Passport.PassportId,
                },
                StudentId = s.StudentId,
                ChatId =  s.ChatId,
                CountOfExamsPassed = s.CountOfExamsPassed,
                Course = s.Course,
                CreditScores = s.CreditScores,
                CriminalRecord = s.CriminalRecord,
                MillitaryId = s.MillitaryId,
                Millitary = s.Millitary,
                PassportId = s.Passport.PassportId,
                SkipHours = s.SkipHours,
            }).FirstOrDefaultAsync());
        return student;
    }

    public async Task<StudentDtoForPage> GetStudentPageAsync(long studentId, CancellationToken token)
    {
        var studentPage = await db.Students
            .Select(s => new StudentDtoForPage()
            {
                studentId = s.StudentId,
                criminalRecord = s.CriminalRecord,
                skipHours = s.SkipHours,
                creditScores = s.CreditScores,
                countOfExamsPassed = s.CountOfExamsPassed,
                course =  s.Course,
                chatId = Convert.ToInt32(s.ChatId),    
                address = s.Passport.Address.AddressString,
                addressId = s.Passport.Address.AddressId,
                firstName = s.Passport.FirstName,
                lastName = s.Passport.LastName,
                middleName = s.Passport.MiddleName,
                dob = s.Passport.BirthData,
                passportId =  s.Passport.PassportId,
                country = s.Passport.Address.Country,
                city = s.Passport.Address.City,
                state = s.Passport.Address.Street,
                houseNumber = s.Passport.Address.HouseNumber,
                serial = Convert.ToString(s.Passport.Serial),
                number = Convert.ToString(s.Passport.Number),
                placeReceipt = s.Passport.PlaceReceipt
            }).Where(s => s.studentId == studentId).FirstOrDefaultAsync(token);
        return studentPage;
    }

    public async Task<(List<StudentTableDTO>, long)> GetStudentTableDto(long firstId, long count, string? sortColumn, string? sortOrder, FilterDto? filter, CancellationToken token)
    {
        logger.Info($"EFStudentRepository, method: GetStudentTableDto, " +
                    $"order and skip information: {firstId}, {count}, {sortColumn}, {sortOrder}","EFRepository:EFStudentRepository");
        sortOrder = sortOrder == "null"? "ASC" : sortOrder;
        sortColumn = sortColumn == "null" ? "Id" : sortColumn;
        sortOrder = sortOrder.ToUpper();
        IQueryable<Student> queryable = db.Students.AsNoTracking();
        if (filter.FilterCourse is not null)
        {
            long numberOfCourse = (long)filter.FilterCourse;
            queryable = queryable.Where(student => student.Course == filter.FilterCourse);
        }

        if (filter.FilterDate[0] != "")
        {
            var filterDateStart = DateOnly.FromDateTime(Convert.ToDateTime(filter.FilterDate[0]));
            var filterDateEnd = DateOnly.FromDateTime(Convert.ToDateTime(filter.FilterDate[1]));
            queryable = queryable.Where(student => student.Passport.BirthData >= filterDateStart && student.Passport.BirthData <= filterDateEnd);    
        }

        if (filter.FilterSkipHoursEnd is not null && filter.FilterSkipHoursStart is not null)
        {
            queryable = queryable.Where(student => student.SkipHours >= filter.FilterSkipHoursStart && student.SkipHours <= filter.FilterSkipHoursEnd);
        }
        if (filter.FilterTotalScore is not null)
        {
            
        }
        var countAsync = await queryable.CountAsync(token);
        if (sortColumn.ToLower() == "fio")
        {
            queryable = sortOrder=="ASC"?
                queryable.OrderBy(s => s.Passport.FirstName + " " + s.Passport.LastName + " " + s.Passport.MiddleName)
                :queryable.OrderByDescending(s => s.Passport.FirstName + " " + s.Passport.LastName + " " + s.Passport.MiddleName);
        }
        else if(sortColumn.ToLower() == "totalscore")
        {
            queryable =  sortOrder == "ASC"?
                queryable.OrderBy(s => s.CountOfExamsPassed > 0 ? (double?)s.CreditScores / s.CountOfExamsPassed : 0)
                :queryable.OrderByDescending(s => s.CountOfExamsPassed > 0 ? (double?)s.CreditScores / s.CountOfExamsPassed : 0);
        }
        else
        {
            sortColumn = _columnMap[sortColumn];
            queryable = queryable.OrderBy($"{sortColumn} {sortOrder}");
        }
        queryable = queryable.Skip((int)(firstId)).Take((int)count);  
        var st = await (queryable.Select(s => new StudentTableDTO()
        {
            studentId = (long)s.StudentId,
            Fio = s.Passport.FirstName + " " + s.Passport.LastName + " " + s.Passport.MiddleName,
            Dob = s.Passport.BirthData,
            Address = s.Passport.Address.AddressString,
            Serial = s.Passport.Serial,
            Number = s.Passport.Number,
            TotalScore = s.CountOfExamsPassed > 0 ? (double?)s.CreditScores / s.CountOfExamsPassed : 0,
            SkipHours =  s.SkipHours??0,
            CreditScore = s.CreditScores??0,
            Course =  (int)s.Course,
            CountOfExamsPassed =  s.CountOfExamsPassed??0
        }).ToListAsync(token));
        return (st, countAsync);
    }


    public async Task<long> GetCountAsync(CancellationToken token)
    {
        return await db.Students.CountAsync(token);
    }

    public async Task<long?> CheckNameAsync(string firstName, string lastName)
    {
        var id = await(db.Students.Where(s => s.Passport.FirstName == firstName && s.Passport.LastName == lastName).Select(s => s.StudentId).FirstOrDefaultAsync());
        return id;
    }
}