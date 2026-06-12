[ADR Home](../README.md)

# composition

This category describes how the application is assembled from runtime services and modules.

## Scope

Look here for dependency registration, service discovery, composition roots, and module wiring.

## Responsibility Boundaries

Composition wires the system together but should not absorb domain, export, or rendering logic.

## How To Start Reading

Start here when changing service registration, dependency discovery, or runtime assembly boundaries.

## ADR List

| ADR | Title |
| --- | --- |
| [024-compose-services-through-dependency-injection.md](./024-compose-services-through-dependency-injection.md) | Compose Services Through Dependency Injection |
| [025-register-services-through-explicit-discovery-markers.md](./025-register-services-through-explicit-discovery-markers.md) | Register Services Through Explicit Discovery Markers |
