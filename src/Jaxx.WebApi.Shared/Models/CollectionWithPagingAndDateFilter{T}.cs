﻿using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jaxx.WebApi.Shared.Models
{
    public class CollectionWithPagingAndDateFilter<T> : Collection<T>
    {
        /// <summary>
        /// Gets or sets the offset of the current page.
        /// </summary>
        /// <value>The offset of the current page.</value>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Offset { get; set; }

        /// <summary>
        /// Gets or sets the limit of the current paging options.
        /// </summary>
        /// <value>The limit of the current paging options.</value>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Limit { get; set; }

        /// <summary>
        /// Gets or sets the total size of the collection (irrespective of any paging options).
        /// </summary>
        /// <value>The total size of the collection.</value>
        public long Size { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Link First { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Link Previous { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Link Next { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Link Last { get; set; }
        /// <summary>
        /// Gets or sets the from date.
        /// </summary>
        /// <value>The from date.</value>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Gets or sets the to date.
        /// </summary>
        /// <value>The to date.</value>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? ToDate { get; set; }

        public static CollectionWithPagingAndDateFilter<T> Create(RouteLink self, T[] items, long size, PagingOptions pagingOptions, DateRangeFilterOptions dateRangeFilterOptions)
           => Create<CollectionWithPagingAndDateFilter<T>>(self, items, size, pagingOptions, dateRangeFilterOptions);

        public static TResponse Create<TResponse>(RouteLink self, T[] items, long size, PagingOptions pagingOptions, DateRangeFilterOptions dateRangeFilterOptions)
            where TResponse : CollectionWithPagingAndDateFilter<T>, new()
            => new TResponse
            {
                Self = self,
                Value = items,
                Size = size,
                Offset = pagingOptions.Offset,
                Limit = pagingOptions.Limit,
                First = GetFirstLink(self, dateRangeFilterOptions),
                Next = GetNextLink(self, size, pagingOptions, dateRangeFilterOptions),
                Previous = GetPreviousLink(self, size, pagingOptions, dateRangeFilterOptions),
                Last = GetLastLink(self, size, pagingOptions, dateRangeFilterOptions),
                FromDate = dateRangeFilterOptions.FromDate,
                ToDate = dateRangeFilterOptions.ToDate
            };

        private static Link GetFirstLink(RouteLink self, DateRangeFilterOptions dateRangeFilterOptions)
        {
            var fromDate = dateRangeFilterOptions.FromDate.HasValue ? dateRangeFilterOptions.FromDate.Value.ToString("yyyy-MM-dd") : null;
            var toDate = dateRangeFilterOptions.ToDate.HasValue ? dateRangeFilterOptions.ToDate.Value.ToString("yyyy-MM-dd") : null;
            var parameters = new RouteValueDictionary(self.RouteValues)
            {
                ["fromDate"] = fromDate,
                ["toDate"] = toDate
            };

            var newLink = ToCollection(self.RouteName, parameters);
            return newLink;

        }
        private static Link GetNextLink(RouteLink self, long size, PagingOptions pagingOptions, DateRangeFilterOptions dateRangeFilterOptions)
        {
            if (pagingOptions?.Limit == null) return null;
            if (pagingOptions?.Offset == null) return null;

            var limit = pagingOptions.Limit.Value;
            var offset = pagingOptions.Offset.Value;
            var fromDate = dateRangeFilterOptions.FromDate.HasValue ? dateRangeFilterOptions.FromDate.Value.ToString("yyyy-MM-dd") : null;
            var toDate = dateRangeFilterOptions.ToDate.HasValue ? dateRangeFilterOptions.ToDate.Value.ToString("yyyy-MM-dd") : null;

            var next = offset + limit;
            if (next >= size) return null;

            var parameters = new RouteValueDictionary(self.RouteValues)
            {
                ["limit"] = limit,
                ["offset"] = next,
                ["fromDate"] = fromDate,
                ["toDate"] = toDate
            };

            var newLink = ToCollection(self.RouteName, parameters);
            return newLink;
        }

        private static Link GetLastLink(RouteLink self, long size, PagingOptions pagingOptions, DateRangeFilterOptions dateRangeFilterOptions)
        {
            if (pagingOptions?.Limit == null) return null;

            var limit = pagingOptions.Limit.Value;
            var fromDate = dateRangeFilterOptions.FromDate.HasValue ? dateRangeFilterOptions.FromDate.Value.ToString("yyyy-MM-dd") : null;
            var toDate = dateRangeFilterOptions.ToDate.HasValue ? dateRangeFilterOptions.ToDate.Value.ToString("yyyy-MM-dd") : null;

            if (size <= limit) return null;

            var offset = Math.Ceiling((size - (double)limit) / limit) * limit;

            var parameters = new RouteValueDictionary(self.RouteValues)
            {
                ["limit"] = limit,
                ["offset"] = offset,
                ["fromDate"] = fromDate,
                ["toDate"] = toDate
            };
            var newLink = ToCollection(self.RouteName, parameters);

            return newLink;
        }

        private static Link GetPreviousLink(RouteLink self, long size, PagingOptions pagingOptions, DateRangeFilterOptions dateRangeFilterOptions)
        {
            if (pagingOptions?.Limit == null) return null;
            if (pagingOptions?.Offset == null) return null;

            var limit = pagingOptions.Limit.Value;
            var offset = pagingOptions.Offset.Value;
            var fromDate = dateRangeFilterOptions.FromDate.HasValue ? dateRangeFilterOptions.FromDate.Value.ToString("yyyy-MM-dd") : null;
            var toDate = dateRangeFilterOptions.ToDate.HasValue ? dateRangeFilterOptions.ToDate.Value.ToString("yyyy-MM-dd") : null;

            if (offset == 0)
            {
                return null;
            }

            if (offset > size)
            {
                return GetLastLink(self, size, pagingOptions, dateRangeFilterOptions);
            }

            var previousPage = Math.Max(offset - limit, 0);

            if (previousPage <= 0)
            {
                return self;
            }

            var parameters = new RouteValueDictionary(self.RouteValues)
            {
                ["limit"] = limit,
                ["offset"] = previousPage,
                ["fromDate"] = fromDate,
                ["toDate"] = toDate
            };
            var newLink = Link.ToCollection(self.RouteName, parameters);

            return newLink;
        }
    }
}
