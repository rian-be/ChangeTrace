[ADR Home](../README.md)

# player

This category describes the playback model for timelines: time, navigation, boundaries, and diagnostics.

## Scope

Look here for virtual time, event cursors, seeking, speed control, transport, stepping, and playback diagnostics.

## Responsibility Boundaries

The player is not the renderer and not the graphics runtime. It owns temporal semantics and event activation.

## How To Start Reading

Start here when changing playback semantics, seeking, timing, or transport behavior.

## ADR List

| ADR | Title |
| --- | --- |
| [039-drive-playback-through-a-separate-transport.md](./039-drive-playback-through-a-separate-transport.md) | Drive Playback Through A Separate Transport |
| [040-keep-seeking-in-a-dedicated-timeline-service.md](./040-keep-seeking-in-a-dedicated-timeline-service.md) | Keep Seeking In A Dedicated Timeline Service |
| [041-model-playback-boundary-behavior-explicitly.md](./041-model-playback-boundary-behavior-explicitly.md) | Model Playback Boundary Behavior Explicitly |
| [042-use-a-virtual-clock-for-playback.md](./042-use-a-virtual-clock-for-playback.md) | Use A Virtual Clock For Playback |
| [043-use-event-cursors-for-timeline-navigation.md](./043-use-event-cursors-for-timeline-navigation.md) | Use Event Cursors For Timeline Navigation |
| [044-compose-players-from-specialized-playback-components.md](./044-compose-players-from-specialized-playback-components.md) | Compose Players From Specialized Playback Components |
| [045-derive-playback-duration-from-active-repository-days.md](./045-derive-playback-duration-from-active-repository-days.md) | Derive Playback Duration From Active Repository Days |
| [046-expose-playback-diagnostics.md](./046-expose-playback-diagnostics.md) | Expose Playback DIagnostics |
| [047-normalize-timeline-time-before-playback.md](./047-normalize-timeline-time-before-playback.md) | Normalize Timeline Time Before Playback |
| [048-ramp-playback-speed-toward-target-values.md](./048-ramp-playback-speed-toward-target-values.md) | Ramp Playback Speed Toward Target Values |
| [049-support-single-event-stepping-separately-from-continuous-playback.md](./049-support-single-event-stepping-separately-from-continuous-playback.md) | Support Single Event Stepping Separately From Continuous Playback |
