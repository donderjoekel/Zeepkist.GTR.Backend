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
        IQueryable<IGrouping<string, Vote>> query = context.Votes.AsNoTracking()
            .OrderBy(x => x.Id)
            .GroupBy(x => x.Level);

        if (req.Level.HasValue())
            query = query.Where(x => x.Key == req.Level);

        List<VotesGetAverageResponseDTO.AverageLevelScore> averageLevelScores = new();

        List<IGrouping<string, Vote>> groupedVotes = await query.ToListAsync(ct);
        foreach (IGrouping<string, Vote> groupedVote in groupedVotes)
        {
            List<Vote> votes = groupedVote.ToList();
            double average = votes.Average(x => x.Score);
            VotesGetAverageResponseDTO.AverageLevelScore averageLevelScore = new()
            {
                Level = votes.First().Level,
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
