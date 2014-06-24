namespace Tunney.Common.Statistics
{
    public interface IStatisticsLogger
    {
        IStatisticsDataAccess Stats { get; set; }
    }
}