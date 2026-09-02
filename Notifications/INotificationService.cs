using ReserveFlow.Data.Entities;

namespace ReserveFlow.Services.Notifications;

public interface INotificationService
{
    Task<IReadOnlyList<Notification>> GetUnreadAsync(
        string userId,
        int limit = 10,
        CancellationToken cancellationToken = default);

    Task<bool> MarkAsReadAsync(
        Guid notificationId,
        string userId,
        CancellationToken cancellationToken = default);

    Task<int> MarkAllAsReadAsync(
        string userId,
        CancellationToken cancellationToken = default);
}