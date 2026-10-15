#!/usr/bin/env python3
"""Check sequential human-approved phase ledger. stdlib only; no network access.

Does NOT verify that a device test really happened or the authenticity of a linked
approval. Use GitHub branch protection and human review for those requirements.
"""
import datetime
import json
import pathlib
import re
import sys

ROOT = pathlib.Path(__file__).resolve().parents[1]
LEDGER = ROOT / "docs" / "PHASE_GATES.json"
NAMES = [f"P{i:02d}" for i in range(13)]
STATES = {"LOCKED", "ACTIVE", "AWAITING_APPROVAL", "BLOCKED", "APPROVED"}
ALLOWED_OPEN = {"ACTIVE", "AWAITING_APPROVAL", "BLOCKED"}
APPROVER = "VegettoSan"
APPROVAL_URL = re.compile(
    r"^https://github\.com/VegettoSan/Hungry-Shark-Evolution-Ps-Vita/"
    r"(issues|pull)/[0-9]+(?:#issuecomment-[0-9]+|#discussion_r[0-9]+|/files)?$"
)
ISO_UTC = re.compile(r"^\d{4}-\d\d-\d\dT\d\d:\d\d:\d\dZ$")


def fail(message):
    print("PHASE GATES ERROR:", message, file=sys.stderr)
    sys.exit(1)


def check(data):
    if data.get("schema_version") != 1:
        fail("Unsupported schema version")
    if data.get("application") != "Hungry Shark Evolution 6.4.2 (Unity 5.6.4p3)":
        fail("Application identity changed unexpectedly")
    if data.get("baseline_sha256") != "52b7d8754ab8fec75c3b656a0dc5ec8dc849565adc402487a3ac093f6a481f7a":
        fail("APK SHA-256 mismatch")
    phases = data.get("phases")
    if not isinstance(phases, list) or [x.get("id") for x in phases] != NAMES:
        fail("Phase IDs must be exactly P00 through P12, in order")
    seen_open = False
    current = None
    for phase in phases:
        name = phase["id"]
        state = phase.get("status")
        if state not in STATES:
            fail(f"{name}: invalid state")
        evidence = phase.get("evidence")
        approved_by = phase.get("approved_by")
        approved_at = phase.get("approved_at")
        approval_reference = phase.get("approval_reference")
        if state == "APPROVED":
            if seen_open:
                fail(f"{name}: approved after an unfinished phase")
            if not isinstance(evidence, list) or len(evidence) < 2:
                fail(f"{name}: at least two evidence references required")
            if any(not isinstance(x, str) or not x.strip() for x in evidence):
                fail(f"{name}: evidence references must be nonempty strings")
            if approved_by != APPROVER:
                fail(f"{name}: only the repository owner can approve")
            if not isinstance(approved_at, str) or not ISO_UTC.fullmatch(approved_at):
                fail(f"{name}: UTC approval time needed (YYYY-MM-DDTHH:MM:SSZ)")
            try:
                datetime.datetime.strptime(approved_at, "%Y-%m-%dT%H:%M:%SZ")
            except ValueError:
                fail(f"{name}: invalid approval date")
            if not isinstance(approval_reference, str) or not APPROVAL_URL.fullmatch(approval_reference):
                fail(f"{name}: link approval to a repository issue/PR comment")
        elif state in ALLOWED_OPEN:
            if seen_open:
                fail(f"{name}: multiple active/blocked/awaiting stages")
            seen_open = True
            current = name
            if any(x is not None for x in (approved_by, approved_at, approval_reference)):
                fail(f"{name}: unfinished stage cannot have approval fields")
            if not isinstance(evidence, list):
                fail(f"{name}: evidence must be a list")
        else:
            if not seen_open:
                fail(f"{name}: cannot be LOCKED before the active stage")
            if any(x not in (None, []) for x in (approved_by, approved_at, approval_reference, evidence)):
                fail(f"{name}: locked stage must be empty and unapproved")
    if not seen_open:
        if data.get("current_phase") != "COMPLETE":
            fail("All stages approved: current_phase must be COMPLETE")
    elif data.get("current_phase") != current:
        fail(f"current_phase must be {current} (first unfinished stage)")
    print(f"PHASE GATES OK: current={data['current_phase']} / approved={sum(x['status'] == 'APPROVED' for x in phases)}/{len(phases)}")


if __name__ == "__main__":
    with LEDGER.open("r", encoding="utf-8") as handle:
        check(json.load(handle))
