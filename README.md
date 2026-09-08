[![CI](https://github.com/PiotrKasperski/netwise-recruitment-task/actions/workflows/ci.yml/badge.svg)](https://github.com/PiotrKasperski/netwise-recruitment-task/actions/workflows/ci.yml)
# Netwise Recruitment Task

A .NET 10 console application that retrieves random cat facts from the [catfact.ninja](https://catfact.ninja/) API and stores them in a local text file.

The application supports both **interactive** and **non-interactive** modes and includes automated unit tests covering the main application behavior.

## Features

* Fetch random cat facts from `catfact.ninja`
* Save fetched facts to `cat_facts.txt`
* Interactive console mode
* `--once` command-line option
* `--count <n>` command-line option
* Interactive `once` and `count <n>` commands
* `help` and `exit` commands
* Graceful cancellation and application shutdown
* Configurable API endpoint, timeout and output file
* Configuration validation on application startup
* Dependency injection using `Microsoft.Extensions.Hosting`
* Typed `HttpClient`
* Automated unit tests with MSTest and Moq
* Code coverage support
* CI pipeline with GitHub Actions

## Requirements

* .NET 10 SDK

## Running the application

Run the application from the repository root:

```bash
dotnet run --project NetwiseRecruitmentTask
```

By default, the application starts in interactive mode.

### Interactive mode

The application provides the following commands:

```text
ENTER: new fact | once | count <n> | help | exit
```

Pressing **Enter** without entering a command fetches a new cat fact.

Available commands:

| Command     | Description                |
| ----------- | -------------------------- |
| `ENTER`     | Fetch a new cat fact       |
| `once`      | Fetch a single cat fact    |
| `count <n>` | Fetch `n` cat facts        |
| `help`      | Display available commands |
| `exit`      | Close the application      |

Commands are case-insensitive.

### Non-interactive mode

A single fact can be fetched using:

```bash
dotnet run --project NetwiseRecruitmentTask -- --once
```

Multiple facts can be fetched using:

```bash
dotnet run --project NetwiseRecruitmentTask -- --count 10
```

The `--count` value must be a positive integer.

These modes automatically terminate after the requested number of fetch operations has completed.

## Output

Fetched facts are stored in:

```text
cat_facts.txt
```

The file is created in the **application binary directory** (`AppContext.BaseDirectory`).

Each saved fact has the following format:

```text
Fact: Cats sleep for most of their lives. length: 38
```

## Configuration

Application configuration is stored in:

```text
NetwiseRecruitmentTask/appsettings.json
```

Example:

```json
{
  "CatFactSettings": {
    "BaseAddress": "https://catfact.ninja/",
    "FactEndpoint": "fact",
    "RequestTimeoutSeconds": 10,
    "OutputFileName": "cat_facts.txt"
  }
}
```

The configuration is validated when the application starts.

The following settings are available:

| Setting                 | Description                          |
| ----------------------- | ------------------------------------ |
| `BaseAddress`           | Base URL of the cat facts API        |
| `FactEndpoint`          | API endpoint used to retrieve a fact |
| `RequestTimeoutSeconds` | HTTP request timeout                 |
| `OutputFileName`        | Name of the output file              |

## Running tests

Run the complete test suite with:

```bash
dotnet test
```

The tests cover:

* `Worker`
* `CatFactApiService`
* `FilesystemService`
* `ServiceConfiguration`
* command-line behavior
* interactive commands
* invalid commands and arguments
* API failures
* filesystem failures
* cancellation
* dependency injection registrations
* configuration validation

### Main components

#### `Worker`

Responsible for application flow:

* interactive command processing
* command-line execution
* fetching requested facts
* graceful shutdown
* error handling

#### `CatFactApiService`

Encapsulates communication with `catfact.ninja`.

It uses a typed `HttpClient` and supports cancellation through `CancellationToken`.

#### `FilesystemService`

Responsible for:

* determining the output file location
* creating the output file when necessary
* asynchronously appending facts to the file

#### `ConsoleService`

Provides an abstraction over console input and output, making the `Worker` easier to test.

#### `CommandLineOptions`

Parses command-line arguments and exposes the selected execution mode to the application.

## Project structure

```text
.
├── NetwiseRecruitmentTask
│   ├── Models
│   │   ├── CatFact.cs
│   │   └── CatFactSettings.cs
│   ├── Services
│   │   ├── CatFactApiService.cs
│   │   ├── ConsoleService.cs
│   │   ├── FilesystemService.cs
│   │   └── Interfaces
│   ├── CommandLineOptions.cs
│   ├── Program.cs
│   ├── ServiceConfiguration.cs
│   ├── Worker.cs
│   ├── appsettings.json
│   └── NetwiseRecruitmentTask.csproj
│
├── NetwiseRecruitmentTask.Test
│   ├── CatFactApiServiceTests.cs
│   ├── FilesystemServiceTests.cs
│   ├── ServiceConfigurationTests.cs
│   ├── WorkerTests.cs
│   ├── MSTestSettings.cs
│   └── NetwiseRecruitmentTask.Test.csproj
│
├── .github
│   └── workflows
│       └── ci.yml
│
├── .gitignore
├── LICENSE
├── README.md
└── NetwiseRecruitmentTask.slnx
```

## CI

The repository uses GitHub Actions to automatically:

1. restore dependencies
2. verify code formatting
3. build the solution
4. run the test suite

The workflow is located at:

```text
.github/workflows/ci.yml
```

## Design decisions

### Generic Host

The application uses `Microsoft.Extensions.Hosting` instead of manually constructing dependencies. This provides:

* dependency injection
* configuration
* logging
* application lifetime management
* a familiar .NET application structure

### Dependency inversion

External concerns such as HTTP communication, filesystem access and console I/O are hidden behind interfaces.

This allows the `Worker` to be tested without performing real HTTP requests or filesystem operations.

### Cancellation

`CancellationToken` is propagated through the application from the hosted service down to HTTP, console and filesystem operations.

Cancellation is treated as a normal application shutdown rather than an application error.

### Configuration validation

Application settings are validated on startup using data annotations. Invalid configuration therefore causes the application to fail early instead of producing an error during normal execution.

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.
