"""Package the Unity project without Library, nested projects or earlier ZIPs."""
from pathlib import Path
import hashlib
import json
import re
import zipfile

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "NakiLegend-Desert-v12.zip"


def main():
    paths = []
    for folder in ("Assets", "Packages", "ProjectSettings", "Art", "Docs", "Tools"):
        paths.extend(p for p in (ROOT / folder).rglob("*")
                     if p.is_file() and "__pycache__" not in p.parts)
    paths.extend(ROOT / n for n in ("README.md", "THIRD_PARTY_NOTICES.md", ".gitignore", ".gitattributes"))
    validation = ROOT / "Validation/DesertV12"
    paths.extend(validation / n for n in (
        "DesertSmoke.cs", "RuntimeReloadSmoke.cs", "smoke-result.txt",
        "reload-result.txt", "navigation-summary.txt", "delivery.json"))
    paths = sorted(set(paths))
    missing = [str(p) for p in paths if not p.exists()]
    if missing:
        raise RuntimeError(f"Missing delivery files: {missing}")

    guids = {}
    for meta in (ROOT / "Assets").rglob("*.meta"):
        match = re.search(r"^guid: ([0-9a-f]{32})$", meta.read_text(encoding="utf-8-sig"), re.M)
        if match:
            if match[1] in guids:
                raise RuntimeError(f"Duplicate Unity GUID: {meta}, {guids[match[1]]}")
            guids[match[1]] = str(meta.relative_to(ROOT))
    main_meta = (ROOT / "Assets/NakiPrototype/NakiPrototype.cs.meta").read_text()
    assert "d922c3c6bbc91794ea00f6c8c00f1911" in main_meta

    with zipfile.ZipFile(OUT, "w", zipfile.ZIP_DEFLATED, compresslevel=5) as z:
        for p in paths:
            z.write(p, p.relative_to(ROOT).as_posix())
    with zipfile.ZipFile(OUT) as z:
        assert z.testzip() is None, "Archive CRC check failed"
        for p in paths:
            if p.suffix in (".cs", ".meta") or "DesertV12" in p.parts:
                assert z.read(p.relative_to(ROOT).as_posix()) == p.read_bytes(), str(p)
        assert not any(n.startswith(("Library/", "Temp/", ".git/")) for n in z.namelist())

    report = {
        "archive": str(OUT), "files": len(paths), "bytes": OUT.stat().st_size,
        "sha256": hashlib.sha256(OUT.read_bytes()).hexdigest(),
        "crc": "PASS", "unique_asset_guids": len(guids),
        "source_and_metadata_match": "PASS",
    }
    (validation / "package-result.json").write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps(report, ensure_ascii=False))


if __name__ == "__main__":
    main()
