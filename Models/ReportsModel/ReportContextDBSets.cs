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
        public DbSet<SaleReportModel> SaleReportModels { get; set; }
        public DbSet<CustomerSaleReportModel> CustomerSaleReports { get; set; }
        public DbSet<CustomerAreaSaleReport> CustomerAreaSaleReports { get; set; }
        public DbSet<CustomerCreditSaleReport> CustomerCreditSaleReports { get; set; }
        public DbSet<ItemSaleProfitReport> ItemSaleProfitReports { get; set; }
        public DbSet<OrderSaleProfitReport> OrderSaleProfitReports { get; set; }
        public DbSet<SaleProfitReport> SaleProfitReports { get; set; }
        public DbSet<MaterialListReportModel> MaterialSaleReportModels { get; set; }
        public DbSet<BillCancellationReportModel> BillCancellationReports { get; set; }
        public DbSet<CustomerListReportModel> CustomerListReportModels { get; set; }
        public DbSet<VendorListReportModel> VendorListReportModels { get; set; }
        public DbSet<DateWiseSaleReportModel> DateWiseSaleReportModels { get; set; }
        public DbSet<DateWiseSaleProfitReportModel> DateWiseSaleProfitReports { get; set; }
        public DbSet<StockReportModel> StockReportModels { get; set; }
        public DbSet<OrderDiscountReportModel> OrderDiscountReportModels { get; set; }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ItemReportModel>().HasNoKey();
            modelBuilder.Entity<UserReportModel>().HasNoKey();
            modelBuilder.Entity<TableReportModel>().HasNoKey();

            modelBuilder.Entity<CategorySaleModel>().HasNoKey();
            modelBuilder.Entity<SaleReportModel>().HasNoKey();
            modelBuilder.Entity<CustomerSaleReportModel>().HasNoKey();

            modelBuilder.Entity<CustomerAreaSaleReport>().HasNoKey();
            modelBuilder.Entity<CustomerCreditSaleReport>().HasNoKey();

            modelBuilder.Entity<ItemSaleProfitReport>().HasNoKey();
            modelBuilder.Entity<OrderSaleProfitReport>().HasNoKey();
            modelBuilder.Entity<SaleProfitReport>().HasNoKey();

            modelBuilder.Entity<MaterialListReportModel>().HasNoKey();
            modelBuilder.Entity<BillCancellationReportModel>().HasNoKey();
            modelBuilder.Entity<CustomerListReportModel>().HasNoKey();
            modelBuilder.Entity<VendorListReportModel>().HasNoKey();
            modelBuilder.Entity<DateWiseSaleReportModel>().HasNoKey();
            modelBuilder.Entity<DateWiseSaleProfitReportModel>().HasNoKey();
            modelBuilder.Entity<StockReportModel>().HasNoKey();
            modelBuilder.Entity<OrderDiscountReportModel>().HasNoKey();


        }
        private static readonly Dictionary<string, Type> ReportsModel = new()
        {
            { "Item Sale Reports", typeof(ItemReportModel) },
            { "User Sale Reports", typeof(UserReportModel) },
            { "Table Sale Reports", typeof(TableReportModel) },
            { "Sale Reports", typeof(SaleReportModel) },
            { "Category Sale Reports", typeof(CategorySaleModel) },
            { "Customer Sale Reports", typeof(CustomerSaleReportModel) },
            { "Customer Area Sale Reports", typeof(CustomerAreaSaleReport) },
            { "Customer Credit Sale Reports", typeof(CustomerCreditSaleReport) },
            { "Item Sale Profit Reports", typeof(ItemSaleProfitReport) },
            { "Order Sale Profit Reports", typeof(OrderSaleProfitReport) },
            { "Sale Profit Reports", typeof(SaleProfitReport) },
            { "MaterialList Reports", typeof(MaterialListReportModel) },
            { "Bill Cancellation Reports", typeof(BillCancellationReportModel) },
            { "CustomerList Reports", typeof(CustomerListReportModel) },
            { "VendorList Reports", typeof(VendorListReportModel) },
            { "Date-Wise Sale Reports", typeof(DateWiseSaleReportModel) },
            { "Date-Wise Sale Profit Reports", typeof(DateWiseSaleProfitReportModel) },
            { "Stock Reports", typeof(StockReportModel) },
            { "Order Discount Reports", typeof(OrderDiscountReportModel) },


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
