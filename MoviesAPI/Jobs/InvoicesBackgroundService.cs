
using Microsoft.EntityFrameworkCore;
using MoviesAPI.Entities;

namespace MoviesAPI.Jobs
{
    public class InvoicesBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public InvoicesBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (IServiceScope scope = _serviceProvider.CreateScope())
                    {
                        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        await EmmitInvoices(context);
                        await SetDefaultingDebtorUsers(context);
                        await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                    }
                }
                catch (OperationCanceledException ex)
                {
                    // Ejecutar accion personalizada al detener operación dle job
                }
            }
        }
    
        private async Task EmmitInvoices(ApplicationDbContext context)
        {
            DateTime today = DateTime.Today;
            DateTime comparisonDate = today.AddMonths(-1);

            bool monthInvoicesHaveAlreadyBeenIssued = await context.IssuedInvoices.AnyAsync(i => i.Year == comparisonDate.Year 
                                                                                                && i.Month == comparisonDate.Month);
            
            if (!monthInvoicesHaveAlreadyBeenIssued)
            {
                DateTime startDate = new DateTime(comparisonDate.Year, comparisonDate.Month, 1);
                DateTime endDate = startDate.AddMonths(1);
                decimal amountPerRequest = 1.0m / 2; // 1 dólar por cada 2 peticiones

                //string query = $"EXEC SP_Create_Invoices '{startDate.ToString("yyyy-MM-dd")}', '{endDate.ToString("yyyy-MM-dd")}'";
                //string query = $"EXEC SP_Create_Invoices {startDate.ToString("yyyy-MM-dd")}, {endDate.ToString("yyyy-MM-dd")}";
                //await context.Database.ExecuteSqlAsync($"{query}");

                IQueryable<Invoice> invoices = from apiRequest in context.APIRequests
                                                join apiKey in context.APIKeys
                                                on apiRequest.APIKeyId equals apiKey.Id
                                                where apiKey.KeyType != KeyType.Free
                                                        && apiRequest.RequestDate >= startDate
                                                        && apiRequest.RequestDate < endDate
                                                group apiRequest by apiKey.UserId into grouped
                                                select new Invoice
                                                {
                                                    UserId = grouped.Key,
                                                    Amount = grouped.Count() * amountPerRequest,
                                                    EmissionDate = DateTime.UtcNow,
                                                    LimitPaymentDate = DateTime.UtcNow.AddDays(60),
                                                    Paid = false
                                                };

                context.Invoices.AddRange(invoices);

                IssuedInvoice issuedInvoice = new IssuedInvoice
                {
                    Month = DateTime.UtcNow.Month == 1 ? 12 : DateTime.UtcNow.Month - 1,
                    Year = DateTime.UtcNow.Month == 1 ? DateTime.UtcNow.Year - 1 : DateTime.UtcNow.Year
                };

                context.IssuedInvoices.Add(issuedInvoice);

                await context.SaveChangesAsync();
                
            }
        }
    
        private async Task SetDefaultingDebtorUsers(ApplicationDbContext context)
        {

            IQueryable<User> usersToUpdate = context.Users.Where(user => context.Invoices.Any(invoice => invoice.UserId == user.Id
                                                                                            && !invoice.Paid
                                                                                            && invoice.LimitPaymentDate < DateTime.UtcNow));

            await usersToUpdate.ForEachAsync(user => user.DefaultingDebtor = true);

            await context.SaveChangesAsync();
        }
    }
}
