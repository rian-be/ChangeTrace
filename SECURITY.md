# Security

ChangeTrace keeps repository history, timeline data, and local auth on your machine.

> [!IMPORTANT]
> Report security issues privately. Do not post exploit details in public issues.

> [!WARNING]
> This is an early experimental version. Security hardening is still in progress.

## Supported Versions

ChangeTrace is currently in active development.

Security fixes are applied to `develop` first.
If needed, maintainers may backport fixes to stable releases.

## Reporting

Please report vulnerabilities directly to maintainers through a private channel.

Include:

- what the issue is
- how to reproduce it
- what impact it has
- affected version or commit
- proof of concept, if available

If a private channel is not available, open a minimal public issue without exploit details and request private follow-up.

## Response Process

Maintainers aim to:

- acknowledge receipt within 7 days
- triage and assess severity
- prepare a fix plan based on impact
- coordinate disclosure after a fix is available

## Scope

This policy applies to:

- ChangeTrace CLI
- auth/session handling
- local storage of sensitive data
- repository processing and export/import paths

## Out of Scope

The following are usually out of scope unless they create real security impact:

- style or formatting issues
- theoretical issues without reproducible path
- issues in unsupported environments

## Disclosure

Please allow maintainers reasonable time to investigate and fix before public disclosure.
