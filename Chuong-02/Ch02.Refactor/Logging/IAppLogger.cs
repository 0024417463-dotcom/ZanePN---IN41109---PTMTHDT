namespace EduHub.Ch02.Logging;

/// <summary>Abstraction ghi log: service phụ thuộc interface này thay vì File.AppendAllText (DIP).</summary>
public interface IAppLogger
{
    void Log(string message);
}
