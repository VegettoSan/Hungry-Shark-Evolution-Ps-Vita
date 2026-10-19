"""Apply the UI-only description adapter to a user-supplied private Unity project.

Does not contain game data, launch Unity, or change prefabs/settings. Rebuild the
private catalog with the installed HSE menu command before building PSP2.
"""
import argparse
import datetime
import hashlib
import json
from pathlib import Path
import shutil


def patched_source(source):
    marker = "public void RefreshGameItemSlots("
    start = source.index(marker)
    head, body = source[:start], source[start:]
    if "HSEVitaStoreDescriptions.GetEffects(shopItem)" in body:
        if "HSEVitaStoreDescriptions.IsAllZero(shopItem, gameplayEffectsForShopItem)" not in body:
            raise ValueError("Partial adapter found; preserve/review its visibility predicate")
        return source
    lookup = "Accessory.GameplayEffects gameplayEffectsForShopItem = ShopManager.GetGameplayEffectsForShopItem(shopItem);"
    predicate = "!gameplayEffectsForShopItem.IsAllZero()"
    if body.count(lookup) != 1 or body.count(predicate) != 1:
        raise ValueError("Unexpected store source; preserve it and review the adapter manually")
    body = body.replace(lookup, """Accessory.GameplayEffects gameplayEffectsForShopItem =
#if UNITY_PSP2 || UNITY_EDITOR
                    HSEVitaInput.Active ? HSEVitaStoreDescriptions.GetEffects(shopItem) : ShopManager.GetGameplayEffectsForShopItem(shopItem);
#else
                    ShopManager.GetGameplayEffectsForShopItem(shopItem);
#endif""", 1)
    body = body.replace(predicate, """
#if UNITY_PSP2 || UNITY_EDITOR
                    !(HSEVitaInput.Active ? HSEVitaStoreDescriptions.IsAllZero(shopItem, gameplayEffectsForShopItem) : gameplayEffectsForShopItem.IsAllZero())
#else
                    !gameplayEffectsForShopItem.IsAllZero()
#endif
                """, 1)
    return head + body


def prepare(project, apply=False):
    project = project.resolve(strict=True)
    version = (project / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8-sig")
    if "m_EditorVersion: 2018.2.19f1" not in version.splitlines():
        raise ValueError("Expected the existing Unity 2018.2.19f1 private project")
    source = project / "Assets/Scripts/Assembly-CSharp/StoreMenuSheet.cs"
    original = source.read_bytes()
    text = original.decode("utf-8-sig").replace("\r\n", "\n")
    updated = patched_source(text)
    files = {source: (b"\xef\xbb\xbf" if original.startswith(b"\xef\xbb\xbf") else b"") +
             updated.replace("\n", "\r\n" if b"\r\n" in original else "\n").encode("utf-8")}
    root = Path(__file__).resolve().parent / "unity"
    for folder, name in [("Scripts/Assembly-CSharp", "HSEVitaStoreDescriptions.cs"),
                         ("Editor", "HSEVitaStoreDescriptionBuilder.cs"),
                         ("Editor", "HSEGameplayCapture.cs")]:
        target = project / "Assets" / folder / name
        contents = (root / name).read_bytes()
        if target.exists() and target.read_bytes() != contents:
            raise ValueError("Existing helper differs; preserve/review it: " + name)
        files[target] = contents
    changed = {target: contents for target, contents in files.items()
               if not target.exists() or target.read_bytes() != contents}
    result = {"mode": "apply" if apply else "check", "changed": [str(f.relative_to(project)) for f in changed]}
    if apply and changed:
        backup = project / "Reports" / ("StoreDescriptionBackup_" + datetime.datetime.now(datetime.timezone.utc).strftime("%Y%m%dT%H%M%S%fZ"))
        backup.mkdir(parents=True, exist_ok=False)
        hashes = {}
        for target in changed:
            if target.exists():
                relative = target.relative_to(project)
                saved = backup / relative
                saved.parent.mkdir(parents=True, exist_ok=True)
                shutil.copy2(target, saved)
                hashes[str(relative)] = hashlib.sha256(target.read_bytes()).hexdigest()
        (backup / "before.json").write_text(json.dumps(hashes, indent=2), encoding="utf-8")
        for target, contents in changed.items():
            target.parent.mkdir(parents=True, exist_ok=True)
            target.write_bytes(contents)
    return result


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("project", type=Path)
    parser.add_argument("--apply", action="store_true", help="Back up existing files, then apply; default is read-only")
    args = parser.parse_args()
    print(json.dumps(prepare(args.project, args.apply), indent=2))
