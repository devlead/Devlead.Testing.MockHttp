# Devlead.Testing.MockHttp

Opinionated .NET source package for mocking HTTP client requests

## Installation

```bash
dotnet add package Devlead.Testing.MockHttp
```

## Usage

### Configuration

The `Routes.json` file is used to configure the mock HTTP client responses. It defines the routes, HTTP methods, URIs, optional headers, and authentication requirements. Each route specifies the expected request and the corresponding response, including content type, status code, and any required authorization.

### Embedded Resources

The `Routes.json` file, along with other resources like `Index.txt` and `Secret.json`, are included as embedded resources in the project. This allows them to be accessed at runtime for simulating HTTP responses. These resources are configured in the `.csproj` file under `<EmbeddedResource>` tags.

### Example Configuration

#### Routes.json

```json
[
  {
    "Request": {
      "Methods": [
        {
          "Method": "GET"
        }
      ],
      "AbsoluteUri": "https://example.com/login/secret.json"
    },
    "Responses": [
      {
        "RequestHeaders": {},
        "ContentResource": "Example.Login.Secret.json",
        "ContentType": "application/json",
        "ContentHeaders": {},
        "StatusCode": 200
      }
    ],
    "Authorization": {
      "Authorization": [
        "Bearer AccessToken"
      ]
    }
  },
  {
    "Request": {
      "Methods": [
        {
          "Method": "GET"
        }
      ],
      "AbsoluteUri": "https://example.com/index.txt"
    },
    "Responses": [
      {
        "RequestHeaders": {},
        "ContentResource": "Example.Index.txt",
        "ContentType": "text/plain",
        "ContentHeaders": {},
        "StatusCode": 200
      }
    ]
  }
]
```

#### Example Resource Files

- **Index.txt**: Contains plain text data used in responses.

```plaintext
Hello, World!
```

- **Secret.json**: Contains JSON data representing a user, used in responses.

```json
{
  "Login": "johdoe",
  "FirstName": "John",
  "LastName": "Doe"
}
```

### Project Configuration

In the `.csproj` file, these resources are specified as embedded resources:

```xml
<ItemGroup>
  <EmbeddedResource Include="Resources\Example\Index.txt" />
  <EmbeddedResource Include="Resources\Routes.json" />
  <EmbeddedResource Include="Resources\Example\Login\Secret.json" />
</ItemGroup>
```

These configurations enable the mock HTTP client to simulate real HTTP requests and responses, facilitating testing without actual network calls.

### Registering and Using the Mock HTTP Client

The mock HTTP client is registered using dependency injection in the `ServiceProviderFixture.cs` file. This setup allows for simulating HTTP requests and responses in a controlled test environment.

#### Registration

Create a class called `ServiceProviderFixture.cs` without name space and implement the partial method:

```csharp
public static partial class ServiceProviderFixture
{
    static partial void InitServiceProvider(IServiceCollection services)
    {
        services.AddSingleton<MyService>()
                .AddMockHttpClient<Constants>();
    }
}
```

The `AddMockHttpClient<Constants>()` method configures the HTTP client for use in tests. Here, `Constants` is a type that serves as a parent to the resources configuration, encapsulating settings and paths for the mock HTTP client to use during testing.

#### Usage in Tests

- **HttpClientTests.cs**: The mock HTTP client is used to perform HTTP GET requests, and the responses are verified.

```csharp
public class HttpClientTests
{
    [Test]
    public async Task GetAsync()
    {
        var httpClient = ServiceProviderFixture.GetRequiredService<HttpClient>();
        var response = await httpClient.GetAsync("https://example.com/index.txt");
        await Verify(response);
    }
}
```

- **MyServiceTests.cs**: The mock HTTP client is used within the `MyService` class to test various scenarios, including unauthorized and authorized access.

```csharp
public class MyServiceTests
{
    [Test]
    public async Task GetSecret()
    {
        var myService = ServiceProviderFixture.GetRequiredService<MyService>(
                            configure => configure.ConfigureMockHttpClient<Constants>(
                                            client => client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                                                "Bearer",
                                                "AccessToken"
                                                )
                                        )
                            );
        var result = await myService.GetSecret();
        await Verify(result);
    }
}
```

This approach ensures that your service logic is tested in isolation, without making actual network requests, by simulating HTTP interactions using the mock HTTP client. The `Constants` type helps manage the configuration of these interactions, providing a centralized way to define and access resource settings.


## Example project

A real world example can be found in the [Blobify](https://github.com/devlead/Blobify) project.
