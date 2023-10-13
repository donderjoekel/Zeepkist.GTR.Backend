// using Quartz;
// using TNRD.Zeepkist.GTR.Database;
// using TNRD.Zeepkist.GTR.Database.Models;
//
// namespace TNRD.Zeepkist.GTR.Backend.Jobs;
//
// internal class CalculateWorldRecordsJob : IJob
// {
//     private readonly GTRContext db;
//
//     public CalculateWorldRecordsJob(GTRContext db)
//     {
//         this.db = db;
//     }
//
//     /// <inheritdoc />
//     public async Task Execute(IJobExecutionContext context)
//     {
//         List<User> users = await db.Users.ToListAsync(context.CancellationToken);
//
//         foreach (User user in users)
//         {
//             int amountOfWorldRecords = await db.Records.AsNoTracking()
//                 .Where(r => r.User == user.Id && r.IsWr)
//                 .CountAsync(context.CancellationToken);
//
//             user.WorldRecords = amountOfWorldRecords;
//         }
//
//         await db.SaveChangesAsync(context.CancellationToken);
//     }
// }
