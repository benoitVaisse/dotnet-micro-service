using Catalog.Api.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

// SCOPE WARNING: for a [SetUpFixture], the *namespace* defines the scope, not the folder
// on disk. This fixture covers its own namespace AND every descendant one, so
// `Catalog.IntegrationsTests` also covers `Catalog.IntegrationsTests.ProductTestCases`.
//
// A sibling namespace would NOT be covered: had this been declared under
// `Catalog.IntegrationsTests.Infrastructure`, tests in `...ProductTestCases` would run with
// no container at all and blow up with a NullReferenceException on _container.
//
// Dropping the namespace entirely (global namespace) would cover the whole assembly, which
// is the safer option if tests are ever added outside this namespace root.
namespace Catalog.IntegrationsTests;

[SetUpFixture]
public class DatabaseFixture
{
    // Static because tests cannot reach the fixture instance: NUnit creates it to run the
    // one-time hooks but never hands it to test classes (no constructor injection like xUnit).
    // Acceptable global state here, since the container really is a process-wide resource.
    //
    // `null!` silences the nullable warning: the field is assigned by [OneTimeSetUp] before
    // any test runs. That is a promise WE make, not one the compiler verifies - see the null
    // check in the teardown for the case where the promise breaks.
    private static PostgreSqlContainer _container = null!;

    // Only known at runtime: Testcontainers maps Postgres onto a random free host port so
    // parallel runs never collide. This is why the connection string cannot live in config.
    public static string ConnectionString => _container.GetConnectionString();

    [OneTimeSetUp]
    public async Task StartDatabaseAsync()
    {
        _container = new PostgreSqlBuilder("postgres:17")
            .WithDatabase("catalog")
            .WithUsername("catalog")
            .WithPassword("catalog")
            // Unique suffix keeps the container easy to spot in Docker Desktop while avoiding
            // the "name already in use" conflict a fixed name would cause after a crashed run.
            .WithName($"catalog-tests-{Guid.NewGuid():N}")
            .Build();

        // Image pinned to postgres:17 to match docker-compose.yml. Two reasons not to drift:
        // a different major version means a different query planner and feature set, and the
        // -alpine variant (musl) uses different collations than the standard image (glibc),
        // which silently changes ORDER BY results on mixed-case and accented strings.
        //
        // No explicit port binding on purpose: a fixed host port would clash with the
        // catalog-db service from docker-compose and prevent running suites in parallel.
        await _container.StartAsync();

        // Order matters: Build -> StartAsync -> Migrate. StartAsync only returns once Postgres
        // actually accepts connections, so migrating earlier would fail with connection refused.
        //
        // MigrateAsync applies the real EF migrations rather than EnsureCreated, so a broken
        // migration fails the suite instead of passing on a schema rebuilt from the model.
        // EF locates them by reflecting over the assembly that declares CatalogDbContext
        // (Catalog.Api.dll), which lands in our bin folder through the ProjectReference.
        //
        // Run once for the whole assembly: this is DDL against a real database and by far the
        // most expensive step. Per-test isolation is handled by IntegrationTestBase instead.
        await using CatalogDbContext context = CreateContext();
        await context.Database.MigrateAsync();
    }

    [OneTimeTearDown]
    public async Task StopDatabaseAsync()
    {
        // The null check is not defensive noise. NUnit still runs the teardown when
        // [OneTimeSetUp] throws (Docker down, image not found, wait strategy timeout), and
        // _container may be null by then. Without the guard, a NullReferenceException here
        // would mask the real error and send you debugging the wrong problem.
        if (_container is not null)
        {
            // DisposeAsync stops AND removes the container; StopAsync would leave it behind
            // in Exited state, piling up across runs.
            await _container.DisposeAsync();
        }
    }

    public static CatalogDbContext CreateContext()
    {
        // These options are built entirely here - nothing is read from Catalog.Api's
        // appsettings.json and Program.cs never runs. We only borrow the CatalogDbContext
        // *type*; we supply its configuration ourselves, which is exactly what the DI
        // container does in production via AddDbContextPool.
        //
        // The <CatalogDbContext> generic argument carries no data: it is a compile-time tag
        // that stops options for one DbContext being passed to another. The builder starts
        // empty, so dropping UseNpgsql would still compile and only fail at runtime with
        // "No database provider has been configured".
        //
        // Same Npgsql provider as production on purpose: the provider is what turns EF's LINQ
        // and migration operations into actual SQL, so testing on another one would prove
        // very little about the queries that ship.
        DbContextOptions<CatalogDbContext> options =
            new DbContextOptionsBuilder<CatalogDbContext>()
                .UseNpgsql(ConnectionString)
                .Options;

        // A brand new context on every call, with an empty change tracker. Tests rely on this
        // to assert through a different context than the one that wrote, so the assertion
        // really hits Postgres instead of reading back an entity EF still holds in memory.
        return new CatalogDbContext(options);
    }
}
