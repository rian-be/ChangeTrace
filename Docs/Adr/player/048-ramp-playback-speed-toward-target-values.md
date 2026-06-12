[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](047-normalize-timeline-time-before-playback.md) | [Next](049-support-single-event-stepping-separately-from-continuous-playback.md)

## [048] Ramp Playback Speed Toward Target Values

*2026-06* | Status: accepted

**Context:**

`VirtualClock` delegates speed dynamics to `SpeedController`, which maintains `CurrentSpeed`, `TargetSpeed`, acceleration, and ramp-state reanchoring. In code, playback speed is not just one number but the progression toward a target value.

**Problem:**

An immediate speed change is simple to implement, but it breaks virtual-time continuity and makes resume behavior after pause, speed changes, or seek less predictable. The player needs a model in which speed changes do not corrupt time position or diagnostics.

**Decision:**

Target speed changes happen through acceleration-based ramping rather than direct jumps. Immediate jumps remain only for operations explicitly treated as snaps, such as preset speed or freeze.

**Rejected:**

- One speed value without distinguishing between `current` and `target`.
- Applying immediate speed changes for every control call.
- Wyliczanie speed w transporcie instead of w zegarze wirtualnym.
- Keeping ramping logic in the rendering or input layer.

**Consequences:**

Playback preserves time continuity and handles pause/resume and target-speed changes better. The tradeoff is a more complex speed model, and diagnostics and tests must distinguish the current ramp state from the target value.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](047-normalize-timeline-time-before-playback.md) | [Next](049-support-single-event-stepping-separately-from-continuous-playback.md)
