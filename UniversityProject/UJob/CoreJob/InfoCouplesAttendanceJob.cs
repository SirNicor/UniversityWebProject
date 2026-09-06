namespace UJob;
using Repository;
using Logger;
using UCore;
using IRepositoryAll;

public class InfoCouplesAttendanceJob : IJob, IInfoCouplesAttendanceJob
{
    public InfoCouplesAttendanceJob(MyLogger myLogger, IStudentRepository studentRepository)
    {
        _myLogger = myLogger;
        _studentsRepository = studentRepository;
    }

    public async Task DoWorkAsync()
    {
        _students = await _studentsRepository.ReturnListAsync();
        long?[] couplesAttendance = new long?[_students.Count];
        int maxindex = 0, minindex = 0;
        for (int i = 0; i < _students.Count; i++)
        {
            couplesAttendance[i] = _students[i].SkipHours;
            Console.WriteLine(couplesAttendance[i]);
            if (couplesAttendance[maxindex] < couplesAttendance[i])
            {
                maxindex = i;
            }

            if (couplesAttendance[minindex] < couplesAttendance[i])
            {
                minindex = i;
            }
        }

        _myLogger.Info($"Максимально количество пропусков= {couplesAttendance[maxindex]}. Данный студент:");
        _students[maxindex].PrintDerivedClass(_myLogger);
        _myLogger.Info($"Минимальное количество пропусков = {couplesAttendance[minindex]}. Данный студент:");
        _students[minindex].PrintDerivedClass(_myLogger);
    }


    private readonly MyLogger _myLogger;
    private Timer _timer;
    private readonly IStudentRepository _studentsRepository;
    private List<Student> _students;
}