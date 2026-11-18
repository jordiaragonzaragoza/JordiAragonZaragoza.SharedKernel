# 🧱 SharedKernel: DDD & Clean Architecture building blocks for .NET

[![NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.svg?label=nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel)

**Stop rebuilding aggregate roots, value objects, CQRS buses and MediatR pipelines in every project. Install the packages that match your architecture and get straight to your domain.**

It is persistence-agnostic: your aggregates can be event-sourced or not, and nothing forces you to take the whole kit.

## 👀 What you get

- 🧩 **Domain building blocks:** entities, strongly typed ids, value objects, aggregate roots that record domain events, and business rule validation
- ⚡ **CQRS out of the box:** command and query buses plus ready-made MediatR pipeline behaviours for validation, logging and more
- 🗄️ **Persistence your way:** a KurrentDB event store, EF Core base implementations, and cached repositories behind an `ICacheService` abstraction
- 📨 **Messaging:** an event bus and integration events built on MassTransit
- 🌐 **HTTP API toolkit:** `BaseApiController`, exception middleware, `CurrentUserService` and a `ResponseBuilder` for ProblemDetails
- 🎯 **Pick only what you need:** each layer ships as its own focused NuGet package

If it saves you time, a ⭐ goes a long way. Thank you!

## 🚀 Quick start

Follow the [documentation](https://sharedkernel-jordiaragonzaragoza.mintlify.site/) for a guided quickstart.

## 📦 Which package do I need?

Every package below is prefixed with `JordiAragonZaragoza.SharedKernel.`

| Layer | What it provides | Packages |
|---|---|---|
| **Core** | Dependency injection that auto-registers services through markup interfaces | `SharedKernel`, `Contracts` |
| **Domain** | Contracts and base implementations for DDD: DomainEvents, ApplicationEvents, ValueObjects, Entities, Repository, BusinessRulesValidations and more | `Domain`, `Domain.Contracts` |
| **Application** | Contracts and base implementations for EventBus, UnitOfWork and CQRS; common MediatR pipelines; integration contracts; a MassTransit-based EventBus | `Application`, `Application.Contracts`, `Application.Contracts.Integration` |
| **Infrastructure** | KurrentDB event store, EF Core base implementation, `BaseCachedRepository` over `ICacheService`, MassTransit event bus base | `Infrastructure`, `Infrastructure.EntityFramework`, `Infrastructure.EventStore` |
| **Presentation** | HTTP RESTful API building blocks | `Presentation.HttpRestfulApi`, `Presentation.HttpRestfulApi.Contracts` |
| **Presentation Integration** | Integration bus building blocks, such as `IntegrationEventHandler` | `Presentation.Integration`, `Presentation.Integration.Contracts` |
| **Hosting** | Common hosts configuration with default loggin and dependency Injection validation `HostApplicationBuilderExtensions` | `Hosting` |

<details>
<summary>NuGet versions</summary>

| Package | Version |
|---|---|
| [`JordiAragonZaragoza.SharedKernel`](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel) | [![NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.svg?label=nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel) |
| [`JordiAragonZaragoza.SharedKernel.Contracts`](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Contracts) | [![NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.Contracts.svg?label=nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Contracts) |
| [`JordiAragonZaragoza.SharedKernel.Domain`](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Domain) | [![NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.Domain.svg?label=nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Domain) |
| [`JordiAragonZaragoza.SharedKernel.Domain.Contracts`](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Domain.Contracts) | [![NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.Domain.Contracts.svg?label=nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Domain.Contracts) |
| [`JordiAragonZaragoza.SharedKernel.Application`](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Application) | [![NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.Application.svg?label=nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Application) |
| [`JordiAragonZaragoza.SharedKernel.Application.Contracts`](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Application.Contracts) | [![NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.Application.Contracts.svg?label=nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Application.Contracts) |
| [`JordiAragonZaragoza.SharedKernel.Application.Contracts.Integration`](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Application.Contracts.Integration) | [![NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.Application.Contracts.Integration.svg?label=nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Application.Contracts.Integration) |
| [`JordiAragonZaragoza.SharedKernel.Infrastructure`](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Infrastructure) | [![NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.Infrastructure.svg?label=nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Infrastructure) |
| [`JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework`](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework) | [![NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework.svg?label=nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Infrastructure.EntityFramework) |
| [`JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore`](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore) | [![NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore.svg?label=nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Infrastructure.EventStore) |
| [`JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi`](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi) | [![NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.svg?label=nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi) |
| [`JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Contracts`](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Contracts) | [![NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Contracts.svg?label=nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Contracts) |
| [`JordiAragonZaragoza.SharedKernel.Presentation.Integration`](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Presentation.Integration) | [![NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.Presentation.Integration.svg?label=nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Presentation.Integration) |
| [`JordiAragonZaragoza.SharedKernel.Presentation.Integration.Contracts`](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Presentation.Integration.Contracts) | [![NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.Presentation.Integration.Contracts.svg?label=nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Presentation.Integration.Contracts) |
| [`JordiAragonZaragoza.SharedKernel.Hosting`](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Hosting) | [![NuGet](https://img.shields.io/nuget/v/JordiAragonZaragoza.SharedKernel.Hosting.svg?label=nuget)](https://www.nuget.org/packages/JordiAragonZaragoza.SharedKernel.Hosting) |

</details>

## 🎬 See it in action

**[JordiAragonZaragoza.Cinema](https://github.com/jordiaragonzaragoza/JordiAragonZaragoza.Cinema/)** is a runnable cinema management system built entirely on these packages. It combines DDD, Event Sourcing, a microservices and event-driven architecture, and Vertical Slices. Clone it, press `F5` and explore how the pieces fit together.

## 🛠️ Build from source

- Install the .NET 10 SDK (or later) and an IDE: Visual Studio, Rider or VS Code.
- Open `JordiAragonZaragoza.SharedKernel.sln`: it contains every library project.

## ⚒️ Libraries used

<details>
<summary>Show the full list</summary>

- Entity Framework
- KurrentDB.Client
- MediatR
- MassTransit
- FastEndpoints
- Ardalis.Result, Ardalis.Specification and Ardalis.SmartEnums
- FluentValidation
- Serilog
- StyleCop and SonarAnalyzer

</details>

## 🧪 Testing

GitHub workflow CI with SonarCloud integration.

[![SonarCloud](https://sonarcloud.io/images/project_badges/sonarcloud-white.svg)](https://sonarcloud.io/summary/new_code?id=jordiaragonzaragoza_JordiAragon.SharedKernel)

## 🙏 Resources and Inspiration

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

## 🔀 Versions

The main branch targets .NET 10. Previous versions are available here:
* [.NET 9](https://github.com/jordiaragonzaragoza/JordiAragonZaragoza.SharedKernel/tree/net9.0)
* [.NET 8](https://github.com/jordiaragonzaragoza/JordiAragonZaragoza.SharedKernel/tree/net8.0)
* [.NET 7](https://github.com/jordiaragonzaragoza/JordiAragonZaragoza.SharedKernel/tree/net7.0)

## 🧐 About

The SharedKernel Project was developed by <a href="https://www.linkedin.com/in/jordiaragonzaragoza/" target="_blank">Jordi Aragón Zaragoza</a>

## 👮 License

[Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International License](http://creativecommons.org/licenses/by-nc-nd/4.0/).

[![Creative Commons License](https://i.creativecommons.org/l/by-nc-nd/4.0/88x31.png)](http://creativecommons.org/licenses/by-nc-nd/4.0/)