using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using Helper;

class UserClient
{
    private const string ServerIp = "127.0.0.1"; // Server IP
    private const int Port = 7338;
    private TcpClient _client;
    private NetworkStream _stream;
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public class Command
    {
        public string response_id { get; set; }
        public string type { get; set; }
        public string remote_control_screen_index { get; set; }
        public string remote_control_mouse_xyz { get; set; }
    }

    public async Task Local_Server_Connect()
    {
        try
        {
            _client = new TcpClient();
            await _client.ConnectAsync(ServerIp, Port);

            _stream = _client.GetStream();

            // Send username to identify the user
            string username = Environment.UserName;
            await Local_Server_Send_Message($"username${username}");

            // Start listening for messages from the server
            _ = Local_Server_Handle_Server_Messages(_cancellationTokenSource.Token);

            Console.WriteLine("Connected to the local server.");

            // Optionally, you can send an initial request (e.g., get device identity)
            await Local_Server_Send_Message("get_device_identity");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to the local server: {ex.Message}");
        }
    }

    private async Task Local_Server_Handle_Server_Messages(CancellationToken cancellationToken)
    {
        try
        {
            byte[] buffer = new byte[10 * 1024 * 1024]; // 10 MB

            while (!cancellationToken.IsCancellationRequested)
            {
                int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                if (bytesRead == 0) // Server disconnected
                    break;

                // Create random number
                Random random = new Random();
                int randomNumber = random.Next(0, 1000);

                Console.WriteLine($"Random number: {randomNumber}");

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received message: {message}");

                // Command json
                Command command = JsonSerializer.Deserialize<Command>(message);

                if (command.type == "0") // Screen Capture
                {
                    // Capture screen as image directly to memory stream
                    string base64Image = Helper.ScreenCapture.CaptureScreenToBase64(Convert.ToInt32(command.remote_control_screen_index));

                    //Console.WriteLine($"Screen captured: {base64Image.Length} bytes");

                    // Save the image to a file (optional for testing purposes only)
                    //File.WriteAllBytes("screenshot.png", Convert.FromBase64String(base64Image));

                    await Local_Server_Send_Message($"screen_capture${command.response_id}${base64Image}");
                }
                else if (command.type == "1") // Move Mouse
                {
                    // Split the mouse coordinates
                    string[] mouseCoordinates = command.remote_control_mouse_xyz.Split(',');
                    int x = Convert.ToInt32(mouseCoordinates[0]);
                    int y = Convert.ToInt32(mouseCoordinates[1]);

                    // Move the mouse to the specified coordinates
                    Helper.MouseControl.MoveMouse(x, y);

                    Thread.Sleep(50);

                    MouseControl.ClickMouse();


                    await Local_Server_Send_Message($"mouse_moved${command.response_id}");
                }

                // Handle specific server messages (process based on type)
                string[] messageParts = message.Split('$');
                if (messageParts[0] == "device_identity")
                {
                    Console.WriteLine($"Device Identity: {messageParts[1]}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to handle server messages: {ex.Message}");
        }
    }

    public async Task Local_Server_Send_Message(string message)
    {
        try
        {
            if (_stream != null && _client.Connected)
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                await _stream.WriteAsync(messageBytes, 0, messageBytes.Length);
                await _stream.FlushAsync();

                Console.WriteLine($"Sent message to remote agent");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send message to the local server: {ex.Message}");
        }
    }

    public void Disconnect()
    {
        _cancellationTokenSource.Cancel();
        _stream?.Close();
        _client?.Close();
        Console.WriteLine("Disconnected from the server.");
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        var client = new UserClient();
        await client.Local_Server_Connect();

        // Keep the application running to listen for server messages
        Console.WriteLine("Press Enter to disconnect...");
        Console.ReadLine();

        client.Disconnect();
    }
}
