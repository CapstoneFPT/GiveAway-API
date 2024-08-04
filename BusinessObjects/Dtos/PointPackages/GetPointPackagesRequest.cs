using System.ComponentModel.DataAnnotations;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.PointPackages;

public class GetPointPackagesRequest
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public PointPackageStatus[] Status { get; set; } = [];
}