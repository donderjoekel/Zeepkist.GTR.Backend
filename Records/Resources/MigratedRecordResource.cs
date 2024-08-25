using System.ComponentModel.DataAnnotations;

namespace TNRD.Zeepkist.GTR.Backend.Records.Resources;

public class MigratedRecordResource : RecordResource
{
    [Required] public int UserId { get; set; }
    [Required] public DateTimeOffset CreationDate { get; set; }
    [Required] public DateTimeOffset UpdateDate { get; set; }
}
