using System.Globalization;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

// Load configuration. Default path matches Game/appsettings.json
// You can override via environment variable ASPNETCORE_ prefixed or user secrets.
builder.Configuration
       .AddJsonFile("appsettings.json")
       .AddEnvironmentVariables();

var app = builder.Build();

string dbPath = builder.Configuration.GetValue<string>("database.path", "data/games.db");
string connectionString = $"Data Source={dbPath};Cache=Shared";

app.MapGet("/api/stats", () => GetStats(connectionString))
   .WithName("GetStats");

app.MapGet("/", async context =>
{
    DateTime? selectedDay = null;
    if (context.Request.Query.TryGetValue("day", out var dayVals))
    {
        var dayStr = (string?)dayVals;
        if (!string.IsNullOrWhiteSpace(dayStr))
        {
            if (DateTime.TryParseExact(dayStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var day))
            {
                selectedDay = day.Date;
            }
        }
    }

    var stats = GetStats(connectionString, selectedDay);

    await context.Response.WriteAsync(RenderHtml(stats));
});

app.Run();


static string getDayName(DateTime date)
{
    var dayZero = new DateTime(2025, 12, 26);
    var diffDays = (int)date.Date.Subtract(dayZero.Date).TotalDays;
    return $"Day {diffDays}";
}

static StatsDto GetStats(string connectionString, DateTime? selectedDay = null)
{
    var dto = new StatsDto();
    dto.SelectedDay = selectedDay;

    using var conn = new SqliteConnection(connectionString);
    conn.Open();

    // Games per day
    using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = @"SELECT date(Start) as Day, 
                            COUNT(*) as Games
                            FROM GamesPlayed
                            WHERE PointsPlayer1 > 0 OR PointsPlayer2 > 0 OR DurationInSeconds > 5
                            GROUP BY Day
                            ORDER BY Day DESC";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var day = reader.GetDateTime(0);
            var dayName = getDayName(day);

            var games = reader.GetInt32(1);
            dto.GamesPerDay.Add(new DayCount(dayName, games));
            dto.TotalGames += games;
        }
    }

    // Highscore per day (max of both players per game, then max per day)
    using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = @"SELECT date(Start) as Day,
                                   MAX(CASE WHEN PointsPlayer1 > PointsPlayer2 THEN PointsPlayer1 ELSE PointsPlayer2 END) as Highscore
                            FROM GamesPlayed
                            GROUP BY date(Start)
                            ORDER BY Day DESC";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var day = reader.GetDateTime(0);
            var dayName = getDayName(day);

            var high = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
            dto.DailyHighscores.Add(new DayCount(dayName, high));
            
            if (dto.OverallHighscore < high) 
                dto.OverallHighscore = high;
        }
    }

    // Overall or per-day Highscore list (TOP rank with ties, Points, Name)
    using (var cmd = conn.CreateCommand())
    {
        // Fetch enough rows to cover ties within TOP 10 ranks.
        // We compute competition ranks (1,2,2,4,...) in C# below.
        if (selectedDay.HasValue)
        {
            cmd.CommandText = @"SELECT Points, Name
                                 FROM Highscore
                                 WHERE date(Date) = $day
                                 ORDER BY Points DESC, Date DESC
                                 LIMIT 200";
            var p = cmd.CreateParameter();
            p.ParameterName = "$day";
            p.Value = selectedDay.Value.ToString("yyyy-MM-dd");
            cmd.Parameters.Add(p);
        }
        else
        {
            cmd.CommandText = @"SELECT Points, Name
                                 FROM Highscore
                                 ORDER BY Points DESC, Date DESC
                                 LIMIT 200";
        }
        using var reader = cmd.ExecuteReader();
        int processed = 0;
        int currentRank = 0;
        int? lastPoints = null;
        while (reader.Read())
        {
            var points = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
            var name = reader.IsDBNull(1) ? "" : reader.GetString(1);

            processed++;
            if (lastPoints == null || points != lastPoints.Value)
            {
                currentRank = processed; // competition ranking: first position of this score bucket
                lastPoints = points;
            }

            if (currentRank > 10) break; // we only need ranks up to TOP 10

            dto.Highscores.Add(new HighscoreEntry(currentRank, points, name));
        }
    }

    conn.Close();
    
    return dto;
}

static string RenderHtml(StatsDto s)
{
    // Simple HTML without external dependencies
    var sb = new System.Text.StringBuilder();
    sb.Append("<!doctype html><html lang=\"de\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
    sb.Append("<title>Snake im Kidspace</title>");
    sb.Append("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:20px;} table{border-collapse:collapse;margin:10px 0;min-width:320px;} th,td{border:1px solid #ccc;padding:6px 10px;text-align:right;} th:first-child, td:first-child{text-align:left;} h2{margin-top:30px;} .totals{display:flex;gap:30px;flex-wrap:wrap} .card{border:1px solid #ddd;padding:10px 14px;border-radius:8px;background:#fafafa}</style>");
    sb.Append("</head><body>");
    sb.Append("<h1>Snake im Kidspace</h1>");

    sb.Append("<div class=\"totals\">\n");
    sb.Append($"<div class=card><div>Games played</div><div style=\"font-size:1.6em;font-weight:600\">{s.TotalGames:N0}</div></div>");
    sb.Append($"<div class=card><div>Highscore (overall)</div><div style=\"font-size:1.6em;font-weight:600\">{s.OverallHighscore:N0}</div></div>");
    sb.Append("</div>");

    // Place the three tables side-by-side when space allows
    sb.Append("<div style=\"display:flex;gap:20px;flex-wrap:wrap;align-items:flex-start\">");

    // Games per Day
    sb.Append("<div><h2>Games played per Day</h2>\n<table><thead><tr><th>Day</th><th>Games played</th></tr></thead><tbody>");
    foreach (var row in s.GamesPerDay)
        sb.Append($"<tr><td>{row.Day}</td><td>{row.Value}</td></tr>");
    sb.Append("</tbody></table>");

    // Highscore per Day
    sb.Append("<h2>Daily highscore</h2>\n<table><thead><tr><th>Day</th><th>Highscore</th></tr></thead><tbody>");
    foreach (var row in s.DailyHighscores)
    {
        var linkDay = GetDateFromDayName(row.Day);
        var href = linkDay.HasValue ? "/?day=" + linkDay.Value.ToString("yyyy-MM-dd") : "#";
        sb.Append($"<tr><td><a href=\"{href}\">{row.Day}</a></td><td>{row.Value}</td></tr>");
    }
    sb.Append("</tbody></table></div>");

    // Highscore list with TOP rank, Points, Name
    var bestPlayersTitle = s.SelectedDay.HasValue
        ? $"Best players of day {getDayName(s.SelectedDay.Value)}"
        : "Best players";
    sb.Append($"<div><h2>{bestPlayersTitle}</h2>\n<table><thead><tr><th></th><th style=\"text-align:right\">Points</th><th>Name</th></tr></thead><tbody>");
    foreach (var row in s.Highscores)
        sb.Append($"<tr><td>TOP {row.Rank}</td><td>{row.Points}</td><td style=\"text-align:left\">{System.Net.WebUtility.HtmlEncode(row.Name)}</td></tr>");
    sb.Append("</tbody></table></div>");

    sb.Append("</div>");

    sb.Append("</body></html>");
    return sb.ToString();
}

// Helper: reconstruct a date from our custom day label ("Day X")
static DateTime? GetDateFromDayName(string dayLabel)
{
    if (dayLabel.StartsWith("Day ", StringComparison.OrdinalIgnoreCase))
    {
        var numPart = dayLabel.Substring(4).Trim();
        if (int.TryParse(numPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out var diff))
        {
            var dayZero = new DateTime(2025, 12, 26);
            return dayZero.Date.AddDays(diff);
        }
    }
    return null;
}

record DayCount(string Day, int Value);

class StatsDto
{
    public List<DayCount> GamesPerDay { get; } = new();
    public List<DayCount> DailyHighscores { get; } = new();
    public List<HighscoreEntry> Highscores { get; } = new();
    public int TotalGames { get; set; }
    public int OverallHighscore { get; set; }
    public DateTime? SelectedDay { get; set; }
}

record HighscoreEntry(int Rank, int Points, string Name);
