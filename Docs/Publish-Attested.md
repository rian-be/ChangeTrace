# Publish Attested

`Publish Attested` is the workflow used to publish runtime packages and attach provenance attestations.

It gives you a repeatable way to confirm where a release artifact came from before you use it.

## What it is for

Use it when you want to verify that an artifact:

- was produced by ChangeTrace CI
- was signed from the expected workflow file
- matches repository identity and (optionally) branch ref

The workflow file is:

`.github/workflows/publish-attested.yml`

## What it publishes

Current runtime artifacts:

- `ChangeTrace-win-x64.zip`
- `ChangeTrace-linux-x64.zip`
- `ChangeTrace-linux-arm64.zip`
- `ChangeTrace-osx-x64.zip`
- `ChangeTrace-osx-arm64.zip`

Each artifact gets a provenance attestation from `actions/attest-build-provenance@v2`.

## Verify

Prerequisites:

- GitHub CLI with `gh attestation verify`
- access to this repository
- local artifact file to check

The usual flow is simple:

1. download one artifact (for example `ChangeTrace-linux-x64.zip`)
2. verify it against repository + signer workflow
3. optionally enforce source ref (`main`, `develop`, tag)

Example:

```bash
gh attestation verify ChangeTrace-linux-x64.zip \
  --repo rian-be/ChangeTrace \
  --signer-workflow rian-be/ChangeTrace/.github/workflows/publish-attested.yml
```

If this passes, the artifact provenance matches the expected workflow identity.

## Stricter Verification

Pin verification to a specific ref:

```bash
gh attestation verify ChangeTrace-linux-x64.zip \
  --repo rian-be/ChangeTrace \
  --signer-workflow rian-be/ChangeTrace/.github/workflows/publish-attested.yml \
  --source-ref refs/heads/main
```

For automation/policies, use JSON output:

```bash
gh attestation verify ChangeTrace-linux-x64.zip \
  --repo rian-be/ChangeTrace \
  --format json
```
