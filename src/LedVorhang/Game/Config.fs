module Game.Config

open System
open Microsoft.Extensions.Configuration

// Build configuration once and reuse
let private buildConfig () =
    ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional = true, reloadOnChange = true)
        .AddEnvironmentVariables()
        .Build()

let config: IConfigurationRoot = buildConfig ()

let get (path: string) (defaultValue: string) =
    match config.[path] with
    | null | "" -> defaultValue
    | v -> v

let getInt (path: string) (defaultValue: int) =
    match config.[path] with
    | null | "" -> defaultValue
    | v -> match Int32.TryParse v with | true, i -> i | _ -> defaultValue

let getBool (path: string) (defaultValue: bool) =
    match config.[path] with
    | null | "" -> defaultValue
    | v -> match Boolean.TryParse v with | true, b -> b | _ -> defaultValue

let getString (path: string) (defaultValue: string) = get path defaultValue

let getStringList (path: string) : string list =
    let section = config.GetSection(path)
    if isNull (box section) then []
    else
        section.GetChildren() |> Seq.map (fun c -> c.Value) |> Seq.toList
