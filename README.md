## **CONTENT**

- Introduction
  
- Requirements
  
- Installation
  
- Setup
  

# **INTRODUCTION**

HackerRank is a project designed with the intention to gamify the development process and create incentives for the developer through achievements and scoring systems.<br><br>

# **REQUIREMENTS**<br>

.NET Core 5 <br>

.NET Tools<br>

UltraHook<br>

Ruby & RubyGems<br><br>

# **INSTALLATION**<br>

Install .NET Core:

[Download .NET (Linux, macOS, and Windows)](https://dotnet.microsoft.com/download)

Install .NET Tools:

[.NET tools - .NET CLI | Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools)

Install UltraHook:

[UltraHook - Receive webhooks on localhost](https://www.ultrahook.com/faq)

Install Ruby:

[RubyInstaller for Windows](https://rubyinstaller.org/)

Install RubyGems:

[Download RubyGems](https://rubygems.org/pages/download)<br><br>

# **SETUP**<br>

Clone the project

**Create migrations for the main database:** <br>

VS: <br>

`add-migration Init -Context HackerRankContext`

VS Code: <br>

`dotnet ef migrations add Init --context HackerRankContext`

**Update both databases:** <br>

VS: <br>

`update-database -Context HackerRankContext` <br>

`update-database -Context HangFireContext`

VS Code: <br>

`dotnet ef database update --context HackerRankContext` <br>

`dotnet ef database update --context HangFireContext`

**Create access token from GitLab:** <br>

Go to edit profile -> access token <br>

Create a new token with the following scopes: read_user, read_api, read_repository, read_registry <br>

This will generate your AccessToken <br>

**Create an application in GitLab:** <br>

Go to edit profile -> applications <br>

Create a new application with the following scopes: read_user, email, openid, profile <br>

This will generate your ClientSecret and ClientId <br>

These secret keys will be used in the next step

**Add user secrets:** <br>

`dotnet user-secrets init`

Change each value for the corresponding values generated in the previous step <br>

`dotnet user-secrets set "Authentication-GitLab-ClientSecret" "ClientSecret"` <br>

`dotnet user-secrets set "Authentication-GitLab-ClientId" "ClientId"` <br>

`dotnet user-secrets set "Authentication-GitLab-APIKey" "AccessToken"` <br>

For both of these commands, use a password manager to generate a long random string of characters to use as the "SecretToken"<br>

`dotnet user-secrets set "Authentication-GitLab-WebHookAuthentication" "SecretTokenForActions"`<br>

`dotnet user-secrets set "Authentication-GitLab-WebHookAuthenticationGroups" "SecretTokenForGroups"`<br>

**Webhooks:**<br>

Go to settings -> webhooks, of the parent group<br>

Create a new wehook with the following scopes: push events, coments, issue events, merge request events<br>The URL shoud follow the following structure: "https://localhost:YOUR_PORT_HERE/api/webhook/receive"<br>

The secret should be the SecretTokenForActions from the previous step<br>

Create yet another new wehook with the following scopes: member events and subgroup events<br>The URL shoud follow the following structure: "https://localhost:YOUR_PORT_HERE/api/webhook/receivegroups"<br>

The secret should be the SecretTokenForGroups from the previous step<br>

**UltraHook:**<br>

Run this command after installing ultrahook and following the instructions to get an API key<br>

`ultrahook gitlab https://localhost:YOUR_PORT_HERE/api/webhook/receive`<br>

If for some reason ultrahook cannot read the API key stored on your device use this command instead<br>

`ultrahook gitlab -k API_KEY_HERE https://localhost:YOUR_PORT_HERE/api/webhook/receive`<br>

Copy the URL which ultrahook generates (should look something like this: username-gitlab.ultrahook.com)<br>

Go to the project you want to track -> settings -> webhooks<br>

Add the URL you copied in the previous step to the URL field<br>

Add "SecretToken" you generated earlier to the secret token field<br>

Check the following scopes: push events, comments, merge request events, issue events and then click "Add webhook"<br>

**SMTP:**<br>

In the EmailService class update the host, port and options of the smtp client connection to match the smtp client you are using. [Papercut](https://github.com/ChangemakerStudios/Papercut-SMTP) is a good free option which runs locally on your machine.<br>

From your client of choice get both a ClientId and a ClientSecret and run the following commands:<br>

`dotnet user-secrets set "Authentication-MailTrap-ClientSecret" "ClientSecret"`<br>

`dotnet user-secrets set "Authentication-MailTrap-ClientId" "ClientId"`<br>

**That's it!**