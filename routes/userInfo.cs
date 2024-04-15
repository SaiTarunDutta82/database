using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class UserInformationRouter
{
    private readonly ILogger _logger;
    private userInformationServices _userInfoService;

    public UserInformationRouter(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<UserInformationRouter>();
        _userInfoService = new userInformationServices(_logger);
    }

    [Function("GetOrCreateUser")]
    public async Task<IActionResult> GetOrCreateUser(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "user")] HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("Processing a request to create or get a new user");
            string? deviceID = req.Query["deviceID"];
            string? name = req.Query["name"];
            string? profilePic = req.Query["profilePicture"] ?? "1";

            if (string.IsNullOrEmpty(deviceID) || string.IsNullOrEmpty(name))
            {
                var errorResponse = new { message = "Error: deviceID is required." };
                string errorJsonResponse = JsonConvert.SerializeObject(errorResponse);
                return new BadRequestObjectResult(errorJsonResponse);
            }

            string userID = await _userInfoService.GetOrCreateUser(deviceID, name, profilePic);

            return new ContentResult
            {
                Content = userID,
                ContentType = "application/json",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing the request");
            return new BadRequestObjectResult("Error: Invalid request.");
        }
    }


    [Function("GetAllUsers")]
    public async Task<IActionResult> GetAllUsers(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "user")] HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("Processing a request to all new users");
            string? deviceID = req.Query["deviceID"];

            if (string.IsNullOrEmpty(deviceID))
            {
                var errorResponse = new { message = "Error: deviceID is required." };
                string errorJsonResponse = JsonConvert.SerializeObject(errorResponse);
                return new BadRequestObjectResult(errorJsonResponse);
            }

            string users = await _userInfoService.GetAllUsers(deviceID);

            // Construct the JSON object string with the "users" key
            string jsonObject = $"{{ \"users\": {users} }}";

            return new ContentResult
            {
                Content = jsonObject,
                ContentType = "application/json",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing the request");
            return new BadRequestObjectResult("Error: Invalid request.");
        }
    }


    [Function("PrimaryUser")]
    public async Task<IActionResult> UpdatePrimaryUser(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "user/{userID}/primaryUser")] HttpRequestData req, string userID)
    {
        try
        {
            _logger.LogInformation("Processing a request to all new users");
            string? deviceID = req.Query["deviceID"];

            if (string.IsNullOrEmpty(deviceID) || string.IsNullOrEmpty(userID))
            {
                var errorResponse = new { message = "Error: deviceID/UserId is required." };
                string errorJsonResponse = JsonConvert.SerializeObject(errorResponse);
                return new BadRequestObjectResult(errorJsonResponse);
            }

            await _userInfoService.UpdateUserAsPrimary(deviceID, userID);
            return new ContentResult
            {
                Content = "Updated Primary User Successfully...",
                ContentType = "application/json",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing the request");
            return new BadRequestObjectResult("Error: Invalid request.");
        }
    }


    [Function("updateName")]
    public async Task<IActionResult> UpdateName(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "user/{userID}/updateName")] HttpRequestData req, string userID)
    {

        try
        {
            _logger.LogInformation("Processing a request to update a name for userID");
            string? name = req.Query["name"];

            if (string.IsNullOrEmpty(userID))
            {
                var errorResponse = new { message = "Error: userID is required." };
                string errorJsonResponse = JsonConvert.SerializeObject(errorResponse);
                return new BadRequestObjectResult(errorJsonResponse);
            }

            if (string.IsNullOrEmpty(name))
            {
                var errorResponse = new { message = "Error: name is required." };
                string errorJsonResponse = JsonConvert.SerializeObject(errorResponse);
                return new BadRequestObjectResult(errorJsonResponse);
            }

            await _userInfoService.UpdateName(userID, name);

            return new ContentResult
            {
                Content = "Updated Name Successfully...",
                ContentType = "application/json",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing the request");
            return new BadRequestObjectResult("Error: Invalid request.");
        }
    }
}