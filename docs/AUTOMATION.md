# Automated releases

- Pushes to `main` build, run offline checks and publish a GitHub Release.
- Every six hours (00:23, 06:23, 12:23, 18:23 UTC), Actions checks NuGet for the latest stable CounterStrikeSharp.API. GitHub may delay scheduled runs.
- Both main pushes and scheduled builds select the latest stable NuGet API. The project and lock file are updated only inside the runner; no automatic commits are made.
- A release is identified by the full source commit SHA and CSS version. Existing releases are skipped; failures publish nothing and are retried by the next schedule.
- Pull requests use the committed dependency versions and run checks without publishing.
- Use Actions → Build and release → Run workflow on main to check manually.

Each ZIP contains `addons/counterstrikesharp/plugins/Game-Manager-GoldKingZ/`, translations, Newtonsoft.Json, UPGRADE.md, the resolved lock file and BUILD.json. SHA256SUMS accompanies the ZIP. ClientPrefs is a separate prerequisite, not bundled.

Only the release job receives contents:write through GITHUB_TOKEN. No additional secrets or external Codex automation are required. Actions must be enabled. GitHub can disable schedules in inactive public repositories; re-enable the workflow if necessary.

A successful build confirms compilation and offline checks, not live server compatibility. New CSS versions with breaking changes fail visibly in Actions and do not replace the last successful release. Upgrade notes describing the reviewed 1.0.378 build remain historical; consult BUILD.json for each automated release's actual API version.
