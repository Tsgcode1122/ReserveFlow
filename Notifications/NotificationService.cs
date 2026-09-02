using Microsoft.EntityFrameworkCore;
using ReserveFlow.Data;
using ReserveFlow.Data.Entities;

namespace ReserveFlow.Services.Notifications;

public sealed class NotificationService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory)
    : INotificationService
{
    private readonly IDbContextFactory<ApplicationDbContext>
        _dbContextFactory = dbContextFactory;

    /// <summary>
    /// Returns the user's newest unread notifications.
    /// </summary>
    public async Task<IReadOnlyList<Notification>> GetUnreadAsync(
        string userId,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        return await dbContext.Notifications
            .AsNoTracking()
            .Where(notification =>
                notification.UserId == userId &&
                !notification.IsRead)
            .OrderByDescending(notification => notification.CreatedAt)
            .Take(Math.Clamp(limit, 1, 50))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Marks one notification as read after verifying ownership.
    /// </summary>
    public async Task<bool> MarkAsReadAsync(
        Guid notificationId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var notification = await dbContext.Notifications
            .SingleOrDefaultAsync(
                notification =>
                    notification.Id == notificationId &&
                    notification.UserId == userId,
                cancellationToken);

        if (notification is null)
        {
            return false;
        }

        notification.IsRead = true;
        notification.ReadAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Marks every unread notification belonging to the user as read.
    /// </summary>
    public async Task<int> MarkAllAsReadAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var readAt = DateTimeOffset.UtcNow;

        // ExecuteUpdate sends one UPDATE statement instead of loading
        // every notification into application memory.
        return await dbContext.Notifications
            .Where(notification =>
                notification.UserId == userId &&
                !notification.IsRead)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        notification => notification.IsRead,
                        true)
                    .SetProperty(
                        notification => notification.ReadAt,
                        readAt),
                cancellationToken);
    }
}