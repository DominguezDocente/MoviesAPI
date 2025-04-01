namespace MoviesAPI.DTOs
{
    public class MoviesFilterDTO
    {
        public int Page { get; set; } = 1;
        public int RecordsPerPage { get; set; } = 10;

        public PaginationDTO Pagination 
        {
            get { return new PaginationDTO { Page = Page, RecordsPerPage = RecordsPerPage }; } 
        }

        public string Title { get; set; }
        public int GenderId { get; set; }
        public bool InCinema { get; set; }
        public bool NextReleases { get; set; }
        public string OrderByField { get; set; }
        public bool OrderByAscending { get; set; }
    }
}
