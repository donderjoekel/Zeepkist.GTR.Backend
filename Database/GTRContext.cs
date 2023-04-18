using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TNRD.Zeepkist.GTR.Backend.Database.Models;

namespace TNRD.Zeepkist.GTR.Backend.Database;

internal class GTRContext : DbContext
{
    public DbSet<Record> Records { get; set; } = null!;
    public DbSet<Level> Levels { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDbFunction(DateTruncMethod).HasName("date_trunc");
    }

    private static readonly MethodInfo DateTruncMethod =
        typeof(GTRContext).GetRuntimeMethod(nameof(DateTrunc), new[] { typeof(string), typeof(DateTime) })!;

    public static DateTime DateTrunc(string field, DateTime source)
        => throw new NotSupportedException();
}
