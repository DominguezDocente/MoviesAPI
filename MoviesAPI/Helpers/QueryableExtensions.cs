using MoviesAPI.DTOs;

namespace MoviesAPI.Helpers
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> Paginate<T>(this IQueryable<T> queryable, PaginationDTO dto)
        {
            return queryable.Skip((dto.Page - 1) * dto.RecordsPerPage)
                            .Take(dto.RecordsPerPage);
        }
    }
}
