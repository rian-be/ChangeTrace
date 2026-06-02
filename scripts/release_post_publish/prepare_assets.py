#!/usr/bin/env python3
"""Download and filter release assets for post-publish Discord steps."""

import glob
import os
import pathlib
import shutil
import subprocess
import sys

from scripts.workflow_utils import env, load_config, require_env


def download_assets(release_tag: str, repository: str, assets_dir: pathlib.Path, github_token: str) -> None:
    """Download all assets for a release into the target directory."""
    env = {**os.environ, "GH_TOKEN": github_token}
    subprocess.run(
        [
            "gh",
            "release",
            "download",
            release_tag,
            "--repo",
            repository,
            "--dir",
            str(assets_dir),
        ],
        env=env,
        check=True,
    )


def apply_exclude_globs(assets_dir: pathlib.Path, patterns: list[str]) -> None:
    """Remove downloaded assets that match configured exclusion globs."""
    for pattern in patterns:
        for path in glob.glob(str(assets_dir / pattern)):
            try:
                os.remove(path)
            except FileNotFoundError:
                pass


def ensure_included_assets(assets_dir: pathlib.Path, include_glob: str) -> int:
    """Count assets that match the configured upload pattern."""
    return len(glob.glob(str(assets_dir / include_glob)))


def main() -> int:
    """Prepare release assets for downstream notification and upload steps."""
    config = load_config(os.environ["CONFIG_PATH"])
    assets_cfg = config["assets"]

    release_tag, repository = require_env("RELEASE_TAG", "REPOSITORY")
    assets_dir = pathlib.Path(env("ASSETS_DIR", "release-assets"))
    github_token = env("GITHUB_TOKEN")

    if assets_dir.exists():
        shutil.rmtree(assets_dir)
    assets_dir.mkdir(parents=True, exist_ok=True)

    download_assets(release_tag, repository, assets_dir, github_token)
    apply_exclude_globs(assets_dir, list(assets_cfg.get("remove_globs", [])))

    include_glob = assets_cfg["include_glob"]
    if ensure_included_assets(assets_dir, include_glob) == 0:
        print(f"No assets matched {include_glob!r} after filtering.", file=sys.stderr)
        return 1

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
