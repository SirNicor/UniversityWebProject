namespace IRepositoryAll;
using UCore;
using Logger;
using System.Collections.Generic;

public interface IStudentRepository
{
    public Task PrintAllAsync();
    public Task<List<Student>> ReturnListAsync();
    Task<long> CreateAsync(StudentDtoForPage student, CancellationToken token);
    Task<long?> UpdateAsync(StudentDtoForPage studentDto, CancellationToken token);
    Task DeleteAsync(long id, CancellationToken token);
    Task DeleteAddressAsync(long id);
    Task DeletePassportAsync(long id);
    Task<Student> GetAsync(long id);
    Task<StudentDtoForPage> GetStudentPageAsync(long studentId, CancellationToken token);
    Task<(List<StudentTableDTO>, long)> GetStudentTableDto(long firstId, long count, string? sortColumn,
        string? sortOrder, FilterDto? filter, CancellationToken token);
    Task<long> GetCountAsync(CancellationToken token);
    public Task<long?> CheckNameAsync(string firstName, string lastName);
}