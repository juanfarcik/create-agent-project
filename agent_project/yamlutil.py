"""Minimal, dependency-free YAML writer/reader for our own config shapes.

We intentionally avoid a PyYAML dependency (Section 63/70: keep the
generator lightweight and dependency-free). This handles the small,
well-known shapes used by project.yaml / architecture.yaml — nested dicts,
lists of scalars, and lists of dicts. It is not a general YAML library.
"""

from __future__ import annotations

from typing import Any


def _scalar(value: Any) -> str:
    if isinstance(value, bool):
        return "true" if value else "false"
    if value is None:
        return "null"
    if isinstance(value, (int, float)):
        return str(value)
    text = str(value)
    if text == "" or any(ch in text for ch in [":", "#", "\n"]) or text.strip() != text:
        escaped = text.replace('"', '\\"').replace("\n", "\\n")
        return f'"{escaped}"'
    return text


def _unscalar(text: str) -> Any:
    if text in ("true", "false"):
        return text == "true"
    if text == "null":
        return None
    if text == "[]":
        return []
    if text.startswith('"') and text.endswith('"'):
        return text[1:-1].replace('\\"', '"').replace("\\n", "\n")
    try:
        return int(text) if "." not in text else float(text)
    except ValueError:
        return text


def _indent_of(line: str) -> int:
    return (len(line) - len(line.lstrip(" "))) // 2


def _parse_list(lines, i, indent):
    items = []
    while i < len(lines):
        line = lines[i]
        if not line.strip():
            i += 1
            continue
        if _indent_of(line) < indent or not line.strip().startswith("- "):
            break
        rest = line.strip()[2:]
        if ":" in rest and not rest.startswith('"'):
            key, _, val = rest.partition(":")
            key, val = key.strip(), val.strip()
            item = {}
            i += 1
            if val:
                item[key] = _unscalar(val)
            else:
                sub, i = _parse_dict(lines, i, indent + 1)
                item[key] = sub
            while i < len(lines):
                nxt = lines[i]
                if not nxt.strip():
                    i += 1
                    continue
                if _indent_of(nxt) == indent and not nxt.strip().startswith("- "):
                    k2, _, v2 = nxt.strip().partition(":")
                    item[k2.strip()] = _unscalar(v2.strip())
                    i += 1
                else:
                    break
            items.append(item)
        else:
            items.append(_unscalar(rest))
            i += 1
    return items, i


def _parse_dict(lines, i, indent):
    result = {}
    while i < len(lines):
        line = lines[i]
        if not line.strip():
            i += 1
            continue
        cur = _indent_of(line)
        if cur != indent or line.strip().startswith("- "):
            break
        key, _, rest = line.strip().partition(":")
        key, rest = key.strip(), rest.strip()
        if rest:
            result[key] = _unscalar(rest)
            i += 1
        else:
            if i + 1 < len(lines) and lines[i + 1].strip().startswith("- "):
                items, i = _parse_list(lines, i + 1, indent + 1)
                result[key] = items
            else:
                sub, i = _parse_dict(lines, i + 1, indent + 1)
                result[key] = sub
    return result, i


def load(text: str) -> dict:
    lines = text.splitlines()
    data, _ = _parse_dict(lines, 0, 0)
    return data


def dump(data: dict, indent: int = 0) -> str:
    lines = []
    pad = "  " * indent
    for key, value in data.items():
        if isinstance(value, dict):
            lines.append(f"{pad}{key}:")
            lines.append(dump(value, indent + 1))
        elif isinstance(value, list):
            if not value:
                lines.append(f"{pad}{key}: []")
            elif all(isinstance(v, dict) for v in value):
                lines.append(f"{pad}{key}:")
                for item in value:
                    item_lines = dump(item, indent + 1).splitlines()
                    if item_lines:
                        lines.append(f"{pad}  - {item_lines[0].strip()}")
                        lines.extend(item_lines[1:])
            else:
                lines.append(f"{pad}{key}:")
                for item in value:
                    lines.append(f"{pad}  - {_scalar(item)}")
        else:
            lines.append(f"{pad}{key}: {_scalar(value)}")
    return "\n".join(lines)
