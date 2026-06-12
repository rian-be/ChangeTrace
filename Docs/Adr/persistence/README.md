[ADR Home](../README.md)

# persistence

This category describes durable data contracts and the persistence model around `.gittrace` and related DTOs.

## Scope

Look here for questions about storage formats, MessagePack mapping, DTO boundaries, and file-level persistence concerns.

## Responsibility Boundaries

Persistence owns durable representations and I/O contracts. It should not redefine domain meaning or provider behavior.

## How To Start Reading

Start here when changing serialized formats, DTOs, or persistence-oriented file access.

## ADR List

| ADR | Title |
| --- | --- |
| [017-optimize-gittrace-serialization-hot-paths.md](./017-optimize-gittrace-serialization-hot-paths.md) | Optimize gittrace Serialization Hot Paths |
| [018-persist-timelines-as-portable-gittrace-files.md](./018-persist-timelines-as-portable-gittrace-files.md) | Persist Timelines As Portable gittrace Files |
| [019-use-streaming-serializers-for-gittrace-writes.md](./019-use-streaming-serializers-for-gittrace-writes.md) | Use Streaming Serializers For gittrace Writes |
| [032-separate-persisted-dtos-from-domain-objects.md](./032-separate-persisted-dtos-from-domain-objects.md) | Separate Persisted DTOs From Domain Objects |
| [037-keep-file-access-behind-file-manager-abstractions.md](./037-keep-file-access-behind-file-manager-abstractions.md) | Keep File Access Behind File Manager Abstractions |
| [038-use-messagepack-for-timeline-persistence.md](./038-use-messagepack-for-timeline-persistence.md) | Use MessagePack For Timeline Persistence |
