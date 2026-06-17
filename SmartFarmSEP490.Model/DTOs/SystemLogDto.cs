using System;

namespace SmartFarmSEP490.Model.DTOs
{
    public class SystemLogDto
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string UserEmail { get; set; }
        public string Action { get; set; }
        public string EntityName { get; set; }
        public Guid? EntityId { get; set; }
        public string Description { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SystemLogFilterDto
    {
        public string Action { get; set; }
        public string EntityName { get; set; }
        public string SearchTerm { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class PaginatedList<T>
    {
        public System.Collections.Generic.List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }

        public PaginatedList(System.Collections.Generic.List<T> items, int count, int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            TotalCount = count;
            Items = items;
        }
    }
}
