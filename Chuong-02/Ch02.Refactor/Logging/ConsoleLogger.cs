namespace EduHub.Ch02.Logging;

public class ConsoleLogger : IAppLogger
{
    public void Log(string message) => Console.WriteLine($"[LOG] {message}");
}
