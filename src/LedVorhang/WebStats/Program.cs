using System.Globalization;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

// Load configuration. Default path matches Game/appsettings.json
// You can override via environment variable ASPNETCORE_ prefixed or user secrets.
builder.Configuration
       .AddJsonFile("appsettings.json", optional: true)
       .AddEnvironmentVariables();

var app = builder.Build();

string dbPath = builder.Configuration.GetValue<string>("database.path", "data/games.db");
string connectionString = $"Data Source={dbPath};Cache=Shared";

app.MapGet("/api/stats", () => GetStats(connectionString))
   .WithName("GetStats");

app.MapGet("/", async context =>
{
    var stats = GetStats(connectionString);

    await context.Response.WriteAsync(RenderHtml(stats));
});

app.Run();


static string getDayName(DateTime date)
{
    var dayZero = new DateTime(2025, 12, 26);
    var diff = date.Date.Subtract(dayZero.Date).TotalDays;

    return $"Day {diff}";
    
    return date.ToLongDateString();
}

static StatsDto GetStats(string connectionString)
{
    var dto = new StatsDto();

    using var conn = new SqliteConnection(connectionString);
    conn.Open();

    // Games per day
    using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = @"SELECT date(Start) as Day, COUNT(*) as Games
                            FROM GamesPlayed
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
            dto.SumOfDailyHighscores += high; // Sum over days
        }
    }

    // Overall highest score
    using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = @"SELECT MAX(CASE WHEN PointsPlayer1 > PointsPlayer2 THEN PointsPlayer1 ELSE PointsPlayer2 END)
                            FROM GamesPlayed";
        var scalar = cmd.ExecuteScalar();
        dto.OverallHighscore = scalar == DBNull.Value || scalar == null ? 0 : Convert.ToInt32(scalar, CultureInfo.InvariantCulture);
    }

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

    sb.Append("<h2>Games per Day</h2>\n<table><thead><tr><th>Day</th><th>Games played</th></tr></thead><tbody>");
    foreach (var row in s.GamesPerDay)
        sb.Append($"<tr><td>{row.Day}</td><td>{row.Value}</td></tr>");
    sb.Append("</tbody></table>");

    sb.Append("<h2>Highscore per Day</h2>\n<table><thead><tr><th>Day</th><th>Highscore</th></tr></thead><tbody>");
    foreach (var row in s.DailyHighscores)
        sb.Append($"<tr><td>{row.Day}</td><td>{row.Value}</td></tr>");
    sb.Append("</tbody></table>");

    sb.Append("</body></html>");
    return sb.ToString();
}

record DayCount(string Day, int Value);

class StatsDto
{
    public List<DayCount> GamesPerDay { get; } = new();
    public List<DayCount> DailyHighscores { get; } = new();
    public int TotalGames { get; set; }
    public int SumOfDailyHighscores { get; set; }
    public int OverallHighscore { get; set; }
}
