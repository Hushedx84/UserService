using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        string usersDataPath = "Users.csv";
        UserService userService = new UserService("http://localhost:9090/", usersDataPath);
        await userService.Start();
    }
}

public class UserService
{
    private HttpListener _listener;
    private string _usersDataPath;

    public UserService(string prefix, string usersDataPath)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add(prefix);
        _usersDataPath = usersDataPath;
    }

    public async Task<string> GetTablets()
    {
        using (var client = new HttpClient())
        {
            try
            {
                var response = await client.GetAsync("http://localhost:9091/tablets");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    Console.WriteLine("Помилка при отриманні даних планшетів.");
                    return null;
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Помилка HTTP: {e.Message}");
                return null;
            }
        }
    }

    public async Task Start()
    {
        _listener.Start();
        Console.WriteLine("UserService слухає...");

        try
        {
            while (true)
            {
                var context = await _listener.GetContextAsync();
                var request = context.Request;
                var response = context.Response;

                if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/register")
                {
                    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        var userData = reader.ReadToEnd();
                        // TODO: Добавьте проверку и обработку userData перед записью.
                        File.AppendAllText(_usersDataPath, userData + "\n");
                    }

                    var responseString = "Реєстрація користувача пройшла успішно!";
                    var buffer = Encoding.UTF8.GetBytes(responseString);
                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }

                response.OutputStream.Close();
            }
        }
        finally
        {
            _listener.Close();
        }
    }
}

