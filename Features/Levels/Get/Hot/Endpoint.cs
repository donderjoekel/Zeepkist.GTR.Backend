using System.Data.Common;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TNRD.Zeepkist.GTR.Backend.Database;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Levels.Get.Hot;

internal class Endpoint : EndpointWithoutRequest<LevelsGetHotResponseDTO>
{
    private readonly GTRContext context;

    /// <inheritdoc />
    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    /// <inheritdoc />
    public override void Configure()
    {
        AllowAnonymous();
        Get("levels/hot");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CancellationToken ct)
    {
        List<LevelsGetHotResponseDTO.Info> infos = new();

        using (DbConnection connection = context.Database.GetDbConnection())
        {
            using (DbCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT l.id as level_id, l.name AS level_name, COUNT(DISTINCT r.user) AS records_count
FROM records r
         INNER JOIN levels l ON r.level = l.id
WHERE DATE(r.date_created) = CURRENT_DATE
GROUP BY l.id
ORDER BY records_count DESC;
";
                await connection.OpenAsync(ct);
                using (DbDataReader reader = await cmd.ExecuteReaderAsync(ct))
                {
                    while (await reader.ReadAsync(ct))
                    {
                        int levelId = reader.GetInt32(0);
                        string levelName = reader.GetString(1);
                        int recordsCount = reader.GetInt32(2);

                        LevelsGetHotResponseDTO.Info info = new LevelsGetHotResponseDTO.Info()
                        {
                            RecordsCount = recordsCount,
                            LevelId = levelId,
                            LevelName = levelName
                        };

                        infos.Add(info);
                    }
                }
            }
        }

        await SendOkAsync(new LevelsGetHotResponseDTO()
            {
                Levels = infos
            },
            ct);
    }
}
