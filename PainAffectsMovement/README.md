# Pain Affects Movement

Adds a direct, tunable link from Pain -> Move Speed, instead of relying on
vanilla's indirect Pain -> Consciousness -> Moving chain (which also drags
down manipulation, sight, talking, etc. since Consciousness is a 1-for-1
multiplier on most of those).

## The curve

- Pain 0-20%: no penalty (buffer zone)
- Pain 20-80%: move speed factor ramps linearly from x1.00 down to x0.60
- Pain 80%+: capped at x0.60 (a flat -40% penalty)

All the math lives in `Source/PainAffectsMovement/StatPart_PainMoveSpeed.cs`,
in the `PainToFactorCurve` field, if you want to retune the buffer, the cap
percentage, or the max penalty.

## Important caveat

This adds a *new* multiplier on top of the `MoveSpeed` stat. It does **not**
remove vanilla's existing pain -> Consciousness -> Moving penalty, because
that's computed deep in RimWorld's health/capacity code rather than through
a normal StatDef part, so it can't be reached with a plain XML/StatPart mod
like this one. In practice that means pain will now hit move speed a bit
harder than the curve above alone suggests, since both effects stack.

If you want the vanilla consciousness-driven movement penalty fully replaced
(not just supplemented) by this curve, that requires a Harmony patch on the
capacity-level calculation - happy to add that as a follow-up if you want it.

## How to build - no local installs, entirely in the browser

You need a compiled DLL because StatPart subclasses can't be written in pure
XML, but you don't need to install anything on your own machine. This repo
is set up to build itself on GitHub's servers via GitHub Actions, using the
`Krafs.Rimworld.Ref` NuGet package (official reference assemblies for
RimWorld, published with Ludeon's permission), so it never needs your actual
game files either.

1. Go to github.com, sign in (or make a free account), and create a new
   **public** repository (e.g. "PainAffectsMovement").
2. On the repo's page, click **Add file > Upload files**, then drag in the
   *entire contents* of this folder (keep the folder structure - `About/`,
   `Patches/`, `Source/`, `.github/` etc. all need to land at the repo root).
   Commit the upload.
   (Note: the `.github` folder is hidden on most systems. If your drag-and-drop
   doesn't pick it up, click **Add file > Create new file** instead, type
   `.github/workflows/build.yml` as the filename - GitHub will create the
   folders for you - and paste in the contents of that file.)
3. Click the **Actions** tab. A workflow called "Build mod DLL" should
   already be running (it auto-triggers on push). Give it a minute or two.
4. Once it finishes (green check), click into that workflow run, scroll to
   **Artifacts**, and download `PainAffectsMovement-dll`. That's a zip
   containing your compiled `PainAffectsMovement.dll`.
5. Unzip it and drop `PainAffectsMovement.dll` into this mod's `Assemblies/`
   folder (replacing the empty placeholder).
6. Copy the whole `PainAffectsMovement` folder into your RimWorld `Mods`
   folder, enable it in the in-game mod list, and restart.

If you'd rather build locally later (e.g. in Visual Studio or Rider), the
`.csproj` still works fine offline once you have the .NET SDK - just run
`dotnet build` from `Source/PainAffectsMovement/`.

## Folder layout

```
PainAffectsMovement/
  About/About.xml                  - mod metadata
  Patches/PainAffectsMovement_Patches.xml   - hooks the StatPart into MoveSpeed
  Source/PainAffectsMovement/      - C# source + csproj
  Assemblies/                      - compiled DLL goes here (empty until you build)
  .github/workflows/build.yml      - GitHub Actions build script
```

## Verifying it in-game

Select a pawn with some injuries, open the Moving speed stat tooltip
(hover the move-speed stat on the pawn's info card or health tab) once
their pain is above 20% - you should see a "Pain (X%): x0.YY" line in the
breakdown.
