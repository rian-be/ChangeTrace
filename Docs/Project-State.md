# Project State

ChangeTrace is being built in steps so the code stays easy to work with and the UI can grow without getting messy.

## State 1

State 1 is the stability phase.

The focus here is to make the current code solid before adding more things. That means:

* keep the export and inspection flow smooth
* clean up rough edges in auth and local storage
* tighten the code paths that already exist
* keep the current CLI and rendering flow predictable
* make the whole codebase easier to trust

State 1 is not about adding new UI for the sake of it. It is not the HUD phase yet.

The goal is simple: make the project feel stable and ready for the next step.

## State 2

State 2 is the HUD phase.

Once the base is stable, the next step is to make the on-screen experience better. That means:

* a clearer HUD layout
* better information hierarchy
* easier reading during playback
* more useful overlays for inspection and debugging
* a stronger overall screen experience

The point is not to add clutter. The point is to make the data easier to read while the timeline is on screen.

## Why This Order

The code is easier to extend when the base is steady first.

State 1 keeps the core behavior from shifting while the project is still settling. State 2 can then focus on the HUD without having to work around moving core logic at the same time.

## Current Position

Right now the project is in `State 1`.

The next work should mostly help with stability, cleanup, and consistency before the bigger HUD changes start.
