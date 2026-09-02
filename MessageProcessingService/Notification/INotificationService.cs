namespace MessageProcessingService.Notification;

public interface INotificationService
{
    Task SendAnomalyAlertAsync(string message);
    Task SendHighUsageAlertAsync(string message);
}