using Microsoft.EntityFrameworkCore;

namespace TopDeck.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    #region Statements

    private const string _schemaData = "data";
    private const string _schemaRef = "ref";

    #endregion

    #region DbSets
    
    //

    #endregion

    #region DbContext

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        
    }

    #endregion
}