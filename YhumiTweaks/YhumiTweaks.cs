using Dalamud.Game.Chat;
using Dalamud.Game.Command;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using ECommons;
using ECommons.Automation.LegacyTaskManager;
using ECommons.DalamudServices;
using ECommons.Throttlers;
using Microsoft.VisualBasic;
using System;
using System.IO;
using System.Linq;
using YhumiTweaks.Controllers;
using YhumiTweaks.Data;
using YhumiTweaks.Helpers;
using YhumiTweaks.IPC;
using YhumiTweaks.UI;

namespace YhumiTweaks;

public sealed class YhumiTweaks : IDalamudPlugin
{
    public string Name => "MintTweaks";
    public string Command => "/minty";

    internal static YhumiTweaks P = null;

    internal TweaksWindow TweaksUI;
    internal WindowSystem ws;
    internal Configuration Config;

    internal TaskManager TM;

    internal GlamController GlamController;
    internal PenumbraIPC PenumbraIPC;

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

        new ECommons.Schedulers.TickScheduler(Load);

        Svc.PluginInterface.UiBuilder.Draw += ws.Draw;
        Svc.PluginInterface.UiBuilder.OpenConfigUi += DrawSettingsUI;
        Svc.Framework.Update += Tick;
        Svc.ClientState.Login += OnClientLogin;
        Svc.ClientState.TerritoryChanged += OnTerritoryChanged;
        Svc.Chat.ChatMessage += OnChatMessage;

        if (Svc.ClientState.IsLoggedIn && P.Config.AutoCorrectCameraHeight)
            GameSettings.UpdateTiltToExpected();
    }

    public void Load()
    {
        Svc.Log.Info($"Setting up IPC...");
        PenumbraIPC = new PenumbraIPC();
    }

    private void OnChatMessage(IHandleableChatMessage message)
    {
        var sender = message.Sender;
        var type = message.LogKind;
        var msg = message.OriginalMessage.ToDalamudString();

        var senderPayload = sender.Payloads.OfType<PlayerPayload>().FirstOrDefault();
        var senderName = senderPayload?.PlayerName ?? "";
        var senderWorld = senderPayload?.World.Value.Name.ToString() ?? "";

        Svc.Log.Info($"Chat message received: Type={type}, Sender={senderName} ({senderWorld}), Message={msg}");
    }

    private void OnTerritoryChanged(uint obj)
    {
        Svc.Log.Info($"Instance changed. But ");

        if (P.Config.AutoCorrectCameraHeight)
            GameSettings.UpdateTiltToExpected();
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

        if (P.Config.AutoCorrectCameraHeight)
            GameSettings.UpdateTiltToExpected();
    }

    private void OnCommand(string command, string args)
    {
        TweaksUI.IsOpen = true;
    }
}
