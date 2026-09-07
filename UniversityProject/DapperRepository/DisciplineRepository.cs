namespace Repository;
using IRepositoryAll;
using UCore;
using Logger;
using Dapper;
using System.Data;
using System.Data.SqlClient;
public class DisciplineRepository(IGetConnectionString getConnectionString, MyLogger logger) : IDisciplineRepository
{
    private readonly string _sqlSelectDisciplineQuery = @"SELECT Id AS DisciplineId, NameDiscipline FROM Discipline ";

    private const string SqlSelectTacherOfDisciplineQuery = @"SELECT 
        dp.DisciplineId,
        tc.Id AS PersonId,
        tc.Salary,
        tc.CriminalRecord,
        im.LevelId AS MilitaryIdAvailability,
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
    FROM TeacherOfDiscipline dp
    JOIN Teacher tc ON tc.Id = dp.TeacherId
    INNER JOIN Passport p ON tc.PassportId = p.ID
    INNER JOIN Address a ON p.AddressId = a.ID
    INNER JOIN IdMilitary im ON tc.MilitaryId = im.ID ";
    private readonly string _connectionString = getConnectionString.ReturnConnectionString();
    
    public Discipline Get(long id)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        List<Teacher> teachers = db.Query<Teacher, Passport, Address, Teacher>(
            SqlSelectTacherOfDisciplineQuery + @"WHERE dp.DisciplineId = @ID", 
            (teacher, passport, address) =>
            {
                passport.Address = address;
                teacher.Passport = passport;
                return teacher;
            },
            new { Id = id }, splitOn: "PassportId, AddressId").ToList();
        Discipline discipline = db.Query<Discipline>(_sqlSelectDisciplineQuery + @"WHERE Id = @ID", new { id }).First();
        discipline.Teachers = teachers;
        return discipline;
    }

    public List<Discipline> ReturnList()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        List<TeacherOfDisciplineDto> teachers = db.Query<TeacherOfDisciplineDto, Teacher, Passport, Address, TeacherOfDisciplineDto>(
            SqlSelectTacherOfDisciplineQuery, 
            (teacherOfDisciplineDto,teacher, passport, address) =>
            {
                passport.Address = address;
                teacher.Passport = passport;
                teacherOfDisciplineDto.Teacher = teacher;
                return teacherOfDisciplineDto;
            }, splitOn: "PersonId, PassportID, AddressID").ToList();
        List<Discipline> disciplines = db.Query<Discipline>(_sqlSelectDisciplineQuery).ToList();
        var dictionaryTeachers = teachers
            .GroupBy(t => t.DisciplineId)
            .ToDictionary(x => x.Key, x => x.Select(ToD => ToD.Teacher).ToList());
        foreach (var discipline in disciplines)
        {
            discipline.Teachers = dictionaryTeachers.GetValueOrDefault(discipline.DisciplineId);
        }
        return disciplines;
    }
    public long Create(DisciplineDto discipline)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        db.Open();
        using IDbTransaction transaction = db.BeginTransaction();
        try
        {
            var sqlQuery = @"INSERT INTO Discipline(NameDiscipline) VALUES(@NameDiscipline);
                    SELECT SCOPE_IDENTITY();";
            discipline.DisciplineId = db.QuerySingle<int>(sqlQuery, discipline, transaction);
            var teachers = discipline.TeacherId.Select(teacherId => new
            {
                DisciplineId = discipline.DisciplineId,
                TeacherId = teacherId
            }).ToList();
            sqlQuery = @"INSERT INTO TeacherOfDiscipline(DisciplineId, TeacherId) VALUES(@DisciplineId, @TeacherId)";
            db.Execute(sqlQuery,  teachers , transaction);
            transaction.Commit();
            return discipline.DisciplineId;
        }
        catch (Exception ex)
        {
            logger.Error("An error occured during transaction" + ex.Message, "DapperRepository:DisciplineRepository");
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
            string sqlQuery = @"DELETE FROM Discipline WHERE ID = @ID";
            db.Execute(sqlQuery, new { ID = id },  transaction);
            transaction.Commit();
        }
        catch (Exception ex)
        {
            logger.Error("An error occured during transaction" + ex.Message, "DapperRepository:DisciplineRepository");
            transaction.Rollback();
        }
    }

    public long Update(DisciplineDto discipline)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        db.Open();
        using IDbTransaction transaction = db.BeginTransaction();
        try
        {
            var sqlQuery = @"UPDATE Discipline SET NameDiscipline = @NameDiscipline WHERE ID = @DisciplineId";
            db.Execute(sqlQuery, discipline, transaction);
            sqlQuery = @"DELETE FROM TeacherOfDiscipline WHERE DisciplineId = @DisciplineId";
            db.Execute(sqlQuery, discipline, transaction);
            var teachers = discipline.TeacherId.Select(teacherId => new
            {
                DisciplineId = discipline.DisciplineId,
                TeacherId = teacherId
            }).ToList();
            sqlQuery = @"INSERT INTO TeacherOfDiscipline(DisciplineId, TeacherId) VALUES(@DisciplineId, @TeacherId)";
            db.Execute(sqlQuery,  teachers , transaction);
            transaction.Commit();
            return discipline.DisciplineId;
        }
        catch (Exception ex)
        {
            logger.Error("An error occured during transaction" + ex.Message, "DapperRepository:DisciplineRepository");
            transaction.Rollback();
            throw;
        }
    }
}