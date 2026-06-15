[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](024-compose-services-through-dependency-injection.md)

## [025] Register Services Through Explicit Discovery Markers

*2026-02* | Status: accepted

**Context:**

These decisions define how the application composes dependencies and where the boundary sits between a service implementation and the way it is provided to runtime. The goal is to reduce manual wiring and keep responsibility boundaries readable.

**Problem:**

Manually registering every service quickly increases boilerplate.
Without this decision, dependencies would quickly start being assembled manually in many places, and modules would lose clear responsibility boundaries.

**Decision:**

Services can be registered through discovery attributes when that fits the module.
Choosing DI and discovery means application composition is treated as a separate startup responsibility rather than a detail of handlers or domain services.

**Rejected:**

- Fully manual registration of everything.
- Magic scanning without explicit markers.
- A mixed model in which some services are composed through DI and others through manual singletons.
- Initializing dependencies only at call sites instead of in one composition root.

**Consequences:**

Adding services becomes simpler, but a missing attribute can mean a missing runtime service.
This improves testability and implementation swapability, but it requires disciplined registration, lifetimes, and clear contracts between modules.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](024-compose-services-through-dependency-injection.md)
