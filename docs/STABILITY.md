# Accepted build 2.1.6-stable.1-rc

**Status: accepted by the project owner on 2026-11-10. The requested verification
stage is complete.** The owner confirmed the plugin works for the intended use
after the server and client pilot, and chose to close this testing stage.
The observations and remaining technical limitations below are retained as the
test record; acceptance does not turn unperformed checks into measured results.

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

The subsequent client and command checks are recorded below. The owner accepted
this scope as sufficient to complete the current verification stage. Sustained
active-load performance was not measured; fleet rollout is a separate operation.

## Client pilot acceptance, 2026-11-10

The owner and other connected players subsequently tested public server #34:

- Radar, chat and weapon HUD hiding persisted through respawn and restored after
  disabling each setting via reload, without reconnecting.
- Mode 1 name blocking transferred the player to spectators, displayed the
  countdown, blocked team joining, extended the deadline after another name
  change, and unblocked normally. Reconnecting discarded the old connection's
  countdown and restrictions.
- Delayed corpse hiding worked at three seconds; respawn remained visible and
  fast respawn was unaffected by the previous body's timer.
- Knife/Zeus damage was suppressed while pistol/AWP damage remained normal;
  disabling the settings restored knife/Zeus damage without reconnecting.
- Personal headshot muting was confirmed on the victim when killed in the head.
  Attacker-side behavior and preference persistence were not separately accepted.

A temporary console-only harness issued harmless commands through the native
server API in a nonexempt human player's context, wrapping the production command
listener to inspect its actual return values. Prefix, substring and case tests
returned Handled; a control command returned Continue. Calling the production
diagnostic permission guard denied that player and allowed root. All six checks
passed. The harness and test rules were removed. Its temporary inotify instance
limit increase was restored from 256 to 128; no persistent sysctl file changed.

Known limitation: VIP RainbowModel writes alpha 255 during recoloring, overriding
HideLegs' alpha 254. The owner chose to retain this compatibility limitation; no
RainbowModel patch was deployed. Initial hiding after respawn was observed, but
sustained hiding with RainbowModel is not accepted.

AimPunch could not be assessed independently of VIP behavior on this AWP server.
It remains unverified, as do sustained active-load performance and untested sound/
weapon modes. The owner accepted these boundaries when closing verification.
Test settings were restored after the pilot.
