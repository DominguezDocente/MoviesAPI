using Microsoft.EntityFrameworkCore;

namespace MoviesAPI.Helpers
{
    public static class HttpContextExtensions
    {
        public async static Task InsertPaginationParameters<T>(this HttpContext httpContext, IQueryable<T> queryable, int recordsPerPage)
        {
            double quantity = await queryable.CountAsync();
            double pagesQuantity = Math.Ceiling(quantity / recordsPerPage);
            httpContext.Response.Headers.Add("Pages-Quantity", pagesQuantity.ToString());
        }
    }
}
