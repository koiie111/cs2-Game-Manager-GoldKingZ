# Offline checks

Run `dotnet restore tests/Tests.csproj --locked-mode`, then
`dotnet run --project tests/Tests.csproj -c Release --no-restore`.
The executable links production config parsing, name-block state, OnTick, the
two-second name checker, disconnect, frame and timer guards. Only CSS calls,
localization, chat delivery and the final configured punishment command have
managed substitutes. No native ABI, actual timer cadence or client display is tested.

256 assertions cover config migration/rejection, ordered rules and literal sound
matching, 0/1/8/32 blocked users among 32 sessions, tick delivery, countdown reuse,
both name policies, expiry owned by the checker, disconnect/identical-ID slot reuse,
map/config generation invalidation and cleanup of 1000 completed timers.

`--benchmark` retains the original small formatter experiment.
`--runtime-benchmark` compares the production OnTick with the baseline OnTick copied
from commit `f7ff9e909649cd6fe597457b994201d30a0a766b` in `BenchmarkBaseline.cs`;
only its method name changes. It uses the same managed fixtures and 32 connected
sessions, 0/1/8/32 active recipients, 20000 warm-up frames and three windows of
20000 frames. Delivery counts must match. Run with `DOTNET_TieredCompilation=0`
for the recorded experiment. Localization and sending are substitutes; these
bytes and times are synthetic, not native or full-tick costs. Stopwatch allocation
is outside the measured interval. A real countdown boundary can allocate one new
localized string per active user, as intended.

`python tests/verify_measurements.py` independently validates the nine historical
idle raw inputs, their SHA256 and nearest-rank statistics, plus delivery counts
in all 24 new runtime windows. Their provenance and limitations are in
`docs/performance/O5.md`; idle timing drift does not establish a causal speedup.
