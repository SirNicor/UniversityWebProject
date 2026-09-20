using FluentMigrator;

namespace Repository.Migrations;

[Migration(19, "Add view for student table")]
public class M019AddViewForStudentTable : Migration
{
    public override void Up()
    {
        Execute.Sql(
            $@"CREATE VIEW view_student AS(SELECT 
        s.Id AS PersonId,
        s.SkipHours,
        s.CountOfExamsPassed, 
        s.CreditScores,
        s.CourseID,
        s.ChatId,
        s.CriminalRecord,
        ds.LevelDegrees,
        im.LevelId AS LevelId,
        im.Id AS MillitaryId,
        p.ID AS PassportID,
        p.Serial,
        p.Number,
        p.placeReceipt,
        p.FirstName,
        p.LastName,
        p.MiddleName,
        p.BirthData,
        a.ID AS AddressID,
        a.AddressString,
        a.Country,
        a.City,
        a.Street,
        a.HouseNumber
    FROM Student s
    INNER JOIN Passport p ON s. PassportId = p.ID
    INNER JOIN Address a ON p.AddressId = a.ID
    INNER JOIN DegreesStudy ds ON s.CourseId = ds.ID
    INNER JOIN IdMilitary im ON s.MilitaryId = im.ID)");
    }
    
    public override void Down()
    {
        Execute.Sql("DROP VIEW IF EXISTS view_student");
    }
}