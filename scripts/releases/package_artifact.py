#!/usr/bin/env python3

import pathlib
import shutil
import sys
import zipfile

from scripts.workflow_utils import env, require_env


def create_zip(runtime: str, publish_dir: pathlib.Path, dist_dir: pathlib.Path) -> int:
    if not publish_dir.is_dir():
        print(f"Publish directory not found: {publish_dir}", file=sys.stderr)
        return 1

    dist_dir.mkdir(parents=True, exist_ok=True)
    archive_path = dist_dir / f"ChangeTrace-{runtime}.zip"

    with zipfile.ZipFile(
        archive_path, "w", compression=zipfile.ZIP_DEFLATED
    ) as archive:
        for path in sorted(publish_dir.rglob("*")):
            if path.is_file():
                archive.write(path, path.relative_to(publish_dir))

    return 0


def copy_bundle(runtime: str, bundle_path: pathlib.Path, dist_dir: pathlib.Path) -> int:
    if not bundle_path.is_file():
        print(f"Bundle path not found: {bundle_path}", file=sys.stderr)
        return 1

    dist_dir.mkdir(parents=True, exist_ok=True)
    target = dist_dir / f"ChangeTrace-{runtime}.sigstore.json"
    shutil.copyfile(bundle_path, target)
    return 0


def main() -> int:
    runtime = require_env("RUNTIME")[0]
    dist_dir = pathlib.Path(env("DIST_DIR", "dist"))
    bundle_path = env("BUNDLE_PATH")
    publish_dir = pathlib.Path(env("PUBLISH_DIR"))

    if bundle_path:
        return copy_bundle(runtime, pathlib.Path(bundle_path), dist_dir)
    return create_zip(runtime, publish_dir, dist_dir)


if __name__ == "__main__":
    raise SystemExit(main())
