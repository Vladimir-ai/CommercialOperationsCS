using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace App.Domain.WEB.Models
{
    public class PaginatedList<T> : List<T>
    {
        public int TotalPages { get; private set; }
        public int PageIndex { get; private set; }
        public int PageSize { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int) Math.Ceiling(count / (double) pageSize);
            PageSize = pageSize;

            AddRange(items);
        }

        public bool HasPreviousPage { get => PageIndex > 1; }

        public bool HasNextPage { get => PageIndex < TotalPages; }

        public static PaginatedList<T> CreateList(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}