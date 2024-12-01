using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using KamiLib.Classes;
using KamiLib.CommandManager;
using KamiLib.Extensions;
using KamiLib.Window;
using Mappy.Classes;
using Mappy.Data;

namespace Mappy.Windows;

public class ConfigurationWindow : Window {
    private readonly TabBar tabBar = new("mappy_tab_bar", [
        new IconConfigurationTab(),
        new MapFunctionsTab(),
        new StyleOptionsTab(),
        new PlayerOptionsTab(),
    ]);
    
    public ConfigurationWindow() : base("Mappy 配置窗口", new Vector2(500.0f, 580.0f)) {
        System.CommandManager.RegisterCommand(new CommandHandler {
            Delegate = _ => System.ConfigWindow.Toggle(),
            ActivationPath = "/",
        });
    }
    
    protected override void DrawContents() 
        => tabBar.Draw();
}

public class MapFunctionsTab : ITabItem {
    public string Name => "地图功能";
    
    public bool Disabled => false;
    
    public void Draw() {
        var configChanged = false;
        
        ImGuiTweaks.Header("按键输入");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox("忽略ESC键", ref System.SystemConfig.IgnoreEscapeKey);
        }
        
        ImGuiTweaks.Header("缩放选项");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox("使用线性缩放", ref System.SystemConfig.UseLinearZoom);
            configChanged |= ImGui.Checkbox("图标尺寸跟随缩放", ref System.SystemConfig.ScaleWithZoom);
            configChanged |= ImGuiTweaks.Checkbox("自动缩放", ref System.SystemConfig.AutoZoom, "自动根据地图大小将缩放设置为一个合理的值。");
            
            ImGuiHelpers.ScaledDummy(5.0f);
            
            configChanged |= ImGui.SliderFloat("缩放速度", ref System.SystemConfig.ZoomSpeed, 0.001f, 0.500f);
            configChanged |= ImGui.SliderFloat("图标尺寸", ref System.SystemConfig.IconScale, 0.10f, 3.0f);
        }
        
        ImGuiTweaks.Header("打开地图时");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox("跟随打开", ref System.SystemConfig.FollowOnOpen);   
            
            ImGuiHelpers.ScaledDummy(5.0f);

            DrawCenterModeRadio();
        }
        
        ImGuiTweaks.Header("链接行为");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox("以旗帜居中", ref System.SystemConfig.CenterOnFlag);
            configChanged |= ImGui.Checkbox("以采集区域居中", ref System.SystemConfig.CenterOnGathering);
            configChanged |= ImGui.Checkbox("以任务居中", ref System.SystemConfig.CenterOnQuest);
        }
        
        ImGuiTweaks.Header("杂项");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox("显示其他工具提示", ref System.SystemConfig.ShowMiscTooltips);
            configChanged |= ImGui.Checkbox("锁定地图居中", ref System.SystemConfig.LockCenterOnMap);
            configChanged |= ImGui.Checkbox("显示其他玩家", ref System.SystemConfig.ShowPlayers);
            
            ImGuiHelpers.ScaledDummy(5.0f);
            
            configChanged |= ImGui.Checkbox("调试模式", ref System.SystemConfig.DebugMode);
        }
        
        ImGuiTweaks.Header("工具栏");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox("始终显示", ref System.SystemConfig.AlwaysShowToolbar);
            configChanged |= ImGui.Checkbox("悬停时显示", ref System.SystemConfig.ShowToolbarOnHover);
            
            ImGuiHelpers.ScaledDummy(5.0f);
            configChanged |= ImGui.DragFloat("不透明度##toolbar", ref System.SystemConfig.ToolbarFade, 0.01f, 0.0f, 1.0f);
        }
        
        ImGuiTweaks.Header("坐标");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox("显示坐标栏", ref System.SystemConfig.ShowCoordinateBar);
            
            ImGuiHelpers.ScaledDummy(5.0f);
            configChanged |= ImGuiTweaks.ColorEditWithDefault("文字颜色", ref System.SystemConfig.CoordinateTextColor, KnownColor.White.Vector());

            ImGuiHelpers.ScaledDummy(5.0f);
            configChanged |= ImGui.DragFloat("不透明度##coordinatebar", ref System.SystemConfig.CoordinateBarFade, 0.01f, 0.0f, 1.0f);
        }
        
        if (configChanged) {
            SystemConfig.Save();
        }
    }
    
    private void DrawCenterModeRadio() {
        var enumObject = System.SystemConfig.CenterOnOpen;
        var firstLine = true;

        foreach (Enum enumValue in Enum.GetValues(enumObject.GetType())) {
            if (!firstLine) ImGui.SameLine();

            if (ImGui.RadioButton(enumValue.GetDescription(), enumValue.Equals(enumObject))) {
                System.SystemConfig.CenterOnOpen = (CenterTarget) enumValue;
                SystemConfig.Save();
            }

            firstLine = false;
        }
        
        ImGui.SameLine(); 
        ImGui.Text("\t\t开启时居中");
    }
}

public class StyleOptionsTab : ITabItem {
    public string Name => "样式";
    
    public bool Disabled => false;
    
    public void Draw() {
        var configChanged = false;
        
        ImGuiTweaks.Header("窗口选项");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox("保持打开", ref System.SystemConfig.KeepOpen);
            configChanged |= ImGui.Checkbox("锁定窗口位置", ref System.SystemConfig.LockWindow);
            configChanged |= ImGui.Checkbox("隐藏窗口边框", ref System.SystemConfig.HideWindowFrame);
            configChanged |= ImGui.Checkbox("启用 Shift + 拖动来移动窗口边框", ref System.SystemConfig.EnableShiftDragMove);
        }
        
        ImGuiTweaks.Header("窗口隐藏");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox("在打开游戏界面时隐藏", ref System.SystemConfig.HideWithGameGui);
            configChanged |= ImGui.Checkbox("在切换区域时隐藏", ref System.SystemConfig.HideBetweenAreas);
            configChanged |= ImGui.Checkbox("在副本中隐藏", ref System.SystemConfig.HideInDuties);
            configChanged |= ImGui.Checkbox("在战斗状态时隐藏", ref System.SystemConfig.HideInCombat);
        }
        
        ImGuiTweaks.Header("窗口标题");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox("显示区域文本", ref System.SystemConfig.ShowRegionLabel);
            configChanged |= ImGui.Checkbox("显示地图文本", ref System.SystemConfig.ShowMapLabel);
            configChanged |= ImGui.Checkbox("显示地区文本", ref System.SystemConfig.ShowAreaLabel);
            configChanged |= ImGui.Checkbox("显示子地区文本", ref System.SystemConfig.ShowSubAreaLabel);
        }
        
        ImGuiTweaks.Header("窗口位置");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.DragFloat2("窗口位置", ref System.SystemConfig.WindowPosition);
            configChanged |= ImGui.DragFloat2("窗口尺寸", ref System.SystemConfig.WindowSize);
        }
        
        ImGuiTweaks.Header("淡化选项");
        using (ImRaii.PushIndent()) {
            using (var columns = ImRaii.Table("fade_options_toggles", 2)) {
                if (!columns) return;

                var value = System.SystemConfig.FadeMode;
                ImGui.TableNextColumn();

                foreach (Enum enumValue in Enum.GetValues(value.GetType())) {
                    var isFlagSet = value.HasFlag(enumValue);
                    if (ImGuiComponents.ToggleButton(enumValue.ToString(), ref isFlagSet)) {
                        var sourceValue = Convert.ToInt32(value);
                        var targetValue = Convert.ToInt32(enumValue);

                        if (value.HasFlag(enumValue)) {
                            System.SystemConfig.FadeMode = (FadeMode) Enum.ToObject(value.GetType(), sourceValue & ~targetValue);
                        }
                        else {
                            System.SystemConfig.FadeMode = (FadeMode) Enum.ToObject(value.GetType(), sourceValue | targetValue);
                        }

                        configChanged = true;
                    }
                    ImGui.SameLine();
                    ImGui.TextUnformatted(enumValue.GetDescription());

                    ImGui.TableNextColumn();
                } 
            }

            configChanged |= ImGui.DragFloat("淡化不透明度", ref System.SystemConfig.FadePercent, 0.01f, 0.05f, 1.0f);
        }
        
        ImGuiTweaks.Header("区域风格");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGuiTweaks.ColorEditWithDefault("区域颜色", ref System.SystemConfig.AreaColor, KnownColor.CornflowerBlue.Vector() with { W = 0.33f });
            configChanged |= ImGuiTweaks.ColorEditWithDefault("区域轮廓颜色", ref System.SystemConfig.AreaOutlineColor, KnownColor.CornflowerBlue.Vector() with { W = 0.30f });
        }

        if (configChanged) {
            if (System.MapWindow.SizeConstraints is { } constraints) {
                System.SystemConfig.WindowSize.X = MathF.Max(System.SystemConfig.WindowSize.X, constraints.MinimumSize.X);
                System.SystemConfig.WindowSize.Y = MathF.Max(System.SystemConfig.WindowSize.Y, constraints.MinimumSize.Y);
            }
            
            System.MapWindow.RefreshTitle();
            SystemConfig.Save();
        }
    }
}

public class PlayerOptionsTab : ITabItem {
    public string Name => "玩家";
    
    public bool Disabled => false;
    
    public void Draw() {
        var configChanged = false;
        
        ImGuiTweaks.Header("锥体选项");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox("缩放玩家锥体", ref System.SystemConfig.ScalePlayerCone);

            ImGuiHelpers.ScaledDummy(5.0f);
            configChanged |= ImGui.DragFloat("锥体尺寸", ref System.SystemConfig.ConeSize, 0.25f);

            ImGuiHelpers.ScaledDummy(5.0f);
            configChanged |= ImGuiTweaks.ColorEditWithDefault("锥体颜色", ref System.SystemConfig.PlayerConeColor, KnownColor.CornflowerBlue.Vector() with { W = 0.33f });
            configChanged |= ImGuiTweaks.ColorEditWithDefault("锥体轮廓颜色", ref System.SystemConfig.PlayerConeOutlineColor, KnownColor.CornflowerBlue.Vector() with { W = 1.00f });
        }
        
        ImGuiTweaks.Header("雷达选项");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox("显示雷达半径", ref System.SystemConfig.ShowRadar);

            ImGuiHelpers.ScaledDummy(5.0f);
            
            configChanged |= ImGuiTweaks.ColorEditWithDefault("雷达区域颜色", ref System.SystemConfig.RadarColor, KnownColor.Gray.Vector() with { W = 0.10f });
            configChanged |= ImGuiTweaks.ColorEditWithDefault("雷达轮廓颜色", ref System.SystemConfig.RadarOutlineColor, KnownColor.Gray.Vector() with { W = 0.30f });
        }
        
        ImGuiTweaks.Header("玩家图标选项");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox("显示玩家图标", ref System.SystemConfig.ShowPlayerIcon);
            
            ImGuiHelpers.ScaledDummy(5.0f);
            configChanged |= ImGui.DragFloat("玩家图标尺寸", ref System.SystemConfig.PlayerIconScale, 0.05f);
        }

        if (configChanged) {
            SystemConfig.Save();
        }
    }
}

public class IconConfigurationTab : ITabItem {
    public string Name => "图标设置";
    
    public bool Disabled => false;

    private IconSetting? currentSetting;

    public void Draw() {
        using (var leftChild = ImRaii.Child("left_child", new Vector2(48.0f * ImGuiHelpers.GlobalScale + ImGui.GetStyle().ItemSpacing.X , ImGui.GetContentRegionAvail().Y))) {
            if (leftChild) {
                using var selectionList = ImRaii.ListBox("iconSelection", ImGui.GetContentRegionAvail());
            
                foreach (var (iconId, settings) in System.IconConfig.IconSettingMap.OrderBy(pairData =>  pairData.Key)) {
                    if (iconId is 0) continue;
                    if (DrawHelpers.IsDisallowedIcon(iconId)) continue;

                    var texture = Service.TextureProvider.GetFromGameIcon(iconId).GetWrapOrEmpty();
                    var cursorStart = ImGui.GetCursorScreenPos();
                    if (ImGui.Selectable($"##iconSelect{iconId}", currentSetting == settings, ImGuiSelectableFlags.None, ImGuiHelpers.ScaledVector2(32.0f, 32.0f))) {
                        currentSetting = currentSetting == settings ? null : settings;
                    }  
                
                    ImGui.SetCursorScreenPos(cursorStart);
                    ImGui.Image(texture.ImGuiHandle, texture.Size / 2.0f * ImGuiHelpers.GlobalScale);
                } 
            }
        }
        
        ImGui.SameLine();
       
        using (var rightChild = ImRaii.Child("right_child", ImGui.GetContentRegionAvail(), false, ImGuiWindowFlags.NoScrollbar)) {
            if (rightChild) {
                if (currentSetting is null) {
                    using var textColor = ImRaii.PushColor(ImGuiCol.Text, KnownColor.Orange.Vector());

                    ImGui.SetCursorPosY(ImGui.GetContentRegionAvail().Y / 2.0f);
                    ImGuiHelpers.CenteredText("选择一个图标来编辑设置");
                }
                else {
                    // Draw background texture
                    var settingsChanged = false;
                    var texture = Service.TextureProvider.GetFromGameIcon(currentSetting.IconId).GetWrapOrEmpty();
                    var smallestAxis = MathF.Min(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y);

                    if (ImGui.GetContentRegionAvail().X > ImGui.GetContentRegionAvail().Y) {
                        var remainingSpace = ImGui.GetContentRegionAvail().X - smallestAxis;
                        ImGui.SetCursorPosX(remainingSpace / 2.0f);
                    }
                
                    ImGui.Image(texture.ImGuiHandle, new Vector2(smallestAxis, smallestAxis), Vector2.Zero, Vector2.One, new Vector4(1.0f, 1.0f, 1.0f, 0.20f));
                    ImGui.SetCursorPos(Vector2.Zero);
                    
                    // Draw settings
                    ImGuiTweaks.Header($"配置标记 #{currentSetting.IconId}");
                    using (ImRaii.PushIndent()) {
                        settingsChanged |= ImGui.Checkbox("隐藏图标", ref currentSetting.Hide);
                        settingsChanged |= ImGui.Checkbox("启用提示框", ref currentSetting.AllowTooltip);
                        settingsChanged |= ImGui.Checkbox("启用点击交互", ref currentSetting.AllowClick);
                
                        ImGuiHelpers.ScaledDummy(5.0f);
                        settingsChanged |= ImGuiTweaks.ColorEditWithDefault("颜色", ref currentSetting.Color, KnownColor.White.Vector());

                        ImGuiHelpers.ScaledDummy(5.0f);
                        settingsChanged |= ImGui.DragFloat("图标尺寸", ref currentSetting.Scale, 0.01f, 0.05f, 20.0f);
                    }

                    ImGui.SetCursorPosY(ImGui.GetContentRegionMax().Y - 25.0f * ImGuiHelpers.GlobalScale);
                    if (ImGui.Button("重置为默认值", new Vector2(ImGui.GetContentRegionAvail().X, 25.0f * ImGuiHelpers.GlobalScale))) {
                        currentSetting.Reset();
                        System.IconConfig.Save();
                    }
                
                    if (settingsChanged) {
                        System.IconConfig.Save();
                    }
                }
            }
        }
    }
}