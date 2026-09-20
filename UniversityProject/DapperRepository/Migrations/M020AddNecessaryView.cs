using FluentMigrator;

namespace Repository.Migrations;

[Migration(20, "Create view for department table")]
public class M020AddNecessaryView  :Migration
{
    public override void Up()
    {
        Execute.Sql(
            @"CREATE VIEW view_department AS
                (SELECT AOD.DepartmentId, ad.Id as PersonId, ad.Salary, ad.CriminalRecord,
                ad.MilitaryID, ad.PassportID, p.Serial, p.Number, p.FirstName, p.LastName,
                    p.MiddleName, p.BirthData, p.AddressId AS AddressId, a.Country, a.City, a.Street, a.HouseNumber 
                FROM AdministrationOfDepartment AOD
                JOIN Administrator ad ON ad.Id = AOD.AdministratorId
                INNER JOIN Passport p ON ad.PassportId = p.ID
                INNER JOIN Address a ON p.AddressId = a.ID  
                INNER JOIN IdMilitary im ON ad.MilitaryId = im.ID )");
        Execute.Sql(
            @"CREATE VIEW view_studentOfDirectionQuery AS
                (SELECT SoD.DirectionId, s.Id AS PersonId,s.SkipHours,s.CountOfExamsPassed, 
                s.CreditScores,ds.LevelDegrees,im.LevelId AS MilitaryIdAvailability,p.ID AS PassportID,
                p.Serial,p.Number,p.FirstName,p.LastName,p.MiddleName,p.BirthData,
                a.ID AS AddressID, a.Country,a.City,a.Street,a.HouseNumber 
                FROM StudentOfDirection SoD
                INNER JOIN Student s ON s.Id = SoD.StudentId
                INNER JOIN Passport p ON s.PassportId = p.ID
                INNER JOIN Address a ON p.AddressId = a.ID
                INNER JOIN DegreesStudy ds ON s.CourseId = ds.ID
                INNER JOIN IdMilitary im ON s.MilitaryId = im.ID )");
        Execute.Sql(
            @"CREATE VIEW view_direction AS
                SELECT dr.Id AS DirectionId, dr.DegreesStudyId as NumberOfCourse, dr.NameDirection, 
                dr.ChatId, dp.Id as DepartmentId, dp.NameDepartment, fc.ID AS FacultyId, 
                fc.NameFaculty, fc.IdUniversity AS UniversityId, un.Budget, un.NameUniversity FROM Direction dr
                JOIN Department dp ON dp.Id = dr.DepartmentId
                JOIN Faculty fc ON fc.Id = dp.FacultyId
                JOIN University un ON un.Id = fc.IdUniversity ");
        Execute.Sql(
            @"CREATE VIEW view_discipline AS 
                (SELECT 
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
                INNER JOIN IdMilitary im ON tc.MilitaryId = im.ID )");
        Execute.Sql(
            @"CREATE VIEW view_faculty AS
                (SELECT ADO.IdFaculty AS FacultyId, ad.Id as PersonId, ad.Salary, ad.CriminalRecord,
                ad.MilitaryID, ad.PassportID, p.Serial, p.Number, p.FirstName, p.LastName,
                p.MiddleName, p.BirthData, p.AddressId AS AddressId, a.Country, a.City, a.Street, a.HouseNumber FROM AdministrationOfFaculty ADO
                JOIN Administrator ad ON ad.Id = ADO.IdAdministrator
                INNER JOIN Passport p ON ad.PassportId = p.ID
                INNER JOIN Address a ON p.AddressId = a.ID
                INNER JOIN IdMilitary im ON ad.MilitaryId = im.ID )");
        Execute.Sql(
            @"CREATE VIEW view_role AS
                (SELECT 
                r.Id AS Id,
                r.Name AS NameRole,
                tor.Name AS TypeOperation,
                ap.Name AS AccessPage
                FROM Role r
                INNER JOIN RoleAccess ra ON r.Id = ra.IdRole
                INNER JOIN TypeOperationRole tor ON ra.IdTypeOperation = tor.Id
                INNER JOIN AccessPage ap ON ra.IdAccessPage = ap.Id)");
        Execute.Sql(
            @"CREATE VIEW view_university AS
                (SELECT un.ID as universityId, un.NameUniversity, un.Budget FROM University un)");
        Execute.Sql(
            @"CREATE VIEW view_personalOfUniversity AS
                (SELECT PU.IdUniversity AS UniversityId, ad.Id as PersonId, ad.Salary, ad.CriminalRecord,
                ad.MilitaryID, ad.PassportID, p.Serial, p.Number, p.FirstName, p.LastName,
                p.MiddleName, p.BirthData, p.AddressId, a.Country, a.City, a.Street, a.HouseNumber FROM PersonalOfUniversity PU
                JOIN Administrator ad ON ad.Id = PU.IdAdministrator
                INNER JOIN Passport p ON ad.PassportId = p.ID
                INNER JOIN Address a ON p.AddressId = a.ID
                INNER JOIN IdMilitary im ON ad.MilitaryId = im.ID)");
        Execute.Sql(
            @"CREATE VIEW view_administrator AS
                (SELECT 
                ad.Id AS PersonId,
                ad.Salary,
                ad.CriminalRecord,
                im.Id AS MillitaryId,
                im.LevelId AS LevelId,
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
                FROM Administrator ad
                INNER JOIN Passport p ON ad.PassportId = p.ID
                INNER JOIN Address a ON p.AddressId = a.ID
                INNER JOIN IdMilitary im ON ad.MilitaryId = im.ID)");
        Execute.Sql(
            @"CREATE VIEW view_teacher AS
                    (SELECT 
                    tc.Id AS PersonId,
                    tc.Salary,
                    tc.CriminalRecord,
                    im.Id AS MillitaryId,
                    im.LevelId AS LevelId,
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
                    FROM Teacher tc
                    INNER JOIN Passport p ON tc.PassportId = p.ID
                    INNER JOIN Address a ON p.AddressId = a.ID
                    INNER JOIN IdMilitary im ON tc.MilitaryId = im.ID)");
        Execute.Sql(@"CREATE VIEW view_fullInfoAboutSchedule AS
(SELECT sc.Id, sc.DataWeekForScheduleId, dw.DaysOfWeek AS DataWeek, 
       sc.DataСoupleForScheduleId, dc.StartCouple, dc.EndCouple, sc.DirectionId, dr.DegreesStudyId as NumberOfCourse, dr.NameDirection, 
       dr.ChatId, dp.Id as DepartmentId, dp.NameDepartment, fc.ID AS FacultyId, 
       fc.NameFaculty, fc.IdUniversity AS UniversityId, un.Budget, un.NameUniversity ,
       sc.DisciplineId, ds.NameDiscipline, sc.TeacherId,tc.Id AS PersonId, tc.Salary,
       tc.CriminalRecord, im.LevelId AS MilitaryIdAvailability, p.ID AS PassportID,
       p.Serial,p.Number, p.FirstName, p.LastName, p.MiddleName, p.BirthData,
       a.ID AS AddressID, a.Country, a.City, a.Street, a.HouseNumber
    FROM Schedule sc
    JOIN Direction dr ON dr.Id = sc.DirectionId
    JOIN Department dp ON dp.Id = dr.DepartmentId
    JOIN Faculty fc ON fc.Id = dp.FacultyId
    JOIN University un ON un.Id = fc.IdUniversity
    JOIN Discipline ds ON ds.Id = sc.DisciplineId
    JOIN Teacher tc ON tc.id = sc.TeacherId
    JOIN Passport p ON tc.PassportId = p.ID
    JOIN Address a ON p.AddressId = a.ID
    JOIN IdMilitary im ON tc.MilitaryId = im.ID
    JOIN DataWeekForSchedule dw ON dw.Id = sc.DataWeekForScheduleId
    JOIN DataСoupleForSchedule dc ON dc.Id = sc.DataСoupleForScheduleId)");
    }

    public override void Down()
    {
        Execute.Sql(
            @"DROP VIEW IF EXISTS view_department
            DROP VIEW IF EXISTS view_studentOfDirectionQuery
            DROP VIEW IF EXISTS view_direction
            DROP VIEW IF EXISTS view_discipline
            DROP VIEW IF EXISTS view_faculty
            DROP VIEW IF EXISTS view_role
            DROP VIEW IF EXISTS view_personalOfUniversity
            DROP VIEW IF EXISTS view_university
            DROP VIEW IF EXISTS view_administrator
            DROP VIEW IF EXISTS view_teacher
            DROP VIEW IF EXISTS view_fullInfoAboutSchedule");
    }
}