using FastEndpoints;
using TNRD.Zeepkist.GTR.Database;
using TNRD.Zeepkist.GTR.Database.Models;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Votes.Get.Average;

internal class Endpoint : Endpoint<VotesGetAverageRequestDTO, VotesGetAverageResponseDTO>
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
        Get("votes/average");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(VotesGetAverageRequestDTO req, CancellationToken ct)
    {
        IQueryable<IGrouping<int?, Vote>> query = context.Votes.AsNoTracking()
            .OrderBy(x => x.Id)
            .Include(x => x.LevelNavigation)
            .GroupBy(x => x.Level);

        if (req.LevelId.HasValue)
            query = query.Where(x => x.Key == req.LevelId.Value);

        List<VotesGetAverageResponseDTO.AverageLevelScore> averageLevelScores = new();

        List<IGrouping<int?, Vote>> groupedVotes = await query.ToListAsync(ct);
        foreach (IGrouping<int?, Vote> groupedVote in groupedVotes)
        {
            List<Vote> votes = groupedVote.ToList();
            double average = votes.Average(x => x.Score!.Value);
            VotesGetAverageResponseDTO.AverageLevelScore averageLevelScore = new()
            {
                Level = votes.First().LevelNavigation!.ToResponseModel(),
                AverageScore = (float)average,
                AmountOfVotes = votes.Count
            };

            averageLevelScores.Add(averageLevelScore);
        }

        await SendOkAsync(new VotesGetAverageResponseDTO()
            {
                AverageScores = averageLevelScores.Skip(req.Offset ?? 0).Take(req.Limit ?? 100).ToList()
            },
            ct);
    }
}
