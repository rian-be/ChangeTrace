#!/usr/bin/env python3
"""Validate shader files and related project wiring used by CI."""

import pathlib
import re
import sys


SHADER_SUFFIXES = {".vert", ".frag", ".comp"}
VERSION_RE = re.compile(r"^#version", re.MULTILINE)
LOCAL_SIZE_RE = re.compile(r"layout\s*\(\s*local_size", re.MULTILINE)
MANIFEST_PATH = pathlib.Path("src/Graphics/Shaders/Registry/ShaderManifest.cs")
PROJECT_FILE = pathlib.Path("ChangeTrace.csproj")


def shader_files(root: pathlib.Path) -> list[pathlib.Path]:
    """Return all shader source files tracked under the repository root."""
    return sorted(
        path
        for path in root.rglob("*")
        if path.is_file() and path.suffix in SHADER_SUFFIXES
    )


def read_text(path: pathlib.Path) -> str:
    """Read a UTF-8 text file used by the shader validation checks."""
    return path.read_text(encoding="utf-8")


def fail(message: str, paths: list[pathlib.Path] | None = None) -> int:
    """Report a validation failure and return a non-zero exit code."""
    print(message, file=sys.stderr)
    for path in paths or []:
        print(path.as_posix(), file=sys.stderr)
    return 1


def collect_shader_sources(shaders: list[pathlib.Path]) -> tuple[dict[pathlib.Path, str], list[pathlib.Path]]:
    """Load shader sources once and collect empty files separately."""
    sources: dict[pathlib.Path, str] = {}
    empty: list[pathlib.Path] = []

    for path in shaders:
        if path.stat().st_size == 0:
            empty.append(path)
            continue
        sources[path] = read_text(path)

    return sources, empty


def find_missing_version(shader_sources: dict[pathlib.Path, str]) -> list[pathlib.Path]:
    """Find shaders that do not declare a GLSL version."""
    return [path for path, text in shader_sources.items() if not VERSION_RE.search(text)]


def find_invalid_compute_shaders(shader_sources: dict[pathlib.Path, str]) -> list[pathlib.Path]:
    """Find compute shaders that do not declare local workgroup sizes."""
    return [
        path
        for path, text in shader_sources.items()
        if path.suffix == ".comp" and not LOCAL_SIZE_RE.search(text)
    ]


def main() -> int:
    """Run shader validation checks and print useful CI diagnostics."""
    root = pathlib.Path(".")
    shaders = shader_files(root)

    print("Looking for shader files...")
    for shader in shaders:
        print(shader.as_posix())

    print(f"Shader files found: {len(shaders)}")
    if not shaders:
        return fail("No shader files found.")

    shader_sources, empty = collect_shader_sources(shaders)
    checks = [
        ("Empty shader files detected:", empty),
        ("Shaders missing #version directive:", find_missing_version(shader_sources)),
        ("Compute shaders missing layout(local_size_x/y/z):", find_invalid_compute_shaders(shader_sources)),
    ]
    for message, failed_paths in checks:
        if failed_paths:
            return fail(message, failed_paths)

    if not (root / MANIFEST_PATH).is_file():
        return fail(f"Missing shader manifest: {MANIFEST_PATH.as_posix()}")

    src_dir = root / "src"
    graphics_refs: list[str] = []
    compute_refs: list[str] = []
    for path in sorted(src_dir.rglob("*")):
        if not path.is_file():
            continue
        try:
            text = read_text(path)
        except UnicodeDecodeError:
            continue
        if 'Shaders.Graphics("' in text:
            graphics_refs.append(path.as_posix())
        if 'Shaders.Compute("' in text:
            compute_refs.append(path.as_posix())

    print("Graphics shader references:")
    for path in graphics_refs:
        print(path)

    print("Compute shader references:")
    for path in compute_refs:
        print(path)

    if "Assets/**/*" not in read_text(root / PROJECT_FILE):
        return fail("ChangeTrace.csproj may not include Assets/**/* for output copy.")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
