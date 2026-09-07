
# Netwise Recruitment Task

[![CI](https://github.com/PiotrKasperski/netwise-recruitment-task/actions/workflows/ci.yml/badge.svg)](https://github.com/PiotrKasperski/netwise-recruitment-task/actions/workflows/ci.yml)

A .NET application that fetches random cat facts from the [Cat Facts API](https://catfact.ninja/) and saves them to a local `cat_facts.txt` file.

## Requirements

* .NET 10 SDK

## Running the application

To run the application, execute:

```bash
dotnet run --project NetwiseRecruitmentTask
```

The application fetches a cat fact from the Cat Facts API and saves the result to `cat_facts.txt` in the application's binary output directory.

For example:

```text
NetwiseRecruitmentTask/
└── bin/
    └── Debug/
        └── net10.0/
            ├── NetwiseRecruitmentTask
            └── cat_facts.txt
```

## Running tests

To run the test suite:

```bash
dotnet test
```

## Project structure

```text
NetwiseRecruitmentTask/
├── NetwiseRecruitmentTask/
│   └── ...
├── NetwiseRecruitmentTask.Test/
│   └── ...
├── NetwiseRecruitmentTask.slnx
└── README.md
```

## API

The application uses the [Cat Facts API](https://catfact.ninja/) to retrieve cat facts.

## License

This project is licensed under the MIT License.

See the [LICENSE](LICENSE) file for details.

