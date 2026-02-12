#!/usr/bin/env bash
# create_unity_folders_and_readmes.sh
# Run from your Unity project root

set -euo pipefail

folders=(
  "Assets"
  "Assets/_Game"
  "Assets/_Game/Audio"
  "Assets/_Game/Art"
  "Assets/_Game/Art/Materials"
  "Assets/_Game/Art/Models"
  "Assets/_Game/Art/Textures"
  "Assets/_Game/Prefabs"
  "Assets/_Game/Prefabs/Core"
  "Assets/_Game/Prefabs/UI"
  "Assets/_Game/Prefabs/Zone1"
  "Assets/_Game/Prefabs/Zone2"
  "Assets/_Game/Prefabs/Boss"
  "Assets/_Game/Scenes"
  "Assets/_Game/Scenes/MainMenu"
  "Assets/_Game/Scenes/MasterScene"
  "Assets/_Game/Scenes/Sandboxes"
  "Assets/_Game/Scripts"
  "Assets/_Game/Scripts/Core"
  "Assets/_Game/Scripts/Interaction"
  "Assets/_Game/Scripts/Puzzles"
  "Assets/_Game/Scripts/Puzzles/Zone1"
  "Assets/_Game/Scripts/Puzzles/Zone2"
  "Assets/_Game/Scripts/Puzzles/Boss"
  "Assets/_Game/Scripts/UI"
  "Assets/Plugins"
  "Assets/Resources"
)

mkdir -p "${folders[@]}"

# description function
desc() {
  case "$1" in
    "Assets") echo "Unity project content root. Keep most custom work under Assets/_Game. Third-party under Assets/Plugins. Unity Resources under Assets/Resources." ;;
    "Assets/_Game") echo "ALL custom game code and assets live here. Keeps your project clean and makes upgrades/migrations easier." ;;
    "Assets/_Game/Audio") echo "All audio content: music, ambience, SFX, UI sounds, audio mixers, audio-related ScriptableObjects." ;;
    "Assets/_Game/Art") echo "All art assets that are NOT prefabs: models, textures, materials, shaders, source art exports." ;;
    "Assets/_Game/Art/Materials") echo "Unity materials (and related shader graphs) used by models/prefabs. Prefer reusable, named by usage (M_Wood, M_Metal...)." ;;
    "Assets/_Game/Art/Models") echo "3D models (FBX/OBJ), animations, rigs. Keep source exports organized by set/zone if needed." ;;
    "Assets/_Game/Art/Textures") echo "Textures: albedo, normal, ORM, masks, UI atlases. Keep naming consistent and include resolution/usage if helpful." ;;
    "Assets/_Game/Prefabs") echo "All prefabs: gameplay objects, environment pieces, UI prefabs. Subfolders separate by ownership/area." ;;
    "Assets/_Game/Prefabs/Core") echo "Core/manager prefabs: GameManager, AudioManager, SaveSystem, bootstrap objects. Minimal, stable dependencies." ;;
    "Assets/_Game/Prefabs/UI") echo "UI prefabs: screens, panels, widgets, HUD elements. Avoid putting scripts here—scripts go in Scripts/UI." ;;
    "Assets/_Game/Prefabs/Zone1") echo "Zone 1 specific prefabs: props, interactables, puzzle pieces unique to Zone 1." ;;
    "Assets/_Game/Prefabs/Zone2") echo "Zone 2 specific prefabs: props, interactables, puzzle pieces unique to Zone 2." ;;
    "Assets/_Game/Prefabs/Boss") echo "Boss area prefabs: boss entity, arena props, hazards, VFX hooks, boss-specific puzzle parts." ;;
    "Assets/_Game/Scenes") echo "All scenes for the project. Keep naming consistent and avoid duplicates." ;;
    "Assets/_Game/Scenes/MainMenu") echo "Main menu / front-end scenes: main menu, options, credits, loading, splash." ;;
    "Assets/_Game/Scenes/MasterScene") echo "The main playable game scene(s). Often your core gameplay or world root." ;;
    "Assets/_Game/Scenes/Sandboxes") echo "Isolation playground scenes for experiments/juniors. Safe area to build puzzles without breaking main game." ;;
    "Assets/_Game/Scripts") echo "All C# scripts. Organize by system. Prefer assembly definitions later if project grows." ;;
    "Assets/_Game/Scripts/Core") echo "Singleton managers and global systems: GameState, Audio, Save/Load, SceneLoader, ServiceLocator, etc." ;;
    "Assets/_Game/Scripts/Interaction") echo "Interaction system: pickups, switches, doors, triggers, interact prompts, interfaces, base classes." ;;
    "Assets/_Game/Scripts/Puzzles") echo "Puzzle systems and reusable puzzle building blocks. Zone folders are for puzzle implementations unique to each zone." ;;
    "Assets/_Game/Scripts/Puzzles/Zone1") echo "Zone 1 puzzle scripts: implementations and glue code specific to Zone 1." ;;
    "Assets/_Game/Scripts/Puzzles/Zone2") echo "Zone 2 puzzle scripts: implementations and glue code specific to Zone 2." ;;
    "Assets/_Game/Scripts/Puzzles/Boss") echo "Boss puzzle scripts: boss mechanics, phase logic, arena triggers, special interactions." ;;
    "Assets/_Game/Scripts/UI") echo "UI scripts: screen controllers, view models, UI events, navigation, animations." ;;
    "Assets/Plugins") echo "3rd party assets and plugins (DoTween, Cinemachine extras, vendor SDKs). Avoid mixing custom code here." ;;
    "Assets/Resources") echo "Unity Resources folder (loaded via Resources.Load). Use sparingly; prefer Addressables when scaling." ;;
    *) echo "Project folder. Put only the relevant assets for this folder here. Keep it organized and named consistently." ;;
  esac
}

for f in "${folders[@]}"; do
  readme="$f/README.txt"
  cat > "$readme" <<EOF
Folder: $f
Purpose:
$(desc "$f")
Guidelines:
- Prefer clear names and stable structure.
- Keep zone-specific items inside their zone folders.
- Keep code in Assets/_Game/Scripts and prefabs in Assets/_Game/Prefabs.
EOF
done

echo "✅ Folder structure + README.txt created in every folder."
#!/usr/bin/env bash
# create_unity_folders.sh
# Run from your Unity project root (where Assets/ should live)

set -euo pipefail

mkdir -p \
  Assets/_Game/Audio \
  Assets/_Game/Art/Materials \
  Assets/_Game/Art/Models \
  Assets/_Game/Art/Textures \
  Assets/_Game/Prefabs/Core \
  Assets/_Game/Prefabs/UI \
  Assets/_Game/Prefabs/Zone1 \
  Assets/_Game/Prefabs/Zone2 \
  Assets/_Game/Prefabs/Boss \
  Assets/_Game/Scenes/MainMenu \
  Assets/_Game/Scenes/MasterScene \
  Assets/_Game/Scenes/Sandboxes \
  Assets/_Game/Scripts/Core \
  Assets/_Game/Scripts/Interaction \
  Assets/_Game/Scripts/Puzzles/Zone1 \
  Assets/_Game/Scripts/Puzzles/Zone2 \
  Assets/_Game/Scripts/Puzzles/Boss \
  Assets/_Game/Scripts/UI \
  Assets/Plugins \
  Assets/Resources

echo "✅ Unity folder structure created."
