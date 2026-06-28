using Sunny.UI;
using System.Drawing.Drawing2D;
using System.Text.Json;
using tmcreator.Models.EffectPreview;

namespace tmcreator
{
    public partial class EffectPreviewForm : UIForm
    {
        private static readonly Color ClrBg = Color.FromArgb(9, 14, 21);
        private static readonly Color ClrPanel = Color.FromArgb(17, 25, 36);
        private static readonly Color ClrPanelAlt = Color.FromArgb(22, 32, 46);
        private static readonly Color ClrInput = Color.FromArgb(10, 16, 24);
        private static readonly Color ClrBorder = Color.FromArgb(52, 70, 92);
        private static readonly Color ClrSoftBorder = Color.FromArgb(38, 52, 70);
        private static readonly Color ClrText = Color.FromArgb(238, 244, 252);
        private static readonly Color ClrSubText = Color.FromArgb(156, 174, 198);
        private static readonly Color ClrMuted = Color.FromArgb(105, 126, 150);
        private static readonly Color ClrAccent = Color.FromArgb(65, 210, 198);
        private static readonly Color ClrAccentBlue = Color.FromArgb(87, 117, 238);
        private static readonly Color ClrAccentGold = Color.FromArgb(246, 181, 70);

        private static readonly Font FontTitle = new("Microsoft YaHei UI", 17F, FontStyle.Bold, GraphicsUnit.Point);
        private static readonly Font FontSection = new("Microsoft YaHei UI", 10.5F, FontStyle.Bold, GraphicsUnit.Point);
        private static readonly Font FontBody = new("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        private static readonly Font FontBodyBold = new("Microsoft YaHei UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        private static readonly Font FontSmall = new("Microsoft YaHei UI", 8F, FontStyle.Regular, GraphicsUnit.Point);
        private static readonly Font FontInspector = new("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

        private readonly System.Windows.Forms.Timer _renderTimer;
        private readonly List<EffectPreset> _presets;
        private readonly JsonSerializerOptions _jsonOptions;

        private EffectPreset _preset;
        private EffectEngine _engine;
        private EffectEditorLayer _activeLayer = EffectEditorLayer.Projectile;
        private bool _paused;
        private bool _slowMode;
        private DateTime _lastFrameTime;
        private Point _dragStartMouse;
        private PointF _dragStartWorld;
        private bool _draggingCanvas;

        private readonly BufferedPanel _header = new();
        private readonly BufferedPanel _leftPanel = new();
        private readonly BufferedPanel _canvas = new();
        private readonly BufferedPanel _rightPanel = new();
        private readonly BufferedPanel _bottomBar = new();
        private readonly ListBox _presetList = new();
        private readonly ListBox _layerList = new();
        private readonly BufferedPanel _propViewport = new();
        private readonly Panel _propContent = new();
        private readonly BufferedPanel _propScrollBar = new();
        private readonly Label _fpsLabel = new();
        private readonly Label _presetDescription = new();
        private readonly UIButton _btnPlay = new();
        private readonly UIButton _btnSlow = new();
        private int _propContentHeight;
        private int _propScrollOffset;
        private bool _propScrollDragging;
        private int _propScrollDragStartY;
        private int _propScrollDragStartOffset;

        public EffectPreviewForm()
        {
            _presets = EffectPresets.GetAllPresets();
            _preset = _presets[0].Clone();
            _engine = new EffectEngine(_preset);
            _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            _jsonOptions.Converters.Add(new DrawingColorJsonConverter());

            Text = "特效可视化编辑器";
            ClientSize = new Size(1280, 760);
            MinimumSize = new Size(1020, 640);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = ClrBg;
            Style = UIStyle.Black;

            _renderTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _renderTimer.Tick += RenderTimer_Tick;

            BuildLayout();
            Resize += (s, e) => LayoutShell();
            FormClosing += (s, e) => _renderTimer.Stop();

            _lastFrameTime = DateTime.UtcNow;
            _renderTimer.Start();
        }

        private void BuildLayout()
        {
            SuspendLayout();
            Controls.Clear();

            BuildHeader();
            BuildLeftPanel();
            BuildCanvas();
            BuildRightPanel();
            BuildBottomBar();

            Controls.Add(_header);
            Controls.Add(_leftPanel);
            Controls.Add(_canvas);
            Controls.Add(_rightPanel);
            Controls.Add(_bottomBar);

            LayoutShell();
            ResumeLayout(false);
        }

        private void BuildHeader()
        {
            _header.BackColor = ClrBg;
            _header.Paint += (s, e) =>
            {
                e.Graphics.Clear(ClrBg);
                using var border = new Pen(ClrSoftBorder);
                e.Graphics.DrawLine(border, 0, _header.Height - 1, _header.Width, _header.Height - 1);
                using var accent = new Pen(ClrAccent, 3f);
                e.Graphics.DrawLine(accent, 0, _header.Height - 1, 260, _header.Height - 1);
            };

            _header.Controls.Add(CreateLabel("特效预览", new Point(24, 12), new Size(280, 32), FontTitle, ClrText));
            _header.Controls.Add(CreateLabel("实时调整弹幕头部、流光尾焰与碰撞散开，导出完整 ModProjectile .cs", new Point(26, 47), new Size(620, 22), FontBody, ClrSubText));

            _fpsLabel.Text = "FPS: --";
            _fpsLabel.Font = FontSmall;
            _fpsLabel.ForeColor = ClrMuted;
            _fpsLabel.TextAlign = ContentAlignment.MiddleRight;
            _fpsLabel.BackColor = Color.Transparent;
            _header.Controls.Add(_fpsLabel);
        }

        private void BuildLeftPanel()
        {
            _leftPanel.BackColor = ClrPanel;
            _leftPanel.Paint += Panel_Paint;

            _leftPanel.Controls.Add(CreateLabel("预设", new Point(18, 16), new Size(120, 24), FontSection, ClrText));
            _presetDescription.SetBounds(18, 42, 220, 40);
            _presetDescription.Font = FontSmall;
            _presetDescription.ForeColor = ClrSubText;
            _presetDescription.BackColor = Color.Transparent;
            _leftPanel.Controls.Add(_presetDescription);

            ConfigureDarkList(_presetList, 26);
            _presetList.DrawItem += PresetList_DrawItem;
            _presetList.SelectedIndexChanged += PresetList_SelectedIndexChanged;
            _leftPanel.Controls.Add(_presetList);

            _leftPanel.Controls.Add(CreateLabel("图层", new Point(18, 372), new Size(120, 24), FontSection, ClrText));
            ConfigureDarkList(_layerList, 34);
            _layerList.DrawItem += LayerList_DrawItem;
            _layerList.SelectedIndexChanged += LayerList_SelectedIndexChanged;
            _leftPanel.Controls.Add(_layerList);

            RefreshPresetList();
            RefreshLayerList();
        }

        private void BuildCanvas()
        {
            _canvas.BackColor = Color.Black;
            _canvas.Paint += Canvas_Paint;
            _canvas.MouseDown += Canvas_MouseDown;
            _canvas.MouseMove += Canvas_MouseMove;
            _canvas.MouseUp += Canvas_MouseUp;
            _canvas.MouseWheel += Canvas_MouseWheel;
            _canvas.TabStop = true;
        }

        private void BuildRightPanel()
        {
            _rightPanel.BackColor = ClrPanel;
            _rightPanel.Paint += Panel_Paint;
            _rightPanel.Controls.Add(CreateLabel("参数检查器", new Point(18, 16), new Size(180, 24), FontSection, ClrText));

            _propViewport.BackColor = ClrPanel;
            _propViewport.Paint += (s, e) => e.Graphics.Clear(ClrPanel);
            _propViewport.MouseWheel += PropScroll_MouseWheel;

            _propContent.BackColor = ClrPanel;
            _propContent.AutoScroll = false;
            _propContent.MouseWheel += PropScroll_MouseWheel;
            _propViewport.Controls.Add(_propContent);

            _propScrollBar.BackColor = ClrPanel;
            _propScrollBar.Cursor = Cursors.Hand;
            _propScrollBar.Paint += PropScrollBar_Paint;
            _propScrollBar.MouseDown += PropScrollBar_MouseDown;
            _propScrollBar.MouseMove += PropScrollBar_MouseMove;
            _propScrollBar.MouseUp += (s, e) => _propScrollDragging = false;

            _rightPanel.Controls.Add(_propViewport);
            _rightPanel.Controls.Add(_propScrollBar);

            RefreshPropertyPanel();
        }

        private void BuildBottomBar()
        {
            _bottomBar.BackColor = ClrPanel;
            _bottomBar.Paint += (s, e) =>
            {
                e.Graphics.Clear(ClrPanel);
                using var pen = new Pen(ClrSoftBorder);
                e.Graphics.DrawLine(pen, 0, 0, _bottomBar.Width, 0);
            };

            var btnImport = CreateActionButton("导入配置", Color.FromArgb(35, 48, 65), ClrBorder);
            btnImport.Click += (s, e) => ImportConfig();
            _bottomBar.Controls.Add(btnImport);

            var btnExportConfig = CreateActionButton("导出配置", Color.FromArgb(35, 48, 65), ClrBorder);
            btnExportConfig.Click += (s, e) => ExportConfig();
            _bottomBar.Controls.Add(btnExportConfig);

            var btnExportCs = CreateActionButton("导出 .cs", ClrAccentGold, Color.FromArgb(183, 125, 32), Color.FromArgb(20, 24, 30));
            btnExportCs.Click += (s, e) => ExportProjectileCode();
            _bottomBar.Controls.Add(btnExportCs);

            var btnImpact = CreateActionButton("触发碰撞", Color.FromArgb(42, 108, 220), Color.FromArgb(55, 126, 240));
            btnImpact.Click += (s, e) =>
            {
                _engine.TriggerImpact();
                _canvas.Invalidate();
            };
            _bottomBar.Controls.Add(btnImpact);

            _btnPlay.Text = "暂停";
            StyleActionButton(_btnPlay, Color.FromArgb(35, 48, 65), ClrBorder);
            _btnPlay.Click += (s, e) =>
            {
                _paused = !_paused;
                _btnPlay.Text = _paused ? "播放" : "暂停";
            };
            _bottomBar.Controls.Add(_btnPlay);

            _btnSlow.Text = "慢速";
            StyleActionButton(_btnSlow, Color.FromArgb(35, 48, 65), ClrBorder);
            _btnSlow.Click += (s, e) =>
            {
                _slowMode = !_slowMode;
                _btnSlow.FillColor = _slowMode ? Color.FromArgb(53, 199, 183) : Color.FromArgb(35, 48, 65);
                _btnSlow.ForeColor = _slowMode ? Color.FromArgb(8, 18, 24) : ClrText;
            };
            _bottomBar.Controls.Add(_btnSlow);

            var btnReset = CreateActionButton("重置", Color.FromArgb(35, 48, 65), ClrBorder);
            btnReset.Click += (s, e) =>
            {
                _engine.Reset();
                _canvas.Invalidate();
            };
            _bottomBar.Controls.Add(btnReset);
        }

        private void LayoutShell()
        {
            int margin = 14;
            int headerH = 78;
            int bottomH = 62;
            int leftW = 260;
            int rightW = 352;
            int gap = 10;

            _header.SetBounds(margin, margin, ClientSize.Width - margin * 2, headerH);
            _bottomBar.SetBounds(margin, ClientSize.Height - margin - bottomH, ClientSize.Width - margin * 2, bottomH);

            int top = _header.Bottom + gap;
            int bottom = _bottomBar.Top - gap;
            int h = Math.Max(260, bottom - top);

            _leftPanel.SetBounds(margin, top, leftW, h);
            _rightPanel.SetBounds(ClientSize.Width - margin - rightW, top, rightW, h);
            _canvas.SetBounds(_leftPanel.Right + gap, top, Math.Max(220, _rightPanel.Left - _leftPanel.Right - gap * 2), h);

            _fpsLabel.SetBounds(_header.Width - 130, 28, 120, 24);

            _presetList.SetBounds(18, 92, _leftPanel.Width - 36, 266);
            _layerList.SetBounds(18, 404, _leftPanel.Width - 36, Math.Max(120, _leftPanel.Height - 424));
            _propViewport.SetBounds(14, 52, _rightPanel.Width - 38, Math.Max(120, _rightPanel.Height - 62));
            _propScrollBar.SetBounds(_propViewport.Right + 8, _propViewport.Top, 8, _propViewport.Height);
            LayoutPropertyContent();

            LayoutBottomButtons();
        }

        private void LayoutBottomButtons()
        {
            int x = 18;
            foreach (UIButton btn in _bottomBar.Controls.OfType<UIButton>())
            {
                int w = btn.Text == "导出 .cs" ? 116 : 96;
                btn.SetBounds(x, 14, w, 34);
                x += w + 10;
            }
        }

        private void LayoutPropertyContent()
        {
            int viewportHeight = Math.Max(1, _propViewport.Height);
            int contentHeight = Math.Max(viewportHeight, _propContentHeight);
            _propContent.SetBounds(0, -_propScrollOffset, _propViewport.Width, contentHeight);
            _propScrollBar.Visible = _propContentHeight > viewportHeight + 2;
            _propScrollBar.Invalidate();
        }

        private void ClampPropertyScroll()
        {
            int maxOffset = Math.Max(0, _propContentHeight - _propViewport.Height);
            _propScrollOffset = Math.Clamp(_propScrollOffset, 0, maxOffset);
        }

        private void PropScroll_MouseWheel(object? sender, MouseEventArgs e)
        {
            if (_propContentHeight <= _propViewport.Height)
                return;

            _propScrollOffset -= Math.Sign(e.Delta) * 56;
            ClampPropertyScroll();
            LayoutPropertyContent();
        }

        private void PropScrollBar_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.Clear(ClrPanel);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using var track = new SolidBrush(Color.FromArgb(28, 40, 56));
            using var thumb = new SolidBrush(Color.FromArgb(82, 106, 132));
            Rectangle trackRect = new(2, 4, Math.Max(2, _propScrollBar.Width - 4), Math.Max(8, _propScrollBar.Height - 8));
            FillRoundedRect(e.Graphics, track, trackRect, 3);

            Rectangle thumbRect = GetPropertyScrollThumbBounds();
            if (thumbRect.Height > 0)
                FillRoundedRect(e.Graphics, thumb, thumbRect, 3);
        }

        private Rectangle GetPropertyScrollThumbBounds()
        {
            int viewportHeight = Math.Max(1, _propViewport.Height);
            if (_propContentHeight <= viewportHeight)
                return Rectangle.Empty;

            int trackTop = 4;
            int trackHeight = Math.Max(8, _propScrollBar.Height - 8);
            int thumbHeight = Math.Clamp((int)(viewportHeight / (float)_propContentHeight * trackHeight), 36, trackHeight);
            int maxOffset = Math.Max(1, _propContentHeight - viewportHeight);
            int travel = Math.Max(0, trackHeight - thumbHeight);
            int thumbTop = trackTop + (int)(travel * (_propScrollOffset / (float)maxOffset));
            return new Rectangle(2, thumbTop, Math.Max(2, _propScrollBar.Width - 4), thumbHeight);
        }

        private void PropScrollBar_MouseDown(object? sender, MouseEventArgs e)
        {
            _propScrollDragging = true;
            _propScrollDragStartY = e.Y;
            _propScrollDragStartOffset = _propScrollOffset;
        }

        private void PropScrollBar_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!_propScrollDragging)
                return;

            int viewportHeight = Math.Max(1, _propViewport.Height);
            int maxOffset = Math.Max(0, _propContentHeight - viewportHeight);
            Rectangle thumb = GetPropertyScrollThumbBounds();
            int travel = Math.Max(1, _propScrollBar.Height - 8 - Math.Max(1, thumb.Height));
            int delta = e.Y - _propScrollDragStartY;
            _propScrollOffset = _propScrollDragStartOffset + (int)(delta / (float)travel * maxOffset);
            ClampPropertyScroll();
            LayoutPropertyContent();
        }

        private void WirePropertyMouseWheel(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                child.MouseWheel -= PropScroll_MouseWheel;
                child.MouseWheel += PropScroll_MouseWheel;
                if (child.HasChildren)
                    WirePropertyMouseWheel(child);
            }
        }

        private int InspectorContentWidth => Math.Max(220, _propViewport.ClientSize.Width - 10);
        private int InspectorInputX => 106;
        private int InspectorInputWidth => Math.Max(96, InspectorContentWidth - InspectorInputX - 8);
        private int InspectorValueX => Math.Max(142, InspectorContentWidth - 98);
        private int InspectorValueWidth => Math.Max(90, InspectorContentWidth - InspectorValueX - 8);
        private int InspectorSliderWidth => Math.Max(160, InspectorContentWidth - 12);

        private void RefreshPresetList()
        {
            _presetList.Items.Clear();
            foreach (var preset in _presets)
                _presetList.Items.Add(new PresetListItem(preset));
            _presetList.SelectedIndex = 0;
            UpdatePresetDescription();
        }

        private void RefreshLayerList()
        {
            EffectEditorLayer selected = _activeLayer;
            _layerList.Items.Clear();
            _layerList.Items.Add(new LayerListItem(EffectEditorLayer.Projectile, $"类型  {ProjectileEffectKindNames.GetDisplayName(_preset.Projectile.EffectKind)}"));
            _layerList.Items.Add(new LayerListItem(EffectEditorLayer.Head, _preset.Projectile.ShowHead ? "弹幕头部  开" : "弹幕头部  关"));
            _layerList.Items.Add(new LayerListItem(EffectEditorLayer.Trail, _preset.Projectile.ShowTrail ? "流光尾焰  开" : "流光尾焰  关"));
            _layerList.Items.Add(new LayerListItem(EffectEditorLayer.Impact, _preset.Projectile.ShowImpact ? "碰撞散开  开" : "碰撞散开  关"));

            int index = _layerList.Items.OfType<LayerListItem>().ToList().FindIndex(item => item.Layer == selected);
            _layerList.SelectedIndex = index >= 0 ? index : 0;
        }

        private void PresetList_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_presetList.SelectedItem is not PresetListItem item)
                return;

            _preset = item.Preset.Clone();
            _engine = new EffectEngine(_preset);
            _activeLayer = EffectEditorLayer.Projectile;
            UpdatePresetDescription();
            RefreshLayerList();
            RefreshPropertyPanel();
            _canvas.Invalidate();
        }

        private void LayerList_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_layerList.SelectedItem is not LayerListItem item)
                return;

            _activeLayer = item.Layer;
            RefreshPropertyPanel();
            _canvas.Invalidate();
        }

        private void UpdatePresetDescription()
        {
            _presetDescription.Text = _preset.Description;
        }

        private void RefreshPropertyPanel()
        {
            _propContent.SuspendLayout();
            _propContent.Controls.Clear();
            int y = 8;

            switch (_activeLayer)
            {
                case EffectEditorLayer.Projectile:
                    BuildProjectileProperties(ref y);
                    break;
                case EffectEditorLayer.Head:
                    BuildHeadProperties(ref y);
                    break;
                case EffectEditorLayer.Trail:
                    BuildTrailProperties(ref y);
                    break;
                case EffectEditorLayer.Impact:
                    BuildImpactProperties(ref y);
                    break;
            }

            _propContentHeight = y + 12;
            WirePropertyMouseWheel(_propContent);
            ClampPropertyScroll();
            LayoutPropertyContent();
            _propContent.ResumeLayout(false);
        }

        private void BuildProjectileProperties(ref int y)
        {
            var p = _preset.Projectile;
            AddSection("导出", ref y);
            AddEffectKindSelector("特效类型", p.EffectKind, value =>
            {
                p.EffectKind = value;
                ApplyKindDefaults(p);
                _engine.Reset();
                RefreshLayerList();
                RefreshPropertyPanel();
            }, ref y);
            AddVisualSourceSelector("绘制来源", p.VisualSource, value =>
            {
                p.VisualSource = value;
                _engine.Reset();
                RefreshPropertyPanel();
            }, ref y);
            AddTextBox("命名空间", p.ExportNamespace, value => p.ExportNamespace = value, ref y);
            AddTextBox("类名", p.ExportClassName, value => p.ExportClassName = value, ref y);
            if (p.VisualSource == ProjectileVisualSource.CustomTexture)
                AddFilePicker("预览贴图", p.PreviewTexturePath, value =>
                {
                    p.PreviewTexturePath = value;
                    p.VisualSource = ProjectileVisualSource.CustomTexture;
                    _engine.Reset();
                }, ref y);
            AddTextBox("Mod贴图路径", p.TextureOverride, value => p.TextureOverride = value, ref y);

            AddSection("弹幕属性", ref y);
            AddSlider("宽度", 4, 64, p.Width, 1, v => p.Width = (int)v, ref y);
            AddSlider("高度", 4, 64, p.Height, 1, v => p.Height = (int)v, ref y);
            AddSlider("存在时间", 60, 1800, p.TimeLeft, 10, v => p.TimeLeft = (int)v, ref y);
            AddSlider("穿透", -1, 8, p.Penetrate, 1, v => p.Penetrate = (int)v, ref y);
            AddSlider("额外更新", 0, 4, p.ExtraUpdates, 1, v => p.ExtraUpdates = (int)v, ref y);
            AddCheckBox("局部命中冷却", p.UsesLocalNpcImmunity, value => p.UsesLocalNpcImmunity = value, ref y);
            AddSlider("命中冷却", 1, 60, p.LocalNpcHitCooldown, 1, v => p.LocalNpcHitCooldown = (int)v, ref y);

            AddSection("预览运动", ref y);
            AddCheckBox("启用速度曲线", p.UseSpeedCurve, value =>
            {
                p.UseSpeedCurve = value;
                _engine.Reset();
                RefreshPropertyPanel();
            }, ref y);
            if (p.UseSpeedCurve)
            {
                AddSlider("起始速度", 20, 720, (decimal)p.LaunchSpeed, 5, v => p.LaunchSpeed = (float)v, ref y);
                AddSlider("目标速度", 20, 840, (decimal)p.TargetSpeed, 5, v => p.TargetSpeed = (float)v, ref y);
                AddSlider("加速时长", 0.05m, 3, (decimal)p.SpeedRampSeconds, 0.05m, v => p.SpeedRampSeconds = (float)v, ref y);
            }
            else
            {
                AddSlider("预览速度", 40, 620, (decimal)p.PreviewSpeed, 5, v => p.PreviewSpeed = (float)v, ref y);
            }
            AddSlider("路径长度", 220, 980, (decimal)p.PreviewPathLength, 10, v => p.PreviewPathLength = (float)v, ref y);
            AddSlider("弧线幅度", 0, 160, (decimal)p.PreviewArcAmplitude, 2, v => p.PreviewArcAmplitude = (float)v, ref y);
            AddSlider("弧线频率", 0, 4, (decimal)p.PreviewArcFrequency, 0.05m, v => p.PreviewArcFrequency = (float)v, ref y);
            AddSlider("画布缩放", 0.35m, 2.5m, (decimal)p.PreviewZoom, 0.05m, v => p.PreviewZoom = (float)v, ref y);
            AddColorButton("光照颜色", p.LightColor, value => p.LightColor = value, ref y);
            AddSlider("光照强度", 0, 2.5m, (decimal)p.LightIntensity, 0.05m, v => p.LightIntensity = (float)v, ref y);

            BuildKindSpecificProperties(ref y, p);
        }

        private void BuildKindSpecificProperties(ref int y, ProjectileEffectSettings p)
        {
            AddSection($"{ProjectileEffectKindNames.GetDisplayName(p.EffectKind)}参数", ref y);
            switch (p.EffectKind)
            {
                case ProjectileEffectKind.EmptyProjectile:
                    AddHint("空弹幕不强制绘制预设弹头，可只用贴图、拖影、发射尾焰和碰撞效果组合。", ref y);
                    break;
                case ProjectileEffectKind.SpiralComet:
                    AddSlider("缠绕股数", 1, 8, p.SpiralStrands, 1, v => p.SpiralStrands = (int)v, ref y);
                    AddSlider("螺旋半径", 2, 64, (decimal)p.SpiralRadius, 1, v => p.SpiralRadius = (float)v, ref y);
                    AddSlider("螺旋圈数", 0.5m, 8, (decimal)p.SpiralTwist, 0.1m, v => p.SpiralTwist = (float)v, ref y);
                    AddSlider("光点大小", 1, 18, (decimal)p.SpiralDotSize, 0.25m, v => p.SpiralDotSize = (float)v, ref y);
                    break;
                case ProjectileEffectKind.OrbitingOrbs:
                    AddSlider("卫星数量", 1, 12, p.OrbitCount, 1, v => p.OrbitCount = (int)v, ref y);
                    AddSlider("环绕半径", 4, 90, (decimal)p.OrbitRadius, 1, v => p.OrbitRadius = (float)v, ref y);
                    AddSlider("环绕速度", 0, 14, (decimal)p.OrbitSpeed, 0.2m, v => p.OrbitSpeed = (float)v, ref y);
                    AddSlider("卫星大小", 1, 20, (decimal)p.OrbitDotSize, 0.25m, v => p.OrbitDotSize = (float)v, ref y);
                    break;
                case ProjectileEffectKind.NovaBurst:
                    AddSlider("光环层数", 1, 8, p.NovaRingCount, 1, v => p.NovaRingCount = (int)v, ref y);
                    AddSlider("爆发半径", 20, 220, (decimal)p.NovaRadius, 2, v => p.NovaRadius = (float)v, ref y);
                    AddSlider("尖刺长度", 10, 180, (decimal)p.NovaSpikeLength, 2, v => p.NovaSpikeLength = (float)v, ref y);
                    AddSlider("旋转速度", 0, 8, (decimal)p.NovaSpinSpeed, 0.1m, v => p.NovaSpinSpeed = (float)v, ref y);
                    break;
                case ProjectileEffectKind.ScatterShard:
                    AddSlider("碎片数量", 1, 20, p.ScatterShardCount, 1, v => p.ScatterShardCount = (int)v, ref y);
                    AddSlider("扇形角度", 10, 360, (decimal)p.ScatterConeAngle, 2, v => p.ScatterConeAngle = (float)v, ref y);
                    AddSlider("碎片长度", 12, 220, (decimal)p.ScatterShardLength, 2, v => p.ScatterShardLength = (float)v, ref y);
                    AddSlider("波动速度", 0, 8, (decimal)p.ScatterWaveSpeed, 0.1m, v => p.ScatterWaveSpeed = (float)v, ref y);
                    break;
                default:
                    AddHint("流光弹使用尾焰、头部、碰撞参数组合生成。", ref y);
                    break;
            }
        }

        private void BuildHeadProperties(ref int y)
        {
            var p = _preset.Projectile;
            AddSection("头部显示", ref y);
            AddCheckBox("显示弹幕头部", p.ShowHead, value =>
            {
                p.ShowHead = value;
                RefreshLayerList();
            }, ref y);
            AddSlider("弹头大小", 4, 32, (decimal)p.HeadSize, 0.5m, v => p.HeadSize = (float)v, ref y);
            AddSlider("外光范围", 0.5m, 5m, (decimal)p.HeadOuterScale, 0.05m, v => p.HeadOuterScale = (float)v, ref y);
            AddSlider("内光范围", 0.2m, 3m, (decimal)p.HeadInnerScale, 0.05m, v => p.HeadInnerScale = (float)v, ref y);
            AddSlider("脉冲速度", 0, 40, (decimal)p.HeadPulseSpeed, 0.5m, v => p.HeadPulseSpeed = (float)v, ref y);
            AddSlider("脉冲幅度", 0, 0.4m, (decimal)p.HeadPulseAmount, 0.01m, v => p.HeadPulseAmount = (float)v, ref y);
            AddColorButton("外光颜色", p.HeadGlowOuterColor, value => p.HeadGlowOuterColor = value, ref y);
            AddColorButton("内光颜色", p.HeadGlowInnerColor, value => p.HeadGlowInnerColor = value, ref y);
            AddColorButton("核心颜色", p.HeadCoreColor, value => p.HeadCoreColor = value, ref y);
        }

        private void BuildTrailProperties(ref int y)
        {
            var p = _preset.Projectile;
            AddSection("尾焰显示", ref y);
            AddCheckBox("显示流光尾焰", p.ShowTrail, value =>
            {
                p.ShowTrail = value;
                RefreshLayerList();
            }, ref y);
            AddSlider("轨迹缓存", 2, 40, p.TrailCacheLength, 1, v => p.TrailCacheLength = (int)v, ref y);
            AddSlider("拖影持续秒", 0.04m, 1.5m, (decimal)p.TrailDurationSeconds, 0.02m, v => p.TrailDurationSeconds = (float)v, ref y);
            AddSlider("细分步数", 1, 16, p.TrailSubdivisionSteps, 1, v => p.TrailSubdivisionSteps = (int)v, ref y);
            AddSlider("最大轨迹点", 3, 24, p.MaxTrailPoints, 1, v => p.MaxTrailPoints = (int)v, ref y);
            AddSlider("断点距离", 40, 360, (decimal)p.TrailBreakDistance, 5, v => p.TrailBreakDistance = (float)v, ref y);

            AddSection("宽度与透明度", ref y);
            AddSlider("外层起宽", 1, 28, (decimal)p.OuterTrailWidthStart, 0.25m, v => p.OuterTrailWidthStart = (float)v, ref y);
            AddSlider("外层末宽", 0.2m, 8, (decimal)p.OuterTrailWidthEnd, 0.1m, v => p.OuterTrailWidthEnd = (float)v, ref y);
            AddSlider("内层起宽", 1, 20, (decimal)p.InnerTrailWidthStart, 0.25m, v => p.InnerTrailWidthStart = (float)v, ref y);
            AddSlider("核心起宽", 0.5m, 10, (decimal)p.CoreTrailWidthStart, 0.1m, v => p.CoreTrailWidthStart = (float)v, ref y);
            AddSlider("起始透明", 0.1m, 2, (decimal)p.TrailOpacityStart, 0.05m, v => p.TrailOpacityStart = (float)v, ref y);
            AddSlider("末端透明", 0, 1, (decimal)p.TrailOpacityEnd, 0.02m, v => p.TrailOpacityEnd = (float)v, ref y);
            AddSlider("脉冲速度", 0, 40, (decimal)p.TrailPulseSpeed, 0.5m, v => p.TrailPulseSpeed = (float)v, ref y);
            AddSlider("脉冲幅度", 0, 0.35m, (decimal)p.TrailPulseAmount, 0.01m, v => p.TrailPulseAmount = (float)v, ref y);

            AddSection("颜色", ref y);
            AddColorButton("外层起色", p.OuterTrailStartColor, value => p.OuterTrailStartColor = value, ref y);
            AddColorButton("外层末色", p.OuterTrailEndColor, value => p.OuterTrailEndColor = value, ref y);
            AddColorButton("内层起色", p.InnerTrailStartColor, value => p.InnerTrailStartColor = value, ref y);
            AddColorButton("内层末色", p.InnerTrailEndColor, value => p.InnerTrailEndColor = value, ref y);
            AddColorButton("核心起色", p.CoreTrailStartColor, value => p.CoreTrailStartColor = value, ref y);
            AddColorButton("核心末色", p.CoreTrailEndColor, value => p.CoreTrailEndColor = value, ref y);

            AddSection("发射尾焰", ref y);
            AddCheckBox("显示发射尾焰", p.ShowMuzzleFlame, value => p.ShowMuzzleFlame = value, ref y);
            AddSlider("尾焰时长", 0.03m, 0.8m, (decimal)p.MuzzleFlameDuration, 0.01m, v => p.MuzzleFlameDuration = (float)v, ref y);
            AddSlider("尾焰长度", 4, 120, (decimal)p.MuzzleFlameLength, 2, v => p.MuzzleFlameLength = (float)v, ref y);
            AddSlider("尾焰宽度", 2, 48, (decimal)p.MuzzleFlameWidth, 0.5m, v => p.MuzzleFlameWidth = (float)v, ref y);
            AddColorButton("尾焰内色", p.MuzzleFlameStartColor, value => p.MuzzleFlameStartColor = value, ref y);
            AddColorButton("尾焰外色", p.MuzzleFlameEndColor, value => p.MuzzleFlameEndColor = value, ref y);
        }

        private void BuildImpactProperties(ref int y)
        {
            var p = _preset.Projectile;
            AddSection("碰撞散开", ref y);
            AddCheckBox("显示碰撞散开", p.ShowImpact, value =>
            {
                p.ShowImpact = value;
                RefreshLayerList();
            }, ref y);
            AddImpactKindSelector("爆开类型", p.ImpactKind, value =>
            {
                p.ImpactKind = value;
                ApplyImpactDefaults(p);
                RefreshPropertyPanel();
            }, ref y);
            AddDustSelector("Dust 类型", p.ImpactDustType, value => p.ImpactDustType = value, ref y);
            AddSlider("粒子数量", 0, 80, p.ImpactDustCount, 1, v => p.ImpactDustCount = (int)v, ref y);
            AddSlider("散开速度", 0, 10, (decimal)p.ImpactDustSpeed, 0.1m, v => p.ImpactDustSpeed = (float)v, ref y);
            AddSlider("粒子缩放", 0.2m, 4, (decimal)p.ImpactDustScale, 0.05m, v => p.ImpactDustScale = (float)v, ref y);
            AddSlider("预览寿命", 0.15m, 2, (decimal)p.ImpactPreviewLife, 0.05m, v => p.ImpactPreviewLife = (float)v, ref y);
            AddCheckBox("循环时自动触发", p.AutoImpactOnLoop, value => p.AutoImpactOnLoop = value, ref y);
            AddColorButton("爆散起色", p.ImpactColorA, value => p.ImpactColorA = value, ref y);
            AddColorButton("爆散末色", p.ImpactColorB, value => p.ImpactColorB = value, ref y);

            AddSection($"{ImpactEffectKindNames.GetDisplayName(p.ImpactKind)}参数", ref y);
            switch (p.ImpactKind)
            {
                case ImpactEffectKind.RingShockwave:
                    AddSlider("光环层数", 1, 8, p.ImpactRingCount, 1, v => p.ImpactRingCount = (int)v, ref y);
                    AddSlider("光环半径", 16, 220, (decimal)p.ImpactRingRadius, 2, v => p.ImpactRingRadius = (float)v, ref y);
                    break;
                case ImpactEffectKind.CrossSlash:
                    AddSlider("斩击长度", 20, 240, (decimal)p.ImpactRingRadius, 2, v => p.ImpactRingRadius = (float)v, ref y);
                    AddSlider("斩击粗细", 0.4m, 5, (decimal)p.ImpactDustScale, 0.1m, v => p.ImpactDustScale = (float)v, ref y);
                    break;
                case ImpactEffectKind.SmokeBloom:
                    AddSlider("烟云大小", 10, 140, (decimal)p.ImpactCloudSize, 2, v => p.ImpactCloudSize = (float)v, ref y);
                    AddSlider("烟团数量", 4, 80, p.ImpactDustCount, 1, v => p.ImpactDustCount = (int)v, ref y);
                    break;
                case ImpactEffectKind.LightningFork:
                    AddSlider("闪电分支", 1, 16, p.ImpactLightningBranches, 1, v => p.ImpactLightningBranches = (int)v, ref y);
                    AddSlider("裂解范围", 24, 240, (decimal)p.ImpactRingRadius, 2, v => p.ImpactRingRadius = (float)v, ref y);
                    break;
                case ImpactEffectKind.ShardSpray:
                    AddSlider("碎片数量", 4, 100, p.ImpactDustCount, 1, v => p.ImpactDustCount = (int)v, ref y);
                    AddSlider("碎片大小", 0.4m, 5, (decimal)p.ImpactDustScale, 0.1m, v => p.ImpactDustScale = (float)v, ref y);
                    break;
                default:
                    AddSlider("星芒数量", 4, 100, p.ImpactDustCount, 1, v => p.ImpactDustCount = (int)v, ref y);
                    AddSlider("星芒长度", 10, 180, (decimal)p.ImpactRingRadius, 2, v => p.ImpactRingRadius = (float)v, ref y);
                    break;
            }
        }

        private void AddSection(string text, ref int y)
        {
            var label = CreateLabel(text, new Point(6, y), new Size(InspectorContentWidth - 12, 22), FontBodyBold, ClrAccent);
            _propContent.Controls.Add(label);
            y += 24;

            var line = new Panel { BackColor = ClrSoftBorder, Location = new Point(6, y), Size = new Size(InspectorContentWidth - 12, 1) };
            _propContent.Controls.Add(line);
            y += 12;
        }

        private void AddEffectKindSelector(string label, ProjectileEffectKind value, Action<ProjectileEffectKind> onChange, ref int y)
        {
            _propContent.Controls.Add(CreateLabel(label, new Point(8, y), new Size(92, 28), FontInspector, ClrSubText));
            var combo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(InspectorInputX, y),
                Size = new Size(InspectorInputWidth, 28),
                Font = FontInspector,
                ForeColor = ClrText,
                BackColor = ClrInput
            };
            StyleDarkCombo(combo);

            foreach (ProjectileEffectKind kind in Enum.GetValues<ProjectileEffectKind>())
                combo.Items.Add(new EffectKindOption(kind));

            bool initializing = true;
            combo.SelectedItem = combo.Items.OfType<EffectKindOption>().FirstOrDefault(item => item.Kind == value) ?? combo.Items[0];
            initializing = false;

            combo.SelectedIndexChanged += (s, e) =>
            {
                if (initializing || combo.SelectedItem is not EffectKindOption option || option.Kind == _preset.Projectile.EffectKind)
                    return;

                onChange(option.Kind);
                _canvas.Invalidate();
            };
            _propContent.Controls.Add(combo);
            y += 40;
        }

        private void AddVisualSourceSelector(string label, ProjectileVisualSource value, Action<ProjectileVisualSource> onChange, ref int y)
        {
            _propContent.Controls.Add(CreateLabel(label, new Point(8, y), new Size(92, 28), FontInspector, ClrSubText));
            var combo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(InspectorInputX, y),
                Size = new Size(InspectorInputWidth, 28),
                Font = FontInspector,
                ForeColor = ClrText,
                BackColor = ClrInput
            };
            StyleDarkCombo(combo);

            foreach (ProjectileVisualSource source in Enum.GetValues<ProjectileVisualSource>())
                combo.Items.Add(new VisualSourceOption(source));

            bool initializing = true;
            combo.SelectedItem = combo.Items.OfType<VisualSourceOption>().FirstOrDefault(item => item.Source == value) ?? combo.Items[0];
            initializing = false;

            combo.SelectedIndexChanged += (s, e) =>
            {
                if (initializing || combo.SelectedItem is not VisualSourceOption option || option.Source == _preset.Projectile.VisualSource)
                    return;

                onChange(option.Source);
                _canvas.Invalidate();
            };
            _propContent.Controls.Add(combo);
            y += 40;
        }

        private void AddImpactKindSelector(string label, ImpactEffectKind value, Action<ImpactEffectKind> onChange, ref int y)
        {
            _propContent.Controls.Add(CreateLabel(label, new Point(8, y), new Size(92, 28), FontInspector, ClrSubText));
            var combo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(InspectorInputX, y),
                Size = new Size(InspectorInputWidth, 28),
                Font = FontInspector,
                ForeColor = ClrText,
                BackColor = ClrInput
            };
            StyleDarkCombo(combo);

            foreach (ImpactEffectKind kind in Enum.GetValues<ImpactEffectKind>())
                combo.Items.Add(new ImpactKindOption(kind));

            bool initializing = true;
            combo.SelectedItem = combo.Items.OfType<ImpactKindOption>().FirstOrDefault(item => item.Kind == value) ?? combo.Items[0];
            initializing = false;

            combo.SelectedIndexChanged += (s, e) =>
            {
                if (initializing || combo.SelectedItem is not ImpactKindOption option || option.Kind == _preset.Projectile.ImpactKind)
                    return;

                onChange(option.Kind);
                _engine.TriggerImpact();
                _canvas.Invalidate();
            };
            _propContent.Controls.Add(combo);
            y += 40;
        }

        private static void ApplyKindDefaults(ProjectileEffectSettings p)
        {
            switch (p.EffectKind)
            {
                case ProjectileEffectKind.EmptyProjectile:
                    p.ShowTrail = false;
                    p.ShowHead = false;
                    p.ShowMuzzleFlame = false;
                    p.Penetrate = Math.Max(1, p.Penetrate);
                    p.PreviewSpeed = Math.Max(120f, p.PreviewSpeed);
                    break;
                case ProjectileEffectKind.NovaBurst:
                    p.ShowTrail = false;
                    p.ShowHead = true;
                    p.Penetrate = -1;
                    p.ExtraUpdates = 0;
                    p.TimeLeft = Math.Min(p.TimeLeft, 180);
                    p.PreviewSpeed = 0f;
                    p.PreviewArcAmplitude = 0f;
                    break;
                case ProjectileEffectKind.ScatterShard:
                    p.ShowTrail = false;
                    p.ShowHead = true;
                    p.PreviewArcAmplitude = 0f;
                    p.PreviewSpeed = Math.Max(120f, p.PreviewSpeed);
                    break;
                case ProjectileEffectKind.SpiralComet:
                    p.ShowTrail = true;
                    p.ShowHead = true;
                    p.PreviewSpeed = Math.Max(160f, p.PreviewSpeed);
                    p.PreviewArcAmplitude = Math.Max(20f, p.PreviewArcAmplitude);
                    break;
                case ProjectileEffectKind.OrbitingOrbs:
                    p.ShowTrail = true;
                    p.ShowHead = true;
                    p.PreviewSpeed = Math.Max(140f, p.PreviewSpeed);
                    break;
                default:
                    p.ShowTrail = true;
                    p.ShowHead = true;
                    p.PreviewSpeed = Math.Max(160f, p.PreviewSpeed);
                    break;
            }
        }

        private static void ApplyImpactDefaults(ProjectileEffectSettings p)
        {
            switch (p.ImpactKind)
            {
                case ImpactEffectKind.RingShockwave:
                    p.ImpactRingCount = Math.Max(p.ImpactRingCount, 3);
                    p.ImpactRingRadius = Math.Max(p.ImpactRingRadius, 96f);
                    p.ImpactDustCount = Math.Max(p.ImpactDustCount, 18);
                    p.ImpactPreviewLife = Math.Max(p.ImpactPreviewLife, 0.75f);
                    break;
                case ImpactEffectKind.CrossSlash:
                    p.ImpactRingRadius = Math.Max(p.ImpactRingRadius, 92f);
                    p.ImpactDustCount = Math.Max(p.ImpactDustCount, 14);
                    p.ImpactPreviewLife = Math.Max(p.ImpactPreviewLife, 0.55f);
                    break;
                case ImpactEffectKind.SmokeBloom:
                    p.ImpactCloudSize = Math.Max(p.ImpactCloudSize, 52f);
                    p.ImpactDustCount = Math.Max(p.ImpactDustCount, 28);
                    p.ImpactDustSpeed = Math.Min(Math.Max(p.ImpactDustSpeed, 1.4f), 4.2f);
                    p.ImpactPreviewLife = Math.Max(p.ImpactPreviewLife, 1.0f);
                    break;
                case ImpactEffectKind.LightningFork:
                    p.ImpactLightningBranches = Math.Max(p.ImpactLightningBranches, 6);
                    p.ImpactRingRadius = Math.Max(p.ImpactRingRadius, 110f);
                    p.ImpactDustCount = Math.Max(p.ImpactDustCount, 12);
                    p.ImpactPreviewLife = Math.Max(p.ImpactPreviewLife, 0.5f);
                    break;
                case ImpactEffectKind.ShardSpray:
                    p.ImpactDustCount = Math.Max(p.ImpactDustCount, 32);
                    p.ImpactDustSpeed = Math.Max(p.ImpactDustSpeed, 4.2f);
                    p.ImpactDustScale = Math.Max(p.ImpactDustScale, 1.2f);
                    p.ImpactPreviewLife = Math.Max(p.ImpactPreviewLife, 0.75f);
                    break;
                default:
                    p.ImpactDustCount = Math.Max(p.ImpactDustCount, 18);
                    p.ImpactRingRadius = Math.Max(p.ImpactRingRadius, 72f);
                    break;
            }
        }

        private void AddHint(string text, ref int y)
        {
            var label = CreateLabel(text, new Point(8, y), new Size(InspectorContentWidth - 16, 42), FontSmall, ClrMuted);
            _propContent.Controls.Add(label);
            y += 48;
        }

        private void AddTextBox(string label, string value, Action<string> onChange, ref int y)
        {
            _propContent.Controls.Add(CreateLabel(label, new Point(8, y), new Size(92, 28), FontInspector, ClrSubText));
            var box = new TextBox
            {
                Text = value,
                Location = new Point(InspectorInputX, y),
                Size = new Size(InspectorInputWidth, 28),
                Font = FontInspector,
                ForeColor = ClrText,
                BackColor = ClrInput,
                BorderStyle = BorderStyle.FixedSingle
            };
            box.TextChanged += (s, e) => onChange(box.Text);
            _propContent.Controls.Add(box);
            y += 40;
        }

        private void AddFilePicker(string label, string value, Action<string> onChange, ref int y)
        {
            _propContent.Controls.Add(CreateLabel(label, new Point(8, y), new Size(92, 28), FontInspector, ClrSubText));
            int buttonWidth = 36;
            var box = new TextBox
            {
                Text = value,
                Location = new Point(InspectorInputX, y),
                Size = new Size(Math.Max(64, InspectorInputWidth - buttonWidth - 6), 28),
                Font = FontInspector,
                ForeColor = ClrText,
                BackColor = ClrInput,
                BorderStyle = BorderStyle.FixedSingle
            };
            var button = new Button
            {
                Text = "...",
                Location = new Point(box.Right + 6, y),
                Size = new Size(buttonWidth, 28),
                Font = FontBodyBold,
                ForeColor = ClrText,
                BackColor = Color.FromArgb(35, 48, 65),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderColor = ClrBorder;
            box.TextChanged += (s, e) =>
            {
                onChange(box.Text);
                _canvas.Invalidate();
            };
            button.Click += (s, e) =>
            {
                using var dialog = new OpenFileDialog
                {
                    Title = "选择弹幕预览贴图",
                    Filter = "图片文件|*.png;*.jpg;*.jpeg;*.bmp;*.gif|所有文件|*.*",
                    FileName = box.Text
                };
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    box.Text = dialog.FileName;
                    onChange(dialog.FileName);
                    _canvas.Invalidate();
                }
            };
            _propContent.Controls.Add(box);
            _propContent.Controls.Add(button);
            y += 40;
        }

        private void AddCheckBox(string text, bool value, Action<bool> onChange, ref int y)
        {
            var check = new CheckBox
            {
                Text = text,
                Checked = value,
                Location = new Point(8, y),
                Size = new Size(InspectorContentWidth - 16, 28),
                Font = FontInspector,
                ForeColor = ClrText,
                BackColor = Color.Transparent
            };
            check.CheckedChanged += (s, e) =>
            {
                onChange(check.Checked);
                _canvas.Invalidate();
            };
            _propContent.Controls.Add(check);
            y += 36;
        }

        private void AddSlider(string label, decimal min, decimal max, decimal value, decimal step, Action<decimal> onChange, ref int y)
        {
            step = Math.Max(step, 0.01m);
            int ticks = Math.Max(1, (int)Math.Round((max - min) / step));
            int initialTick = DecimalToTick(value, min, step, ticks);
            int decimals = GetDecimalPlaces(step);

            _propContent.Controls.Add(CreateLabel(label, new Point(8, y), new Size(Math.Max(108, InspectorValueX - 14), 26), FontInspector, ClrSubText));
            var number = new TextBox
            {
                Text = FormatDecimal(Math.Clamp(value, min, max), decimals),
                Location = new Point(InspectorValueX, y),
                Size = new Size(InspectorValueWidth, 28),
                Font = FontInspector,
                ForeColor = ClrText,
                BackColor = ClrInput,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = HorizontalAlignment.Right
            };
            _propContent.Controls.Add(number);
            y += 30;

            var slider = new DarkSlider
            {
                Minimum = 0,
                Maximum = ticks,
                Value = initialTick,
                Location = new Point(6, y),
                Size = new Size(InspectorSliderWidth, 28),
                BackColor = ClrPanel
            };

            bool syncing = false;
            slider.ValueChanged += (s, e) =>
            {
                if (syncing) return;
                syncing = true;
                decimal newValue = Math.Clamp(min + slider.Value * step, min, max);
                number.Text = FormatDecimal(newValue, decimals);
                onChange(newValue);
                syncing = false;
                _canvas.Invalidate();
            };

            void CommitNumber()
            {
                if (syncing) return;
                if (!TryParseDecimal(number.Text, out decimal parsed))
                {
                    number.Text = FormatDecimal(min + slider.Value * step, decimals);
                    return;
                }

                syncing = true;
                decimal newValue = Math.Clamp(parsed, min, max);
                slider.Value = DecimalToTick(newValue, min, step, ticks);
                number.Text = FormatDecimal(newValue, decimals);
                onChange(newValue);
                syncing = false;
                _canvas.Invalidate();
            }

            number.Leave += (s, e) => CommitNumber();
            number.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    CommitNumber();
                    e.SuppressKeyPress = true;
                }
            };

            _propContent.Controls.Add(slider);
            y += 48;
        }

        private void AddColorButton(string label, Color value, Action<Color> onChange, ref int y)
        {
            _propContent.Controls.Add(CreateLabel(label, new Point(8, y), new Size(Math.Max(108, InspectorValueX - 14), 28), FontInspector, ClrSubText));
            var swatch = new Button
            {
                Location = new Point(InspectorValueX, y),
                Size = new Size(InspectorValueWidth, 28),
                BackColor = value,
                FlatStyle = FlatStyle.Flat
            };
            swatch.FlatAppearance.BorderColor = ClrBorder;
            swatch.Click += (s, e) =>
            {
                using var dialog = new ColorDialog { Color = swatch.BackColor, FullOpen = true };
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    swatch.BackColor = dialog.Color;
                    onChange(dialog.Color);
                    _canvas.Invalidate();
                }
            };
            _propContent.Controls.Add(swatch);
            y += 40;
        }

        private void AddDustSelector(string label, int value, Action<int> onChange, ref int y)
        {
            _propContent.Controls.Add(CreateLabel(label, new Point(8, y), new Size(92, 28), FontInspector, ClrSubText));
            var combo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(InspectorInputX, y),
                Size = new Size(InspectorInputWidth, 28),
                Font = FontInspector,
                ForeColor = ClrText,
                BackColor = ClrInput
            };
            StyleDarkCombo(combo);

            var options = new[]
            {
                new DustOption("GoldCoin 57", 57),
                new DustOption("PlatinumCoin 58", 58),
                new DustOption("Torch 6", 6),
                new DustOption("MagicMirror 15", 15),
                new DustOption("PurpleCrystal 27", 27),
                new DustOption("Electric 226", 226),
                new DustOption("GemTopaz 87", 87)
            };
            combo.Items.AddRange(options);
            combo.SelectedItem = options.FirstOrDefault(option => option.Id == value) ?? options[0];
            combo.SelectedIndexChanged += (s, e) =>
            {
                if (combo.SelectedItem is DustOption option)
                {
                    onChange(option.Id);
                    _canvas.Invalidate();
                }
            };
            _propContent.Controls.Add(combo);
            y += 40;
        }

        private void Canvas_Paint(object? sender, PaintEventArgs e)
        {
            try
            {
                _engine.Render(e.Graphics, _canvas.ClientRectangle);
            }
            catch (Exception ex)
            {
                e.Graphics.Clear(Color.FromArgb(40, 12, 16));
                using var brush = new SolidBrush(Color.FromArgb(255, 230, 140, 120));
                e.Graphics.DrawString($"渲染错误: {ex.Message}", FontBodyBold, brush, 18, 18);
            }
        }

        private void Canvas_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            _canvas.Focus();
            _draggingCanvas = true;
            _dragStartMouse = e.Location;
            _dragStartWorld = new PointF(_preset.WorldCenterX, _preset.WorldCenterY);
        }

        private void Canvas_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!_draggingCanvas)
                return;

            _preset.WorldCenterX = _dragStartWorld.X + e.X - _dragStartMouse.X;
            _preset.WorldCenterY = _dragStartWorld.Y + e.Y - _dragStartMouse.Y;
            _canvas.Invalidate();
        }

        private void Canvas_MouseUp(object? sender, MouseEventArgs e)
        {
            _draggingCanvas = false;
        }

        private void Canvas_MouseWheel(object? sender, MouseEventArgs e)
        {
            float factor = e.Delta > 0 ? 1.08f : 0.92f;
            _preset.Projectile.PreviewZoom = Math.Clamp(_preset.Projectile.PreviewZoom * factor, 0.35f, 2.5f);
            if (_activeLayer == EffectEditorLayer.Projectile)
                RefreshPropertyPanel();
            _canvas.Invalidate();
        }

        private void RenderTimer_Tick(object? sender, EventArgs e)
        {
            var now = DateTime.UtcNow;
            float delta = (float)(now - _lastFrameTime).TotalSeconds;
            _lastFrameTime = now;
            delta = Math.Clamp(delta, 0.001f, 0.1f);

            if (!_paused)
                _engine.Update(delta * (_slowMode ? 0.35f : 1f));

            _fpsLabel.Text = $"FPS: {(1f / Math.Max(delta, 0.001f)):F0}";
            _canvas.Invalidate();
        }

        private void ImportConfig()
        {
            using var dialog = new OpenFileDialog
            {
                Title = "导入特效配置",
                Filter = "TMCreator 特效配置|*.tmfx.json;*.json|所有文件|*.*"
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                string json = File.ReadAllText(dialog.FileName);
                var preset = JsonSerializer.Deserialize<EffectPreset>(json, _jsonOptions)
                    ?? throw new InvalidOperationException("配置为空。");
                _preset = preset;
                _engine = new EffectEngine(_preset);
                _activeLayer = EffectEditorLayer.Projectile;
                UpdatePresetDescription();
                RefreshLayerList();
                RefreshPropertyPanel();
                _canvas.Invalidate();
                UIMessageBox.Show($"已导入配置：{Path.GetFileName(dialog.FileName)}");
            }
            catch (Exception ex)
            {
                UIMessageBox.Show($"导入失败：{ex.Message}");
            }
        }

        private void ExportConfig()
        {
            using var dialog = new SaveFileDialog
            {
                Title = "导出特效配置",
                Filter = "TMCreator 特效配置|*.tmfx.json|JSON 文件|*.json|所有文件|*.*",
                FileName = $"{SanitizeFileName(_preset.Name)}.tmfx.json"
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                string json = JsonSerializer.Serialize(_preset, _jsonOptions);
                File.WriteAllText(dialog.FileName, json, System.Text.Encoding.UTF8);
                UIMessageBox.Show($"配置已导出：{Path.GetFileName(dialog.FileName)}");
            }
            catch (Exception ex)
            {
                UIMessageBox.Show($"导出失败：{ex.Message}");
            }
        }

        private void ExportProjectileCode()
        {
            string className = EffectProjectileExporter.NormalizeClassName(_preset.Projectile.ExportClassName);
            using var dialog = new SaveFileDialog
            {
                Title = "导出 ModProjectile .cs",
                Filter = "C# 文件|*.cs|所有文件|*.*",
                FileName = $"{className}.cs"
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                _preset.Projectile.ExportClassName = className;
                _preset.Projectile.ExportNamespace = EffectProjectileExporter.NormalizeNamespace(_preset.Projectile.ExportNamespace);
                string code = EffectProjectileExporter.GenerateProjectileCode(_preset);
                File.WriteAllText(dialog.FileName, code, System.Text.Encoding.UTF8);
                RefreshPropertyPanel();
                UIMessageBox.Show($"弹幕代码已导出：{Path.GetFileName(dialog.FileName)}");
            }
            catch (Exception ex)
            {
                UIMessageBox.Show($"导出失败：{ex.Message}");
            }
        }

        private static void ConfigureDarkList(ListBox list, int itemHeight)
        {
            list.BackColor = ClrInput;
            list.ForeColor = ClrText;
            list.BorderStyle = BorderStyle.None;
            list.Font = FontBody;
            list.DrawMode = DrawMode.OwnerDrawFixed;
            list.ItemHeight = itemHeight;
            list.IntegralHeight = false;
            list.ScrollAlwaysVisible = false;
        }

        private static void StyleDarkCombo(ComboBox combo)
        {
            combo.FlatStyle = FlatStyle.Flat;
            combo.DrawMode = DrawMode.OwnerDrawFixed;
            combo.ItemHeight = 24;
            combo.DropDownHeight = 220;
            combo.DrawItem -= DarkCombo_DrawItem;
            combo.DrawItem += DarkCombo_DrawItem;
        }

        private static void DarkCombo_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (sender is not ComboBox combo)
                return;

            bool selected = e.State.HasFlag(DrawItemState.Selected);
            bool edit = e.State.HasFlag(DrawItemState.ComboBoxEdit);
            Color bg = selected && !edit ? Color.FromArgb(34, 76, 92) : ClrInput;
            using var bgBrush = new SolidBrush(bg);
            e.Graphics.FillRectangle(bgBrush, e.Bounds);

            string text = (e.Index >= 0 ? combo.GetItemText(combo.Items[e.Index]) : combo.Text) ?? string.Empty;
            var textRect = new Rectangle(e.Bounds.Left + 8, e.Bounds.Top, Math.Max(1, e.Bounds.Width - 16), e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, text, FontInspector, textRect, selected || edit ? ClrText : ClrSubText, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);

            if (edit)
            {
                using var border = new Pen(ClrBorder);
                e.Graphics.DrawRectangle(border, e.Bounds.Left, e.Bounds.Top, e.Bounds.Width - 1, e.Bounds.Height - 1);
            }
        }

        private static void PresetList_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (sender is not ListBox list || e.Index < 0 || list.Items[e.Index] is not PresetListItem item)
                return;
            DrawListItem(e, item.Preset.Name, item.Preset.Description, e.State.HasFlag(DrawItemState.Selected));
        }

        private static void LayerList_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (sender is not ListBox list || e.Index < 0 || list.Items[e.Index] is not LayerListItem item)
                return;
            DrawListItem(e, item.Title, item.Layer.ToString(), e.State.HasFlag(DrawItemState.Selected));
        }

        private static void DrawListItem(DrawItemEventArgs e, string title, string subtitle, bool selected)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Color bg = selected ? Color.FromArgb(31, 72, 86) : ClrInput;
            using var bgBrush = new SolidBrush(bg);
            e.Graphics.FillRectangle(bgBrush, e.Bounds);

            if (selected)
            {
                using var accent = new SolidBrush(ClrAccent);
                e.Graphics.FillRectangle(accent, e.Bounds.Left, e.Bounds.Top + 3, 3, e.Bounds.Height - 6);
                using var border = new Pen(Color.FromArgb(72, 104, 126));
                e.Graphics.DrawRectangle(border, e.Bounds.Left, e.Bounds.Top, e.Bounds.Width - 1, e.Bounds.Height - 1);
            }

            using var titleBrush = new SolidBrush(selected ? Color.White : ClrText);
            using var subBrush = new SolidBrush(ClrMuted);
            e.Graphics.DrawString(title, FontBodyBold, titleBrush, e.Bounds.Left + 12, e.Bounds.Top + 3);
            if (e.Bounds.Height >= 32)
                e.Graphics.DrawString(subtitle, FontSmall, subBrush, e.Bounds.Left + 12, e.Bounds.Top + 19);
        }

        private static Label CreateLabel(string text, Point location, Size size, Font font, Color color)
        {
            return new Label
            {
                Text = text,
                AutoSize = false,
                Location = location,
                Size = size,
                Font = font,
                ForeColor = color,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
        }

        private static UIButton CreateActionButton(string text, Color fill, Color border, Color? fore = null)
        {
            var btn = new UIButton { Text = text };
            StyleActionButton(btn, fill, border, fore);
            return btn;
        }

        private static void StyleActionButton(UIButton btn, Color fill, Color border, Color? fore = null)
        {
            btn.Font = FontBodyBold;
            btn.Style = UIStyle.Black;
            btn.FillColor = fill;
            btn.FillHoverColor = Blend(fill, Color.White, 0.12f);
            btn.FillPressColor = Blend(fill, Color.Black, 0.22f);
            btn.RectColor = border;
            btn.ForeColor = fore ?? ClrText;
            btn.Radius = 4;
            btn.Cursor = Cursors.Hand;
        }

        private static Color Blend(Color a, Color b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return Color.FromArgb(
                (int)(a.A + (b.A - a.A) * t),
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t));
        }

        private static void Panel_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Control control)
                return;

            e.Graphics.Clear(control.BackColor);
            using var pen = new Pen(ClrBorder);
            e.Graphics.DrawRectangle(pen, 0, 0, control.Width - 1, control.Height - 1);
        }

        private static void FillRoundedRect(Graphics g, Brush brush, Rectangle rect, int radius)
        {
            if (rect.Width <= 0 || rect.Height <= 0)
                return;

            int diameter = Math.Max(1, radius * 2);
            using var path = new GraphicsPath();
            path.AddArc(rect.Left, rect.Top, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            g.FillPath(brush, path);
        }

        private static int DecimalToTick(decimal value, decimal min, decimal step, int maxTick)
        {
            return Math.Clamp((int)Math.Round((value - min) / step), 0, maxTick);
        }

        private static int GetDecimalPlaces(decimal value)
        {
            string text = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            int dot = text.IndexOf('.');
            return dot < 0 ? 0 : text.Length - dot - 1;
        }

        private static string FormatDecimal(decimal value, int decimals)
        {
            return value.ToString(decimals <= 0 ? "0" : "0." + new string('0', decimals), System.Globalization.CultureInfo.InvariantCulture);
        }

        private static bool TryParseDecimal(string text, out decimal value)
        {
            if (decimal.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.CurrentCulture, out value))
                return true;
            return decimal.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value);
        }

        private static string SanitizeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "effect";
            foreach (char c in Path.GetInvalidFileNameChars())
                value = value.Replace(c, '_');
            return value;
        }

        private sealed class PresetListItem
        {
            public PresetListItem(EffectPreset preset) => Preset = preset;
            public EffectPreset Preset { get; }
            public override string ToString() => Preset.Name;
        }

        private sealed class LayerListItem
        {
            public LayerListItem(EffectEditorLayer layer, string title)
            {
                Layer = layer;
                Title = title;
            }

            public EffectEditorLayer Layer { get; }
            public string Title { get; }
            public override string ToString() => Title;
        }

        private sealed class DustOption
        {
            public DustOption(string name, int id)
            {
                Name = name;
                Id = id;
            }

            public string Name { get; }
            public int Id { get; }
            public override string ToString() => Name;
        }

        private sealed class EffectKindOption
        {
            public EffectKindOption(ProjectileEffectKind kind) => Kind = kind;
            public ProjectileEffectKind Kind { get; }
            public override string ToString() => ProjectileEffectKindNames.GetDisplayName(Kind);
        }

        private sealed class VisualSourceOption
        {
            public VisualSourceOption(ProjectileVisualSource source) => Source = source;
            public ProjectileVisualSource Source { get; }
            public override string ToString() => ProjectileVisualSourceNames.GetDisplayName(Source);
        }

        private sealed class ImpactKindOption
        {
            public ImpactKindOption(ImpactEffectKind kind) => Kind = kind;
            public ImpactEffectKind Kind { get; }
            public override string ToString() => ImpactEffectKindNames.GetDisplayName(Kind);
        }

        private sealed class DarkSlider : Control
        {
            private int _minimum;
            private int _maximum = 100;
            private int _value;
            private bool _dragging;

            public event EventHandler? ValueChanged;

            [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
            public int Minimum
            {
                get => _minimum;
                set
                {
                    _minimum = value;
                    if (_maximum < _minimum)
                        _maximum = _minimum;
                    Value = Math.Clamp(_value, _minimum, _maximum);
                    Invalidate();
                }
            }

            [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
            public int Maximum
            {
                get => _maximum;
                set
                {
                    _maximum = Math.Max(value, _minimum);
                    Value = Math.Clamp(_value, _minimum, _maximum);
                    Invalidate();
                }
            }

            [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
            public int Value
            {
                get => _value;
                set
                {
                    int next = Math.Clamp(value, _minimum, _maximum);
                    if (_value == next)
                        return;

                    _value = next;
                    Invalidate();
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }

            public DarkSlider()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
                Height = 28;
                Cursor = Cursors.Hand;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.Clear(BackColor);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                int trackY = Height / 2 - 2;
                Rectangle track = new(0, trackY, Math.Max(1, Width - 1), 4);
                using var trackBrush = new SolidBrush(Color.FromArgb(37, 51, 68));
                FillRoundedRect(e.Graphics, trackBrush, track, 2);

                float ratio = _maximum == _minimum ? 0f : (_value - _minimum) / (float)(_maximum - _minimum);
                int thumbX = Math.Clamp((int)Math.Round(ratio * (Width - 14)) + 7, 7, Math.Max(7, Width - 7));
                Rectangle fill = new(0, trackY, Math.Max(2, thumbX), 4);
                using var fillBrush = new SolidBrush(ClrAccentBlue);
                FillRoundedRect(e.Graphics, fillBrush, fill, 2);

                Rectangle thumb = new(thumbX - 6, Height / 2 - 8, 12, 16);
                using var thumbBrush = new SolidBrush(ClrAccent);
                using var glowBrush = new SolidBrush(Color.FromArgb(42, ClrAccent));
                e.Graphics.FillEllipse(glowBrush, thumb.X - 4, thumb.Y - 3, thumb.Width + 8, thumb.Height + 6);
                FillRoundedRect(e.Graphics, thumbBrush, thumb, 4);
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);
                if (e.Button != MouseButtons.Left)
                    return;

                _dragging = true;
                Capture = true;
                SetValueFromMouse(e.X);
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                if (_dragging)
                    SetValueFromMouse(e.X);
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);
                _dragging = false;
                Capture = false;
            }

            private void SetValueFromMouse(int x)
            {
                float ratio = Math.Clamp((x - 7) / (float)Math.Max(1, Width - 14), 0f, 1f);
                Value = _minimum + (int)Math.Round((_maximum - _minimum) * ratio);
            }
        }

        private sealed class BufferedPanel : Panel
        {
            public BufferedPanel()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
                SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.UserPaint,
                    true);
                UpdateStyles();
            }
        }
    }
}
