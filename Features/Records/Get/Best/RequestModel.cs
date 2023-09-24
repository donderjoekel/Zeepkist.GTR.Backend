using System.ComponentModel;
using FastEndpoints;

namespace TNRD.Zeepkist.GTR.Backend.Features.Records.Get.Best;

public class RequestModel
{
    [QueryParam, DefaultValue(null)] public int? User { get; set; }
    [QueryParam, DefaultValue(null)] public int? Level { get; set; }
    [QueryParam, DefaultValue(null)] public bool? ValidOnly { get; set; }
    [QueryParam, DefaultValue(null)] public bool? InvalidOnly { get; set; }
    [QueryParam, DefaultValue(null)] public string? Before { get; set; }
    [QueryParam, DefaultValue(null)] public string? After { get; set; }
    [QueryParam, DefaultValue(100)] public int? Limit { get; set; }
    [QueryParam, DefaultValue(0)] public int? Offset { get; set; }
}
