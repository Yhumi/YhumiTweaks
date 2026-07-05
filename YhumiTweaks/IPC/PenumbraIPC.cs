using ECommons.DalamudServices;
using ECommons.EzIpcManager;
using Penumbra.Api.Enums;
using Penumbra.Api.IpcSubscribers;
using System;
using System.Collections.Generic;
using System.Text;

namespace YhumiTweaks.IPC
{
    public class PenumbraIPC
    {
        private GetCollections _getCollections;
        private GetModList _getModList;
        private GetCurrentModSettings _getCurrentModSettings;

        private TrySetMod _trySetMod;
        private TrySetModSettings _trySetModSettings;
        private TrySetModPriority _trySetModPriority;

        public PenumbraIPC()
        {
            _getCollections = new GetCollections(Svc.PluginInterface);
            _getModList = new GetModList(Svc.PluginInterface);
            _getCurrentModSettings = new GetCurrentModSettings(Svc.PluginInterface);

            _trySetMod = new TrySetMod(Svc.PluginInterface);
            _trySetModSettings = new TrySetModSettings(Svc.PluginInterface);
            _trySetModPriority = new TrySetModPriority(Svc.PluginInterface);
        }

        public Dictionary<Guid, string> GetCollections()
        {
            try
            {
                return _getCollections.Invoke();
            }
            catch (Exception ex)
            {
                Svc.Log.Error($"Could not get collections: {ex.Message}");
                return new Dictionary<Guid, string>();
            }
        }

        public Dictionary<string, string> GetModList()
        {
            try
            {
                return _getModList.Invoke();
            }
            catch (Exception ex)
            {
                Svc.Log.Error($"Could not get mods: {ex.Message}");
                return new Dictionary<string, string>();
            }
        }

        public (PenumbraApiEc result, (bool enabled, int priority, Dictionary<string, List<string>> settings, bool unk)? settings) GetModSettings(Guid collection, string modName)
        {
            try
            {
                return _getCurrentModSettings.Invoke(collection, modName);
            }
            catch (Exception ex)
            {
                Svc.Log.Error($"Could not get mods: {ex.Message}");
                return (PenumbraApiEc.UnknownError, null);
            }
        }

        public void TrySetMod(Guid targetCollectionId, string modName, bool enabled)
        {
            try
            {
                _trySetMod.Invoke(targetCollectionId, modName, enabled);
            }
            catch (Exception ex)
            {
                Svc.Log.Error($"TrySetMod Error: {ex.Message}");
            }
        }

        public void TrySetModSettings(Guid targetCollectionId, string modName, Dictionary<string, List<string>> settings)
        {
            try
            {
                foreach (var setting in settings)
                {
                    _trySetModSettings.Invoke(targetCollectionId, modName, setting.Key, setting.Value);
                }
            }
            catch (Exception ex)
            {
                Svc.Log.Error($"TrySetModSettings Error: {ex.Message}");
            }
        }
    
        public void TrySetModPriority(Guid targetCollectionId, string modName, int prio)
        {
            try
            {
                _trySetModPriority.Invoke(targetCollectionId, modName, prio);
            }
            catch (Exception ex)
            {
                Svc.Log.Error($"TrySetModPriority Error: {ex.Message}");
            }
        }
    }
}
