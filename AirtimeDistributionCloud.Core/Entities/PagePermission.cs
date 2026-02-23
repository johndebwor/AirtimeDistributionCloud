namespace AirtimeDistributionCloud.Core.Entities;

public class PagePermission
{
    public int Id { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string PageKey { get; set; } = string.Empty;
    public bool IsAllowed { get; set; }
}
