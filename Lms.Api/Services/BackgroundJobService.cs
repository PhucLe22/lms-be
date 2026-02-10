namespace Lms.Api.Services;

public class BackgroundJobService
{
    private readonly ILogger<BackgroundJobService> _logger;

    public BackgroundJobService(ILogger<BackgroundJobService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Simulates sending a "Course Completed" email notification.
    /// In production, this would use an email service.
    /// </summary>
    public void SendCourseCompletedEmail(string userEmail, string courseName)
    {
        _logger.LogInformation(
            "[BackgroundJob] Sending 'Course Completed' email to {Email} for course '{Course}'",
            userEmail, courseName);

        // Simulate email processing delay
        Thread.Sleep(2000);

        _logger.LogInformation(
            "[BackgroundJob] Email sent successfully to {Email}", userEmail);
    }

    /// <summary>
    /// Cleanup old incomplete lesson progress records (older than 6 months with 0% completion).
    /// Runs as a recurring job.
    /// </summary>
    public void CleanupStaleProgressRecords()
    {
        _logger.LogInformation("[BackgroundJob] Starting cleanup of stale progress records...");

        // Simulate cleanup work
        Thread.Sleep(1000);

        _logger.LogInformation("[BackgroundJob] Stale progress cleanup completed.");
    }
}
