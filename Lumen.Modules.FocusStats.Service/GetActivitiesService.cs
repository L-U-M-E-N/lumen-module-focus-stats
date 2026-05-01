using Lumen.Modules.FocusStats.Common.Dto;

using System.Net.Http.Json;

namespace Lumen.Modules.FocusStats.Service;

public sealed class GetActivitiesService(ILogger<GetActivitiesService> logger) {
    public List<NewUserActivityDto> CachedActivities = [];

    public static string LUMEN_API_KEY;
    public static string LUMEN_API_URL;

    public void GetActivity() {
        var (title, exe) = ForegroundWindowInfo.GetFocusedWindowInfo(logger);
        var cleanTitle = title.Replace('–', '-');

        var lastEntry = CachedActivities.LastOrDefault();
        if (lastEntry?.AppOrExe == exe && lastEntry?.Name == cleanTitle) {
            lastEntry.SecondsDuration++;
        } else {
            var date = DateTime.UtcNow;
            date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, DateTimeKind.Utc); // Round it down to the second

            if (string.IsNullOrWhiteSpace(exe)) {
                logger.LogWarning("Empty exe, skipping");
                if (lastEntry != null) {
                    lastEntry.SecondsDuration++;
                }
                return;
            }

            if (string.IsNullOrWhiteSpace(cleanTitle)) {
                cleanTitle = "?";
            }

            CachedActivities.Add(
                new NewUserActivityDto() {
                    AppOrExe = exe,
                    Name = cleanTitle,
                    Device = System.Environment.MachineName,
                    SecondsDuration = 1,
                    StartTime = date
                }
            );
        }
    }

    public async Task SubmitActivities() {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("X-Api-Key", LUMEN_API_KEY);
        var activityCount = CachedActivities.Count;
        var url = $"{LUMEN_API_URL}/FocusStats/activities";
        var res = await httpClient.PostAsJsonAsync(url, CachedActivities);

        logger.LogDebug("HttpRequest {Date} {URL} {Result} {ActivityCount}", DateTime.UtcNow, url, res.StatusCode, activityCount);

        if (res.IsSuccessStatusCode) {
            CachedActivities.RemoveRange(0, activityCount);
        } else {
            throw new Exception(await res.Content.ReadAsStringAsync());
        }
    }
}
