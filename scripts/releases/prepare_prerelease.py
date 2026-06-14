#!/usr/bin/env python3

import re
import sys

from scripts.workflow_utils import env, require_env

TAG_PATTERN = re.compile(r"^v[0-9]+(\.[0-9]+){1,2}-(pre|alpha|beta|rc)(\.[0-9]+)?$")


def main() -> int:
    event_name = env("EVENT_NAME")
    if event_name == "workflow_dispatch":
        tag_name = env("INPUT_TAG_NAME")
        release_name = env("INPUT_RELEASE_NAME")
    else:
        tag_name = env("GITHUB_REF_NAME")
        release_name = ""

    if not release_name:
        release_name = tag_name

    if not TAG_PATTERN.match(tag_name):
        print(
            "Tag must be a pre-release tag like v0.62.24-pre.1, "
            "v0.62.24-alpha.1, v0.62.24-beta.1, or v0.62.24-rc.1",
            file=sys.stderr,
        )
        return 1

    github_output = require_env("GITHUB_OUTPUT")[0]

    # Sanitize release_name to prevent GitHub Actions environment variable injection
    release_name = release_name.replace("\n", "").replace("\r", "")

    with open(github_output, "a", encoding="utf-8") as handle:
        handle.write(f"tag_name={tag_name}\n")
        handle.write(f"release_version={tag_name.removeprefix('v')}\n")
        handle.write(f"release_name={release_name}\n")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
