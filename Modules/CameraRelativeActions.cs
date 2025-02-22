using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using ActionManager = Hypostasis.Game.Structures.ActionManager;

namespace ReAction.Modules;

public unsafe class CameraRelativeActions : PluginModule
{
    public override bool ShouldEnable => ReAction.Config.EnableCameraRelativeDirectionals || ReAction.Config.EnableCameraRelativeDashes;

    protected override bool Validate() => Game.fpSetGameObjectRotation != null && Common.CameraManager != null;
    protected override void Enable() => ActionStackManager.PostActionStack += PostActionStack;
    protected override void Disable() => ActionStackManager.PostActionStack -= PostActionStack;

    private static void PostActionStack(ActionManager* actionManager, uint actionType, uint actionID, uint adjustedActionID, ref long targetObjectID, uint param, uint useType, int pvp)
    {
        if (!ReAction.actionSheet.TryGetValue(adjustedActionID, out var a)
            || !CheckAction(actionType, actionID, adjustedActionID)
            || actionManager->CS.GetActionStatus((ActionType)actionType, adjustedActionID) != 0
            || actionManager->animationLock != 0)
            return;

        PluginLog.Debug($"Rotating camera {actionType}, {adjustedActionID}");

        Game.SetCharacterRotationToCamera();
    }

    private static bool CheckAction(uint actionType, uint actionID, uint adjustedActionID)
    {
        if (!ReAction.actionSheet.TryGetValue(adjustedActionID, out var a)) return false;
        if (ReAction.Config.EnableCameraRelativeDirectionals && a.IsPlayerAction && (a.Unknown50 == 6 || (a.CastType is 3 or 4 && a.CanTargetSelf))) return true; // Channeled abilities and cones and rectangles
        if (!ReAction.Config.EnableCameraRelativeDashes) return false;
        if (!a.AffectsPosition && adjustedActionID != 29494) return false; // Block non movement abilities
        if (!a.CanTargetSelf) return false; // Block non self targeted abilities
        if (ReAction.Config.EnableNormalBackwardDashes && a.BehaviourType is 3 or 4) return false; // Block backwards dashes if desired
        return a.BehaviourType > 1; // Block abilities like Loom and Shukuchi
    }
}