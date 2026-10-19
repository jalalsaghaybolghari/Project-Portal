using Microsoft.Identity.Client;

const string tenantId = "5bd92470-d1ef-4362-b42d-896c6960a6e7";
const string clientId = "de8db540-cc87-490f-974f-0221da26559b";

// var scopes = new[]
// {
//     "api://17a33b29-bcdc-4daa-9177-9c4faed1dda8/access_as_user"
// };

var scopes = new[]
{
    "api://17a33b29-bcdc-4daa-9177-9c4faed1dda8/projects_read"
};

var app = PublicClientApplicationBuilder
    .Create(clientId)
    .WithAuthority(
        $"https://login.microsoftonline.com/{tenantId}")
    .WithRedirectUri("http://localhost")
    .Build();

var result = await app
    .AcquireTokenInteractive(scopes)
    .WithPrompt(Prompt.ForceLogin)
    .ExecuteAsync();

Console.WriteLine("Access token acquired successfully.");
Console.WriteLine(result.AccessToken);