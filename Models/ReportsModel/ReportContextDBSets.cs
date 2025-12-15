using eOrderTouchApp.Models.ReportsModel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace eOrderTouchApp.Models
{
    public partial class eOrderTouchContext
    {

        public DbSet<ItemReportModel> ItemReportResults { get; set; }
        public DbSet<UserReportModel> UserReportResults { get; set; }
        public DbSet<TableReportModel> TableReportResults { get; set; }
        public DbSet<CategorySaleModel> CategorySaleReports { get; set; }
        public DbSet<DailySaleReportModel> DailySaleReportModels { get; set; }
        public DbSet<CustomerSaleReportModel> CustomerSaleReports { get; set; }
        public DbSet<CustomerAreaSaleReport> CustomerAreaSaleReports { get; set; }
        public DbSet<CustomerCreditSaleReport> CustomerCreditSaleReports { get; set; }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ItemReportModel>().HasNoKey();
            modelBuilder.Entity<UserReportModel>().HasNoKey();
            modelBuilder.Entity<TableReportModel>().HasNoKey();

            modelBuilder.Entity<CategorySaleModel>().HasNoKey();
            modelBuilder.Entity<DailySaleReportModel>().HasNoKey();
            modelBuilder.Entity<CustomerSaleReportModel>().HasNoKey();

            modelBuilder.Entity<CustomerAreaSaleReport>().HasNoKey();
            modelBuilder.Entity<CustomerCreditSaleReport>().HasNoKey();
        }
        private static readonly Dictionary<string, Type> ReportsModel = new()
        {
            { "Item Sale Reports", typeof(ItemReportModel) },
            { "User Sale Reports", typeof(UserReportModel) },
            { "Table Sale Reports", typeof(TableReportModel) },
            { "Daily Sale Reports", typeof(DailySaleReportModel) },
            { "Category Sale Reports", typeof(CategorySaleModel) },
            { "Customer Sale Reports", typeof(CustomerSaleReportModel) },
            { "Customer Area Sale Reports", typeof(CustomerAreaSaleReport) },
            { "Customer Credit Sale Reports", typeof(CustomerCreditSaleReport) }

        };

        public async Task<object> ExecuteReport(
        string reportName, int businessId, DateTime fromDate, DateTime toDate)
        {


            if (!ReportsModel.ContainsKey(reportName))
                throw new Exception("Invalid ReportId!");

            var modelType = ReportsModel[reportName];

            var sql = "EXEC Pro_GenerateReport @ReportName, @BusinessId, @FromDate, @ToDate";

            var p1 = new SqlParameter("@ReportName", reportName);
            var p2 = new SqlParameter("@BusinessId", businessId);
            var p3 = new SqlParameter("@FromDate", fromDate);
            var p4 = new SqlParameter("@ToDate", toDate);

            // STEP 1 — DbSet<T>
            var method = this.GetType()
                .GetMethod("Set", Type.EmptyTypes)
                .MakeGenericMethod(modelType);

            var dbSet = method.Invoke(this, null);

            var queryable = (IQueryable)dbSet;

            // STEP 2 — find correct FromSqlRaw method via reflection
            var fromSqlMethod = typeof(RelationalQueryableExtensions)
                .GetMethods()
                .First(m =>
                    m.Name == "FromSqlRaw"
                    && m.GetParameters().Length == 3
                )
                .MakeGenericMethod(modelType);

            var sqlQuery = fromSqlMethod.Invoke(null, new object[]
            {
            queryable,
            sql,
            new object[] { p1, p2, p3, p4 }
            });

            // STEP 3 — call ToListAsync<T>
            var toListMethod = typeof(EntityFrameworkQueryableExtensions)
                .GetMethods()
                .First(m =>
                    m.Name == "ToListAsync"
                    && m.GetParameters().Length == 2
                )
                .MakeGenericMethod(modelType);

            var task = (Task)toListMethod.Invoke(null, new object[]
            {
            sqlQuery,
            CancellationToken.None
            });

            await task;

            // STEP 4 — extract task.Result
            var resultProp = task.GetType().GetProperty("Result");
            return resultProp.GetValue(task);
        }

    }
}
