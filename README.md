# Ogu.Extensions.Logging.Timings

[![.NET Core Desktop](https://github.com/ogulcanturan/Ogu.Extensions.Logging.Timings/actions/workflows/dotnet.yml/badge.svg?branch=master)](https://github.com/ogulcanturan/Ogu.Extensions.Logging.Timings/actions/workflows/dotnet-desktop.yml)
[![NuGet](https://img.shields.io/nuget/v/Ogu.Extensions.Logging.Timings.svg?color=1ecf18)](https://nuget.org/packages/Ogu.Extensions.Logging.Timings)
[![Nuget](https://img.shields.io/nuget/dt/Ogu.Extensions.Logging.Timings.svg?logo=nuget)](https://nuget.org/packages/Ogu.Extensions.Logging.Timings)

## Introduction

This library brings the feature-rich capabilities of [SerilogTimings](https://github.com/nblumhardt/serilog-timings/) to Microsoft.Extensions.Logging. Designed to seamlessly integrate with Microsoft.Extensions.Logging, this library enables developers to incorporate structured timing measurements into their log entries, enhancing the logging experience and providing valuable performance insights. If you are already familiar with [SerilogTimings](https://github.com/nblumhardt/serilog-timings/), you'll find it easy to use this library as it maintains the same ease of use and flexibility while targeting Microsoft.Extensions.Logging.


## Features

- Structured Timing Logs: Add precise timing information to your log entries using structured logs, enhancing your ability to analyze and diagnose application performance.
- Effortless Integration: Seamlessly integrate the timing functionality into your existing Microsoft.Extensions.Logging workflow without any additional dependencies.
- Flexibility: Enjoy the benefits of timings in your Microsoft.Extensions.Logging-based projects, keeping the freedom to choose your preferred logging providers.

## Installation

You can install the library via NuGet Package Manager:

```bash
dotnet add package Ogu.Extensions.Logging.Timings
```
## Usage

**example 1:**
```csharp
using (logger.TimeOperation("User: {UserId} is saving to database", userId))
{
    ...
}
```

output

```bash
info: Timings.Console.Program[0]
      User: 1 is saving to database completed in 0.4712ms
```

**example 2:**
```csharp
using (var op = logger.BeginOperation("User: {UserId} is removing from database.", userId))
{
    if (true)
    {
        op.Abandon();
    }

    // Do some operations

    op.Complete("Username", "ogulcanturan");
}
```

output

```bash
warn: Timings.Console.Program[0]
      User: 1 is removing from database. abandoned in 0.0581ms
```

**example 3:**
```csharp
using (var op = logger.BeginOperation("You will not see this message on console, because of this statement => 'op.Cancel()'"))
{
    if (true)
    {
        op.Cancel();
    }

    // Do some operations

    op.Complete();
}
```

output

```bash

```


**example 4:**
```csharp
using (var op = logger.BeginOperation("Doing some operations..."))
{
    try
    {
        int calculation = int.Parse("You cannot parse this to number!");
    }
    catch (Exception ex)
    {
        op.SetException(ex);
    }

    op.Complete(); // If you don't call 'op.Complete()' it will call implicitly 'op.Abandon()'
}
```

output

```bash
info: Timings.Console.Program[0]
      Doing some operations... completed in 1434.2430ms
      System.FormatException: The input string 'You cannot parse this to number!' was not in a correct format.
         at System.Number.ThrowOverflowOrFormatException(ParsingStatus status, ReadOnlySpan`1 value, TypeCode type)
         at System.Int32.Parse(String s)
         at Timings.Console.Program.Main(String[] args) in ...
```


**example 5:**
```csharp
using (logger.OperationAt(LogLevel.Trace).Time("App is closing..."))
{
    // Do some operations
}
```

output

```bash
trce: Timings.Console.Program[0]
      App is closing... completed in 0.0015ms
```
