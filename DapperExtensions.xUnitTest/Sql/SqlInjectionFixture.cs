using NUnit.Framework;

namespace DapperExtensions.xUnitTest.Sql
{
    [Parallelizable(ParallelScope.All)]
    public static class SqlInjectionFixture
    {
        public abstract class SqlInjectionFixtureBase
        {
            protected IDapperExtensionsConfiguration Configuration = new DapperExtensionsConfiguration();
        }

        public class SqlInjectionTest : SqlInjectionFixtureBase
        {
        }
    }
}