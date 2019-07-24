# NHS Login Demo

This repo contains a sample application of how to connect to NHS Login using ASP.NET Core.

## Prerequisites:

 - VS Code/Visual Studio
 - ASP.NET Core 2.2
 - Generated keypair
 - Access to NHS login sandpit

## Getting started:

Insert your client details into the following files which are located in the root of the project.

**Startup.cs**
```
   .AddOpenIdConnect(options =>
                {
                    options.ClientId = "YOUR-CLIENT-ID";
                    options.Authority = "https://auth.sandpit.signin.nhs.uk/";
                    options.ResponseType = "code";
```

**TokenHelper.cs**

```
var payload = new Dictionary<string, object>()
            {
                {"sub", "YOUR-CLIENT-ID"},
                {"aud", "https://auth.sandpit.signin.nhs.uk/token"},
                {"iss", "YOUR-CLIENT-ID"},
                {"exp", DateTimeOffset.Now.AddMinutes(60).ToUnixTimeSeconds() },
                {"jti", Guid.NewGuid()}
            };
```


To run the sample, run the following command in the root of the project.
```
    dotnet run
```
