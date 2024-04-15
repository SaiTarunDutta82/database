using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class UserInformationRouter
{
    private readonly ILogger<UserInformationRouter> _logger;
    private readonly UserInformationServices _userInfoService;

    public UserInformationRouter(ILoggerFactory loggerFactory, UserInformationServices userInfoService)
    {
        _logger = loggerFactory.CreateLogger<UserInformationRouter>();
        _userInfoService = userInfoService;
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

            if (string.IsNullOrEmpty(deviceID) || string.IsNullOrEmpty(name))
            {
                return new BadRequestObjectResult("Error: deviceID or name is required.");
            }

            string userID = await _userInfoService.GetOrCreateUser(deviceID, name);

            return new OkObjectResult(userID);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing the request");
            return new BadRequestObjectResult("Error: Invalid request.");
        }
    }
}
    