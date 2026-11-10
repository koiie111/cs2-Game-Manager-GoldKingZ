# Stability candidate 2.1.6-stable.1-rc

This candidate preserves the perf.2 countdown cache, active-name-block index,
per-tick delivery and two-second name-check cadence. No diagnostics polling,
per-tick logging or new OnTick work was added.

## Operator commands

- `css_gm_status`: version, map, preference-store initialization, gamedata and
  native cvar-hook state, loaded rule documents, session/name-block/guarded-timer
  counts and last reload outcome. It prints no database credentials or player IPs.
- `css_gm_reload`: validate main/chat/weapon documents before detaching existing
  handlers; reject invalid input and retain running state. Both commands accept
  the server console/RCON or players with `@css/root`. The original configurable
  reload commands retain their existing permission rules.

Changing ClientPrefs storage/backend options still requires a cold plugin/server
restart. `prefs=ready` means the store was created; it does not independently
verify database availability or successful persistence.

## Changes

Command prefix/substring rules use one global command listener only when rules
exist. Matching uses indexed loops without predicate closures; permissions remain
live. Nonmatching server-console commands pass through unchanged.

Rule documents are read without rewriting them. Empty legacy rule documents are
accepted as empty rules on startup; empty documents during explicit reload are
rejected. Replace intentionally empty legacy documents with `{}` before reloading.
Main-config normalization and its pre-upgrade backup retain their existing policy.

Cvars changed by the plugin retain their pre-change values for restoration on
reload/unload/map end. HUD restoration clears only bits added by this plugin.
Render colors restore only while still equal to this plugin's last applied color.
Entity handles include their generation, and stale pawn entries are pruned when
the snapshot collection reaches 128 entries. HideLegs now triggers spawn handling
even when the other HUD/body options are off.

Valid local gamedata loads before network updates. Disabled auto-update makes no
HTTP request when a valid local file exists. Downloads are JSON-validated before
atomic replacement; network failure retains usable local data. An already loaded
native hook uses its loaded signatures until the next map/reload. Failed gamedata
initialization can be retried; diagnostic hook state reflects successful hooking.

## Verification

SDK 10.0.400, CSS API 1.0.378: Release build, zero warnings/errors; 275 managed
assertions. Added checks cover command prefixes/substrings/case and invalid/empty
rule documents without modifying their contents. Historical measurement verifier
still passes. The synthetic production-OnTick fixture retains equal delivery
counts and zero managed allocation between countdown transitions at 32 recipients.
Native transport and full-server ticks are outside that measurement.

An empty-server pilot checks cold initialization with ClientPrefs 1.0.3, diagnostics,
rejection of each invalid document, valid reload, radar cvar restoration, repeated
reloads and a full map reload. Preserve the private complete pre-pilot backup when
rolling back; do not copy this instance's config to other servers.

Real-client HUD/rendering, permission denial, command interception, reconnect and
damage/sound/weapon acceptance under active load remain necessary before declaring
a fleet-wide stable release. Empty-server success does not certify those paths.
