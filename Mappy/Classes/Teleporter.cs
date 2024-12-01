using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;
using Lumina.Excel.GeneratedSheets;

namespace Mappy.Classes;

public class Teleporter {
    private readonly ICallGateSubscriber<uint, byte, bool> teleportIpc = Service.PluginInterface.GetIpcSubscriber<uint, byte, bool>("Teleport");
    private readonly ICallGateSubscriber<bool> showChatMessageIpc = Service.PluginInterface.GetIpcSubscriber<bool>("Teleport.ChatMessage");

    public void Teleport(Aetheryte aetheryte) {
        try {
            var didTeleport = teleportIpc.InvokeFunc(aetheryte.RowId, (byte) aetheryte.SubRowId);
            var showMessage = showChatMessageIpc.InvokeFunc();

            if (!didTeleport) {
                UserError("在这种情况下无法传送。");
            }
            else if (showMessage) {
                Service.ChatGui.Print(new XivChatEntry {
                    Message = new SeStringBuilder()
                        .AddUiForeground("[Mappy] ", 45)
                        .AddUiForeground($"[Teleport] ", 62)
                        .AddText($"传送到 ")
                        .AddUiForeground(aetheryte.PlaceName.Value?.Name ?? "无法读取名称", 576)
                        .Build(),
                });
            }
        } catch (IpcNotReadyError) {
            Service.Log.Error("找不到传送IPC");
            UserError("要使用传送功能，必须安装“Teleporter”插件");
        }
    }

    private static void UserError(string error) {
        Service.ChatGui.PrintError(error);
        Service.ToastGui.ShowError(error);
    }
}