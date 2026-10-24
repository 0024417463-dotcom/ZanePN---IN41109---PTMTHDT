namespace EduHub.Ch02.Logging;

/// <summary>Abstraction ghi log (DIP): service không biết log đi đâu.</summary>
public interface IAppLogger
{
    void Info(string message);
}
