namespace MoviesAPI.DTOs
{
    public class PaginationDTO
    {
        public int Page { get; set; } = 1;
        
        public int RecordsPerPage
        { 
            get => recordsPerPage; 
            set
            {
                recordsPerPage = value > maxRecordsPerPage ? maxRecordsPerPage : value;
            }
        }

        private int recordsPerPage = 10;

        private readonly int maxRecordsPerPage = 50;

        
    }
}
