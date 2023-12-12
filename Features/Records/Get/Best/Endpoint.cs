using FastEndpoints;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Records.Get.Best;

public class Endpoint : Endpoint<RequestModel, RecordsGetResponseDTO>
{
    private readonly GTRContext context;

    public Endpoint(GTRContext context)
    {
        this.context = context;
    }

    public override void Configure()
    {
        AllowAnonymous();
        Get("records/best");
    }

    public override async Task HandleAsync(RequestModel req, CancellationToken ct)
    {
        IQueryable<PersonalBest> query = context.PersonalBests
            .Include(x => x.RecordNavigation)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(req.Level))
        {
            query = query.Where(x => x.Level == req.Level);
        }

        DateTime? beforeDateTime = null;
        if (!string.IsNullOrEmpty(req.Before) && long.TryParse(req.Before, out long before))
        {
            beforeDateTime = DateTimeOffset.FromUnixTimeSeconds(before).UtcDateTime;
            query = query.Where(x => x.DateCreated < beforeDateTime);
        }

        DateTime? afterDateTime = null;
        if (!string.IsNullOrEmpty(req.After) && long.TryParse(req.After, out long after))
        {
            afterDateTime = DateTimeOffset.FromUnixTimeSeconds(after).UtcDateTime;
            query = query.Where(x => x.DateCreated > afterDateTime);
        }

        List<PersonalBest> personalBests = await query
            .Skip(req.Offset!.Value)
            .Take(req.Limit!.Value)
            .ToListAsync(ct);

        List<RecordResponseModel> records = personalBests
            .Select(x => x.RecordNavigation!.ToResponseModel())
            .ToList();

        await SendOkAsync(new RecordsGetResponseDTO()
            {
                TotalAmount = -1,
                After = afterDateTime,
                Before = beforeDateTime,
                Records = records
            },
            ct);
    }
}
