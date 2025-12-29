using Oracle.ManagedDataAccess.Client;
using Testcontainers.Oracle;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace OracleTestContainersExample;

public class OracleContainerTests : IAsyncLifetime
{
    private OracleContainer? _oracleContainer;

    public async Task InitializeAsync()
    {
        _oracleContainer = new OracleBuilder()
            .WithImage("gvenzl/oracle-free:latest")
            .Build();

        await _oracleContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (_oracleContainer != null)
        {
            await _oracleContainer.DisposeAsync();
        }
    }

    [Fact]
    public async Task CanConnectToOracleDatabase()
    {
        // Arrange
        var connectionString = _oracleContainer!.GetOracleFreeConnectionString();

        // Act
        await using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();
        
        // Assert
        Assert.Equal(System.Data.ConnectionState.Open, connection.State);
    }

    [Fact]
    public async Task CanCreateTableAndInsertData()
    {
        // Arrange
        var connectionString = _oracleContainer!.GetOracleFreeConnectionString();

        var options = new DbContextOptionsBuilder<EmployeeDbContext>()
            .UseOracle(connectionString)
            .Options;

        await using var context = new EmployeeDbContext(options);
        
        await context.Database.EnsureCreatedAsync();

        // Act
        var employee = new Employee
        {
            Id = 1,
            Name = "John Doe",
            Email = "john.doe@example.com",
            HireDate = DateTime.Now
        };

        context.Employees.Add(employee);
        var rowsAffected = await context.SaveChangesAsync();

        // Assert
        Assert.Equal(1, rowsAffected);

        var result = await context.Employees.FirstOrDefaultAsync(e => e.Id == 1);

        Assert.NotNull(result);
        Assert.Equal("John Doe", result.Name);
        Assert.Equal("john.doe@example.com", result.Email);
    }

    [Fact]
    public async Task CanExecuteStoredProcedure()
    {
        // Arrange
        var connectionString = _oracleContainer!.GetOracleFreeConnectionString();
        
        var options = new DbContextOptionsBuilder<EmployeeDbContext>()
            .UseOracle(connectionString)
            .Options;

        await using var context = new EmployeeDbContext(options);
        
        var createProcedureSql = @"
            CREATE OR REPLACE PROCEDURE GET_SYSDATE (p_date OUT DATE) IS
            BEGIN
                SELECT SYSDATE INTO p_date FROM DUAL;
            END;";

        await context.Database.ExecuteSqlRawAsync(createProcedureSql);

        // Act
        var connection = context.Database.GetDbConnection() as OracleConnection;
        await connection!.OpenAsync();
        
        using var command = connection.CreateCommand();
        command.CommandText = "GET_SYSDATE";
        command.CommandType = System.Data.CommandType.StoredProcedure;

        var outputParam = new OracleParameter("p_date", OracleDbType.Date)
        {
            Direction = System.Data.ParameterDirection.Output
        };
        command.Parameters.Add(outputParam);

        await command.ExecuteNonQueryAsync();

        // Assert
        var oracleDate = (Oracle.ManagedDataAccess.Types.OracleDate)outputParam.Value;
        var resultDate = oracleDate.Value;
        Assert.NotEqual(default, resultDate);
        Assert.True(Math.Abs((DateTime.Now - resultDate).TotalMinutes) < 5);
    }
}
