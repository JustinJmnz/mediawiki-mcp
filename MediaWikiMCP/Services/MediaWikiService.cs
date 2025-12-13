using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace MediaWikiMCP.Services;

public class MediaWikiService
{
    private readonly string _baseUrl;
    private readonly HttpClient _httpClient;
    private const string ApiPath = "/api.php";

    public MediaWikiService(string baseUrl, HttpClient httpClient)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _httpClient = httpClient;
    }

    public async Task<object> SearchPages(string query)
    {
        var url = BuildApiUrl(new Dictionary<string, string>
        {
            { "action", "query" },
            { "list", "search" },
            { "srsearch", query },
            { "format", "json" },
            { "srlimit", "20" }
        });

        var response = await _httpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);

        var search = doc.RootElement
            .GetProperty("query")
            .GetProperty("search");

        var results = new List<object>();
        foreach (var item in search.EnumerateArray())
        {
            results.Add(new
            {
                title = item.GetProperty("title").GetString(),
                snippet = item.GetProperty("snippet").GetString(),
                size = item.GetProperty("size").GetInt32(),
                wordcount = item.GetProperty("wordcount").GetInt32()
            });
        }

        return new { success = true, results = results };
    }

    public async Task<object> GetPageContent(string title)
    {
        var url = BuildApiUrl(new Dictionary<string, string>
        {
            { "action", "query" },
            { "titles", title },
            { "prop", "revisions" },
            { "rvprop", "content" },
            { "format", "json" }
        });

        var response = await _httpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);

        var pages = doc.RootElement
            .GetProperty("query")
            .GetProperty("pages");

        foreach (var page in pages.EnumerateObject())
        {
            if (page.Value.TryGetProperty("missing", out _))
            {
                return new { success = false, error = "Page not found" };
            }

            var text = "";
            if (page.Value.TryGetProperty("revisions", out var revisions))
            {
                var revArray = revisions.EnumerateArray().FirstOrDefault();
                if (revArray.ValueKind != JsonValueKind.Undefined && 
                    revArray.TryGetProperty("*", out var contentProp))
                {
                    text = contentProp.GetString() ?? "";
                }
            }

            return new
            {
                success = true,
                title = page.Value.GetProperty("title").GetString(),
                content = text,
                pageid = page.Value.GetProperty("pageid").GetInt32()
            };
        }

        return new { success = false, error = "Page not found" };
    }

    public async Task<object> GetPageInfo(string title)
    {
        var url = BuildApiUrl(new Dictionary<string, string>
        {
            { "action", "query" },
            { "titles", title },
            { "prop", "info|revisions" },
            { "format", "json" }
        });

        var response = await _httpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);

        var pages = doc.RootElement
            .GetProperty("query")
            .GetProperty("pages");

        foreach (var page in pages.EnumerateObject())
        {
            if (page.Value.TryGetProperty("missing", out _))
            {
                return new { success = false, error = "Page not found" };
            }

            var lastRev = page.Value.TryGetProperty("lastrevid", out var rev)
                ? rev.GetInt32()
                : 0;

            return new
            {
                success = true,
                title = page.Value.GetProperty("title").GetString(),
                pageid = page.Value.GetProperty("pageid").GetInt32(),
                length = page.Value.GetProperty("length").GetInt32(),
                lastrevid = lastRev,
                touched = page.Value.GetProperty("touched").GetString()
            };
        }

        return new { success = false, error = "Page not found" };
    }

    public async Task<object> GetRecentChanges(int limit = 10)
    {
        var url = BuildApiUrl(new Dictionary<string, string>
        {
            { "action", "query" },
            { "list", "recentchanges" },
            { "rclimit", Math.Min(limit, 100).ToString() },
            { "rcprop", "title|timestamp|user|comment" },
            { "format", "json" }
        });

        var response = await _httpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);

        var changes = doc.RootElement
            .GetProperty("query")
            .GetProperty("recentchanges");

        var results = new List<object>();
        foreach (var change in changes.EnumerateArray())
        {
            results.Add(new
            {
                title = change.GetProperty("title").GetString(),
                user = change.GetProperty("user").GetString(),
                timestamp = change.GetProperty("timestamp").GetString(),
                comment = change.TryGetProperty("comment", out var comment) 
                    ? comment.GetString() 
                    : ""
            });
        }

        return new { success = true, changes = results };
    }

    public async Task<object> GetSiteInfo()
    {
        var url = BuildApiUrl(new Dictionary<string, string>
        {
            { "action", "query" },
            { "meta", "siteinfo" },
            { "siprop", "general|namespaces" },
            { "format", "json" }
        });

        var response = await _httpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);

        var general = doc.RootElement
            .GetProperty("query")
            .GetProperty("general");

        return new
        {
            success = true,
            sitename = general.GetProperty("sitename").GetString(),
            servername = general.GetProperty("servername").GetString(),
            mainpage = general.GetProperty("mainpage").GetString(),
            imagewhitelistenabled = general.TryGetProperty("imagewhitelistenabled", out _),
            langallowusertemplates = general.TryGetProperty("langallowusertemplates", out _)
        };
    }

    public async Task<object> ListAllPages(int limit = 50)
    {
        var url = BuildApiUrl(new Dictionary<string, string>
        {
            { "action", "query" },
            { "list", "allpages" },
            { "aplimit", Math.Min(limit, 500).ToString() },
            { "approp", "size|timestamp|ids" },
            { "format", "json" }
        });

        var response = await _httpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);

        var pages = doc.RootElement
            .GetProperty("query")
            .GetProperty("allpages");

        var results = new List<object>();
        foreach (var page in pages.EnumerateArray())
        {
            results.Add(new
            {
                title = page.GetProperty("title").GetString(),
                pageid = page.GetProperty("pageid").GetInt32(),
                size = page.GetProperty("size").GetInt32(),
                timestamp = page.GetProperty("timestamp").GetString()
            });
        }

        return new { success = true, pages = results };
    }

    private string BuildApiUrl(Dictionary<string, string> parameters)
    {
        var queryParams = new List<string>();
        foreach (var param in parameters)
        {
            queryParams.Add($"{param.Key}={HttpUtility.UrlEncode(param.Value)}");
        }

        return $"{_baseUrl}{ApiPath}?{string.Join("&", queryParams)}";
    }
}
