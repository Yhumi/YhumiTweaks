using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Hooking;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace YhumiTweaks.Controllers
{
    internal unsafe class EmoteController : IDisposable
    {
        private uint CURRENT_EMOTE_ID = 0;

        public Action<IPlayerCharacter, ushort>? OnEmote;

        public delegate void OnEmoteFuncDelegate(ulong unk, ulong instigatorAddr, ushort emoteId, ulong targetId, ulong unk2);
        private readonly Hook<OnEmoteFuncDelegate>? hookEmote;

        public bool IsValid = false;

        public EmoteController()
        {
            try
            {
                hookEmote = Svc.Hook.HookFromSignature<OnEmoteFuncDelegate>("E8 ?? ?? ?? ?? 48 8D 8B ?? ?? ?? ?? 4C 89 74 24", OnEmoteDetour);
                hookEmote.Enable();

                IsValid = true;
            }
            catch (Exception ex)
            {
                Svc.Log.Error(ex, "failed to hook emotes!");
            }
        }

        public void Dispose()
        {
            hookEmote?.Dispose();
            IsValid = false;
        }

        private void OnEmoteDetour(ulong unk, ulong instigatorAddr, ushort emoteId, ulong targetId, ulong unk2)
        {
            Svc.Log.Info($"Emote >> unk:{unk:X}, instigatorAddr:{instigatorAddr:X}, emoteId:{emoteId}, targetId:{targetId:X}, unk2:{unk2:X}");

            if (Svc.Objects.LocalPlayer != null)
            {
                if (instigatorAddr == (ulong)Svc.Objects.LocalPlayer.Address)
                {
                    CURRENT_EMOTE_ID = emoteId;
                    OnEmote?.Invoke(Svc.Objects.LocalPlayer, emoteId);
                }
            }

            hookEmote?.Original(unk, instigatorAddr, emoteId, targetId, unk2);
        }
    }
}
