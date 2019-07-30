# Example CSOD Reporting API code

## Introduction

This project provides sample code for authenticating and consuming Cornerstone OnDemand's (CSOD) [Reporting API](https://apiexplorer.csod.com/apiconnectorweb/apiexplorer#/apidoc/7f1beda8-ec8a-41ad-a615-417d27d8e568).

It utilizes CSOD's OAuth 2.0 authentication to obtain an access token via provided ClientId and ClientSecret.

It also provides an example of the paging functionality of CSOD APIs.

## Provisioning Access and Configuring the Project

1. Follow directions in the API documentation to [register an application with OAuth 2.0](https://apiexplorer.csod.com/apiconnectorweb/apiexplorer#/info).
    1. You will be provided with a *ClientId* and *ClientSecret*.
1. Next, modify `ConsoleApp/Program.cs` file and replace:
    1. *ClientId* value with the value from step 1
    1. *ClientSecret* value with the value from Step 1
    1. The `[portal]` section of *ApiBaseAddress* value with your client's portal URL

As an example, your modified file would appear as follows:

```csharp
    private const string ApiBaseAddress = "https://acme.csod.com";
    private const string ClientId = "test";
    private const string ClientSecret = "asasdf3241234va";
```

## Building the Project

Open `edge-api-samples.sln` with your C# IDE of choice and build the project.  `ConsoleApp/Program.cs` is the entry point of the solution, and the solution is configured to generate a simple console application.

Running the project will then launch a console application that presents several options for calling Reporting API.  The project is currently calling the `vw_rpt_user` resource for example purposes.

Options for calls:

* Get metadata only
* Get only counts from `vw_rpt_user`
* Get all data from `vw_rpt_user`
* Get data from `vw_rpt_user` by pages

## Source file descriptions

* `Api/EdgeApiClient.cs` - Using this class to Call the Api.
* `Api/EdgeApiError.cs` - Using this class to Build Error Message.
* `Api/EdgeApiODataPayload.cs` - This is a used to bind the response from the service for following properties like (Context,NextLink,Count and ErrorValue).
* `ConsoleApp/Program.cs` - This is the execution class to call the OAuth2 Get Access Token and return the token and corresponding details.  Users has to set the values for ClientId and ClientSecret  according to the portal they are looking for.
