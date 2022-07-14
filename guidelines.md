# Guidelines

This project serves as a bigger sample application showing new technologies using .NET, C#, Microsoft Azure creating Microservices and client applications.

## Use modern technologies

Prefer using modern APIs, e.g.

* Minimal APIs instead of controllers
* C# records if possible and useful instead of classes and structs
* Primary records when possible
* Use *nullable reference types*, *file-scoped

## Released versions

Use released versions (e.g. .NET 6) to make it easier using the applications and services from all developers.

> An exception from this rule can be applied if a pre-release version is not too far away, it's not expected the pre-release version adds many required changes on the code, and offers many advantages on the implementation (e.g. .NET 7 with Minimal APIs).  If needed, a long-time branch can be used to use pre-release versions where this exception does not apply.

## Older technologies

Older technologies can be used if required for some scenarios, e.g. **WPF** as needed for a workshop. **WinUI** should be preferred to implement more features.

## Productivity

> Productivity is an important attribute of this project.

Use shorter code and use built-in .NET features if possible, but don't rely on external libraries (with exceptions). 

Create simple code using new language features, but not too simple. Don't create flexible types thinking on future enhancements - this future might not appear. Instead, if the future appears refactor the implementation to create a generic type or a base type.

## Links

[C# Coding Style](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md)

[Microsoft REST API Guidelines](https://github.com/microsoft/api-guidelines/blob/vNext/Guidelines.md)

[Azure REST API Guidelines](https://github.com/microsoft/api-guidelines/blob/vNext/azure/Guidelines.md)
