#!/usr/bin/env python3
"""Send the release changelog to the dedicated Discord webhook."""

import os

from scripts.workflow_utils import env, format_template, load_config, post_json, require_env

def truncate_body(body: str, max_length: int, suffix: str) -> str:
    """Trim long changelog text to fit Discord embed limits."""
    if len(body) <= max_length:
        return body
    return body[:max_length] + "\n\n" + suffix

def main() -> int:
    """Build and post the changelog embed for the current release."""
    webhook_url, config_path, release_tag, release_url = require_env(
        "WEBHOOK_URL", "CONFIG_PATH", "RELEASE_TAG", "RELEASE_URL"
    )
    release_body = env("RELEASE_BODY")

    config = load_config(config_path)
    color = config["discord"]["color"]
    changelog_cfg = config["discord"]["changelog"]

    body = truncate_body(release_body, int(changelog_cfg["max_length"]), changelog_cfg["truncated_suffix"])

    payload = {
        "embeds": [
            {
                "title": format_template(changelog_cfg["title_template"], tag=release_tag),
                "url": release_url,
                "color": color,
                "description": body,
            }
        ]
    }

    post_json(webhook_url, payload)
    return 0

if __name__ == "__main__":
    raise SystemExit(main())