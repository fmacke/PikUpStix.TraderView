namespace traderview.Server.DTOs;

/// <summary>
/// DTO for asset value at a specific point in time
/// </summary>
public class AssetValueOverTimeDto
{
    /// <summary>
    /// The report date
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// The total asset value on this date
    /// </summary>
    public decimal TotalAssetValue { get; set; }

    /// <summary>
    /// The total long asset value on this date
    /// </summary>
    public decimal TotalLongValue { get; set; }

    /// <summary>
    /// The total short asset value on this date
    /// </summary>
    public decimal TotalShortValue { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public AssetValueOverTimeDto(DateTime date, decimal totalAssetValue, decimal totalLongValue, decimal totalShortValue)
    {
        Date = date;
        TotalAssetValue = totalAssetValue;
        TotalLongValue = totalLongValue;
        TotalShortValue = totalShortValue;
    }

    /// <summary>
    /// Parameterless constructor for serialization
    /// </summary>
    public AssetValueOverTimeDto()
    {
    }
}
