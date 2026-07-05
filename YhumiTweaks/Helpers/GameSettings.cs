using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using System;
using System.Collections.Generic;
using System.Text;

namespace YhumiTweaks.Helpers
{
    public static class GameSettings
    {
        public static void UpdateTiltToExpected()
        {
            var expectedTilt = Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundByDuty] ? P.Config.SavedInstanceHeight : P.Config.SavedOutOfInstanceHeight;

            Svc.Log.Info($"Instanced? {Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundByDuty]}. Expected: {expectedTilt}. Actual: {GetTilt()}");

            if (expectedTilt != GetTilt())
                SetTilt(expectedTilt);
        }

        public static unsafe float GetTilt()
        {
            var worldCamera = CameraManager.Instance()->Camera;
            return worldCamera->TiltOffset;
        }

        public static unsafe void SetTilt(float tilt)
        {
            var worldCamera = CameraManager.Instance()->Camera;
            worldCamera->SetTiltOffset(tilt);
        }
    }
}
