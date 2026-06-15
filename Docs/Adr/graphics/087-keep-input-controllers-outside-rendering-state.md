[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](086-use-opentk-as-the-windowing-and-opengl-runtime.md) | [Next](088-use-debug-windows-for-rendering-development.md)

## [087] Keep Input Controllers Outside Rendering State

*2026-05* | Status: accepted

**Context:**

Mouse and keyboard input influence camera motion, selection, and playback control, but they are not themselves part of the scene model. The graphics runtime needs to convert operator input into commands without turning transient device state into persistent rendering state.

**Problem:**

If input handling is embedded in the scene graph or scattered across render outputs, interaction rules become hard to test and easy to desynchronize from player and camera behavior. The runtime needs one place where input intent is interpreted before scene or playback state changes.

**Decision:**

Input is handled by dedicated controllers outside rendering state. Controllers read window events and translate them into camera, hover, or playback actions without making scene state structures own device event policy directly.

**Rejected:**

- Handling input inside the scene graph.
- Spreading keyboard conditions across render outputs.
- Binding camera and playback controls directly to raw window callbacks in many places.
- Treating input state as part of the persistent render state model.

**Consequences:**

Interaction logic becomes easier to reason about and easier to align with player and camera contracts. The tradeoff is that controller behavior becomes its own integration layer between windows, rendering state, and playback.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](086-use-opentk-as-the-windowing-and-opengl-runtime.md) | [Next](088-use-debug-windows-for-rendering-development.md)
