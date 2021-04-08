CONTENTS OF THIS FILE
---------------------

 * Introduction
 * Requirements
 * Installation


INTRODUCTION
------------

HackerRank is a project designed with the intention to gamify the development process and create incentives for the developer through achievements and scoring systems.


REQUIREMENTS
------------

.NET Core 5 <br>
.NET Tools


INSTALLATION
------------

Install .NET Core:
https://dotnet.microsoft.com/download

Install tools:
https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools

Clone the project

**Create migrations for the main database:** <br>
VS: <br>
`add-migration Init -Context HackerRankContext`

VS Code: <br>
`dotnet ef migrations add Init --context HackerRankContext`

**Update both databases:** <br>
VS: <br>
`update-database -Context HackerRankContext` <br>
`update-database -Context HangFireContext`

VS Code: <br>
`dotnet ef database update --context HackerRankContext` <br>
`dotnet ef database update --context HangFireContext`

**Create access token from GitLab:** <br>
Go to edit profile -> access token <br>
Create a new token with the following scopes: read_user, read_api, read_repository, read_registry <br>
This will generate your AccessToken <br>

**Create an application in GitLab:** <br>
Go to edit profile -> applications <br>
Create a new application with the following scopes: read_user, email, openid, profile <br>
This will generate your ClientSecret and ClientId <br>

These secret keys will be used in the next step

**Add user secrets:** <br>
`dotnet user-secrets init`

Change each value for the corresponding values generated in the previous step <br><br>
`dotnet user-secrets set "Authentication:GitLab:ClientSecret" "ClientSecret"` <br>
`dotnet user-secrets set "Authentication:GitLab:ClientId" "ClientId"` <br>
`dotnet user-secrets set "Authentication:GitLab:APIKey" "AccessToken"` <br>

**That's it!**
