[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](048-ramp-playback-speed-toward-target-values.md)

## [049] Support Single Event Stepping Separately From Continuous Playback

*2026-06* | Status: accepted

**Context:**

The player code distinguishes between two timeline advance modes: continuous playback driven by ticks and single event stepping through `TimelineStepper`. The stepper updates the cursor, moves the clock to the event time, and can emit an event without starting the transport.

**Problem:**

Debugging, precise navigation, and paused playback need single event stepping rather than only time flowing with transport ticks. When stepping is treated as a special case of `Play`, it becomes hard to preserve precision and predictable emitted events.

**Decision:**

Stepping through the timeline is modeled as a separate operation over the cursor and clock, independent from continuous playback. `TimelinePlayer` exposes `StepForward` and `StepBackward`, but delegates their semantics to a dedicated stepper.

**Rejected:**

- Simulating stepping by briefly starting the transport and pausing it immediately.
- Allowing external layers to perform stepping through direct cursor access.
- Combining stepping with seeking without a dedicated per event contract.
- Emitting step events without updating virtual time.

**Consequences:**

The player has a stable navigation model both for continuous playback and for step by step debugging. The cost is that stepping semantics must remain aligned with the cursor, seeking, and diagnostics, otherwise those three axes will start reporting different timeline positions.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](048-ramp-playback-speed-toward-target-values.md)
