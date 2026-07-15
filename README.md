# SharedKernel — DDD & Clean Architecture building blocks for .NET

SharedKernel gives .NET developers a production-ready foundation for Domain-Driven Design and Clean Architecture. Rather than reinventing aggregate roots, value objects, CQRS buses, and MediatR pipeline behaviors in every project, you install the packages that match your architecture and focus on your domain logic.

It intentionally remains persistence-agnostic, allowing aggregates to be implemented with or without event sourcing.

## Why SharedKernel?

Every .NET project that follows DDD and Clean Architecture needs the same foundational pieces: strongly-typed entity IDs, aggregate roots that record domain events, a CQRS command/query bus, pipeline behaviors for validation and logging, and persistence abstractions. SharedKernel provides all of these as tested, composable NuGet packages.

# Give it a star ⭐

Loving it? Please show your support by giving this project a star!

## Example of Use 🚨

- [JordiAragonZaragoza.Cinema](https://github.com/jordiaragonzaragoza/JordiAragonZaragoza.Cinema/) A showcase project demonstrating a fictional cinema management system built with .NET.

  The solution follows Domain-Driven Design (DDD) and Event Sourcing principles, and adopts a Microservices Architecture with an Event-Driven approach and Vertical Slice Architecture.

## Read the documentation 📝

- Explore the [documentation](https://sharedkernel-jordiaragonzaragoza.mintlify.site/) to get a successful quickstart.

## Package overview 📦

SharedKernel is split into focused packages so you only add what you need:

| Package group | What it provides |
| --- | --- |
| **SharedKernel and Contracts** | Dependency injection, auto-registering services through markup interfaces |
| **Domain and Contracts** | Common contracts and base implementations for DDD principles: DomainEvents, ApplicationEvents, ValueObjects, Entities, Repository, BusinessRulesValidations, and other DDD building blocks |
| **Application and Contracts** | Application common contracts and base implementations for EventBus, UnitOfWork, and CQRS; common MediatR pipeline implementations; integration contracts and base implementation for the EventBus; common EventBus implementation using MassTransit |
| **Infrastructure and Contracts** | EF Core base implementation for auditing and outbox dispatch of domain event notifications; `BaseCachedRepository` built on the `ICacheService` abstraction; base implementation of the MassTransit event bus |
| **Presentation and Contracts** | HTTP RESTful API building blocks such as `BaseApiController`, `ExceptionMiddleware`, `CurrentUserService`, and a `ResponseBuilder` for ProblemDetails |
| **Presentation Integration and Contracts** | Integration bus building blocks, such as `IntegrationEventHandler` |

## NuGets ready to use: ⚙️

- [![JordiAragonZaragoza.SharedKernel - NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.svg?label=JordiAragonZaragoza.SharedKernel%20-%20nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel)
[![JordiAragonZaragoza.SharedKernel.Contracts - NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.Contracts.svg?label=JordiAragonZaragoza.SharedKernel.Contracts%20-%20nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Contracts)
- [![JordiAragonZaragoza.SharedKernel.Domain - NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.Domain.svg?label=JordiAragonZaragoza.SharedKernel.Domain%20-%20nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Domain)
[![JordiAragonZaragoza.SharedKernel.Domain.Contracts - NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.Domain.Contracts.svg?label=JordiAragonZaragoza.SharedKernel.Domain.Contracts%20-%20nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Domain.Contracts)
- [![JordiAragonZaragoza.SharedKernel.Application - NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.Application.svg?label=JordiAragonZaragoza.SharedKernel.Application%20-%20nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Application)
[![JordiAragonZaragoza.SharedKernel.Application.Contracts - NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.Application.Contracts.svg?label=JordiAragonZaragoza.SharedKernel.Application.Contracts%20-%20nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Application.Contracts)
[![JordiAragonZaragoza.SharedKernel.Application.Contracts.Integration - NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.Application.Contracts.Integration.svg?label=JordiAragonZaragoza.SharedKernel.Application.Contracts.Integration%20-%20nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Application.Contracts.Integration)
- [![JordiAragonZaragoza.SharedKernel.Infrastructure - NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.Infrastructure.svg?label=JordiAragonZaragoza.SharedKernel.Infrastructure%20-%20nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Infrastructure)
[![JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework - NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.svg?label=JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework%20-%20nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework)
[![JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore - NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.svg?label=JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore%20-%20nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore)
- [![JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi - NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.svg?label=JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi%20-%20nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi)
[![JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Contracts - NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Contracts.svg?label=JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Contracts%20-%20nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Contracts)
- [![JordiAragonZaragoza.SharedKernel.Presentation.Integration - NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.Presentation.Integration.svg?label=JordiAragonZaragoza.SharedKernel.Presentation.Integration%20-%20nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Presentation.Integration)
[![JordiAragonZaragoza.SharedKernel.Presentation.Integration.Contracts - NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.Presentation.Integration.Contracts.svg?label=JordiAragonZaragoza.SharedKernel.Presentation.Integration.Contracts%20-%20nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Presentation.Integration.Contracts)

## How to debug: 🐛

- You will need the latest Visual Studio 2022 and the latest .NET Core SDK (at least .NET 10 SDK)
- All the projects library are contained in JordiAragonZaragoza.SharedKernel.sln solution file.

## Used libraries: ⚒️

- Entity Framework
- KurrentDB.Client
- MediatR
- MassTransit
- FastEndpoints
- Ardalis.Result
- Ardalis.Specification
- Ardalis.SmartEnums
- Ardalis.GuardClauses
- FluentValidator
- Serilog
- StyleCop & SonarAnalyzer

## Testing 🧪

GitHub workflow CI with SonarCloud integration

[![SonarCloud](https://sonarcloud.io/images/project_badges/sonarcloud-white.svg)](https://sonarcloud.io/summary/new_code?id=jordiaragonzaragoza_JordiAragon.SharedKernel)

## Resources and Inspiration 🙏

Special thanks to all these authors for sharing their knowledge and expertise:

- <a href="https://github.com/ardalis/CleanArchitecture" target="_blank">Ardalis: Clean Architecture</a>
- <a href="https://github.com/jasontaylordev/CleanArchitecture" target="_blank">Jason Taylor: Clean Architecture</a>
- <a href="https://www.oreilly.com/library/view/implementing-domain-driven-design/9780133039900/" target="_blank">Vaughn Vernon: Implementing Domain-Driven Design (book)</a>
- <a href="https://kalele.io/books/ddd-destilado/" target="_blank">Vaughn Vernon: Domain-Driven Design Destilado (book)</a>
- <a href="https://www.amazon.com/Hands-Domain-Driven-Design-NET-ebook/dp/B07C5WSR9B" target="_blank">Alexey Zimarev: Hands-on Domain-Driven Design (book)</a>
- <a href="https://github.com/oskardudycz/EventSourcing.NetCore" target="_blank">Oskar Dudycz: EventSourcing .NET</a>
- <a href="https://eventuous.dev/" target="_blank">Alexey Zimarev: Eventuous</a>
- <a href="https://github.com/kgrzybek/sample-dotnet-core-cqrs-api" target="_blank">Kamil Grzybek: Sample .NET Core REST API CQRS</a>
- <a href="https://github.com/kgrzybek/modular-monolith-with-ddd" target="_blank">Kamil Grzybek: Modular Monolith With DDD</a>
- <a href="https://www.youtube.com/watch?v=Lw04HRF8ies" target="_blank">NDC Oslo: Udi Dahan - Talk Session: CQRS pitfalls and patterns</a>
- <a href="https://www.youtube.com/watch?v=26xrX113KZc" target="_blank">Explore DDD: Mauro Servienti - Talk Session: Welcome to the (State) Machine</a>
- <a href="https://www.youtube.com/watch?v=KkzvQSuYd5I" target="_blank">Explore DDD: Mauro Servienti - Talk Session: All Our Aggregates Are Wrong</a>
- <a href="https://www.youtube.com/watch?v=tVnIUZbsxWI" target="_blank">NDC Oslo: Adam Ralph - Talk Session: Finding your service boundaries - a practical guide</a>
- <a href="https://www.youtube.com/watch?v=fGm62ra_mQ8" target="_blank">Øredev: Alberto Brandolini - Talk Session: 100,000 Orange Stickies Later</a>
- <a href="https://www.confluent.io/events/kafka-summit-london-2024/event-modeling-anti-patterns/" target="_blank">KS2024: Oskar Dudycz - Talk Session: Event Modeling Anti-patterns</a>
- <a href="https://www.youtube.com/watch?v=kPV1SkdSnhE" target="_blank">Vladimir Khorikov: DDD in a nutshell</a>
- <a href="https://odysee.com/@sunnyAtticSoftware:a?view=content" target="_blank">Diego Martin: SunnyAttic Software Videos</a>
- <a href="https://www.youtube.com/watch?v=SUiWfhAhgQw" target="_blank">Jimmy Bogard: Vertical Slice Architecture</a>
- <a href="https://www.youtube.com/@CodeOpinion" target="_blank">CodeOpinion: Youtube Channel</a>
- <a href="https://www.youtube.com/@amantinband" target="_blank">Amichai Mantinband: Youtube Channel</a>
- <a href="https://www.youtube.com/@MilanJovanovicTech" target="_blank">Milan Jovanović: Youtube Channel</a>

## Versions

The main branch is now on .NET 10. The following previous versions are available:
* [.NET 9](https://github.com/jordiaragonzaragoza/JordiAragonZaragoza.SharedKernel/tree/net9.0)
* [.NET 8](https://github.com/jordiaragonzaragoza/JordiAragonZaragoza.SharedKernel/tree/net8.0)
* [.NET 7](https://github.com/jordiaragonzaragoza/JordiAragonZaragoza.SharedKernel/tree/net7.0)

## About: 🧐

The SharedKernel Project was developed by <a href="https://www.linkedin.com/in/jordiaragonzaragoza/" target="_blank">Jordi Aragón Zaragoza</a>

## License: 👮‍♂️

[Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International License](http://creativecommons.org/licenses/by-nc-nd/4.0/).

[![Creative Commons License](https://i.creativecommons.org/l/by-nc-nd/4.0/88x31.png)](http://creativecommons.org/licenses/by-nc-nd/4.0/)