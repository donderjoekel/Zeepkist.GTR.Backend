using System.Data.Common;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using TNRD.Zeepkist.GTR.Backend.Database;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Levels.Get.Popular;

internal class Endpoint : EndpointWithoutRequest<LevelsGetPopularResponseDTO>
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
        Get("levels/popular");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CancellationToken ct)
    {
        // TODO: Should definitely add caching here

        List<LevelsGetPopularResponseDTO.Info> infos = new();

        using (DbConnection connection = context.Database.GetDbConnection())
        {
            using (DbCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT l.id as level_id, l.name AS level_name, COUNT(DISTINCT r.user) AS records_count
FROM records r
         INNER JOIN levels l ON r.level = l.id
WHERE DATE_TRUNC('month', r.date_created) = DATE_TRUNC('month', CURRENT_DATE)
GROUP BY l.id
ORDER BY records_count DESC
LIMIT 50;";

                await connection.OpenAsync(ct);
                using (DbDataReader reader = await cmd.ExecuteReaderAsync(ct))
                {
                    while (await reader.ReadAsync(ct))
                    {
                        int levelId = reader.GetInt32(0);
                        string levelName = reader.GetString(1);
                        int recordsCount = reader.GetInt32(2);

                        LevelsGetPopularResponseDTO.Info info = new()
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

        await SendOkAsync(new LevelsGetPopularResponseDTO()
            {
                Levels = infos
            },
            ct);
    }
}
