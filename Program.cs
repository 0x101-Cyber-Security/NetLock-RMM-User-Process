using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Helper;
using Microsoft.AspNetCore.SignalR.Client;

class UserClient
{
    // Remote Server URL
    private static string remote_server_url = "http://127.0.0.1:7338"; // Set your server URL here
    private static HubConnection remote_server_client;
    private static Timer remote_server_clientCheckTimer;
    private static bool remote_server_client_setup = false;

    public class Command
    {
        public string response_id { get; set; }
        public string type { get; set; }
        public string remote_control_screen_index { get; set; }
        public string remote_control_mouse_action { get; set; }
        public string remote_control_mouse_xyz { get; set; }
        public string remote_control_keyboard_input { get; set; }
    }

    public static async Task Setup_SignalR()
    {
        try
        {
            remote_server_client = new HubConnectionBuilder()
               .WithUrl(remote_server_url, options =>
               {
                   options.Headers.Add("Username", Environment.UserName); // Füge den Benutzernamen zu den Headern hinzu
               })
               .Build();

            // Handle incoming messages
            remote_server_client.On<string>("ReceiveMessage", async (command) =>
            {
                Console.WriteLine($"Message received: {command}");
                Command command_object = JsonSerializer.Deserialize<Command>(command);
                await ProcessCommand(command_object);
            });

            // Start the connection
            await remote_server_client.StartAsync();
            remote_server_client_setup = true;
            Console.WriteLine("Connected to remote server.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting up SignalR: {ex.Message}");
        }
    }

    private static async Task ProcessCommand(Command command)
    {
        try
        {
            switch (command.type)
            {
                case "0": // Screen Capture
                    string base64Image = await ScreenCapture.CaptureScreenToBase64(Convert.ToInt32(command.remote_control_screen_index));
                    await remote_server_client.InvokeAsync("ReceiveClientResponse", $"{command.response_id}$screen_capture${base64Image}");
                    break;

                case "1": // Move Mouse
                    string[] mouseCoordinates = command.remote_control_mouse_xyz.Split(',');
                    int x = Convert.ToInt32(mouseCoordinates[0]);
                    int y = Convert.ToInt32(mouseCoordinates[1]);
                    await MouseControl.MoveMouse(x, y);

                    if (command.remote_control_mouse_action == "0") // Left Click
                        await MouseControl.LeftClickMouse();
                    else if (command.remote_control_mouse_action == "1") // Right Click
                        await MouseControl.RightClickMouse();
                    break;

                case "2": // Keyboard Input
                    byte asciiCode = Convert.ToByte(command.remote_control_keyboard_input);
                    await KeyboardControl.SendKey(asciiCode);
                    break;

                default:
                    Console.WriteLine("Unknown command type.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to process command: {ex.Message}");
        }
    }

    public static async Task CheckConnectionStatus()
    {
        if (remote_server_client_setup && remote_server_client.State == HubConnectionState.Disconnected)
        {
            Console.WriteLine("Connection lost. Trying to reconnect...");
            try
            {
                await remote_server_client.StartAsync();
                Console.WriteLine("Reconnected to the server.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reconnection failed: {ex.Message}");
            }
        }
    }

    public static async Task Disconnect()
    {
        if (remote_server_client != null)
        {
            await remote_server_client.StopAsync();
            await remote_server_client.DisposeAsync();
            remote_server_client_setup = false;
            Console.WriteLine("Disconnected from server.");
        }
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("Starting User Process...");

            await UserClient.Setup_SignalR();

            // Timer that checks every 15 seconds if the client is still connected
            Timer timer = new Timer(async _ =>
            {
                await UserClient.CheckConnectionStatus();
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(15));

            // Keep the application running until manually exited
            Console.WriteLine("Press 'q' to disconnect and exit.");
            while (Console.ReadLine()?.ToLower() != "q") { }

            await UserClient.Disconnect();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
