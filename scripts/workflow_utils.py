#!/usr/bin/env python3
"""Shared helpers for workflow-oriented Python scripts."""

import json
import os
import subprocess
import sys
import urllib.request


def load_config(path: str) -> dict:
    """Load a YAML config file, falling back to Ruby if PyYAML is unavailable."""
    try:
        import yaml  # type: ignore

        with open(path, "r", encoding="utf-8") as handle:
            return yaml.safe_load(handle)
    except ModuleNotFoundError:
        result = subprocess.run(
            [
                "ruby",
                "-rjson",
                "-ryaml",
                "-e",
                "puts JSON.generate(YAML.safe_load(File.read(ARGV[0])))",
                path,
            ],
            check=True,
            capture_output=True,
            text=True,
        )
        return json.loads(result.stdout)


def post_json(url: str, payload: dict) -> None:
    """Send a JSON payload to a webhook endpoint."""
    data = json.dumps(payload).encode("utf-8")
    request = urllib.request.Request(
        url,
        data=data,
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    with urllib.request.urlopen(request) as response:
        if response.status >= 300:
            raise RuntimeError(f"Webhook failed with status {response.status}")


def gh(*args: str, token: str) -> subprocess.CompletedProcess[str]:
    """Run a GitHub CLI command with the provided token."""
    return subprocess.run(
        ["gh", *args],
        env={**os.environ, "GH_TOKEN": token},
        text=True,
        capture_output=True,
        check=False,
    )


def env(name: str, default: str = "") -> str:
    """Read an environment variable with an optional default value."""
    return os.environ.get(name, default)


def require_env(*names: str) -> list[str]:
    """Read required environment variables or exit with a clear error."""
    values: list[str] = []
    missing: list[str] = []
    for name in names:
        value = os.environ.get(name, "")
        values.append(value)
        if not value:
            missing.append(name)
    if missing:
        print(f"Missing environment variables: {', '.join(missing)}", file=sys.stderr)
        raise SystemExit(1)
    return values


def env_flag(name: str) -> bool:
    """Interpret an environment variable as a boolean workflow flag."""
    return os.environ.get(name, "").lower() == "true"


def format_template(template: str, **values: str) -> str:
    """Format simple %{key} workflow templates with named values."""
    for key, value in values.items():
        template = template.replace(f"%{{{key}}}", value)
    return template
