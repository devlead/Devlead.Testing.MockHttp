# Devlead.Testing.MockHttp.Tool

.NET global tool that hosts a `Routes.json` file and response resources as a local mock HTTP API.

## Installation

```bash
dotnet tool install -g Devlead.Testing.MockHttp.Tool
```

## Usage

```bash
dotnet mockhttp serve <RESOURCES> [--port 5080] [--url http://127.0.0.1:5080] [--match-host]
```

`serve` is the default command, so `<RESOURCES>` can be passed directly.

- `<RESOURCES>` is a directory that contains `Routes.json` and the response files it references.
- `--port` binds `http://127.0.0.1:<port>` (default 5080).
- `--url` overrides `--port` and uses the URL authority only.
- `--match-host` requires the request `Host` to match the host in each route `AbsoluteUri`. By default only HTTP method, path, and query are matched so a client can use the mock as its `BaseAddress`.

Point an `HttpClient` at the listen URL:

```bash
dotnet mockhttp serve ./Resources --port 5080
```

```csharp
var client = new HttpClient { BaseAddress = new Uri("http://127.0.0.1:5080") };
var response = await client.GetAsync("/index.txt");
```

```bash
curl http://127.0.0.1:5080/index.txt
curl -i -H "Authorization: Bearer AccessToken" http://127.0.0.1:5080/login/secret.json
```

Those requests match `Routes.json` entries whose `AbsoluteUri` values are `https://example.com/index.txt` and `https://example.com/login/secret.json`.

While the host is running, the console shows a live table of configured routes and recent requests. Open `http://127.0.0.1:5080/inspect` for the inspect page: route enable/disable, request activity, and DevTools-style request/response headers. `GET /inspect/api` returns the same snapshot as JSON.

## Routes.json and resources

The JSON contract is the same as [Devlead.Testing.MockHttp](https://github.com/devlead/Devlead.Testing.MockHttp): methods, `AbsoluteUri`, optional authorization, request-header-specific responses, status codes, and `EnableRequests` / `DisableRequests`.

`ContentResource` values such as `Example.Login.Secret.json` resolve to `Example/Login/Secret.json` under the resources directory.
