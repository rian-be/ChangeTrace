#!/usr/bin/env python3
"""Build and send the main Discord message for a published release."""

import glob
import hashlib
import os

from scripts.workflow_utils import env, env_flag, gh, load_config, post_json, require_env


def fetch_previous_tag(repository: str, github_token: str) -> str:
    """Fetch the previous release tag to avoid repeating the same message variant."""
    if not repository or not github_token:
        return ""
    result = gh(
        "api",
        f"repos/{repository}/releases?per_page=2",
        "--jq",
        '.[1].tag_name // ""',
        token=github_token,
    )
    return result.stdout.strip() if result.returncode == 0 else ""


def choose_message(messages: list[str], release_tag: str, previous_tag: str) -> str:
    """Pick a deterministic message variant and avoid repeating the previous one."""
    if not messages:
        return ""

    current_index = int(hashlib.sha256(release_tag.encode("utf-8")).hexdigest()[:8], 16) % len(messages)
    if previous_tag:
        previous_index = int(hashlib.sha256(previous_tag.encode("utf-8")).hexdigest()[:8], 16) % len(messages)
        if previous_index == current_index:
            current_index = (current_index + 1) % len(messages)
    return messages[current_index]


def collect_platforms(assets_dir: str, include_glob: str) -> tuple[list[str], str]:
    """Collect uploaded ZIP assets and derive a platform summary from their names."""
    asset_paths = sorted(glob.glob(os.path.join(assets_dir, include_glob)))
    platforms = [os.path.basename(path)[len("ChangeTrace-") : -len(".zip")] for path in asset_paths]
    return asset_paths, (", ".join(platforms) if platforms else "unknown")


def main() -> int:
    """Send either the release embed or the manual-run placeholder message."""
    webhook_url, config_path = require_env("WEBHOOK_URL", "CONFIG_PATH")

    config = load_config(config_path)
    color = config["discord"]["color"]
    release_cfg = config["discord"]["release"]

    if env("EVENT_NAME") != "release":
        payload = {
            "embeds": [
                {
                    "title": release_cfg["manual_title"],
                    "color": color,
                }
            ]
        }
        post_json(webhook_url, payload)
        return 0

    include_glob = config["assets"]["include_glob"]
    asset_paths, platform_list = collect_platforms(env("ASSETS_DIR"), include_glob)

    release_tag = env("RELEASE_TAG")
    previous_tag = fetch_previous_tag(env("REPOSITORY"), env("GITHUB_TOKEN"))
    message = choose_message(release_cfg["messages"], release_tag, previous_tag)

    release_type = "prerelease" if env_flag("RELEASE_PRERELEASE") else "stable"
    release_name = env("RELEASE_NAME") or release_tag
    title = release_cfg["title_template"] % {"name": release_name}
    fields_cfg = release_cfg["fields"]

    payload = {
        "embeds": [
            {
                "title": title,
                "url": env("RELEASE_URL"),
                "color": color,
                "description": message,
                "fields": [
                    {"name": fields_cfg["tag"], "value": release_tag, "inline": True},
                    {"name": fields_cfg["release_type"], "value": release_type, "inline": True},
                    {"name": fields_cfg["published_at"], "value": env("RELEASE_PUBLISHED_AT"), "inline": True},
                    {"name": fields_cfg["target"], "value": env("RELEASE_TARGET"), "inline": True},
                    {"name": fields_cfg["published_by"], "value": f"@{env('RELEASE_AUTHOR')}", "inline": True},
                    {"name": fields_cfg["platforms"], "value": platform_list, "inline": False},
                    {"name": fields_cfg["uploaded_zips"], "value": str(len(asset_paths)), "inline": True},
                    {"name": fields_cfg["verification"], "value": release_cfg["verification_text"], "inline": False},
                ],
            }
        ]
    }

    post_json(webhook_url, payload)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
