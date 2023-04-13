// using System.Security.Claims;
// using FastEndpoints;
// using FluentResults;
// using FluentValidation.Results;
// using Newtonsoft.Json;
// using TNRD.ZeepkistGTR.Backend.Directus;
// using TNRD.ZeepkistGTR.Backend.Directus.Models;
//
// namespace TNRD.ZeepkistGTR.Backend.Features.Records.Submit;
//
// internal class TrackOfTheDayPostProcessor : IPostProcessor<RequestModel, ResponseModel>
// {
//     private class CustomTrackOfTheDayRecordModel
//     {
//         internal class RecordModel
//         {
//             internal class UserModel
//             {
//                 [JsonProperty("id")] public int Id { get; set; }
//             }
//
//             [JsonProperty("time")] public float Time { get; set; }
//             [JsonProperty("user")] public UserModel User { get; set; } = null!;
//         }
//
//         [JsonProperty("id")] public int Id { get; set; }
//         [JsonProperty("record")] public RecordModel Record { get; set; } = null!;
//     }
//
//     /// <inheritdoc />
//     public async Task PostProcessAsync(
//         RequestModel req,
//         ResponseModel res,
//         HttpContext ctx,
//         IReadOnlyCollection<ValidationFailure> failures,
//         CancellationToken ct
//     )
//     {
//         if (failures.Count != 0)
//             return;
//
//         IDirectusClient client = ctx.Resolve<IDirectusClient>();
//         ILogger<TrackOfTheDayPostProcessor> logger = ctx.Resolve<ILogger<TrackOfTheDayPostProcessor>>();
//
//         Claim? userIdClaim = ctx.User.FindFirst("UserId");
//         if (userIdClaim == null)
//         {
//             logger.LogCritical("No UserId claim found!");
//             return;
//         }
//
//         int id = int.Parse(userIdClaim.Value.Split('_')[0]);
//
//         DateTime date = res.DateCreated;
//
//         Result<DirectusGetMultipleResponse<TrackOfTheDayModel>> getTotdResult =
//             await client.Get<DirectusGetMultipleResponse<TrackOfTheDayModel>>(
//                 $"items/totd?fields=*.*.*&filter[date][_between]={date:yyyy-MM-dd},{date.Date.AddDays(1):yyyy-MM-dd}",
//                 ct);
//
//         if (getTotdResult.IsFailed)
//         {
//             logger.LogCritical("Unable to get track of the day: {Result}", getTotdResult);
//             return;
//         }
//
//         if (!getTotdResult.Value.HasItems)
//         {
//             logger.LogInformation("No track of the day");
//             return;
//         }
//
//         if (req.Level != getTotdResult.Value.FirstItem!.Level.AsT1.Id)
//         {
//             logger.LogInformation("Level mismatch for track of the day");
//             return;
//         }
//
//         Result<DirectusGetMultipleResponse<CustomTrackOfTheDayRecordModel>> getRecordResult =
//             await client.Get<DirectusGetMultipleResponse<CustomTrackOfTheDayRecordModel>>(
//                 $"items/totd_records?fields=*.*.*&filter[totd][_eq]={getTotdResult.Value.FirstItem!.Id}&filter[record][user][id][_eq]={id}",
//                 ct);
//
//         if (getRecordResult.IsFailed)
//         {
//             logger.LogCritical("Unable to get record for track of the day: {Result}", getRecordResult);
//             return;
//         }
//
//         if (getRecordResult.Value.HasItems)
//         {
//             try
//             {
//                 if (getRecordResult.Value.FirstItem!.Record.Time <= req.Time)
//                 {
//                     logger.LogInformation("Record is not better than current record");
//                     return;
//                 }
//
//                 int itemId = getRecordResult.Value.FirstItem.Id;
//                 Result patchResult =
//                     await client.Patch($"items/totd_records/{itemId}?fields=*.*.*", new { record = res.Id }, ct);
//
//                 logger.LogInformation("Updated record for track of the day: {Result}", patchResult);
//             }
//             catch (Exception e)
//             {
//                 logger.LogCritical(e, "Unable to update record for track of the day");
//             }
//         }
//         else
//         {
//             Result postResult = await client.Post("items/totd_records?fields=*.*.*",
//                 new { totd = getTotdResult.Value.FirstItem.Id, record = res.Id },
//                 ct);
//
//             logger.LogInformation("Posted record for track of the day: {Result}", postResult);
//         }
//     }
// }
