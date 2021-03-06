﻿ using System;
using System.Collections.Generic;
using System.Linq;
 using DotJEM.Diagnostic;
 using DotJEM.Json.Index;
using DotJEM.Json.Index.Searching;
 using DotJEM.Web.Host.Diagnostics.Performance;
 using DotJEM.Web.Host.Providers.Pipeline;
 using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace DotJEM.Web.Host.Providers.Services
{
    public sealed class SearchResult
    {
        public long TotalCount { get; set; }
        public IEnumerable<dynamic> Results { get; set; }

        public SearchResult(ISearchResult result)
        {
            //Note: We must do the actual enumeration here to kick of the search, otherwise TotalCount is 0.
            Results = result.Select(hit => hit.Json).ToArray();
            TotalCount = result.TotalCount;
        }

        public SearchResult(IEnumerable<dynamic> results, long totalCount)
        {
            Results = results;
            TotalCount = totalCount;
        }
    }

    public interface ISearchService
    {
        SearchResult Search(string query, int skip = 0, int take = 20);
        SearchResult Search(dynamic query, string contentType = null, int skip = 0, int take = 20, string sort = "$created:desc");
    }

    public class SearchService : ISearchService
    {
        private readonly IStorageIndex index;
        private readonly IPipeline pipeline;
        private readonly ILogger performance;

        public SearchService(IStorageIndex index, IPipeline pipeline, ILogger performance)
        {
            this.index = index;
            this.pipeline = pipeline;
            this.performance = performance;
        }

        public SearchResult Search(string query, int skip = 0, int take = 20)
        {
            //TODO: Throw exception on invalid query.
            //if (string.IsNullOrWhiteSpace(query))
            //    Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Must specify a query.");

            ISearchResult result = index
                .Search(query)
                .Skip(skip)
                .Take(take);

            //TODO: extract contenttype based on configuration.
            SearchResult searchResult = new SearchResult(result
                .Select(hit => pipeline.ExecuteAfterGet(hit.Json, (string)hit.Json.contentType, pipeline.CreateContext((string)hit.Json.contentType, (JObject)hit.Json)))
                .ToArray(), result.TotalCount);

            performance.LogAsync("search", new
            {
                totalTime = (long)result.TotalTime.TotalMilliseconds,
                searchTime = (long)result.SearchTime.TotalMilliseconds,
                loadTime = (long)result.LoadTime.TotalMilliseconds,
                query, skip, take,
                results = result.TotalCount
            });
            return searchResult;
        }

        public SearchResult Search(dynamic query, string contentType = null, int skip = 0, int take = 20, string sort = "$created:desc")
        {
            JObject reduceObj = query as JObject ?? JObject.FromObject(query);

            ISearchResult result = index.Search(reduceObj).Skip(skip).Take(take);
            if (!string.IsNullOrEmpty(sort))
            {
                result.Sort(CreateSortObject(sort));
            }
            SearchResult searchResult = new SearchResult(result);
            return searchResult;
        }

        private Sort CreateSortObject(string sort)
        {
            return new Sort(sort.Split(',').Select(CreateSortField).ToArray());
        }

        private SortField CreateSortField(string sort)
        {
            //TODO: Sort by other types as well.
            string[] fields = sort.Split(':');
            return new SortField(fields[0], SortField.LONG, fields[1].Equals("desc", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}