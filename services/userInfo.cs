using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging; // Add the appropriate namespace for ILogger

public class UserInformationServices
{
    private readonly ILogger<UserInformationServices> _logger; // Use ILogger<T> instead of ILogger
    private static DatabaseConnection dbConnection = DatabaseConnection.GetInstance();

    public UserInformationServices(ILogger<UserInformationServices> logger) // Use ILogger<T> instead of ILogger
    {
        _logger = logger;
        dbConnection = DatabaseConnection.GetInstance();
        dbConnection.OpenConnection();
    }

    public async Task<string> GetOrCreateUser(string deviceID, string name)
    {
        JObject userObject = await IsExistingUserBasedOnDeviceIDAndName(deviceID, name);
        if (userObject?.HasValues == true)
        {
            return userObject["UserID"]?.ToString() ?? "";
        }
        else
        {
            await RemoveOtherPrimaryUser(deviceID);
            userObject = await CreateUser(deviceID, name);
            return userObject["UserID"]?.ToString() ?? "";
        }
    }

    public async Task RemoveOtherPrimaryUser(string deviceID)
    {
        string query = $"UPDATE UserData SET IsPrimaryUser = 0 WHERE DeviceID = '{deviceID}'";
        await dbConnection.ExecuteQueryAsync(query);
    }

        private async Task<JObject> CreateUser(string deviceID, string name)
    {
        JObject userObject = new JObject();
        string userID = $"{deviceID}_{name}";
        userObject["UserID"] = userID;
        userObject["DeviceID"] = deviceID;
        userObject["Name"] = name;
        userObject["IsPrimaryUser"] = 1;
        string query = $"INSERT INTO UserData (UserID, DeviceID, Name, IsPrimaryUser) " +
               $"VALUES ('{userID}', '{deviceID}', '{name}', 1)";
        await dbConnection.ExecuteQueryAsync(query);
        return userObject;
    }

    public async Task<JObject> IsExistingUserBasedOnDeviceIDAndName(string deviceID, string name)
    {
        string query = $"SELECT * FROM UserData WHERE DeviceID = '{deviceID}' AND Name = '{name}'";
        List<string> userDetails = await dbConnection.ExecuteQueryAsync(query);
        if (userDetails.Count > 0)
        {
            JObject userObject = JObject.Parse(userDetails[0]);
            return userObject;
        }
        return null;
    }

}