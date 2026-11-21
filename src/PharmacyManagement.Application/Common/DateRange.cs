using PharmacyManagement.Domain.Exceptions;

namespace PharmacyManagement.Application.Common;

public sealed record DateRange(DateTime FromUtc, DateTime ToUtcExclusive)
{
    public DateTime ToUtcInclusive => ToUtcExclusive.AddTicks(-1);

    public static DateRange Create(DateTime? from, DateTime? to, int defaultDays = 30)
    {
        var toDate = (to ?? DateTime.UtcNow).Date;
        var fromDate = (from ?? toDate.AddDays(-defaultDays)).Date;

        if (fromDate > toDate)
        {
            throw new BusinessRuleException("The start date cannot be after the end date.");
        }

        return new DateRange(fromDate, toDate.AddDays(1));
    }
}

public class QueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }
}
