using FluentMigrator;
using FluentMigrator.Builders;

namespace Repository.Migrations;

[Migration(21, "Creating tables designed for the bulk import of data from a CSV file into student tables.")]
public class M021AddFileStudentUpload : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("FileUploadProcessing")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Status").AsString().NotNullable();
        Insert.IntoTable("FileUploadProcessing")
            .Row(new { Status = "Queued" })
            .Row(new { Status = "Processing" })
            .Row(new { Status = "Completed" })
            .Row(new { Status = "Failed" })
            .Row(new { Status = "Canceled" })
            .Row(new { Status = "RetryPending" })  
            .Row(new { Status = "Dead" }); 
        Create.Table("FileStudentUpload")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("FilePath").AsString().NotNullable().Unique()
            .WithColumn("UserId").AsInt64().NotNullable().ForeignKey("AuthorizationTable", "Id")
            .WithColumn("SizeFile").AsInt64().NotNullable()
            .WithColumn("IdFileUploadProcessing").AsInt32().NotNullable().ForeignKey("FileUploadProcessing", "Id")
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
            .WithColumn("CompletedAt").AsDateTime().Nullable()
            .WithColumn("RetryCount").AsInt16().NotNullable().WithDefaultValue(0)
            .WithColumn("ErrorsFilePath").AsString().Nullable()
            .WithColumn("HashCodeFile").AsString().Nullable();
        Create.Index("Ix_FileStudentUpload_IdFileUploadProcessing")
            .OnTable("FileStudentUpload")
            .OnColumn("IdFileUploadProcessing")
            .Ascending();
        Create.Index("Ix_FileStudentUpload_CreatedAt")
            .OnTable("FileStudentUpload")
            .OnColumn("CreatedAt").
            Descending();
        Create.Index("Ix_FileStudentUpload_UserId")
            .OnTable("FileStudentUpload")
            .OnColumn("UserId").
            Descending();
        Create.Index("Ix_FileStudentUpload_HashCodeFile")
            .OnTable("FileStudentUpload")
            .OnColumn("HashCodeFile").Ascending();
        Create.Table("FileUploadErrorsRows")
            .WithColumn("Id").AsInt64().PrimaryKey().Identity()
            .WithColumn("FileId").AsInt32().NotNullable().ForeignKey("FileStudentUpload", "Id").Indexed("Index_FileUploadErrorsRows_FileId")
            .WithColumn("ErrorString").AsString().NotNullable()
            .WithColumn("NumberRow").AsInt32().NotNullable();
    }
}   