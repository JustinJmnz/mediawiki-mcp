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

    public async Task<object> CreateOrEditPage(string title, string content, string summary = "")
    {
        try
        {
            // Step 1: Get CSRF token
            var tokenUrl = BuildApiUrl(new Dictionary<string, string>
            {
                { "action", "query" },
                { "meta", "tokens" },
                { "type", "csrf" },
                { "format", "json" }
            });

            var tokenResponse = await _httpClient.GetAsync(tokenUrl);
            var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
            var tokenDoc = JsonDocument.Parse(tokenContent);
            
            var csrfToken = tokenDoc.RootElement
                .GetProperty("batchcomplete")
                .GetString();
            
            if (!tokenDoc.RootElement.TryGetProperty("query", out var queryEl) ||
                !queryEl.TryGetProperty("tokens", out var tokensEl) ||
                !tokensEl.TryGetProperty("csrftoken", out var tokenEl))
            {
                return new { success = false, error = "Failed to get CSRF token" };
            }

            var token = tokenEl.GetString();

            // Step 2: Edit the page
            var editUrl = BuildApiUrl(new Dictionary<string, string>
            {
                { "action", "edit" },
                { "title", title },
                { "text", content },
                { "summary", summary },
                { "token", token },
                { "format", "json" }
            });

            var editContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "action", "edit" },
                { "title", title },
                { "text", content },
                { "summary", summary },
                { "token", token },
                { "format", "json" }
            });

            var editResponse = await _httpClient.PostAsync($"{_baseUrl}{ApiPath}", editContent);
            var editResponseContent = await editResponse.Content.ReadAsStringAsync();
            var editDoc = JsonDocument.Parse(editResponseContent);

            if (!editDoc.RootElement.TryGetProperty("edit", out var editEl))
            {
                return new { success = false, error = "Invalid response from server" };
            }

            if (editEl.TryGetProperty("result", out var resultEl))
            {
                var result = resultEl.GetString();
                if (result == "Success")
                {
                    return new
                    {
                        success = true,
                        message = $"Page '{title}' created/updated successfully",
                        pageid = editEl.TryGetProperty("pageid", out var pageId) 
                            ? pageId.GetInt32() 
                            : 0
                    };
                }
            }

            return new { success = false, error = "Failed to edit page" };
        }
        catch (Exception ex)
        {
            return new { success = false, error = ex.Message };
        }
    }

    public async Task<object> DeletePage(string title, string reason = "")
    {
        try
        {
            // Step 1: Get CSRF token
            var tokenUrl = BuildApiUrl(new Dictionary<string, string>
            {
                { "action", "query" },
                { "meta", "tokens" },
                { "type", "csrf" },
                { "format", "json" }
            });

            var tokenResponse = await _httpClient.GetAsync(tokenUrl);
            var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
            var tokenDoc = JsonDocument.Parse(tokenContent);

            if (!tokenDoc.RootElement.TryGetProperty("query", out var queryEl) ||
                !queryEl.TryGetProperty("tokens", out var tokensEl) ||
                !tokensEl.TryGetProperty("csrftoken", out var tokenEl))
            {
                return new { success = false, error = "Failed to get CSRF token" };
            }

            var token = tokenEl.GetString();

            // Step 2: Delete the page
            var deleteContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "action", "delete" },
                { "title", title },
                { "reason", reason },
                { "token", token },
                { "format", "json" }
            });

            var deleteResponse = await _httpClient.PostAsync($"{_baseUrl}{ApiPath}", deleteContent);
            var deleteResponseContent = await deleteResponse.Content.ReadAsStringAsync();
            var deleteDoc = JsonDocument.Parse(deleteResponseContent);

            if (!deleteDoc.RootElement.TryGetProperty("delete", out var deleteEl))
            {
                return new { success = false, error = "Invalid response from server" };
            }

            if (deleteEl.TryGetProperty("title", out var titleEl))
            {
                var deletedTitle = titleEl.GetString();
                return new
                {
                    success = true,
                    message = $"Page '{deletedTitle}' deleted successfully"
                };
            }

            return new { success = false, error = "Failed to delete page" };
        }
        catch (Exception ex)
        {
            return new { success = false, error = ex.Message };
        }
    }

    public async Task<object> CreateDraftPage(string title, string content, string summary = "")
    {
        try
        {
            // Create page in Draft namespace with review notice
            var draftTitle = $"Draft:{title}";
            var reviewNotice = "{{Under review}}\n\n";
            var contentWithReview = reviewNotice + content;
            var draftSummary = string.IsNullOrEmpty(summary) 
                ? "Created as draft for review" 
                : $"Draft: {summary}";

            var result = await CreateOrEditPage(draftTitle, contentWithReview, draftSummary);

            if (result is not null)
            {
                return new
                {
                    success = true,
                    message = $"Draft page created for review",
                    draftTitle = draftTitle,
                    reviewUrl = $"{_baseUrl}/wiki/{Uri.EscapeDataString(draftTitle.Replace(" ", "_"))}",
                    publicationTitle = title,
                    note = "Review the draft and call 'publish_draft' to move to main namespace"
                };
            }

            return new { success = false, error = "Failed to create draft page" };
        }
        catch (Exception ex)
        {
            return new { success = false, error = ex.Message };
        }
    }

    public async Task<object> EditDraftPage(string title, string content, string summary = "")
    {
        try
        {
            // Edit existing draft page (keeps review notice)
            var draftTitle = $"Draft:{title}";
            var reviewNotice = "{{Under review}}\n\n";
            var contentWithReview = reviewNotice + content;
            var draftSummary = string.IsNullOrEmpty(summary) 
                ? "Updated draft" 
                : $"Draft updated: {summary}";

            var result = await CreateOrEditPage(draftTitle, contentWithReview, draftSummary);

            if (result is not null)
            {
                return new
                {
                    success = true,
                    message = $"Draft page updated",
                    draftTitle = draftTitle,
                    reviewUrl = $"{_baseUrl}/wiki/{Uri.EscapeDataString(draftTitle.Replace(" ", "_"))}"
                };
            }

            return new { success = false, error = "Failed to edit draft page" };
        }
        catch (Exception ex)
        {
            return new { success = false, error = ex.Message };
        }
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
