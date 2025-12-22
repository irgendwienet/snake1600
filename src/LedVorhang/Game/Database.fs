module Game.Database

open System
open System.IO
open Game.Model
open Microsoft.Data.Sqlite

/// Initializes the SQLite database specified by config key `database.path`.
/// If the file does not exist, it will be created. Ensures table `GamesPlayed` exists.
// Resolve database path from configuration
let private getDbPath () =
    Config.getString "database.path" "ledvorhang.db"

let init () =
    // Determine path from config with a sane default in the working directory
    let dbPath = getDbPath ()

    // Ensure directory exists if a directory portion is present
    let directory = Path.GetDirectoryName(dbPath)
    if not (String.IsNullOrWhiteSpace(directory)) && not (Directory.Exists(directory)) then
        Directory.CreateDirectory(directory) |> ignore

    // Open connection (this will create the file if it doesn't exist)
    use conn = new SqliteConnection($"Data Source={dbPath};Cache=Shared")
    conn.Open()

    // Create the GamesPlayed table if it does not exist
    let createSql =
        """
        CREATE TABLE IF NOT EXISTS GamesPlayed (
          Id INTEGER PRIMARY KEY AUTOINCREMENT,
          Start DATETIME NOT NULL,
          End DATETIME NOT NULL,
          DurationInSeconds INTEGER NOT NULL,
          Mode TEXT NOT NULL,
          PointsPlayer1 INTEGER NOT NULL,
          PointsPlayer2 INTEGER NOT NULL
        );
        """

    use cmd = conn.CreateCommand()
    cmd.CommandText <- createSql
    cmd.ExecuteNonQuery() |> ignore

    // Done
    ()

/// Logs a finished game into the database.
/// Duration is calculated from start and end times.
let LogGame (startTime: DateTime) (endTime: DateTime) (mode: PlayMode) (pointsPlayer1: int) (pointsPlayer2: int) =
    let dbPath = getDbPath ()
    let duration = int (endTime - startTime).TotalSeconds
    use conn = new SqliteConnection($"Data Source={dbPath};Cache=Shared")
    conn.Open()

    use cmd = conn.CreateCommand()
    cmd.CommandText <-
        """
        INSERT INTO GamesPlayed (Start, End, DurationInSeconds, Mode, PointsPlayer1, PointsPlayer2)
        VALUES ($start, $end, $duration, $mode, $p1, $p2);
        """

    let p name value =
        let p = cmd.CreateParameter()
        p.ParameterName <- name
        p.Value <- value
        cmd.Parameters.Add(p) |> ignore

    p "$start" (box startTime)
    p "$end" (box endTime)
    p "$duration" (box duration)
    p "$mode" (box (mode.ToString()))
    p "$p1" (box pointsPlayer1)
    p "$p2" (box pointsPlayer2)

    cmd.ExecuteNonQuery() |> ignore

    ()
