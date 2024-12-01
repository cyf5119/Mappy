using System;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using Mappy.Classes;
using Mappy.Extensions;

namespace Mappy.Modules;

public class FateModule : ModuleBase {
    public override unsafe bool ProcessMarker(MarkerInfo markerInfo) {
        var markerName = markerInfo.PrimaryText?.Invoke();
        if (markerName.IsNullOrEmpty()) return false;
        
        foreach (var fate in FateManager.Instance()->Fates) {
            var name = fate.Value->Name.ToString();
            
            if (name.Equals(markerName, StringComparison.OrdinalIgnoreCase)) {
                var timeRemaining = fate.GetTimeRemaining();
                
                markerInfo.PrimaryText = () => $"Lv. {fate.Value->Level} {fate.Value->Name}";

                if (timeRemaining >= TimeSpan.Zero) {
                    markerInfo.SecondaryText = () => $"剩余时间 {timeRemaining:mm\\:ss}\n进度 {fate.Value->Progress}%";

                    if (timeRemaining.TotalSeconds <= 300) {
                        markerInfo.RadiusColor = fate.GetColor();
                        markerInfo.RadiusOutlineColor = fate.GetColor();
                    }
                }
                else {
                    markerInfo.SecondaryText = () => $"进度 {fate.Value->Progress}%";
                }
                
                return true;
            }
        }

        return false;
    }
}