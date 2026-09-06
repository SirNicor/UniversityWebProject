namespace UJob;
using Repository;
using Logger;
using UCore;
using IRepositoryAll;


public class ScoresOfStudentsJob : IJob, IScoresOfStudentsJob
{
    public ScoresOfStudentsJob(MyLogger myLogger, IStudentRepository studentRepository)
    {
        _myLogger = myLogger;
        _studentsRepository = studentRepository;
    }

    public async Task DoWorkAsync()
    {
        _students = await _studentsRepository.ReturnListAsync();
        double?[] scoreStudent = new double?[_students.Count];
        int maxindex = 0, minindex = 0;
        double? allscores = 0;
        for (int i = 0; i < _students.Count; i++)
        {
            scoreStudent[i] = _students[i].TotalScore;
            Console.WriteLine(scoreStudent[i]);
            if (scoreStudent[maxindex] < scoreStudent[i])
            {
                maxindex = i;
            }

            if (scoreStudent[minindex] < scoreStudent[i])
            {
                minindex = i;
            }

            allscores += scoreStudent[i];
        }


        _myLogger.Info($"Максимальные баллы = {scoreStudent[maxindex]}. Студент с данными баллами:");
        _students[maxindex].PrintDerivedClass(_myLogger);
        _myLogger.Info($"Минимальные баллы = {scoreStudent[minindex]}. Студент с данными баллами:");
        _students[minindex].PrintDerivedClass(_myLogger);
        _myLogger.Info("Средние баллы = " + allscores / _students.Count);
    }


    private readonly MyLogger _myLogger;
    private Timer _timer;
    private List<Student> _students;
    private readonly IStudentRepository _studentsRepository;
}