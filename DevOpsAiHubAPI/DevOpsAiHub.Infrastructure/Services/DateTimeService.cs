using DevOpsAiHub.Application.Common.Interfaces.Services;

namespace DevOpsAiHub.Infrastructure.Services;

public class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
}