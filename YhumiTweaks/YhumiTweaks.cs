using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ECommons;
using ECommons.Automation.LegacyTaskManager;
using ECommons.DalamudServices;
using ECommons.Throttlers;
using System.IO;
using YhumiTweaks.Controllers;
using YhumiTweaks.Data;
using YhumiTweaks.UI;

namespace YhumiTweaks;

public sealed class YhumiTweaks : IDalamudPlugin
{
    public string Name => "YhumiTweaks";
    public string Command => "/yhu";

    internal static YhumiTweaks P = null;

    internal TweaksWindow TweaksUI;
    internal WindowSystem ws;
    internal Configuration Config;

    internal TaskManager TM;

    internal GlamController GlamController;

    public YhumiTweaks(IDalamudPluginInterface pluginInterface)
    {
        ECommonsMain.Init(pluginInterface, this, Module.All);
        P = this;

        LuminaSheets.Init();
        P.Config = Configuration.Load();
        P.Config.Save();

        CharacterInfo.SetCharaInventoryPointers();

        TM = new() { AbortOnTimeout = true, TimeLimitMS = 20000 };

        ws = new();
        Config = P.Config;

        TweaksUI = new TweaksWindow();
        GlamController = new GlamController();

        Svc.Commands.AddHandler(Command, new CommandInfo(OnCommand)
        {
            HelpMessage = "Tweaks plugin command. Opens the settings by default.\n",
            // "/bis settings → Opens settings.",
            ShowInHelp = true,
        });

        Svc.PluginInterface.UiBuilder.Draw += ws.Draw;
        Svc.PluginInterface.UiBuilder.OpenConfigUi += DrawSettingsUI;
        Svc.Framework.Update += Tick;
        Svc.ClientState.Login += OnClientLogin;
    }

    public void Tick(object _)
    {
        if (P.Config.AutoGlamWeddingRing)
            GlamController.Tick();   
    }

    public void Dispose()
    {
        ECommonsMain.Dispose();
        LuminaSheets.Dispose();

        Svc.PluginInterface.UiBuilder.OpenConfigUi -= DrawSettingsUI;
        Svc.PluginInterface.UiBuilder.Draw -= ws.Draw;
        GenericHelpers.Safe(() => Svc.Framework.Update -= Tick);
        Svc.ClientState.Login -= OnClientLogin;

        TweaksUI.Dispose();
        GenericHelpers.Safe(() => Svc.Commands.RemoveHandler(Command));

        ws?.RemoveAllWindows();
        ws = null!;

        GlamController.Dispose();
        P = null!;
    }

    private void DrawSettingsUI()
    {
        TweaksUI.IsOpen = true;
    }

    private void OnClientLogin()
    {
        CharacterInfo.SetCharaInventoryPointers();
    }

    private void OnCommand(string command, string args)
    {
        TweaksUI.IsOpen = true;
    }
}
