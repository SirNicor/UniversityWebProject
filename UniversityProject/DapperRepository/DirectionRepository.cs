namespace Repository;
using UCore;
using Logger;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using IRepositoryAll;
public class DirectionRepository(IGetConnectionString getConnectionString, MyLogger logger) : IDirectionRepository
{
    private readonly string _connectionString = getConnectionString.ReturnConnectionString();
    private readonly MyLogger _logger = logger;

    private const string SqlSelectDirectionQuery =
        @"SELECT  DirectionId,  NumberOfCourse, NameDirection, 
                ChatId, DepartmentId, NameDepartment, FacultyId, 
                NameFaculty, UniversityId, Budget, NameUniversity
        FROM view_direction";
    private const string SqlSelectStudentOfDirectionQuery =
        @"SELECT DirectionId, PersonId, SkipHours,CountOfExamsPassed, 
                CreditScores, LevelDegrees,MilitaryIdAvailability, PassportID,
                Serial,Number,FirstName,LastName,MiddleName,BirthData,
                AddressID, Country,City,Street, HouseNumber 
        FROM view_studentOfDirectionQuery";

    private const string SqlSelectDisciplineOfDirectionQuery =
        @"SELECT DoD.DirectionId, DoD.DisciplineId, ds.NameDiscipline 
FROM DisciplineOfDirection DoD
JOIN Discipline ds ON ds.Id = DoD.DisciplineId";

    public long Create(DirectionDto direction)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        db.Open();
        using IDbTransaction transaction = db.BeginTransaction();
        try
        {
            var sqlQuery = @"
    INSERT INTO Direction (DepartmentId, DegreesStudyId, NameDirection, ChatId)
    OUTPUT INSERTED.ID
    VALUES (@DepartmentId, (SELECT Id FROM DegreesStudy WHERE LevelDegrees = @DegreesStudy), @NameDirection, @ChatId)";
            long id = db.QuerySingle<long>(sqlQuery, new
            {
                direction.DepartmentId, DegreesStudy = direction.DegreesStudy.ToString(),
                direction.NameDirection, direction.ChatId
            }, transaction);
            transaction.Commit();
            return id;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public List<Direction> GetForIds(List<long> ids)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        List<StudentOfDirectionDto> students = db.Query<StudentOfDirectionDto, Student, Passport, Address, StudentOfDirectionDto>(
            SqlSelectStudentOfDirectionQuery + "WHERE DirectionId IN @Id",
            (studentOfDirectionDto, student, passport, address) =>
            {
                passport.Address = address;
                student.Passport = passport;
                studentOfDirectionDto.Student = student;
                return studentOfDirectionDto;
            },
            new { Id = ids }, splitOn: "PersonId,PassportID,AddressID").ToList();
        List<DisciplineOfDirectionDto> disciplines = db.Query<DisciplineOfDirectionDto>(SqlSelectDisciplineOfDirectionQuery + " WHERE DoD.DirectionId IN @Id", new { Id = ids }).ToList();
        var directions = db.Query<Direction, Department, Faculty, University, Direction>(SqlSelectDirectionQuery + "WHERE DirectionId IN @Id",
            (direction, department, faculty, university) =>
            {
                faculty.University = university;
                department.Faculty = faculty;
                direction.Department = department;
                return direction;
            },
            new{ Id = ids}, splitOn: "DirectionId,DepartmentId,FacultyId,UniversityId").ToList();
        var dirStudents = students.GroupBy(x => x.DirectionId)
            .ToDictionary(x => x.Key, x => x.Select(x1 => x1.Student).ToList());
        var dirDisciplines = disciplines.GroupBy(x => x.DirectionId)
            .ToDictionary(x => x.Key, x => x.Select(x1 => x1.Discipline).ToList());
        foreach (var direction in directions)
        {
            direction.Students = dirStudents.GetValueOrDefault(direction.DirectionId);
            direction.Disciplines = dirDisciplines.GetValueOrDefault(direction.DirectionId);
        }
        return directions;
    }

    public Direction GetForId(long id)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        List<Student> student =  db.Query<Student, Passport, Address, Student>(
            SqlSelectStudentOfDirectionQuery + "WHERE DirectionId = @Id",
            (student, passport, address) =>
            {
                passport.Address = address;
                student.Passport = passport;
                return student;
            },
            new { Id = id }, splitOn: "PassportID,AddressID").ToList();
        List<Discipline> disciplines = db.Query<Discipline>(SqlSelectDisciplineOfDirectionQuery + " WHERE DoD.DirectionId IN @Id", new { Id = id }).ToList();
        var direction = db.Query<Direction, Department, Faculty, University, Direction>(SqlSelectDirectionQuery + "WHERE DirectionId = @Id",
            (direction, department, faculty, university) =>
            {
                faculty.University = university;
                department.Faculty = faculty;
                direction.Department = department;
                return direction;
            },
            new{ Id = id}, splitOn: "DirectionId,DepartmentId,FacultyId,UniversityId").First();
        direction.Disciplines = disciplines;
        direction.Students = student;
        return direction;
    }

    public List<Direction> ReturnList()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        List<StudentOfDirectionDto> students = db.Query<StudentOfDirectionDto, Student, Passport, Address, StudentOfDirectionDto>(SqlSelectStudentOfDirectionQuery,
            (studentOfDirectionDto, student, passport, address) =>
            {
                passport.Address = address;
                student.Passport = passport;
                studentOfDirectionDto.Student = student;
                return studentOfDirectionDto;
            }, splitOn: "PersonId,PassportID,AddressID").ToList();
        List<DisciplineOfDirectionDto> disciplines = db.Query<DisciplineOfDirectionDto>(SqlSelectDisciplineOfDirectionQuery).ToList();
        List<Direction> directions = db.Query<Direction, Department, Faculty, University, Direction>(SqlSelectDirectionQuery,
            (direction, department, faculty, university) =>
            {
                faculty.University = university;
                department.Faculty = faculty;
                direction.Department = department;
                return direction;
            }, splitOn: "DirectionId,DepartmentId,FacultyId,UniversityId").ToList();
        var dirStudents = students.GroupBy(x => x.DirectionId)
            .ToDictionary(x => x.Key, x => x.Select(x1 => x1.Student).ToList());
        var dirDisciplines = disciplines.GroupBy(x => x.DirectionId)
            .ToDictionary(x => x.Key, x => x.Select(x1 => x1.Discipline).ToList());
        foreach (var direction in directions)
        {
            direction.Students = dirStudents.GetValueOrDefault(direction.DirectionId);
            direction.Disciplines = dirDisciplines.GetValueOrDefault(direction.DirectionId);
        }
        return directions;
    }

    public void Delete(long id)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var sqlQuery = "DELETE FROM Direction where ID = @ID;";
        db.Execute(sqlQuery, new{ ID = id});
    }

    public long Update(DirectionDto direction)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        db.Open();
        using IDbTransaction transaction = db.BeginTransaction();
        try
        {
            var sqlQuery = @"UPDATE Direction 
    SET DepartmentId = @DepartmentId, DegreesStudyId = (SELECT Id FROM DegreesStudy WHERE LevelDegrees = @DegreesStudy),
    NameDirection = @NameDirection, ChatId = @ChatId
    WHERE Id = @Id";
            long id = db.QuerySingle<long>(sqlQuery, new
            {
                direction.DepartmentId, DegreesStudy = direction.DegreesStudy.ToString(),
                direction.NameDirection, direction.ChatId, Id = direction.DirectionId
            }, transaction);
            transaction.Commit();
            return direction.DirectionId;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public long? CheckNameDirection(string nameDirection, long departmentId)
    {
        string sqlQuery = "SELECT ID FROM Direction WHERE NameDirection = @nameDirection AND DepartmentId = @departmentId";
        using IDbConnection db = new SqlConnection(_connectionString);
        var check = db.Query<long?>(sqlQuery, new {  nameDirection, departmentId }).FirstOrDefault();
        check = check == 0 ? null : check;
        return check;
    }

    public long AuthorizationVerification(long chatId)
    {
        string sqlQuery = "SELECT Id FROM Direction WHERE ChatId = @chatId";
        using IDbConnection db = new SqlConnection(_connectionString);
        var check = db.Query<long>(sqlQuery, new { chatId }).FirstOrDefault();
        return check;
    }

    public bool CheckStudent(long studentId)
    {
        string sqlQuery = "SELECT DirectionId FROM StudentOfDirection WHERE StudentId = @StudentId";
        using IDbConnection db = new SqlConnection(_connectionString);
        var check = db.Query<long?>(sqlQuery, new { StudentId = studentId }).FirstOrDefault();
        check = check == 0 ? null : check;
        return check != null;
    }
}