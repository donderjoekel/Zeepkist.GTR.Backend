using TNRD.Zeepkist.GTR.Database.Data.Entities;

namespace TNRD.Zeepkist.GTR.Backend.Records.Requests;

public record ProcessRecordMediaRequest(Record Record, string GhostData, int Retries = 0);
