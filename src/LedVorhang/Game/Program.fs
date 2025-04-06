module Program

open System.Device.Spi
open System.Runtime.InteropServices
open Elmish
open Game
open Game.Model
open HardwareLayer

let createLedDisplay () =

    let createWs2812B chipSelectLine = 
        let settings = SpiConnectionSettings(0, chipSelectLine)
        settings.ClockFrequency <- 3_400_000
        settings.Mode <- SpiMode.Mode0
        settings.DataBitLength <- 8
        
        Ws2812Vorhang(SpiDevice.Create(settings), 20, 20)

    let device0 = createWs2812B(0)
    let device1 = createWs2812B(1)
    let device2 = createWs2812B(3)
    let device3 = createWs2812B(4)

    QuadDeviceDisplay(device1, device3, device2, device0)

let createConsoleDisolay () =
    ConsoleDisplay(true)
        
if RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then
   let display = createConsoleDisolay()
   
   Program.mkProgram (fun _ -> init true, Cmd.none) State.update (View.view display)
    |> Program.withSubscription Subscription.WindowsPc
    |> Program.run
else
    let display = createLedDisplay()

    Program.mkProgram (fun _ -> init true, Cmd.none) State.update (View.view display)
    |> Program.withSubscription Subscription.RaspberryPi
    |> Program.run

printfn "It's running"

while true do
    System.Threading.Thread.Sleep(100000)
