using System.ComponentModel;
using FastEndpoints;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Records.Get.Best;

public class RequestModel : GenericGetRequestDTO
{
    [QueryParam] [DefaultValue(null)] public string? Level { get; set; }
    [QueryParam] [DefaultValue(null)] public string? After { get; set; }
    [QueryParam] [DefaultValue(null)] public string? Before { get; set; }
}
