using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoviesAPI.Migrations
{
    /// <inheritdoc />
    public partial class Sp_InvoiceCreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"
                CREATE PROCEDURE SP_Create_Invoices
	                -- Add the parameters for the stored procedure here
	                @startDate datetime,
	                @endDate datetime
                AS
                BEGIN
	                -- SET NOCOUNT ON added to prevent extra result sets from
	                -- interfering with SELECT statements.
	                SET NOCOUNT ON;

                    -- Insert statements for procedure here
                    DECLARE @amountPerRequest decimal(4,4) = 1.0/2 -- 1 dólar por cada dos peticiones

                INSERT INTO Invoices(UserId, Amount, EmissionDate, LimitPaymentDate, Paid)

                SELECT 
	                UserId,
	                COUNT(*) * @amountPerRequest as Amount,
	                GETDATE() AS EmissionDate,
	                DATEADD(d, 60, GETDATE()) as LimitPaymentDate,
	                0 as Paid
                FROM 
	                APIRequests
                INNER JOIN 
	                APIKeys
                ON 
	                APIKeys.Id = APIRequests.APIKeyId
                WHERE	
	                APIKeys.KeyType != 1 and  RequestDate >= @startDate AND RequestDate < @endDate
                GROUP BY 
	                UserId

                INSERT INTO IssuedInvoices(Month, Year)

                SELECT
	                CASE MONTH(GetDate())
	                WHEN 1 then 12
	                ELSE MONTH(GetDate()) - 1 END AS Month,
    
	                CASE MONTH(GETDATE())
	                WHEN 1 then YEAR(GETDATE())-1
	                ELSE YEAR(GETDATE()) END AS Year
                END

            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE SP_Create_Invoices");
        }
    }
}
