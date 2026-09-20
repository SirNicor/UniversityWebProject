using  UCore;
using Logger;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using UCore.DTO;

namespace Repository;
using IRepositoryAll;
public class ScheduleRepository(IGetConnectionString getConnectionString, MyLogger logger, 
    IDirectionRepository directionRepository, IWorkerTeacherRepository workerTeacherRepository, IDisciplineRepository disciplineRepository) : IScheduleRepository
{
    private readonly string _connectionString = getConnectionString.ReturnConnectionString();

    private const string QueryScheduleGet =
        @"Select sc.Id, dw.DataWeek, dc.StartCouple, dc.EndCouple, sc.DirectionId, sc.DisciplineId, sc.TeacherId
        FROM Schedule sc
        JOIN DataWeekForSchedule dw ON sc.DataWeekForScheduleId = dw.Id
        JOIN DataСoupleForSchedule dc ON dc.Id = sc.DataСoupleForScheduleId";
    const string SqlQuery = @"SELECT Id, DataWeekForScheduleId, DataWeek, 
       DataСoupleForScheduleId, StartCouple, EndCouple, DirectionId, NumberOfCourse, NameDirection, 
       ChatId, DepartmentId, NameDepartment, FacultyId, 
       NameFaculty, UniversityId, Budget, NameUniversity,
       DisciplineId, NameDiscipline, PersonId, Salary,
       CriminalRecord, MilitaryIdAvailability, PassportID,
       Serial, Number, FirstName, LastName, MiddleName, BirthData,
       AddressID, Country, City, Street, HouseNumber
FROM view_fullInfoAboutSchedule";

    public long Create(ScheduleDto schedule)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        db.Open();
        using IDbTransaction transaction = db.BeginTransaction();
        try
        {
            var sqlQuery = @"
    INSERT INTO Schedule (DirectionId, DisciplineId, TeacherId, DataWeekForScheduleId, DataСoupleForScheduleId)
    OUTPUT INSERTED.ID
    VALUES (@DirectionId, @DisciplineId, @TeacherId, 
            (SELECT ID FROM DataWeekForSchedule WHERE DaysOfWeek = @DataWeek),
            (SELECT ID FROM DataСoupleForSchedule WHERE StartCouple = @StartCouple AND EndCouple = @EndCouple))";
            long id = db.QuerySingle<long>(sqlQuery, new
            {
                schedule.DirectionId, schedule.DisciplineId, schedule.TeacherId,
                DataWeek = schedule.DataWeek.ToString(), StartCouple = schedule.StartCouple, 
                EndCouple = schedule.EndCouple
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

    public Schedule Get(long id)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        db.Open();
        ScheduleDtoForRead scheduleDtoForRead = db.Query<ScheduleDtoForRead>(QueryScheduleGet + "WHERE Id = @Id", new { Id = id }).First();
        Direction direction = directionRepository.GetForId(scheduleDtoForRead.DirectionId);
        Teacher teacher = workerTeacherRepository.GetForId(scheduleDtoForRead.TeacherId);
        Discipline discipline = disciplineRepository.GetForId(scheduleDtoForRead.DisciplineId);
        Schedule schedule = new Schedule()
        {
            Id =  scheduleDtoForRead.Id,
            DataWeek = scheduleDtoForRead.DataWeek,
            StartCouple = scheduleDtoForRead.StartCouple,
            EndCouple = scheduleDtoForRead.EndCouple
        };
        schedule.Discipline = discipline;
        schedule.Teacher = teacher;
        return schedule;
    }

    public List<Schedule> ReturnList()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        db.Open();
        var scheduleDtoForRead = db.Query<ScheduleDtoForRead>(QueryScheduleGet).ToList();
        long length = scheduleDtoForRead.Count;
        
        var directionIds = scheduleDtoForRead.Select(s => s.DirectionId).Distinct().ToList();
        var disciplineIds = scheduleDtoForRead.Select(s => s.DisciplineId).Distinct().ToList();
        var teacherIds = scheduleDtoForRead.Select(s => s.TeacherId).Distinct().ToList();
        
        var direction = directionRepository.GetForIds(directionIds.ToList());
        var teacher = workerTeacherRepository.GetForIds(teacherIds.ToList());
        var discipline = disciplineRepository.GetForIds(disciplineIds.ToList());
        
        var dirDict = direction.ToDictionary(d => d.DirectionId);
        var discDict = discipline.ToDictionary(d => d.DisciplineId);
        var teachDict = teacher.ToDictionary(t => t.TeacherId);
        
        var schedules = scheduleDtoForRead.Select(dto => new Schedule
        {
            Id = dto.Id,
            DataWeek = dto.DataWeek,
            StartCouple = dto.StartCouple,
            EndCouple = dto.EndCouple,
            Direction = dirDict.GetValueOrDefault(dto.DirectionId),
            Discipline = discDict.GetValueOrDefault(dto.DisciplineId),
            Teacher = teachDict.GetValueOrDefault(dto.TeacherId)
        }).ToList();
        return schedules;
    }
    public List<Schedule> ReturnListForDirectionId(long dirId)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        db.Open();
        List<Schedule> schedules = db.Query<Schedule, Direction, Department, Faculty, University, Discipline, Schedule>(
            SqlQuery + " WHERE dr.Id = @dirId", (schedule, direction, department, faculty, universtity, discipline) =>
            {
                faculty.University = universtity;
                department.Faculty = faculty;
                direction.Department = department;
                schedule.Direction = direction;
                schedule.Discipline = discipline;
                return schedule;
            }, new {dirId} , splitOn: "DirectionId,DepartmentId,FacultyId,UniversityId, DisciplineId").AsList();
        List<TeacherOfScheduleDTO> teachers = db.Query<TeacherOfScheduleDTO, Teacher, Passport, Address, TeacherOfScheduleDTO>(
            SqlQuery + " WHERE dr.Id = @dirId",
            (teacherDto, teacher, passport, address) =>
            {
                passport.Address = address;
                teacher.Passport = passport;
                teacherDto.Teacher = teacher;
                return teacherDto;
            }, new { dirId }, splitOn: "TeacherId, PassportId, AddressId").ToList();
        var teachersDir = teachers.GroupBy(x => x.Id)
            .ToDictionary(x => x.Key, x => x.Select(teachers => teachers.Teacher).First());
        foreach (var schedule in schedules)
        {
            schedule.Teacher = teachersDir.GetValueOrDefault(schedule.Id);
        }
        return schedules;
    }
    public void Delete(long ID)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        var sqlQuery = "DELETE FROM Schedule where ID = @ID;";
        db.Execute(sqlQuery, new{ID});
    }

    public long Update(ScheduleDto schedule)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        db.Open();
        using IDbTransaction transaction = db.BeginTransaction();
        try
        {
            var sqlQuery = @"
    UPDATE Schedule SET DirectionId = @DirectionId, DisciplineId = @DisciplineId, TeacherId = @TeacherId,
                        DataWeekForScheduleId = (SELECT ID FROM DataWeekForSchedule WHERE DaysOfWeek = @DataWeek),
                        DataСoupleForScheduleId = (SELECT ID FROM DataСoupleForSchedule WHERE StartCouple = @StartCouple AND EndCouple = @EndCouple)
                        WHERE ID = @Id";
            db.Execute(sqlQuery, new
            {
                schedule.DirectionId, schedule.DisciplineId, schedule.TeacherId,
                DataWeek = schedule.DataWeek.ToString(), StartCouple = schedule.StartCouple, 
                EndCouple = schedule.EndCouple, schedule.Id
            }, transaction);
            transaction.Commit();
            return schedule.Id;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Console.WriteLine(ex.Message);
            throw;
        }
    }
}