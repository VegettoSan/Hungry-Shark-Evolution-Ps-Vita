# HSE Vita 1.0 distribution tools (private candidate)

The supplied prototype has been extended and exercised against the owner-selected
VPK using real Windows xdelta3. Public distribution is **not cleared**. See the
[measured preparation report](../../docs/RELEASE_V1_0_PREPARATION_2026-10-23.md) and
[Spanish/English draft guide](../../docs/RELEASE_V1_0_USER_GUIDE.md).

End users run the compiled `HSERebuild.exe` (Windows x64/.NET Framework 4.8),
without Python, Unity or SDK. It accepts an APK dropped on the EXE, a native file
picker, or `HSERebuild.exe APK --out OUTPUT.vpk`. Keep the base, patch, format2
manifest and pinned `xdelta3decode.exe` beside it. Only install the reconstructed
complete VPK. Manual picker/drag-drop and clean-PC testing remain pending.

Developer requirements: Python 3.11+, Windows .NET Framework csc for the native
extractor, exact user-supplied APK, owner-designated VPK and verified xdelta3 3.2.2.
Never stage these proprietary inputs, generated bases, patches or VPKs in Git.
Use an ignored private directory for all outputs. No command publishes a Release.

```powershell
python tools/release_xdelta/hse_release.py audit --apk APK --vpk APPROVED_VPK --report PRIVATE_AUDIT.json
python tools/release_xdelta/hse_release.py author --apk APK --vpk APPROVED_VPK --data-paths PRIVATE_PATHS.txt --xdelta ENCODER --decoder DECODER --source-mode unity-data-v1 --payload-compression stored --out NEW_PRIVATE_DIR
powershell -NoProfile -File tools/release_xdelta/build_windows.ps1 -OutDir NEW_PRIVATE_BUILD_DIR
python tools/release_xdelta/hse_release.py compare --original APPROVED_VPK --reconstructed REBUILT_VPK
python tools/release_xdelta/inspect_delta.py --patch PRIVATE_PATCH --out PRIVATE_COUNTS.json
```

`data_paths.txt` must be a private explicit list of existing VPK file entries.
The source-normalization algorithm concatenates logical original Unity Data in
ordinal path order, joining numeric split chunks. It is byte preprocessing for
xdelta, not an Android-to-PSP2 converter. The base/payload selection preserves all
entries and does not mean either component is legally redistributable.

Encoder and decoder SHA-256 pins are in `hse_release.py` and `HSERebuild.cs`.
The tested pair requires `-S none`: default LZMA encoding fails and the standalone
decoder rejects DJW. `-D -R` disables external compression helpers; environment
`XDELTA` is cleared. `-A` removes the application header. Both source and target
are checked independently with SHA-256. Legacy xdelta versions are untested.
`printdelta` crashes on exit in this build. `inspect_delta.py` independently
accounts uncompressed default-table VCDIFF v0 instructions; it rejects unsupported
formats and does not replace actual decode/byte-comparison testing.

Reproducible checks:

```powershell
python tools/release_xdelta/test_release.py
powershell -NoProfile -File tools/release_xdelta/test_windows.ps1 -OutDir NEW_PRIVATE_TEST_DIR -Apk APK -Bundle PRIVATE_BUNDLE
python tools/release_xdelta/benchmark.py --apk APK --vpk APPROVED_VPK --data-paths PRIVATE_PATHS.txt --xdelta ENCODER --decoder DECODER --out NEW_PRIVATE_BENCH_DIR
```

If the sandbox TEMP is not writable, set TEMP and TMP to a private writable
workspace directory for the Python tests. Native tests compile and run .NET code;
with APK/bundle they exercise real decoder failure, cancellation and cleanup.
The old mock-xdelta, Linux-shebang smoke test was replaced by portable structural
checks plus Windows tests. Do not call mock tests proof of real reconstruction.

The extractor checks input/components/normalized source/payload/every final member,
progress, space, safe ZIP names, no overwrite, and controlled cancellation. A
unique temporary sibling is cleaned on normal errors/Ctrl+C. Power loss or forced
termination may leave a marked temporary directory; remove it only after the
process exits. The C# ZIP container may differ from Python's container; member
sizes, hashes and byte comparisons determine equivalence.

All manifests are stamped `PRIVATE_NOT_CLEARED`. Tooling success does not close
hardware, rights or phase gates. xdelta licensing does not grant rights over game,
Unity/Sony player, artwork or literal patch bytes. Preserve BetterWinds credit.
