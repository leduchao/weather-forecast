using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Net.Mail;
using WeatherForecast.Models;

namespace WeatherForecast.Controllers;

[ApiController]
[Route("api/v1/weather")]
public class WeatherController : ControllerBase
{
    private const string BASE_URL = "http://api.weatherapi.com/v1";
    private const string PATH = "subcribed_emails.txt";
    private const string CODE_PATH = "codes.txt";

    private readonly string _apiKey;

    private readonly IHttpClientFactory _httpClientFactory;

    private readonly HttpClient _httpClient;

    private readonly IConfiguration _configuration;

    private readonly ILogger<WeatherController> _logger;

    public WeatherController(
        ILogger<WeatherController> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _apiKey = _configuration.GetSection("WeatherApiKey").Value ?? "secret-key";
        _httpClient = _httpClientFactory.CreateClient();
    }

    [HttpGet]
    [Route("get-current-weather")]
    public async Task<ActionResult> GeCurrentWeathert(string q)
    {
        var httpResponseClient = await _httpClient.GetAsync($"{BASE_URL}/current.json?key={_apiKey}&q={q}");

        if (httpResponseClient.IsSuccessStatusCode)
        {
            _logger.LogInformation("Send request successfully!");
            var response = await httpResponseClient.Content.ReadAsStringAsync();
            var jsonConvert = JsonConvert.DeserializeObject<Weather>(response);

            return Ok(jsonConvert);
        }

        return BadRequest(httpResponseClient.StatusCode.ToString());
    }

    [HttpGet]
    [Route("get-current-weather-by-lat-lon")]
    public async Task<ActionResult> GeCurrentWeathertByLatLon(string q)
    {
        var httpResponseClient = await _httpClient.GetAsync($"{BASE_URL}/search.json?key={_apiKey}&q={q}");

        if (httpResponseClient.IsSuccessStatusCode)
        {
            _logger.LogInformation("Send request successfully!");
            var response = await httpResponseClient.Content.ReadAsStringAsync();
            var jsonConvert = JsonConvert.DeserializeObject<Location[]>(response);

            return Ok(jsonConvert);
        }

        return BadRequest(httpResponseClient.StatusCode.ToString());
    }

    [HttpGet]
    [Route("get-weather-forecast")]
    public async Task<ActionResult> GetWeatherForecastAsync(string q, int days = 4)
    {
        var httpResponseClient = await _httpClient.GetAsync($"{BASE_URL}/forecast.json?key={_apiKey}&q={q}&days={days}");
        var response = await httpResponseClient.Content.ReadAsStringAsync();

        if (httpResponseClient.IsSuccessStatusCode)
        {
            var jsonWeather = JsonConvert.DeserializeObject<Weather>(response);
            return Ok(jsonWeather);
        }

        var jsonError = JsonConvert.DeserializeObject<ApiError>(response);
        return BadRequest(jsonError);
    }

    [HttpPost]
    [Route("subscribe-email")]
    public async Task<ActionResult<string>> SubscribeEmail(string email)
    {
        // kiểm tra email đã tồn tại chưa
        if (IsSubEmail(email, PATH))
            //return Ok(
            //    new SubscribeResponse
            //    {
            //        Code = 10,
            //        Message = "This email is already subscribed!"
            //    });
            return Ok("This email is already subscribed!");

        // tạo mã code
        var random = new Random();
        var code = random.Next(100000, 1000000);

        // lưu mã code vào file text
        if (!System.IO.File.Exists(CODE_PATH))
        {
            using var fileStream = System.IO.File.Create(CODE_PATH);
        }

        using var streamWriterCode = System.IO.File.AppendText(CODE_PATH);
        {
            streamWriterCode.WriteLine($"{email}:{code}");
        }

        // gửi mail
        string subject = "Verify email";
        string body = $"<h3>Mã xác nhận: <h1>{code}</h1></h3>";

        var result = await SendEmail(email, subject, body);

        if (result)
        {
            //return Ok(
            //    new SubscribeResponse
            //    {
            //        Code = 20,
            //        Message = "Send email successfully!"
            //    });

            return Ok("Send email successfully!");
        }

        return BadRequest("Can not send email!");
    }

    [HttpPost]
    [Route("confirm-email")]
    public ActionResult<string> ConfirmEmail(int code)
    {
        if (!System.IO.File.Exists(PATH))
        {
            var fileStream = System.IO.File.Create(PATH);
            fileStream.Dispose();
        }

        var codes = GetItemsFromTextFile(CODE_PATH);

        foreach (var c in codes)
        {
            var str = c.Split(":");

            if (str.Length == 2 && code.ToString() == str[1])
            {
                using var streamWriter = System.IO.File.AppendText(PATH);
                {
                    streamWriter.WriteLine(str[0]);
                }

                var isRemoved = codes.Remove(c);

                if (isRemoved) // xóa mã code
                {
                    System.IO.File.Delete(CODE_PATH);
                    System.IO.File.WriteAllLines(CODE_PATH, codes);

                    return Ok("Confirm successfully!");
                }

            }
        }

        return BadRequest("Wrong code or something! Please check the code that you entered!");
    }

    [HttpPost]
    [Route("unsubscribe-email")]
    public async Task<ActionResult> UnsubscribeEmail(string email)
    {
        var emails = GetItemsFromTextFile(PATH);

        var isRemoved = emails.Remove(email);

        if (isRemoved)
        {
            System.IO.File.Delete(PATH);
            System.IO.File.WriteAllLines(PATH, emails);

            var result = await SendEmail(
                email,
                "Unsubscribe email",
                "This email has been unsubscribed from WeatherApp! Thank you!");

            if (result)
                return Ok("This email has been unsubscribed!");
        }

        return NotFound("This email hasn't been subscribed!");
    }

    [HttpGet]
    [Route("send-daily-weather")]
    public ActionResult SendDailyWeather()
    {
        if (GetItemsFromTextFile(PATH).Count > 0)
        {
            //if ()
            return Ok();
        }

        return NotFound();
    }

    private async Task<bool> SendEmail(string to, string subject, string body)
    {
        var mailSettings = _configuration.GetSection("MailSettings");

        using var client = new SmtpClient(mailSettings["Host"])
        {
            Port = Convert.ToInt32(mailSettings["Port"]),
            Credentials = new NetworkCredential(
                mailSettings["MailAddress"],
                mailSettings["Password"]),
            EnableSsl = true
        };

        try
        {
            var message = new MailMessage(
                new MailAddress(mailSettings["MailAddress"]!,
                    "WeatherApp"),
                new MailAddress(to))
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            await client.SendMailAsync(message);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static List<string> GetItemsFromTextFile(string path)
    {
        var items = new List<string>();

        if (System.IO.File.Exists(path))
        {
            using var streamReader = System.IO.File.OpenText(path);
            {
                string s;

                while ((s = streamReader.ReadLine()!) != null)
                {
                    items.Add(s);
                }
            }
        }

        return items;
    }

    private static bool IsSubEmail(string email, string path)
    {
        var emails = GetItemsFromTextFile(path);

        foreach (var e in emails)
        {
            if (email == e) return true;

        }

        return false;
    }
}
