#!/usr/bin/env python3
"""Upload prepared artifacts to a GitHub release or draft release."""

import glob
import os
import sys

from scripts.workflow_utils import env, env_flag, gh, require_env


def resolve_release_tag(repository: str, token: str, explicit_tag: str, find_draft: bool) -> str:
    """Resolve the target release tag from input or the first available draft release."""
    if explicit_tag:
        return explicit_tag
    if not find_draft:
        return ""

    result = gh(
        "api",
        f"repos/{repository}/releases",
        "--jq",
        "[.[] | select(.draft == true)][0].tag_name",
        token=token,
    )
    return result.stdout.strip() if result.returncode == 0 else ""


def collect_asset_paths(dist_dir: str, patterns: list[str]) -> list[str]:
    """Expand configured glob patterns into a unique, sorted asset list."""
    asset_paths: list[str] = []
    for pattern in patterns:
        pattern = pattern.strip()
        if not pattern:
            continue
        asset_paths.extend(sorted(glob.glob(os.path.join(dist_dir, pattern))))
    return sorted(dict.fromkeys(asset_paths))


def ensure_draft_release(repository: str, token: str, release_tag: str, release_title: str) -> int:
    """Ensure the target release remains a draft and optionally update its title."""
    edit_args = [
        "release",
        "edit",
        release_tag,
        "--repo",
        repository,
        "--draft=true",
    ]
    if release_title:
        edit_args.extend(["--title", release_title])

    result = gh(*edit_args, token=token)
    if result.returncode != 0:
        sys.stderr.write(result.stderr)
    return result.returncode


def upload_assets(repository: str, token: str, release_tag: str, asset_paths: list[str]) -> int:
    """Upload assets to the resolved release tag, replacing existing files if needed."""
    result = gh(
        "release",
        "upload",
        release_tag,
        *asset_paths,
        "--clobber",
        "--repo",
        repository,
        token=token,
    )
    if result.returncode != 0:
        sys.stderr.write(result.stderr)
    return result.returncode


def main() -> int:
    """Resolve the target release and upload all matching artifacts."""
    token, repository = require_env("GITHUB_TOKEN", "REPOSITORY")
    dist_dir = env("DIST_DIR", "dist")
    explicit_tag = env("RELEASE_TAG")
    find_draft = env_flag("FIND_DRAFT")
    ensure_draft = env_flag("ENSURE_DRAFT")
    release_title = env("RELEASE_TITLE")
    fail_message = env("FAIL_MESSAGE", "Release tag could not be resolved.")
    pattern_lines = env("ASSET_PATTERNS", "*").splitlines()

    release_tag = resolve_release_tag(repository, token, explicit_tag, find_draft)
    if not release_tag:
        print(fail_message, file=sys.stderr)
        return 1

    asset_paths = collect_asset_paths(dist_dir, pattern_lines)
    if not asset_paths:
        print("No assets matched the configured upload patterns.", file=sys.stderr)
        return 1

    if ensure_draft:
        result = ensure_draft_release(repository, token, release_tag, release_title)
        if result != 0:
            return result

    return upload_assets(repository, token, release_tag, asset_paths)


if __name__ == "__main__":
    raise SystemExit(main())
