# Upgrade and rollback

The reviewed release is `2.1.6-perf.2`, based on upstream `f7ff9e9` (2.1.6).
Installed official 2.1.3 is a separate version. This is not an in-place hot-reload
guarantee or a performance-only binary backport to 2.1.3.

Requirements: .NET 10 runtime, CounterStrikeSharp 1.0.378 target, ClientPrefs
GoldKingZ 1.0.3 and its matching shared API. Do not replace CSS core/runtime or
unrelated plugins. The review package contains this plugin's DLL, deps.json,
Newtonsoft.Json.dll and English translations; the separately identified shared
API reference is not a complete installation of ClientPrefs. Use its official
1.0.3 release and preserve any already installed preference data/configuration.

Before an explicitly authorized canary, identify the exact instance/port and take
a full hash-verified backup outside all autoload directories: plugin folder,
gamedata, translations, configs, dependencies, preference data and relevant cvars.
Record CSS/game/runtime versions and other plugins' hashes. Confirm the selected
instance is empty before any authorized cold replacement through its own manager.
There are no deployment commands or server mutations in this review.

Merge translations rather than replacing customized files. The config loader
backs up the original as `config.json.pre-upgrade`, converts legacy ping,
blood/headshot, bomb-HUD, retention and sound-hash keys, and rejects enabled
`AutoSetPlayerLanguage`, which upstream 2.1.6 removed. Explicitly decide whether
to disable that feature before upgrading. Unknown options are diagnosed and the
original backup retained. Invalid config documents retain the running config on
reload. Retain the backup even after a successful normalization.

Legacy 2.1.3 cookie/MySQL preferences require a separately verified conversion to
ClientPrefs, with unchanged toggle ownership and values; do not silently drop or
overwrite them. The earlier pilot's disabled MySQL/absent legacy cookies do not
prove that every fleet instance has no data to migrate. No DB schema changes or
automatic production migration are part of this patch.

Use the SHA256 manifest to verify the exact offline package, then canary the new
release with a real client: both name-block policies, countdown/reset/zero expiry,
team join, configured punishment, alive/dead, competing center menus, reconnect,
map/config reload, body/HUD timers and enabled damage/sound/weapon modes. Preserve
two-second checks and per-tick HTML. Measure active paths and full ticks before
fleet rollout. The earlier idle windows and perf.1 owner smoke do not accept perf.2.

Rollback triggers include native errors, missing/incorrect UI, stale player
effects, timer growth, changed rights or reproducible tick regressions. Within
the authorized window, cold-stop that instance, restore its complete original
plugin/config/dependency/preference/cvar set from the verified backup, restart
using the same manager, then verify hashes and original loaded version. Remove
temporary dependencies only if they were absent in the backup and no other
plugin needs them. Preserve backups and evidence. Never copy a different
instance's configuration or overwrite unrelated plugins during rollback.
