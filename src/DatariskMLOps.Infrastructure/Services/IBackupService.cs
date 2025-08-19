namespace DatariskMLOps.Infrastructure.Services;

public interface IBackupService
{
    Task<string> CreateDatabaseBackupAsync(CancellationToken cancellationToken = default);
    Task<string> CreateLogsBackupAsync(CancellationToken cancellationToken = default);
    Task CleanupOldBackupsAsync(CancellationToken cancellationToken = default);
}
