// See https://aka.ms/new-console-template for more information
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Testcontainers.PostgreSql;

var container = new PostgreSqlBuilder().WithDatabase("TestDb").Build();

await using (container)
{
    await container.StartAsync();

    var cs = container.GetConnectionString();
    Console.WriteLine(cs);

    var opt1 = new DbContextOptionsBuilder<Ctx1>().UseNpgsql(cs).Options;
    var opt2 = new DbContextOptionsBuilder<Ctx2>().UseNpgsql(cs).Options;
    var ctx1 = new Ctx1(opt1);
    var ctx2 = new Ctx2(opt2);

    await OutputPendingMigrations(ctx1);
    await OutputAppliedMigrations(ctx1);
    await ctx1.Database.MigrateAsync();
    await OutputPendingMigrations(ctx1);
    await OutputAppliedMigrations(ctx1);

    Console.WriteLine();

    await OutputPendingMigrations(ctx2);
    await OutputAppliedMigrations(ctx2);
    await ctx2.Database.MigrateAsync();
    await OutputPendingMigrations(ctx2);
    await OutputAppliedMigrations(ctx2);
}

static async Task OutputAppliedMigrations(DbContext ctx, [CallerArgumentExpression(nameof(ctx))] string? expr = null)
{
    var appliedMigrations = (await ctx.Database.GetAppliedMigrationsAsync()).ToList();
    Console.WriteLine($"Applied migrations for '{expr}':");
    appliedMigrations.ForEach(Console.WriteLine);
}

static async Task OutputPendingMigrations(DbContext ctx, [CallerArgumentExpression(nameof(ctx))] string? expr = null)
{
    var appliedMigrations = (await ctx.Database.GetPendingMigrationsAsync()).ToList();
    Console.WriteLine($"Pending migrations for '{expr}':");
    appliedMigrations.ForEach(Console.WriteLine);
}

internal sealed class Ctx1 : DbContext
{
    public DbSet<Foo> Foos { get; private set; }

    public Ctx1(DbContextOptions<Ctx1> options) : base(options)
    {
    }
}

internal sealed class Ctx2 : DbContext
{
    public DbSet<Bar> Bars { get; private set; }

    public Ctx2(DbContextOptions<Ctx2> options) : base(options)
    {
    }
}

internal sealed class Ctx1Factory : IDesignTimeDbContextFactory<Ctx1>
{
    public Ctx1 CreateDbContext(string[] args)
    {
        var opt = new DbContextOptionsBuilder<Ctx1>().UseNpgsql(args.FirstOrDefault()).Options;
        return new Ctx1(opt);
    }
}

internal sealed class Ctx2Factory : IDesignTimeDbContextFactory<Ctx2>
{
    public Ctx2 CreateDbContext(string[] args)
    {
        var opt = new DbContextOptionsBuilder<Ctx2>().UseNpgsql(args.FirstOrDefault()).Options;
        return new Ctx2(opt);
    }
}

public sealed class Foo
{
    public int Id { get; set; }
}

public sealed class Bar
{
    public int Id { get; set; }
}