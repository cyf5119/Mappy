using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface;
using Dalamud.Memory;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.GeneratedSheets;
using Mappy.Classes;

namespace Mappy.Extensions;

public static class MapMarkerInfoExtensions {
    public  static unsafe void Draw(this MapMarkerInfo marker, Vector2 offset, float scale) {
        var tooltipText = MemoryHelper.ReadSeStringNullTerminated((nint) marker.MapMarker.Subtext);
        
        DrawHelpers.DrawMapMarker(new MarkerInfo {
            // Divide by 16, as it seems they use a fixed scalar
            // Add 1024 * scale, to offset from top-left, to center-based coordinate
            // Add offset for drawing relative to map when its moved around
            Position = new Vector2(marker.MapMarker.X, marker.MapMarker.Y) / 16.0f * scale + DrawHelpers.GetMapCenterOffsetVector() * scale,
            Offset = offset,
            Scale = scale,
            Radius = marker.MapMarker.Scale,
            RadiusColor = KnownColor.MediumPurple.Vector(),
            IconId = marker.MapMarker.IconId,
            PrimaryText = GetMarkerPrimaryTooltip(marker, tooltipText),
            OnLeftClicked = marker.DataType switch {
                1 when !DrawHelpers.IsDisallowedIcon(marker.MapMarker.IconId) => () => System.IntegrationsController.OpenMap(marker.DataKey),
                3 => () => System.Teleporter.Teleport(Service.DataManager.GetExcelSheet<Aetheryte>()!.GetRow(marker.DataKey)!), // Gonna assume that can't be null, because it's a row index that comes from the active gamestate.
                4 when GetAetheryteForAethernet(marker.DataKey) is not null => () => System.Teleporter.Teleport(GetAetheryteForAethernet(marker.DataKey)!),
                _ => null,
            },
            SecondaryText = marker.DataType switch {
                1 when !DrawHelpers.IsDisallowedIcon(marker.MapMarker.IconId) => () => $"打开地图 {Service.DataManager.GetExcelSheet<Map>()!.GetRow(marker.DataKey)?.PlaceName.Value?.Name ?? "无法读取目标地图名称。"}",
                2 => () => $"实例链接 {marker.DataKey}",
                3 => () => $"传送到 {Service.DataManager.GetExcelSheet<Aetheryte>()!.GetRow(marker.DataKey)?.PlaceName.Value?.Name ?? "无法读取以太之光名称"} {GetAetheryteTeleportCost(marker.DataKey)}",
                4 when GetAetheryteForAethernet(marker.DataKey) is not null => () => $"传送到 {GetAetheryteForAethernet(marker.DataKey)?.PlaceName.Value?.Name ?? "无法读取以太之光名称"} {GetAetheryteTeleportCost(GetAetheryteForAethernet(marker.DataKey)!.RowId)}",
                _ => null,
            },
        });
    }

    private static Aetheryte? GetAetheryteForAethernet(uint aethernetKey)
        => System.AetheryteAethernetCache.GetValue(aethernetKey);

    private static string GetAetheryteTeleportCost(uint targetDataKey) 
        => $"({Service.AetheryteList.FirstOrDefault(entry => entry.AetheryteId == targetDataKey)?.GilCost ?? 0:n0} {SeIconChar.Gil.ToIconChar()})";

    private static Func<string> GetMarkerPrimaryTooltip(MapMarkerInfo marker, SeString tooltipText) {
        if (DrawHelpers.IsDisallowedIcon(marker.MapMarker.IconId)) return () => string.Empty;
        if (!System.SystemConfig.ShowMiscTooltips) return () => string.Empty;
        if (!tooltipText.TextValue.IsNullOrEmpty()) return tooltipText.ToString;
        
        return marker.DataType switch {
            4 => () => Service.DataManager.GetExcelSheet<PlaceName>()!.GetRow(marker.DataKey)!.Name.ToString(),
            _ => () => System.TooltipCache.GetValue(marker.MapMarker.IconId) ?? string.Empty,
        };
    }
}