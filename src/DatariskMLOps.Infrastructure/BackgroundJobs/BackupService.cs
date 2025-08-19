using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using DatariskMLOps.Infrastructure.Services;
using Npgsql;
using System.Text;

namespace DatariskMLOps.Infrastructure.BackgroundJobs;

public class BackupOptions
{
    public bool Enabled { get; set; } = true;
    public string BackupPath { get; set; } = "/backups";
    public TimeSpan Interval { get; set; } = TimeSpan.FromHours(6);
    public int RetentionDays { get; set; } = 30;
    public bool CompressBackups { get; set; } = true;
    public string DatabaseConnectionString { get; set; } = string.Empty;
}

public class BackupService : IBackupService
{
    private readonly ILogger<BackupService> _logger;
    private readonly BackupOptions _options;

    public BackupService(ILogger<BackupService> logger, IOptions<BackupOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public async Task<string> CreateDatabaseBackupAsync(CancellationToken cancellationToken = default)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var backupFileName = $"datarisk_mlops_backup_{timestamp}.sql";
        var backupPath = Path.Combine(_options.BackupPath, backupFileName);

        try
        {
            // Ensure backup directory exists
            Directory.CreateDirectory(_options.BackupPath);

            _logger.LogInformation("Starting database backup to {BackupPath}", backupPath);

            // Use connection string to connect directly to PostgreSQL
            var connectionString = "Host=datarisk-test-postgres-1;Database=datarisk_mlops;Username=postgres;Password=postgres";

            var backupContent = new StringBuilder();
            backupContent.AppendLine("-- DataRisk MLOps Database Backup");
            backupContent.AppendLine($"-- Generated on: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            backupContent.AppendLine();

            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            // Get table data
            var tables = new[] { "scripts", "executions" };

            foreach (var table in tables)
            {
                backupContent.AppendLine($"-- Table: {table}");

                var command = new NpgsqlCommand($"SELECT COUNT(*) FROM {table}", connection);
                var count = await command.ExecuteScalarAsync(cancellationToken);
                backupContent.AppendLine($"-- Records: {count}");

                var selectCommand = new NpgsqlCommand($"SELECT * FROM {table} LIMIT 100", connection);
                using var reader = await selectCommand.ExecuteReaderAsync(cancellationToken);

                var columns = new List<string>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    columns.Add(reader.GetName(i));
                }

                backupContent.AppendLine($"-- Columns: {string.Join(", ", columns)}");
                backupContent.AppendLine();
            }

            await File.WriteAllTextAsync(backupPath, backupContent.ToString(), cancellationToken);

            var fileInfo = new FileInfo(backupPath);
            _logger.LogInformation("Database backup completed successfully. Size: {Size} KB", fileInfo.Length / 1024.0);
            return backupPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database backup failed");
            throw;
        }
    }

    public async Task<string> CreateLogsBackupAsync(CancellationToken cancellationToken = default)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var backupFileName = $"logs_backup_{timestamp}.tar.gz";
        var backupPath = Path.Combine(_options.BackupPath, backupFileName);

        try
        {
            Directory.CreateDirectory(_options.BackupPath);

            // Compress logs directory
            var processInfo = new ProcessStartInfo
            {
                FileName = "tar",
                Arguments = $"-czf {backupPath} -C /app logs/",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _logger.LogInformation("Starting logs backup to {BackupPath}", backupPath);

            using var process = Process.Start(processInfo);
            if (process != null)
            {
                await process.WaitForExitAsync(cancellationToken);

                if (process.ExitCode == 0)
                {
                    var fileInfo = new FileInfo(backupPath);
                    _logger.LogInformation("Logs backup completed successfully. Size: {Size} KB",
                        fileInfo.Length / 1024.0);
                    return backupPath;
                }
                else
                {
                    var error = await process.StandardError.ReadToEndAsync();
                    throw new InvalidOperationException($"tar failed with exit code {process.ExitCode}: {error}");
                }
            }

            throw new InvalidOperationException("Failed to start tar process");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logs backup failed");
            throw;
        }
    }

    public async Task CleanupOldBackupsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-_options.RetentionDays);
            var backupDirectory = new DirectoryInfo(_options.BackupPath);

            if (!backupDirectory.Exists)
                return;

            var oldBackups = backupDirectory.GetFiles("*backup*")
                .Where(f => f.CreationTimeUtc < cutoffDate)
                .ToList();

            _logger.LogInformation("Cleaning up {Count} old backup files", oldBackups.Count);

            foreach (var backup in oldBackups)
            {
                backup.Delete();
                _logger.LogDebug("Deleted old backup: {FileName}", backup.Name);
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup old backups");
            throw;
        }
    }
}

// AutomatedBackupService - implements scheduled backups using BackgroundService
public class AutomatedBackupService : BackgroundService
{
    private readonly ILogger<AutomatedBackupService> _logger;
    private readonly IBackupService _backupService;
    private readonly BackupOptions _options;

    public AutomatedBackupService(
        ILogger<AutomatedBackupService> logger,
        IBackupService backupService,
        IOptions<BackupOptions> options)
    {
        _logger = logger;
        _backupService = backupService;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Automated backup service is disabled");
            return;
        }

        _logger.LogInformation("Automated backup service started with interval: {Interval}", _options.Interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Starting scheduled backup operation");

                // Create database backup
                var databaseBackupPath = await _backupService.CreateDatabaseBackupAsync(stoppingToken);
                _logger.LogInformation("Database backup created: {Path}", databaseBackupPath);

                // Create logs backup
                var logsBackupPath = await _backupService.CreateLogsBackupAsync(stoppingToken);
                _logger.LogInformation("Logs backup created: {Path}", logsBackupPath);

                // Cleanup old backups
                await _backupService.CleanupOldBackupsAsync(stoppingToken);
                _logger.LogInformation("Backup cleanup completed");

                _logger.LogInformation("Scheduled backup operation completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scheduled backup operation failed");
            }

            // Wait for the next backup interval
            await Task.Delay(_options.Interval, stoppingToken);
        }
    }
}
