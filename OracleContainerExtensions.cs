using Testcontainers.Oracle;

namespace OracleTestContainersExample;

public static class OracleContainerExtensions
{
    public static string GetOracleFreeConnectionString(this OracleContainer container)
    {
        return container.GetConnectionString().Replace("XEPDB1", "FREEPDB1");
    }
}
