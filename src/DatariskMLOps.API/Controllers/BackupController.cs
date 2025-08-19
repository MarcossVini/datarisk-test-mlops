using Microsoft.AspNetCore.Mvc;
using DatariskMLOps.Infrastructure.BackgroundJobs;
using DatariskMLOps.Infrastructure.Services;

namespace DatariskMLOps.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BackupController : ControllerBase
{
    private readonly IBackupService _backupService;
    private readonly ILogger<BackupController> _logger;

    public BackupController(IBackupService backupService, ILogger<BackupController> logger)
    {
        _backupService = backupService;
        _logger = logger;
    }

    /// <summary>
    /// Triggers a manual database backup
    /// </summary>
    /// <returns>Backup operation result</returns>
    [HttpPost("database")]
    [ProducesResponseType(typeof(BackupResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateDatabaseBackup()
    {
        try
        {
            _logger.LogInformation("Manual database backup requested");
            var backupPath = await _backupService.CreateDatabaseBackupAsync();

            return Ok(new BackupResult
            {
                Success = true,
                BackupPath = backupPath,
                Message = "Database backup created successfully",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Manual database backup failed");
            return StatusCode(500, new BackupResult
            {
                Success = false,
                Message = $"Database backup failed: {ex.Message}",
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Triggers a manual logs backup
    /// </summary>
    /// <returns>Backup operation result</returns>
    [HttpPost("logs")]
    [ProducesResponseType(typeof(BackupResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateLogsBackup()
    {
        try
        {
            _logger.LogInformation("Manual logs backup requested");
            var backupPath = await _backupService.CreateLogsBackupAsync();

            return Ok(new BackupResult
            {
                Success = true,
                BackupPath = backupPath,
                Message = "Logs backup created successfully",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Manual logs backup failed");
            return StatusCode(500, new BackupResult
            {
                Success = false,
                Message = $"Logs backup failed: {ex.Message}",
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Triggers cleanup of old backups
    /// </summary>
    /// <returns>Cleanup operation result</returns>
    [HttpPost("cleanup")]
    [ProducesResponseType(typeof(BackupResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CleanupOldBackups()
    {
        try
        {
            _logger.LogInformation("Manual backup cleanup requested");
            await _backupService.CleanupOldBackupsAsync();

            return Ok(new BackupResult
            {
                Success = true,
                Message = "Backup cleanup completed successfully",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Manual backup cleanup failed");
            return StatusCode(500, new BackupResult
            {
                Success = false,
                Message = $"Backup cleanup failed: {ex.Message}",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}

public class BackupResult
{
    public bool Success { get; set; }
    public string? BackupPath { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
