[ADR Home](../README.md) | [Category Index](./README.md) | [Next](025-register-services-through-explicit-discovery-markers.md)

## [024] Compose Services Through Dependency Injection

*2026-02* | Status: accepted

**Context:**

These decisions define how the application composes dependencies and where the boundary sits between a service implementation and the way it is provided to runtime. The goal is to reduce manual wiring and keep responsibility boundaries readable.

**Problem:**

A growing number of services requires a controlled dependency graph.
Without this decision, dependencies would quickly start being assembled manually in many places, and modules would lose clear responsibility boundaries.

**Decision:**

The application uses Microsoft dependency injection and central initialization.
Choosing DI and discovery means application composition is treated as a separate startup responsibility rather than a detail of handlers or domain services.

**Rejected:**

- Static singletons.
- Manually constructing objects in each handler.
- A mixed model in which some services are composed through DI and others through manual singletons.
- Initializing dependencies only at call sites instead of in one composition root.

**Consequences:**

Modules are testable, but service registration and lifetimes must be maintained consistently.
This improves testability and implementation swapability, but it requires disciplined registration, lifetimes, and clear contracts between modules.

[ADR Home](../README.md) | [Category Index](./README.md) | [Next](025-register-services-through-explicit-discovery-markers.md)
