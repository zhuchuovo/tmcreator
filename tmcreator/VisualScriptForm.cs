using Sunny.UI;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Text.Json;
using tmcreator.Models;
using tmcreator.Models.Flow;

namespace tmcreator
{
    public partial class VisualScriptForm : UIForm
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private readonly ModItemData _item;
        private readonly List<string> _projectileReferences;
        private readonly List<BlockInstance> _topBlocks = new();

        private readonly Panel _header = new BufferedPanel();
        private readonly Panel _palette = new BufferedPanel();
        private readonly FlowLayoutPanel _paletteList = new();
        private readonly Panel _paletteViewport = new BufferedPanel();
        private readonly DarkScrollBar _paletteScroll = new();
        private readonly Panel _workspace = new BufferedPanel();
        private readonly Panel _toolbar = new BufferedPanel();
        private readonly Panel _canvasViewport = new BufferedPanel();
        private readonly Panel _canvas = new BufferedPanel();
        private readonly DarkScrollBar _canvasScroll = new();
        private readonly Label _summaryLabel = new();
        private readonly UIButton _btnSave = new();
        private readonly UIButton _btnClear = new();
        private readonly UIButton _btnImportFlow = new();
        private readonly UIButton _btnExportFlow = new();
        private int _canvasContentHeight;
        private int _canvasContentWidth;
        private float _canvasZoom = 1f;

        private const int WmSetRedraw = 0x000B;
        private const string DragFormat = "tmcreator.flow.block";
        private const int CanvasGridSize = 24;
        private const int CanvasDefaultBlockWidth = 560;
        private const int CanvasMinimumBlockWidth = 420;
        private const float CanvasMinZoom = 0.65f;
        private const float CanvasMaxZoom = 1.65f;
        private const int BranchDropHitPadding = 36;
        private const int ParamDropHitPadding = 14;
        private const int DropOverlapThreshold = 80;
        private const int CanvasDragAutoScrollMargin = 56;
        private const int CanvasDragAutoScrollStep = 28;
        private static readonly JsonSerializerOptions FlowJsonOptions = new() { WriteIndented = true };
        private Point _dragStartScreenPoint;
        private Control? _dragSourceControl;
        private TopLevelDragState? _topLevelDrag;
        private bool _canvasPanning;
        private bool _canvasPanStarted;
        private Point _canvasPanStartScreenPoint;
        private int _canvasPanStartScroll;

        private static readonly HashSet<string> ItemEventIds = new()
        {
            "on_use",
            "on_hit_npc",
            "on_hit_pvp",
            "while_held"
        };

        private static readonly HashSet<string> BuffEventIds = new()
        {
            "buff_on_gain",
            "buff_update",
            "buff_on_end"
        };

        private static readonly HashSet<string> ProjectileEventIds = new()
        {
            "projectile_on_spawn",
            "projectile_on_hit_npc",
            "projectile_on_hit_player",
            "projectile_update"
        };

        private static readonly HashSet<string> AccessoryEventIds = new()
        {
            "accessory_wearing",
            "accessory_attack",
            "accessory_unequip",
            "accessory_equip"
        };

        private static readonly Color ClrBg = Color.FromArgb(14, 20, 28);
        private static readonly Color ClrPanel = Color.FromArgb(22, 31, 43);
        private static readonly Color ClrCanvas = Color.FromArgb(15, 23, 33);
        private static readonly Color ClrCard = Color.FromArgb(27, 38, 52);
        private static readonly Color ClrCardAlt = Color.FromArgb(32, 45, 61);
        private static readonly Color ClrInput = Color.FromArgb(12, 18, 27);
        private static readonly Color ClrBorder = Color.FromArgb(55, 73, 94);
        private static readonly Color ClrSoftBorder = Color.FromArgb(39, 54, 72);
        private static readonly Color ClrText = Color.FromArgb(238, 244, 252);
        private static readonly Color ClrSubText = Color.FromArgb(154, 171, 193);
        private static readonly Color ClrMuted = Color.FromArgb(100, 119, 142);
        private static readonly Color ClrAccent = Color.FromArgb(53, 199, 183);
        private static readonly Color ClrDanger = Color.FromArgb(237, 94, 104);
        private static readonly Color ClrSuccess = Color.FromArgb(88, 204, 132);

        private static readonly Font FontTitle = new("Microsoft YaHei UI", 17F, FontStyle.Bold, GraphicsUnit.Point);
        private static readonly Font FontPanelTitle = new("Microsoft YaHei UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
        private static readonly Font FontBody = new("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        private static readonly Font FontBodyBold = new("Microsoft YaHei UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        private static readonly Font FontSmall = new("Microsoft YaHei UI", 8F, FontStyle.Regular, GraphicsUnit.Point);

        public VisualScriptForm(ModItemData item, IEnumerable<ModItemData>? projectItems = null)
        {
            _item = item;
            _projectileReferences = (projectItems ?? Enumerable.Empty<ModItemData>())
                .Where(projectItem => projectItem.Type == ItemType.Projectile)
                .Select(projectItem => projectItem.Name.Trim())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            Text = $"流程编辑 - {item.DisplayName}";
            ClientSize = new Size(1180, 760);
            MinimumSize = new Size(980, 620);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = ClrBg;
            Style = UIStyle.Black;

            LoadFlowSnapshot();
            BuildLayout();
            RebuildCanvas();
        }

        private void LoadFlowSnapshot()
        {
            _topBlocks.Clear();
            if (_item.Flow == null)
                return;

            // Edit a detached copy. Closing the editor without saving should not mutate the item.
            _topBlocks.AddRange(_item.Flow.Blocks.Select(CloneBlock));
        }

        private void BuildLayout()
        {
            SuspendLayout();
            Controls.Clear();

            BuildHeader();
            BuildPalette();
            BuildWorkspace();

            Controls.Add(_header);
            Controls.Add(_palette);
            Controls.Add(_workspace);

            Resize -= VisualScriptForm_Resize;
            Resize += VisualScriptForm_Resize;
            LayoutShell();

            ResumeLayout(false);
        }

        private void BuildHeader()
        {
            _header.BackColor = ClrBg;
            _header.Paint += Header_Paint;

            var title = CreateLabel("流程工作台", new Point(28, 12), new Size(220, 32), FontTitle, ClrText);
            var subtitle = CreateLabel($"正在编辑: {_item.DisplayName}。先放事件，再把动作和条件串起来。", new Point(30, 46), new Size(620, 22), FontBody, ClrSubText);

            _summaryLabel.AutoSize = false;
            _summaryLabel.Location = new Point(720, 30);
            _summaryLabel.Size = new Size(340, 22);
            _summaryLabel.TextAlign = ContentAlignment.MiddleRight;
            _summaryLabel.Font = FontBodyBold;
            _summaryLabel.ForeColor = ClrSubText;
            _summaryLabel.BackColor = Color.Transparent;

            _header.Controls.Add(title);
            _header.Controls.Add(subtitle);
            _header.Controls.Add(_summaryLabel);
        }

        private void BuildPalette()
        {
            _palette.BackColor = ClrPanel;
            _palette.Paint += Panel_Paint;

            var title = CreateLabel("块库", new Point(22, 18), new Size(120, 24), FontPanelTitle, ClrText);
            var hint = CreateLabel("点击加入主流程，拖动可放入画布/参数槽", new Point(22, 42), new Size(230, 20), FontSmall, ClrSubText);

            _paletteList.FlowDirection = FlowDirection.TopDown;
            _paletteList.WrapContents = false;
            _paletteList.AutoScroll = false;
            _paletteList.BackColor = ClrPanel;
            _paletteList.Padding = new Padding(0);
            _paletteList.Margin = new Padding(0);
            _paletteList.MouseWheel += PaletteList_MouseWheel;

            _paletteViewport.BackColor = ClrPanel;
            _paletteViewport.Paint += Panel_Paint;
            _paletteViewport.Controls.Add(_paletteList);

            _paletteScroll.Width = 10;
            _paletteScroll.SmallChange = 28;
            _paletteScroll.LargeChange = 140;
            _paletteScroll.ValueChanged += (s, e) => LayoutPaletteContent();

            _palette.Controls.Add(title);
            _palette.Controls.Add(hint);
            _palette.Controls.Add(_paletteViewport);
            _palette.Controls.Add(_paletteScroll);

            foreach (BlockCategory category in Enum.GetValues<BlockCategory>())
            {
                AddPaletteCategory(category);
            }
        }

        private void AddPaletteCategory(BlockCategory category)
        {
            var categoryPanel = new Panel
            {
                Size = new Size(232, 34),
                Margin = new Padding(0, 0, 0, 6),
                BackColor = Color.Transparent
            };
            categoryPanel.Paint += (s, e) => DrawAccentPanel(e.Graphics, categoryPanel.ClientRectangle, GetCategoryColor(category), Color.FromArgb(28, 40, 54));
            var categoryLabel = CreateLabel(CategoryName(category), new Point(14, 7), new Size(200, 20), FontBodyBold, ClrText);
            categoryLabel.Name = "PaletteCategory";
            categoryPanel.Controls.Add(categoryLabel);
            _paletteList.Controls.Add(categoryPanel);

            foreach (var definition in GetPaletteDefinitions(category))
            {
                var blockButton = new Panel
                {
                    Size = new Size(214, 34),
                    Margin = new Padding(0, 0, 0, 5),
                    BackColor = ClrCard,
                    Cursor = Cursors.Hand,
                    Tag = definition
                };
                blockButton.Paint += PaletteBlock_Paint;
                blockButton.Click += PaletteBlock_Click;
                RegisterPaletteDrag(blockButton, definition);

                var label = CreateLabel(definition.Name, new Point(14, 7), new Size(130, 20), FontBody, ClrText);
                label.Name = "PaletteName";
                label.Cursor = Cursors.Hand;
                label.Tag = definition;
                label.Click += PaletteBlock_Click;
                RegisterPaletteDrag(label, definition);
                blockButton.Controls.Add(label);

                var meta = CreateLabel(CategoryShortName(category), new Point(164, 8), new Size(42, 18), FontSmall, GetCategoryColor(category));
                meta.Name = "PaletteMeta";
                meta.TextAlign = ContentAlignment.MiddleRight;
                meta.Cursor = Cursors.Hand;
                meta.Tag = definition;
                meta.Click += PaletteBlock_Click;
                RegisterPaletteDrag(meta, definition);
                blockButton.Controls.Add(meta);

                blockButton.MouseEnter += (s, e) => blockButton.BackColor = Color.FromArgb(36, 51, 68);
                blockButton.MouseLeave += (s, e) => blockButton.BackColor = ClrCard;

                _paletteList.Controls.Add(blockButton);
            }
        }

        private IEnumerable<BlockDefinition> GetPaletteDefinitions(BlockCategory category)
        {
            var definitions = BlockRegistry.GetByCategory(category);
            if (category != BlockCategory.Event)
                return definitions;

            return _item.Type switch
            {
                ItemType.Buff => definitions.Where(definition => BuffEventIds.Contains(definition.Id)),
                ItemType.Projectile => definitions.Where(definition => ProjectileEventIds.Contains(definition.Id)),
                ItemType.Accessory => definitions.Where(definition => AccessoryEventIds.Contains(definition.Id)),
                _ => definitions.Where(definition => ItemEventIds.Contains(definition.Id))
            };
        }

        private void BuildWorkspace()
        {
            _workspace.BackColor = ClrPanel;
            _workspace.Paint += Panel_Paint;

            var title = CreateLabel("流程画布", new Point(24, 18), new Size(160, 24), FontPanelTitle, ClrText);
            var hint = CreateLabel("分支会自动收纳子节点，重新渲染时不会再残留旧按钮。", new Point(24, 44), new Size(520, 20), FontSmall, ClrSubText);
            _workspace.Controls.Add(title);
            _workspace.Controls.Add(hint);

            _btnClear.Text = "清空";
            _btnClear.Font = FontBodyBold;
            _btnClear.Size = new Size(86, 34);
            StyleButton(_btnClear, Color.FromArgb(67, 83, 105), ClrBorder);
            _btnClear.Click += (s, e) =>
            {
                _topBlocks.Clear();
                RebuildCanvas();
            };

            _btnSave.Text = "保存流程";
            _btnSave.Font = FontBodyBold;
            _btnSave.Size = new Size(108, 34);
            StyleButton(_btnSave, ClrAccent, Color.FromArgb(35, 144, 134));
            _btnSave.Click += (s, e) =>
            {
                _item.Flow = new FlowScript { Blocks = _topBlocks.Select(CloneBlock).ToList() };
                UIMessageBox.Show("流程已保存。");
                Close();
            };

            _btnImportFlow.Text = "导入流程";
            _btnImportFlow.Font = FontBodyBold;
            _btnImportFlow.Size = new Size(104, 30);
            StyleButton(_btnImportFlow, Color.FromArgb(92, 116, 236), Color.FromArgb(68, 86, 175));
            _btnImportFlow.Click += (s, e) => ImportFlow();

            _btnExportFlow.Text = "导出流程";
            _btnExportFlow.Font = FontBodyBold;
            _btnExportFlow.Size = new Size(104, 30);
            StyleButton(_btnExportFlow, Color.FromArgb(62, 142, 241), Color.FromArgb(43, 102, 178));
            _btnExportFlow.Click += (s, e) => ExportFlow();

            _toolbar.BackColor = ClrPanel;
            _toolbar.Controls.Add(_btnClear);
            _toolbar.Controls.Add(_btnSave);
            _toolbar.Controls.Add(_btnImportFlow);
            _toolbar.Controls.Add(_btnExportFlow);
            _workspace.Controls.Add(_toolbar);

            _canvasViewport.BackColor = ClrCanvas;
            _canvasViewport.Paint += Panel_Paint;
            _canvasViewport.Controls.Add(_canvas);

            _canvasScroll.Width = 10;
            _canvasScroll.SmallChange = 32;
            _canvasScroll.LargeChange = 180;
            _canvasScroll.ValueChanged += (s, e) => LayoutCanvasContent();

            _canvas.AutoScroll = false;
            _canvas.AllowDrop = true;
            _canvas.TabStop = true;
            _canvas.BackColor = ClrCanvas;
            _canvas.Paint += Canvas_Paint;
            _canvas.MouseWheel += Canvas_MouseWheel;
            _canvas.MouseEnter += (s, e) => _canvas.Focus();
            _canvas.MouseDown += CanvasPan_MouseDown;
            _canvas.MouseMove += CanvasPan_MouseMove;
            _canvas.MouseUp += CanvasPan_MouseUp;
            _canvas.DragEnter += Canvas_DragEnter;
            _canvas.DragOver += Canvas_DragEnter;
            _canvas.DragDrop += Canvas_DragDrop;
            _canvasViewport.AllowDrop = true;
            _canvasViewport.MouseDown += CanvasPan_MouseDown;
            _canvasViewport.MouseMove += CanvasPan_MouseMove;
            _canvasViewport.MouseUp += CanvasPan_MouseUp;
            _canvasViewport.DragEnter += Canvas_DragEnter;
            _canvasViewport.DragOver += Canvas_DragEnter;
            _canvasViewport.DragDrop += Canvas_DragDrop;
            _workspace.Controls.Add(_canvasViewport);
            _workspace.Controls.Add(_canvasScroll);
        }

        private void ExportFlow()
        {
            using var dialog = new SaveFileDialog
            {
                Title = "导出流程",
                Filter = "TMCreator 流程|*.tmcreator-flow.json|JSON 文件|*.json|所有文件|*.*",
                FileName = $"{SanitizeFileName(_item.Name)}.tmcreator-flow.json"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                var flow = new FlowScript { Blocks = _topBlocks.Select(CloneBlock).ToList() };
                string json = JsonSerializer.Serialize(flow, FlowJsonOptions);
                File.WriteAllText(dialog.FileName, json, System.Text.Encoding.UTF8);
                UIMessageBox.Show($"流程已导出：\n{dialog.FileName}");
            }
            catch (Exception ex)
            {
                UIMessageBox.Show($"导出流程失败：{ex.Message}");
            }
        }

        private void ImportFlow()
        {
            using var dialog = new OpenFileDialog
            {
                Title = "导入流程",
                Filter = "TMCreator 流程|*.tmcreator-flow.json;*.tmflow.json|JSON 文件|*.json|所有文件|*.*"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                string json = File.ReadAllText(dialog.FileName);
                var flow = JsonSerializer.Deserialize<FlowScript>(json, FlowJsonOptions);
                if (flow == null)
                {
                    UIMessageBox.Show("流程文件内容为空或格式不正确。");
                    return;
                }

                _topBlocks.Clear();
                _topBlocks.AddRange((flow.Blocks ?? new List<BlockInstance>()).Select(CloneBlock));
                RebuildCanvas();
                UIMessageBox.Show("流程已导入，点击「保存流程」后才会写回当前物品。");
            }
            catch (Exception ex)
            {
                UIMessageBox.Show($"导入流程失败：{ex.Message}");
            }
        }

        private static string SanitizeFileName(string name)
        {
            string cleaned = string.Join("_", (name ?? string.Empty).Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).Trim();
            return string.IsNullOrWhiteSpace(cleaned) ? "flow" : cleaned;
        }

        private void LayoutShell()
        {
            const int margin = 18;
            const int headerTop = 38;
            const int headerHeight = 76;
            const int gap = 16;
            const int paletteWidth = 280;

            _header.SetBounds(margin, headerTop, ClientSize.Width - margin * 2, headerHeight);

            int bodyTop = headerTop + headerHeight + 12;
            int bodyHeight = ClientSize.Height - bodyTop - margin;
            _palette.SetBounds(margin, bodyTop, paletteWidth, bodyHeight);
            _workspace.SetBounds(margin + paletteWidth + gap, bodyTop, ClientSize.Width - paletteWidth - gap - margin * 2, bodyHeight);

            _paletteViewport.SetBounds(22, 74, _palette.Width - 56, Math.Max(100, _palette.Height - 96));
            _paletteScroll.SetBounds(_paletteViewport.Right + 4, _paletteViewport.Top, 8, _paletteViewport.Height);
            ResizePaletteItems();
            LayoutPaletteContent();

            _toolbar.SetBounds(_workspace.Width - 246, 14, 222, 70);
            _btnClear.Size = new Size(104, 30);
            _btnSave.Size = new Size(104, 30);
            _btnClear.Location = new Point(0, 0);
            _btnSave.Location = new Point(112, 0);
            _btnImportFlow.Location = new Point(0, 36);
            _btnExportFlow.Location = new Point(112, 36);

            _canvasViewport.SetBounds(24, 112, _workspace.Width - 62, _workspace.Height - 136);
            _canvasScroll.SetBounds(_canvasViewport.Right + 4, _canvasViewport.Top, 8, _canvasViewport.Height);
            LayoutCanvasContent();
            _summaryLabel.Location = new Point(Math.Max(360, _header.Width - 390), 30);
            _summaryLabel.Width = 360;
        }

        private void VisualScriptForm_Resize(object? sender, EventArgs e)
        {
            LayoutShell();
            RebuildCanvas();
        }

        private void ResizePaletteItems()
        {
            int itemWidth = Math.Max(160, _paletteViewport.Width - 2);
            foreach (Control control in _paletteList.Controls)
            {
                control.Width = itemWidth;
                foreach (Control child in control.Controls)
                {
                    if (child.Name == "PaletteMeta")
                    {
                        child.Left = control.Width - child.Width - 14;
                    }
                    else if (child.Name == "PaletteName")
                    {
                        child.Width = Math.Max(60, control.Width - child.Left - 76);
                    }
                    else if (child.Name == "PaletteCategory")
                    {
                        child.Width = Math.Max(60, control.Width - child.Left - 12);
                    }
                    else if (child.Location.X > control.Width - 70)
                    {
                        child.Left = control.Width - child.Width - 12;
                    }
                    else if (child.Width > control.Width - child.Left - 12)
                    {
                        child.Width = control.Width - child.Left - 12;
                    }
                }
            }
        }

        private void LayoutPaletteContent()
        {
            int contentHeight = GetFlowContentHeight(_paletteList);
            ConfigureScrollBar(_paletteScroll, contentHeight, _paletteViewport.Height);
            _paletteList.SetBounds(0, -_paletteScroll.Value, Math.Max(10, _paletteViewport.Width), Math.Max(contentHeight, _paletteViewport.Height));
        }

        private void PaletteList_MouseWheel(object? sender, MouseEventArgs e)
        {
            ScrollBarByWheel(_paletteScroll, e.Delta);
        }

        private void LayoutCanvasContent()
        {
            int contentHeight = Math.Max(_canvasContentHeight, _canvasViewport.Height);
            int contentWidth = Math.Max(_canvasContentWidth, _canvasViewport.Width);
            ConfigureScrollBar(_canvasScroll, contentHeight, _canvasViewport.Height);
            _canvas.SetBounds(0, -_canvasScroll.Value, Math.Max(10, contentWidth), Math.Max(contentHeight, _canvasViewport.Height));
        }

        private void Canvas_MouseWheel(object? sender, MouseEventArgs e)
        {
            float step = e.Delta > 0 ? 1.1f : 1 / 1.1f;
            float nextZoom = Math.Clamp(_canvasZoom * step, CanvasMinZoom, CanvasMaxZoom);
            if (Math.Abs(nextZoom - _canvasZoom) < 0.001f)
                return;

            ApplyCanvasZoom(nextZoom);
        }

        private void ApplyCanvasZoom(float nextZoom)
        {
            if (_canvas.Controls.Count == 0 || _topLevelDrag != null)
                return;

            float previousZoom = _canvasZoom;
            float ratio = nextZoom / previousZoom;
            _canvasZoom = nextZoom;

            SetRedraw(_canvasViewport, false);
            SetRedraw(_canvas, false);
            _canvas.SuspendLayout();
            try
            {
                foreach (Control child in _canvas.Controls)
                {
                    child.Scale(new SizeF(ratio, ratio));
                    HookCanvasInteractions(child);
                }

                var bounds = GetChildrenBounds(_canvas);
                _canvasContentWidth = Math.Max(_canvasViewport.Width, bounds.Right + Zoom(72));
                _canvasContentHeight = Math.Max(_canvasViewport.Height, bounds.Bottom + Zoom(72));
                LayoutCanvasContent();
                UpdateSummary();
            }
            finally
            {
                _canvas.ResumeLayout(false);
                SetRedraw(_canvas, true);
                SetRedraw(_canvasViewport, true);
                _canvasViewport.Invalidate(true);
                _canvas.Invalidate(true);
            }
        }

        private void CanvasPan_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button is not (MouseButtons.Left or MouseButtons.Middle))
                return;

            if (_dragSourceControl != null || _topLevelDrag != null || _canvasPanning)
                return;

            if (e.Button == MouseButtons.Left && !CanStartCanvasPan(sender as Control))
                return;

            _canvasPanning = true;
            _canvasPanStarted = false;
            _canvasPanStartScreenPoint = Control.MousePosition;
            _canvasPanStartScroll = _canvasScroll.Value;
            if (sender is Control control)
                control.Capture = true;

            _canvas.Cursor = Cursors.Hand;
        }

        private void CanvasPan_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!_canvasPanning)
                return;

            var current = Control.MousePosition;
            int deltaY = current.Y - _canvasPanStartScreenPoint.Y;
            if (!_canvasPanStarted && Math.Abs(deltaY) < SystemInformation.DragSize.Height / 2)
                return;

            _canvasPanStarted = true;
            SetCanvasScrollValue(_canvasPanStartScroll - deltaY);
        }

        private void CanvasPan_MouseUp(object? sender, MouseEventArgs e)
        {
            if (!_canvasPanning || e.Button is not (MouseButtons.Left or MouseButtons.Middle))
                return;

            _canvasPanning = false;
            _canvasPanStarted = false;
            if (sender is Control control)
                control.Capture = false;

            _canvas.Cursor = Cursors.Default;
        }

        private bool CanStartCanvasPan(Control? control)
        {
            while (control != null && control != _canvasViewport)
            {
                if (control is TextBoxBase or ComboBox or ButtonBase)
                    return false;

                if (control.Tag is BlockInstance or BranchTag or ParamSlotTag)
                    return false;

                if (control.Tag is TopLevelBlockTag)
                    return true;

                control = control.Parent;
            }

            return true;
        }

        private void SetCanvasScrollValue(int value)
        {
            if (!_canvasScroll.Visible)
                return;

            int maxValue = Math.Max(0, _canvasScroll.Maximum - _canvasScroll.LargeChange + 1);
            _canvasScroll.Value = Math.Clamp(value, _canvasScroll.Minimum, maxValue);
        }

        private static int GetFlowContentHeight(FlowLayoutPanel panel)
        {
            int max = 0;
            foreach (Control control in panel.Controls)
            {
                max = Math.Max(max, control.Bottom + control.Margin.Bottom);
            }

            return max + panel.Padding.Vertical;
        }

        private static void ConfigureScrollBar(DarkScrollBar scrollBar, int contentHeight, int viewportHeight)
        {
            bool needsScroll = contentHeight > viewportHeight;
            scrollBar.Visible = needsScroll;
            if (!needsScroll)
            {
                scrollBar.Value = 0;
                return;
            }

            scrollBar.Minimum = 0;
            scrollBar.LargeChange = Math.Max(1, viewportHeight);
            scrollBar.SmallChange = 32;
            scrollBar.Maximum = Math.Max(0, contentHeight - 1);
            int maxValue = Math.Max(0, scrollBar.Maximum - scrollBar.LargeChange + 1);
            if (scrollBar.Value > maxValue)
                scrollBar.Value = maxValue;
        }

        private static void ScrollBarByWheel(DarkScrollBar scrollBar, int delta)
        {
            if (!scrollBar.Visible)
                return;

            int direction = delta > 0 ? -1 : 1;
            int maxValue = Math.Max(0, scrollBar.Maximum - scrollBar.LargeChange + 1);
            int next = Math.Clamp(scrollBar.Value + direction * scrollBar.SmallChange * 3, scrollBar.Minimum, maxValue);
            scrollBar.Value = next;
        }

        private void PaletteBlock_Click(object? sender, EventArgs e)
        {
            if ((sender as Control)?.Tag is not BlockDefinition definition)
                return;

            var block = CreateBlockInstance(definition);
            PlaceNewTopLevelBlock(block);
            _topBlocks.Add(block);
            RebuildCanvas();
        }

        private void RegisterPaletteDrag(Control control, BlockDefinition definition)
        {
            control.MouseDown += (s, e) =>
            {
                if (e.Button != MouseButtons.Left)
                    return;

                _dragStartScreenPoint = Control.MousePosition;
                _dragSourceControl = control;
            };

            control.MouseMove += (s, e) =>
            {
                if (e.Button != MouseButtons.Left || _dragSourceControl != control || !HasDraggedFarEnough())
                    return;

                StartBlockDrag(control, new BlockDragData { Definition = definition });
            };

            control.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Left && _dragSourceControl == control)
                    _dragSourceControl = null;
            };
        }

        private void RegisterExistingBlockDrag(Control control, BlockInstance block)
        {
            control.MouseDown += (s, e) =>
            {
                if (e.Button != MouseButtons.Left)
                    return;

                _dragStartScreenPoint = Control.MousePosition;
                _dragSourceControl = control;
                BeginExistingBlockDrag(control, block);
            };

            control.MouseMove += (s, e) =>
            {
                if (e.Button != MouseButtons.Left || _dragSourceControl != control || !HasDraggedFarEnough())
                    return;

                if (_topLevelDrag?.Source == control)
                {
                    MoveTopLevelBlockDrag();
                    return;
                }

                StartBlockDrag(control, new BlockDragData { ExistingBlock = block, DropOffset = GetBlockDropOffset(block) });
            };

            control.MouseUp += (s, e) =>
            {
                if (e.Button != MouseButtons.Left || _dragSourceControl != control)
                    return;

                EndTopLevelBlockDrag();
                _dragSourceControl = null;
                control.Capture = false;
            };
        }

        private bool HasDraggedFarEnough()
        {
            var current = Control.MousePosition;
            return Math.Abs(current.X - _dragStartScreenPoint.X) >= SystemInformation.DragSize.Width / 2 ||
                   Math.Abs(current.Y - _dragStartScreenPoint.Y) >= SystemInformation.DragSize.Height / 2;
        }

        private void StartBlockDrag(Control source, BlockDragData data)
        {
            _topLevelDrag = null;
            _dragSourceControl = null;
            var dataObject = new DataObject();
            dataObject.SetData(DragFormat, data);
            source.DoDragDrop(dataObject, DragDropEffects.Copy | DragDropEffects.Move);
        }

        private void BeginExistingBlockDrag(Control source, BlockInstance block)
        {
            _topLevelDrag = null;

            bool isTopLevel = _topBlocks.Contains(block);
            Control? container = isTopLevel
                ? FindTopLevelBlockContainer(source)
                : CreateDetachedBlockPreview(source, block, out _);
            if (container == null)
                return;

            Point canvasPoint = _canvas.PointToClient(Control.MousePosition);
            _topLevelDrag = new TopLevelDragState
            {
                Block = block,
                Source = source,
                Container = container,
                Offset = new Point(canvasPoint.X - container.Left, canvasPoint.Y - container.Top),
                OriginalLocation = container.Location,
                IsDetachedPreview = !isTopLevel
            };
            source.Capture = true;
        }

        private Control? CreateDetachedBlockPreview(Control source, BlockInstance block, out Rectangle sourceBounds)
        {
            sourceBounds = Rectangle.Empty;
            var blockCard = FindRenderedBlockCard(source, block);
            if (blockCard == null)
                return null;

            sourceBounds = GetCanvasBounds(blockCard);
            if (sourceBounds.Width <= 0 || sourceBounds.Height <= 0)
                return null;

            var bitmap = new Bitmap(sourceBounds.Width, sourceBounds.Height);
            blockCard.DrawToBitmap(bitmap, new Rectangle(Point.Empty, blockCard.Size));

            var preview = new BufferedPanel
            {
                Bounds = sourceBounds,
                BackColor = ClrCanvas,
                BackgroundImage = bitmap,
                BackgroundImageLayout = ImageLayout.Stretch,
                Visible = false,
                Enabled = false
            };

            preview.Paint += (s, e) =>
            {
                using var border = new Pen(Color.FromArgb(180, ClrAccent));
                e.Graphics.DrawRectangle(border, 0, 0, preview.Width - 1, preview.Height - 1);
            };

            _canvas.Controls.Add(preview);
            preview.BringToFront();
            return preview;
        }

        private Panel? FindRenderedBlockCard(Control? control, BlockInstance block)
        {
            while (control != null && control != _canvas)
            {
                if (control is Panel panel && IsRenderedBlockCard(panel, block))
                    return panel;

                control = control.Parent;
            }

            return null;
        }

        private static bool IsRenderedBlockCard(Panel panel, BlockInstance block)
        {
            if (ReferenceEquals(panel.Tag, block))
                return true;

            if (panel.Tag is not BranchTag branch || !ReferenceEquals(branch.Block, block))
                return false;

            return panel.Controls.OfType<Panel>().Any(child =>
                child.Location == new Point(1, 1) &&
                child.Height is >= 38 and <= 44);
        }

        private void MoveTopLevelBlockDrag()
        {
            if (_topLevelDrag == null)
                return;

            if (!_topLevelDrag.Started)
            {
                _topLevelDrag.Started = true;
                _topLevelDrag.Container.Visible = true;
                _topLevelDrag.Container.BringToFront();
                RefreshCanvasDragArea(_topLevelDrag.Container.Bounds);
            }

            AutoScrollCanvasDuringDrag();
            Point canvasPoint = _canvas.PointToClient(Control.MousePosition);
            var next = new Point(
                Math.Max(0, canvasPoint.X - _topLevelDrag.Offset.X),
                Math.Max(0, canvasPoint.Y - _topLevelDrag.Offset.Y));

            if (next == _topLevelDrag.Container.Location)
                return;

            Rectangle dirty = Rectangle.Union(_topLevelDrag.Container.Bounds, new Rectangle(next, _topLevelDrag.Container.Size));
            _topLevelDrag.Container.Location = next;
            _topLevelDrag.Container.BringToFront();
            ExtendCanvasForDrag(new Rectangle(next, _topLevelDrag.Container.Size));
            RefreshCanvasDragArea(dirty);
        }

        private void RefreshCanvasDragArea(Rectangle dirty)
        {
            dirty.Inflate(Math.Max(12, Zoom(12)), Math.Max(12, Zoom(12)));
            dirty.Intersect(_canvas.ClientRectangle);
            if (dirty.IsEmpty)
                return;

            _canvas.Invalidate(dirty, true);
            _canvas.Update();
        }

        private void AutoScrollCanvasDuringDrag()
        {
            Point viewportPoint = _canvasViewport.PointToClient(Control.MousePosition);
            if (viewportPoint.Y > _canvasViewport.Height - CanvasDragAutoScrollMargin)
            {
                SetCanvasScrollValue(_canvasScroll.Value + CanvasDragAutoScrollStep);
            }
            else if (viewportPoint.Y < CanvasDragAutoScrollMargin)
            {
                SetCanvasScrollValue(_canvasScroll.Value - CanvasDragAutoScrollStep);
            }
        }

        private void ExtendCanvasForDrag(Rectangle dragBounds)
        {
            int neededWidth = Math.Max(_canvasContentWidth, dragBounds.Right + Zoom(96));
            int neededHeight = Math.Max(_canvasContentHeight, dragBounds.Bottom + Zoom(96));
            if (neededWidth == _canvasContentWidth && neededHeight == _canvasContentHeight)
                return;

            _canvasContentWidth = neededWidth;
            _canvasContentHeight = neededHeight;
            LayoutCanvasContent();
        }

        private void EndTopLevelBlockDrag()
        {
            if (_topLevelDrag == null)
                return;

            try
            {
                if (_topLevelDrag.Started)
                {
                    if (TryAttachTopLevelDrag())
                    {
                        RebuildCanvas();
                        return;
                    }

                    if (_topLevelDrag.IsDetachedPreview)
                    {
                        RemoveBlock(_topLevelDrag.Block);
                        _topBlocks.Add(_topLevelDrag.Block);
                    }

                    _topLevelDrag.Block.CanvasX = Math.Max(16, SnapToGrid(Unzoom(_topLevelDrag.Container.Left)));
                    _topLevelDrag.Block.CanvasY = Math.Max(16, SnapToGrid(Unzoom(_topLevelDrag.Container.Top)));
                    RebuildCanvas();
                }
                else
                {
                    Rectangle dirty = _topLevelDrag.Container.Bounds;
                    _topLevelDrag.Container.Location = _topLevelDrag.OriginalLocation;
                    RefreshCanvasDragArea(Rectangle.Union(dirty, _topLevelDrag.Container.Bounds));
                }
            }
            finally
            {
                _topLevelDrag.Source.Capture = false;
                Rectangle dirty = _topLevelDrag.Container.Bounds;
                if (_topLevelDrag.IsDetachedPreview)
                {
                    var background = _topLevelDrag.Container.BackgroundImage;
                    _canvas.Controls.Remove(_topLevelDrag.Container);
                    _topLevelDrag.Container.Dispose();
                    background?.Dispose();
                }

                _topLevelDrag = null;
                RefreshCanvasDragArea(dirty);
            }
        }

        private Panel? FindTopLevelBlockContainer(Control? control)
        {
            while (control != null && control != _canvas)
            {
                if (control is Panel panel && panel.Tag is TopLevelBlockTag)
                    return panel;

                control = control.Parent;
            }

            return null;
        }

        private bool TryAttachTopLevelDrag()
        {
            if (_topLevelDrag == null)
                return false;

            var drag = new BlockDragData { ExistingBlock = _topLevelDrag.Block };
            Control? target = FindCanvasDropTarget(Control.MousePosition, _topLevelDrag.Container);
            if (TryAttachDragDataToTarget(drag, target))
                return true;

            return TryAttachDragDataToTarget(drag, FindCanvasDropTargetByProbe(_topLevelDrag.Container.Bounds, _topLevelDrag.Container, drag));
        }

        private Control? FindCanvasDropTarget(Point screenPoint, Control? excluded)
        {
            Point canvasPoint = _canvas.PointToClient(screenPoint);
            if (!_canvas.ClientRectangle.Contains(canvasPoint))
                return null;

            return FindDeepestControlAtPoint(_canvas, canvasPoint, excluded);
        }

        private static Control? FindDeepestControlAtPoint(Control parent, Point point, Control? excluded)
        {
            for (int i = parent.Controls.Count - 1; i >= 0; i--)
            {
                Control child = parent.Controls[i];
                if (!child.Visible || ReferenceEquals(child, excluded))
                    continue;

                if (!child.Bounds.Contains(point))
                    continue;

                Point childPoint = new(point.X - child.Left, point.Y - child.Top);
                return FindDeepestControlAtPoint(child, childPoint, excluded) ?? child;
            }

            return null;
        }

        private Control? FindCanvasDropTargetNearPoint(Point screenPoint, Control? excluded, BlockDragData drag)
        {
            Point canvasPoint = _canvas.PointToClient(screenPoint);
            if (!_canvas.ClientRectangle.Contains(canvasPoint))
                return null;

            var probe = new Rectangle(
                canvasPoint.X - BranchDropHitPadding,
                canvasPoint.Y - BranchDropHitPadding,
                BranchDropHitPadding * 2,
                BranchDropHitPadding * 2);

            return FindCanvasDropTargetByProbe(probe, excluded, drag);
        }

        private Control? FindCanvasDropTargetByProbe(Rectangle probeCanvasBounds, Control? excluded, BlockDragData drag)
        {
            Control? best = null;
            int bestScore = DropOverlapThreshold;

            foreach (var candidate in EnumerateDropCandidates(_canvas, excluded))
            {
                var slot = candidate.Tag as ParamSlotTag;
                var branch = candidate.Tag as BranchTag;
                if (slot != null && !CanDropIntoParamSlot(drag, slot))
                    continue;

                if (slot == null && (branch == null || !CanDropIntoBranch(drag, branch)))
                    continue;

                var candidateBounds = GetCanvasBounds(candidate);
                if (candidateBounds.Width <= 0 || candidateBounds.Height <= 0)
                    continue;

                candidateBounds.Inflate(slot != null ? ParamDropHitPadding : BranchDropHitPadding, slot != null ? ParamDropHitPadding : BranchDropHitPadding);
                var intersection = Rectangle.Intersect(probeCanvasBounds, candidateBounds);
                int score = intersection.Width * intersection.Height;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            return best;
        }

        private static IEnumerable<Control> EnumerateDropCandidates(Control parent, Control? excluded)
        {
            foreach (Control child in parent.Controls)
            {
                if (!child.Visible || ReferenceEquals(child, excluded))
                    continue;

                if (child.Tag is ParamSlotTag or BranchTag)
                    yield return child;

                foreach (var nested in EnumerateDropCandidates(child, excluded))
                    yield return nested;
            }
        }

        private Rectangle GetCanvasBounds(Control control)
        {
            return new Rectangle(_canvas.PointToClient(control.PointToScreen(Point.Empty)), control.Size);
        }

        private bool TryAttachDragDataToTarget(BlockDragData drag, Control? target)
        {
            if (target == null)
                return false;

            var slot = FindParamSlotTag(target);
            if (slot != null && CanDropIntoParamSlot(drag, slot))
            {
                var block = TakeDraggedBlock(drag);
                if (block == null)
                    return false;

                slot.Owner.ParamBlocks[slot.Param.Name] = block;
                return true;
            }

            var branch = FindBranchTag(target);
            if (branch != null && CanDropIntoBranch(drag, branch))
            {
                var block = TakeDraggedBlock(drag);
                if (block == null)
                    return false;

                GetBranchList(branch).Add(block);
                return true;
            }

            return false;
        }

        private BlockInstance? TakeDraggedBlock(BlockDragData drag)
        {
            if (drag.ExistingBlock != null)
            {
                RemoveBlock(drag.ExistingBlock);
                return drag.ExistingBlock;
            }

            return drag.Definition != null ? CreateBlockInstance(drag.Definition) : null;
        }

        private static BlockDragData? GetDragData(DragEventArgs e)
        {
            return e.Data?.GetDataPresent(DragFormat) == true
                ? e.Data.GetData(DragFormat) as BlockDragData
                : null;
        }

        private void Canvas_DragEnter(object? sender, DragEventArgs e)
        {
            var drag = GetDragData(e);
            e.Effect = drag == null ? DragDropEffects.None : drag.ExistingBlock == null ? DragDropEffects.Copy : DragDropEffects.Move;
        }

        private void Canvas_DragDrop(object? sender, DragEventArgs e)
        {
            var drag = GetDragData(e);
            if (drag == null)
                return;

            Control? target = FindCanvasDropTarget(new Point(e.X, e.Y), null);
            if (TryAttachDragDataToTarget(drag, target))
            {
                RebuildCanvas();
                return;
            }

            target = FindCanvasDropTargetNearPoint(new Point(e.X, e.Y), null, drag);
            if (TryAttachDragDataToTarget(drag, target))
            {
                RebuildCanvas();
                return;
            }

            var dropPoint = GetCanvasDropPoint(e, drag.DropOffset);
            var block = TakeDraggedBlock(drag);
            if (block == null)
                return;

            block.CanvasX = dropPoint.X;
            block.CanvasY = dropPoint.Y;
            _topBlocks.Add(block);

            RebuildCanvas();
        }

        private void RebuildCanvas()
        {
            if (_canvas.Width <= 0 || _canvas.Height <= 0)
                return;

            int scrollY = _canvasScroll.Value;
            EnsureTopLevelBlockPositions();
            SetRedraw(_canvasViewport, false);
            SetRedraw(_canvas, false);
            _canvas.SuspendLayout();
            try
            {
                ClearCanvasControls();

                if (_topBlocks.Count == 0)
                {
                    AddEmptyState();
                }
                else
                {
                    foreach (var block in _topBlocks)
                    {
                        RenderTopLevelBlock(block);
                    }
                }

                ScaleCanvasControls();
                var bounds = GetChildrenBounds(_canvas);
                _canvasContentWidth = Math.Max(_canvasViewport.Width, bounds.Right + Zoom(72));
                _canvasContentHeight = Math.Max(_canvasViewport.Height, bounds.Bottom + Zoom(72));
                LayoutCanvasContent();
                int maxValue = Math.Max(0, _canvasScroll.Maximum - _canvasScroll.LargeChange + 1);
                _canvasScroll.Value = Math.Min(scrollY, maxValue);
                LayoutCanvasContent();
                UpdateSummary();
            }
            finally
            {
                _canvas.ResumeLayout(false);

                SetRedraw(_canvas, true);
                SetRedraw(_canvasViewport, true);
                _canvasViewport.Invalidate(true);
                _canvas.Invalidate(true);
            }
        }

        private void ClearCanvasControls()
        {
            var controls = _canvas.Controls.Cast<Control>().ToArray();
            _canvas.Controls.Clear();
            foreach (var control in controls)
            {
                ReleaseControlImages(control);
                control.Dispose();
            }
        }

        private static void ReleaseControlImages(Control control)
        {
            foreach (Control child in control.Controls)
                ReleaseControlImages(child);

            if (control.BackgroundImage == null)
                return;

            var image = control.BackgroundImage;
            control.BackgroundImage = null;
            image.Dispose();
        }

        private static void SetRedraw(Control control, bool enabled)
        {
            if (!control.IsHandleCreated)
                return;

            SendMessage(control.Handle, WmSetRedraw, enabled ? new IntPtr(1) : IntPtr.Zero, IntPtr.Zero);
        }

        private void RenderTopLevelBlock(BlockInstance block)
        {
            var group = new BufferedPanel
            {
                Location = new Point(block.CanvasX, block.CanvasY),
                AllowDrop = true,
                BackColor = ClrCanvas,
                Tag = new TopLevelBlockTag { Block = block }
            };
            group.DragEnter += Canvas_DragEnter;
            group.DragOver += Canvas_DragEnter;
            group.DragDrop += Canvas_DragDrop;

            int bottom = RenderBlock(group, block, 0, 0);
            var bounds = GetChildrenBounds(group);
            group.Size = new Size(
                Math.Max(CanvasMinimumBlockWidth, bounds.Right + 4),
                Math.Max(84, Math.Max(bounds.Bottom + 4, bottom)));
            _canvas.Controls.Add(group);
        }

        private void AddEmptyState()
        {
            var empty = new Panel
            {
                Size = new Size(Math.Max(420, Unzoom(_canvasViewport.Width) - 72), 170),
                Location = new Point(28, 28),
                BackColor = Color.FromArgb(18, 28, 40)
            };
            empty.Paint += (s, e) => DrawDashedPanel(e.Graphics, empty.ClientRectangle);

            var title = CreateLabel("还没有流程节点", new Point(28, 34), new Size(260, 28), new Font("Microsoft YaHei UI", 13F, FontStyle.Bold), ClrText);
            var body = CreateLabel("从左侧块库点击一个「事件」开始，再继续添加动作或条件。", new Point(28, 72), new Size(520, 22), FontBody, ClrSubText);
            var tip = CreateLabel("小提示: 条件节点的「是 / 否」分支可以继续嵌套动作和条件。", new Point(28, 104), new Size(520, 22), FontSmall, ClrMuted);

            empty.Controls.Add(title);
            empty.Controls.Add(body);
            empty.Controls.Add(tip);
            _canvas.Controls.Add(empty);
        }

        private void EnsureTopLevelBlockPositions()
        {
            int index = 0;
            foreach (var block in _topBlocks)
            {
                if (block.CanvasX < 0 || block.CanvasY < 0)
                {
                    block.CanvasX = 32 + index % 3 * 48;
                    block.CanvasY = 32 + index * 120;
                }

                index++;
            }
        }

        private void PlaceNewTopLevelBlock(BlockInstance block)
        {
            int index = _topBlocks.Count;
            block.CanvasX = 32 + index % 3 * 48;
            block.CanvasY = 32 + index * 118;
        }

        private Point GetBlockDropOffset(BlockInstance block)
        {
            if (!_topBlocks.Contains(block) || block.CanvasX < 0 || block.CanvasY < 0)
                return new Point(24, 18);

            Point canvasPoint = _canvas.PointToClient(Control.MousePosition);
            return new Point(
                Math.Max(0, Unzoom(canvasPoint.X) - block.CanvasX),
                Math.Max(0, Unzoom(canvasPoint.Y) - block.CanvasY));
        }

        private Point GetCanvasDropPoint(DragEventArgs e, Point offset)
        {
            Point canvasPoint = _canvas.PointToClient(new Point(e.X, e.Y));
            int x = SnapToGrid(Unzoom(canvasPoint.X) - offset.X);
            int y = SnapToGrid(Unzoom(canvasPoint.Y) - offset.Y);
            return new Point(Math.Max(16, x), Math.Max(16, y));
        }

        private int GetBlockWidth(int indent)
        {
            int virtualViewportWidth = Math.Max(CanvasMinimumBlockWidth, Unzoom(_canvasViewport.Width));
            return Math.Max(CanvasMinimumBlockWidth, Math.Min(CanvasDefaultBlockWidth, virtualViewportWidth - indent - 48));
        }

        private int Zoom(int value) => Math.Max(1, (int)Math.Round(value * _canvasZoom));

        private int Unzoom(int value) => (int)Math.Round(value / _canvasZoom);

        private static int SnapToGrid(int value)
        {
            return (int)Math.Round(value / (double)CanvasGridSize) * CanvasGridSize;
        }

        private void ScaleCanvasControls()
        {
            foreach (Control child in _canvas.Controls)
            {
                if (Math.Abs(_canvasZoom - 1f) >= 0.001f)
                    child.Scale(new SizeF(_canvasZoom, _canvasZoom));
                HookCanvasInteractions(child);
            }
        }

        private void HookCanvasInteractions(Control control)
        {
            control.MouseWheel -= Canvas_MouseWheel;
            control.MouseWheel += Canvas_MouseWheel;
            control.MouseDown -= CanvasPan_MouseDown;
            control.MouseMove -= CanvasPan_MouseMove;
            control.MouseUp -= CanvasPan_MouseUp;
            control.MouseDown += CanvasPan_MouseDown;
            control.MouseMove += CanvasPan_MouseMove;
            control.MouseUp += CanvasPan_MouseUp;
            foreach (Control child in control.Controls)
                HookCanvasInteractions(child);
        }

        private static Rectangle GetChildrenBounds(Control parent)
        {
            Rectangle bounds = Rectangle.Empty;
            foreach (Control child in parent.Controls)
            {
                bounds = bounds.IsEmpty ? child.Bounds : Rectangle.Union(bounds, child.Bounds);
            }

            return bounds;
        }

        private int RenderBlock(Panel parent, BlockInstance block, int y, int indent)
        {
            var definition = BlockRegistry.Get(block.BlockDefId);
            if (definition == null)
                return RenderMissingBlock(parent, block, y, indent);

            int width = GetBlockWidth(indent);
            int rowHeight = 34;
            int contentTop = 54;
            int blockHeight = contentTop + Math.Max(1, definition.Params.Count) * rowHeight + 16;
            if (definition.Params.Count == 0)
                blockHeight = 84;

            var card = new Panel
            {
                Location = new Point(indent, y),
                Size = new Size(width, blockHeight),
                BackColor = ClrCard,
                Tag = block
            };
            card.Paint += (s, e) => DrawBlockCard(e.Graphics, card.ClientRectangle, GetCategoryColor(definition.Category));
            if (definition.Category == BlockCategory.Event)
            {
                card.AllowDrop = true;
                card.Tag = new BranchTag { Block = block, IsEventBody = true };
                card.DragEnter += Branch_DragEnter;
                card.DragOver += Branch_DragEnter;
                card.DragDrop += Branch_DragDrop;
            }

            var header = new Panel
            {
                Location = new Point(1, 1),
                Size = new Size(width - 2, 42),
                BackColor = Color.FromArgb(33, 47, 64)
            };
            header.Paint += (s, e) => DrawHeaderAccent(e.Graphics, header.ClientRectangle, GetCategoryColor(definition.Category));
            if (definition.Category == BlockCategory.Event)
            {
                header.AllowDrop = true;
                header.Tag = new BranchTag { Block = block, IsEventBody = true };
                header.DragEnter += Branch_DragEnter;
                header.DragOver += Branch_DragEnter;
                header.DragDrop += Branch_DragDrop;
            }

            var category = CreateLabel(CategoryShortName(definition.Category), new Point(16, 11), new Size(58, 20), FontSmall, GetCategoryColor(definition.Category));
            category.TextAlign = ContentAlignment.MiddleCenter;
            var title = CreateLabel(definition.Name, new Point(82, 10), new Size(width - 170, 22), FontBodyBold, ClrText);
            header.Cursor = Cursors.SizeAll;
            category.Cursor = Cursors.SizeAll;
            title.Cursor = Cursors.SizeAll;
            RegisterExistingBlockDrag(header, block);
            RegisterExistingBlockDrag(category, block);
            RegisterExistingBlockDrag(title, block);

            var delete = CreateLabel("删除", new Point(width - 82, 9), new Size(52, 24), FontBodyBold, ClrDanger);
            delete.TextAlign = ContentAlignment.MiddleCenter;
            delete.Cursor = Cursors.Hand;
            delete.Click += (s, e) =>
            {
                RemoveBlock(block);
                RebuildCanvas();
            };

            header.Controls.Add(category);
            header.Controls.Add(title);
            header.Controls.Add(delete);
            card.Controls.Add(header);

            if (definition.Params.Count == 0)
            {
                var empty = CreateLabel("此节点没有参数。", new Point(20, 54), new Size(220, 20), FontSmall, ClrMuted);
                card.Controls.Add(empty);
            }
            else
            {
                int py = contentTop;
                foreach (var param in definition.Params)
                {
                    AddParameterControl(card, block, param, py, width);
                    py += rowHeight;
                }
            }

            parent.Controls.Add(card);
            int nextY = y + blockHeight + 12;

            if (definition.Category == BlockCategory.Event)
            {
                nextY = RenderEventBody(parent, block, nextY, indent, width);
            }

            if (definition.HasTrueBranch)
            {
                nextY = RenderBranch(parent, block, definition.TrueLabel, true, nextY, indent, width);
            }

            if (definition.HasFalseBranch)
            {
                nextY = RenderBranch(parent, block, definition.FalseLabel, false, nextY, indent, width);
            }

            return nextY + 8;
        }

        private int RenderMissingBlock(Panel parent, BlockInstance block, int y, int indent)
        {
            int width = GetBlockWidth(indent);
            var card = new Panel
            {
                Location = new Point(indent, y),
                Size = new Size(width, 74),
                BackColor = Color.FromArgb(48, 32, 38)
            };
            card.Paint += (s, e) => DrawBlockCard(e.Graphics, card.ClientRectangle, ClrDanger);

            card.Controls.Add(CreateLabel($"未知流程块: {block.BlockDefId}", new Point(18, 18), new Size(width - 120, 22), FontBodyBold, ClrText));
            var delete = CreateLabel("删除", new Point(width - 82, 18), new Size(52, 24), FontBodyBold, ClrDanger);
            delete.TextAlign = ContentAlignment.MiddleCenter;
            delete.Cursor = Cursors.Hand;
            delete.Click += (s, e) =>
            {
                RemoveBlock(block);
                RebuildCanvas();
            };
            card.Controls.Add(delete);

            parent.Controls.Add(card);
            return y + card.Height + 18;
        }

        private int RenderEventBody(Panel parent, BlockInstance owner, int y, int indent, int ownerWidth)
        {
            int branchIndent = indent + 26;
            int width = Math.Max(380, ownerWidth - 26);
            var branchTag = new BranchTag { Block = owner, IsEventBody = true };

            var header = new Panel
            {
                Location = new Point(branchIndent, y),
                Size = new Size(width, 40),
                BackColor = Color.FromArgb(42, 42, 34),
                AllowDrop = true,
                Tag = branchTag
            };
            header.Paint += (s, e) => DrawBranchHeader(e.Graphics, header.ClientRectangle, GetCategoryColor(BlockCategory.Event));
            ApplyBranchDropTarget(header, branchTag);

            var branchLabel = CreateLabel("执行区: 把动作或条件拖到这里", new Point(18, 10), new Size(width - 170, 20), FontBodyBold, Color.FromArgb(255, 222, 120));
            var add = CreateLabel("+ 添加节点", new Point(width - 128, 8), new Size(104, 24), FontBodyBold, ClrText);
            add.TextAlign = ContentAlignment.MiddleCenter;
            add.Cursor = Cursors.Hand;
            add.BackColor = Color.FromArgb(38, GetCategoryColor(BlockCategory.Event));
            add.Tag = branchTag;
            add.Click += AddBranchNode_Click;
            ApplyBranchDropTarget(branchLabel, branchTag);
            ApplyBranchDropTarget(add, branchTag);

            header.Controls.Add(branchLabel);
            header.Controls.Add(add);
            parent.Controls.Add(header);

            y += header.Height + 8;

            if (owner.TrueBranch.Count == 0)
            {
                var emptyZone = new Panel
                {
                    Location = new Point(branchIndent, y),
                    Size = new Size(width, 30),
                    BackColor = Color.Transparent
                };
                ApplyBranchDropTarget(emptyZone, branchTag);

                var empty = CreateLabel("事件触发后还没有要执行的节点。", new Point(18, 4), new Size(width - 36, 20), FontSmall, ClrMuted);
                ApplyBranchDropTarget(empty, branchTag);
                emptyZone.Controls.Add(empty);
                parent.Controls.Add(emptyZone);
                y += emptyZone.Height;
            }
            else
            {
                foreach (var child in owner.TrueBranch)
                {
                    y = RenderBlock(parent, child, y, branchIndent + 18);
                }
            }

            return y + 4;
        }

        private int RenderBranch(Panel parent, BlockInstance owner, string label, bool isTrue, int y, int indent, int ownerWidth)
        {
            var children = isTrue ? owner.TrueBranch : owner.FalseBranch;
            var branchColor = isTrue ? ClrSuccess : ClrDanger;
            int branchIndent = indent + 26;
            int width = Math.Max(380, ownerWidth - 26);
            var branchTag = new BranchTag { Block = owner, IsTrue = isTrue };

            var header = new Panel
            {
                Location = new Point(branchIndent, y),
                Size = new Size(width, 38),
                BackColor = isTrue ? Color.FromArgb(26, 58, 43) : Color.FromArgb(60, 35, 42),
                AllowDrop = true,
                Tag = branchTag
            };
            header.Paint += (s, e) => DrawBranchHeader(e.Graphics, header.ClientRectangle, branchColor);
            ApplyBranchDropTarget(header, branchTag);

            var branchLabel = CreateLabel(isTrue ? $"是: {label}" : $"否: {label}", new Point(18, 9), new Size(width - 160, 20), FontBodyBold, isTrue ? Color.FromArgb(132, 235, 165) : Color.FromArgb(255, 139, 149));
            var add = CreateLabel("+ 添加节点", new Point(width - 128, 7), new Size(104, 24), FontBodyBold, ClrText);
            add.TextAlign = ContentAlignment.MiddleCenter;
            add.Cursor = Cursors.Hand;
            add.BackColor = Color.FromArgb(38, branchColor);
            add.Tag = branchTag;
            add.Click += AddBranchNode_Click;
            ApplyBranchDropTarget(branchLabel, branchTag);
            ApplyBranchDropTarget(add, branchTag);

            header.Controls.Add(branchLabel);
            header.Controls.Add(add);
            parent.Controls.Add(header);

            y += header.Height + 8;

            if (children.Count == 0)
            {
                var emptyZone = new Panel
                {
                    Location = new Point(branchIndent, y),
                    Size = new Size(width, 30),
                    BackColor = Color.Transparent
                };
                ApplyBranchDropTarget(emptyZone, branchTag);

                var empty = CreateLabel("这个分支还没有节点。", new Point(18, 4), new Size(width - 36, 20), FontSmall, ClrMuted);
                ApplyBranchDropTarget(empty, branchTag);
                emptyZone.Controls.Add(empty);
                parent.Controls.Add(emptyZone);
                y += emptyZone.Height;
            }
            else
            {
                foreach (var child in children)
                {
                    y = RenderBlock(parent, child, y, branchIndent + 18);
                }
            }

            return y + 4;
        }

        private void ApplyBranchDropTarget(Control control, BranchTag tag)
        {
            control.AllowDrop = true;
            control.Tag = tag;
            control.DragEnter -= Branch_DragEnter;
            control.DragOver -= Branch_DragEnter;
            control.DragDrop -= Branch_DragDrop;
            control.DragEnter += Branch_DragEnter;
            control.DragOver += Branch_DragEnter;
            control.DragDrop += Branch_DragDrop;
        }

        private void AddParameterControl(Control parent, BlockInstance block, BlockParam param, int y, int width)
        {
            parent.Controls.Add(CreateLabel(param.Label, new Point(22, y + 4), new Size(100, 22), FontBodyBold, ClrSubText));

            int inputX = 132;
            int inputWidth = Math.Min(280, width - inputX - 24);

            if (param.Type == ParamType.Number)
            {
                AddValueSlot(parent, block, param, inputX, y, Math.Min(440, width - inputX - 24));
                return;
            }

            if (param.Type == ParamType.Dropdown && param.Options.Length > 0)
            {
                var combo = CreateCombo(inputX, y, inputWidth);
                combo.Items.AddRange(param.Options);
                string current = block.ParamValues.GetValueOrDefault(param.Name, param.DefaultValue);
                int selected = Array.IndexOf(param.Options, current);
                if (selected < 0)
                    selected = Array.FindIndex(param.Options, option => option.StartsWith(current + " ", StringComparison.Ordinal));
                combo.SelectedIndex = selected >= 0 ? selected : 0;
                combo.Tag = new ParamTag { Block = block, ParamName = param.Name };
                combo.SelectedIndexChanged += ParamCombo_Changed;
                parent.Controls.Add(combo);
                return;
            }

            if (param.Type == ParamType.Target)
            {
                var combo = CreateCombo(inputX, y, inputWidth);
                combo.Items.AddRange(TargetOptions);
                string current = block.ParamValues.GetValueOrDefault(param.Name, param.DefaultValue);
                string? found = combo.Items.Cast<string>().FirstOrDefault(x => x.StartsWith(current + " ", StringComparison.Ordinal));
                combo.SelectedIndex = found != null ? combo.Items.IndexOf(found) : 0;
                combo.Tag = new ParamTag { Block = block, ParamName = param.Name };
                combo.SelectedIndexChanged += ParamCombo_Changed;
                parent.Controls.Add(combo);
                return;
            }

            if (param.Type == ParamType.Particle)
            {
                var combo = CreateCombo(inputX, y, inputWidth);
                combo.Items.AddRange(ParticleOptions);
                string current = block.ParamValues.GetValueOrDefault(param.Name, param.DefaultValue);
                string? found = combo.Items.Cast<string>().FirstOrDefault(x => x.StartsWith(current + " ", StringComparison.Ordinal));
                combo.SelectedIndex = found != null ? combo.Items.IndexOf(found) : 0;
                combo.Tag = new ParamTag { Block = block, ParamName = param.Name };
                combo.SelectedIndexChanged += ParamCombo_Changed;
                parent.Controls.Add(combo);
                return;
            }

            if (param.Type == ParamType.DamageType)
            {
                var combo = CreateCombo(inputX, y, inputWidth);
                combo.Items.AddRange(new[] { "melee", "ranged", "magic", "summon", "generic" });
                string current = block.ParamValues.GetValueOrDefault(param.Name, param.DefaultValue);
                int selected = combo.Items.IndexOf(current);
                combo.SelectedIndex = selected >= 0 ? selected : 0;
                combo.Tag = new ParamTag { Block = block, ParamName = param.Name };
                combo.SelectedIndexChanged += ParamCombo_Changed;
                parent.Controls.Add(combo);
                return;
            }

            if (param.Type == ParamType.Projectile)
            {
                var projectileText = CreateProjectileTextBox(inputX, y, inputWidth);
                projectileText.Text = block.ParamValues.GetValueOrDefault(param.Name, param.DefaultValue);
                projectileText.Tag = new ParamTag { Block = block, ParamName = param.Name };
                projectileText.TextChanged += ParamProjectile_Changed;
                parent.Controls.Add(projectileText);
                return;
            }

            var text = CreateTextBox(inputX, y, param.Type == ParamType.Text ? Math.Min(420, width - inputX - 24) : inputWidth);
            text.Text = block.ParamValues.GetValueOrDefault(param.Name, param.DefaultValue);
            text.Tag = new ParamTag { Block = block, ParamName = param.Name };
            text.TextChanged += ParamText_Changed;
            parent.Controls.Add(text);
        }

        private void AddValueSlot(Control parent, BlockInstance owner, BlockParam param, int x, int y, int width)
        {
            var slot = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, 30),
                BackColor = ClrInput,
                AllowDrop = true,
                Tag = new ParamSlotTag { Owner = owner, Param = param }
            };
            slot.Paint += ValueSlot_Paint;
            slot.DragEnter += ValueSlot_DragEnter;
            slot.DragOver += ValueSlot_DragEnter;
            slot.DragDrop += ValueSlot_DragDrop;

            if (owner.ParamBlocks.TryGetValue(param.Name, out var nestedBlock))
            {
                AddNestedValueChip(slot, owner, param, nestedBlock);
            }
            else
            {
                var text = CreateTextBox(0, 0, Math.Min(160, width - 132));
                text.Text = owner.ParamValues.GetValueOrDefault(param.Name, param.DefaultValue);
                text.Tag = new ParamTag { Block = owner, ParamName = param.Name, IsNumber = true };
                text.TextChanged += ParamText_Changed;
                text.AllowDrop = true;
                text.DragEnter += ValueSlot_DragEnter;
                text.DragOver += ValueSlot_DragEnter;
                text.DragDrop += ValueSlot_DragDrop;
                slot.Controls.Add(text);

                var hint = CreateLabel("拖入数值块", new Point(text.Right + 12, 5), new Size(width - text.Right - 18, 20), FontSmall, ClrMuted);
                hint.TextAlign = ContentAlignment.MiddleRight;
                hint.AllowDrop = true;
                hint.Tag = slot.Tag;
                hint.DragEnter += ValueSlot_DragEnter;
                hint.DragOver += ValueSlot_DragEnter;
                hint.DragDrop += ValueSlot_DragDrop;
                slot.Controls.Add(hint);
            }

            parent.Controls.Add(slot);
        }

        private void AddNestedValueChip(Panel slot, BlockInstance owner, BlockParam param, BlockInstance nestedBlock)
        {
            var definition = BlockRegistry.Get(nestedBlock.BlockDefId);
            string title = BuildValuePreview(nestedBlock);
            Color accent = definition == null ? ClrMuted : GetCategoryColor(definition.Category);

            var chip = new Panel
            {
                Location = new Point(2, 2),
                Size = new Size(slot.Width - 4, 26),
                BackColor = Color.FromArgb(40, 34, 56),
                Cursor = Cursors.SizeAll,
                Tag = nestedBlock
            };
            chip.Paint += (s, e) =>
            {
                using var border = new Pen(Color.FromArgb(120, accent));
                e.Graphics.DrawRectangle(border, 0, 0, chip.Width - 1, chip.Height - 1);
                using var accentBrush = new SolidBrush(accent);
                e.Graphics.FillRectangle(accentBrush, 0, 0, 3, chip.Height);
            };
            RegisterExistingBlockDrag(chip, nestedBlock);

            string valueKindLabel = GetPreviewValueKind(nestedBlock) == ValueExpressionKind.Coordinate ? "坐标" : "数值";
            var label = CreateLabel($"{valueKindLabel}: {title}", new Point(14, 4), new Size(Math.Min(210, chip.Width - 82), 18), FontSmall, ClrText);
            label.Cursor = Cursors.SizeAll;
            RegisterExistingBlockDrag(label, nestedBlock);

            var inlineParam = GetInlineNestedEditorParam(definition, chip.Width);
            if (inlineParam != null)
            {
                AddInlineNestedEditor(chip, nestedBlock, inlineParam, 154, chip.Width - 224);
            }

            var remove = CreateLabel("移除", new Point(chip.Width - 56, 4), new Size(42, 18), FontSmall, ClrDanger);
            remove.TextAlign = ContentAlignment.MiddleCenter;
            remove.Cursor = Cursors.Hand;
            remove.Click += (s, e) =>
            {
                owner.ParamBlocks.Remove(param.Name);
                RebuildCanvas();
            };

            chip.Controls.Add(label);
            chip.Controls.Add(remove);
            slot.Controls.Add(chip);
        }

        private static BlockParam? GetInlineNestedEditorParam(BlockDefinition? definition, int chipWidth)
        {
            if (definition == null || definition.Params.Count == 0 || chipWidth <= 300)
                return null;

            return definition.Id is "value_math" or "value_variable"
                ? null
                : definition.Params[0];
        }

        private static string BuildValuePreview(BlockInstance block)
        {
            return block.BlockDefId switch
            {
                "value_constant" => GetValuePreviewOperand(block, "value", "0"),
                "value_math" => BuildMathPreview(block),
                "value_event_target_position" => "事件目标的位置",
                "value_source_position" => "事件施加者的位置",
                "value_nearest_player_position" => "最近玩家的位置",
                "value_npc_hp" => "目标生命值",
                "value_player_hp" => "玩家生命值",
                "value_player_mana" => "玩家魔力值",
                "value_variable" => $"变量 {block.ParamValues.GetValueOrDefault("name", "myValue")}",
                _ => BlockRegistry.Get(block.BlockDefId)?.Name ?? block.BlockDefId
            };
        }

        private static ValueExpressionKind GetPreviewValueKind(BlockInstance block)
        {
            var definition = BlockRegistry.Get(block.BlockDefId);
            if (block.BlockDefId != "value_math")
                return definition?.ValueKind ?? ValueExpressionKind.Number;

            string op = block.ParamValues.GetValueOrDefault("operator", "*");
            if (op == "=")
                return ValueExpressionKind.Number;

            return GetPreviewParamValueKind(block, "left") == ValueExpressionKind.Coordinate ||
                   GetPreviewParamValueKind(block, "right") == ValueExpressionKind.Coordinate
                ? ValueExpressionKind.Coordinate
                : ValueExpressionKind.Number;
        }

        private static ValueExpressionKind GetPreviewParamValueKind(BlockInstance block, string paramName)
        {
            return block.ParamBlocks.TryGetValue(paramName, out var nestedBlock)
                ? GetPreviewValueKind(nestedBlock)
                : ValueExpressionKind.Number;
        }

        private static string BuildMathPreview(BlockInstance block)
        {
            string left = GetValuePreviewOperand(block, "left", "0");
            string op = block.ParamValues.GetValueOrDefault("operator", "*");
            string right = GetValuePreviewOperand(block, "right", "1");
            return op == "%"
                ? $"{left} * {right}%"
                : $"{left} {op} {right}";
        }

        private static string GetValuePreviewOperand(BlockInstance block, string paramName, string fallback)
        {
            if (block.ParamBlocks.TryGetValue(paramName, out var nestedBlock))
                return BuildValuePreview(nestedBlock);

            return block.ParamValues.TryGetValue(paramName, out var value) && !string.IsNullOrWhiteSpace(value)
                ? value
                : fallback;
        }

        private void AddInlineNestedEditor(Control parent, BlockInstance block, BlockParam param, int x, int width)
        {
            width = Math.Max(90, Math.Min(width, 180));

            if (param.Type == ParamType.Target)
            {
                var combo = CreateCombo(x, 1, width);
                combo.Size = new Size(width, 24);
                combo.Items.AddRange(TargetOptions);
                string current = block.ParamValues.GetValueOrDefault(param.Name, param.DefaultValue);
                string? found = combo.Items.Cast<string>().FirstOrDefault(item => item.StartsWith(current + " ", StringComparison.Ordinal));
                combo.SelectedIndex = found != null ? combo.Items.IndexOf(found) : 0;
                combo.Tag = new ParamTag { Block = block, ParamName = param.Name };
                combo.SelectedIndexChanged += ParamCombo_Changed;
                parent.Controls.Add(combo);
                return;
            }

            var text = CreateTextBox(x, 1, width);
            text.Size = new Size(width, 24);
            text.Text = block.ParamValues.GetValueOrDefault(param.Name, param.DefaultValue);
            text.Tag = new ParamTag { Block = block, ParamName = param.Name, IsNumber = param.Type == ParamType.Number };
            text.TextChanged += ParamText_Changed;
            parent.Controls.Add(text);
        }

        private void ValueSlot_DragEnter(object? sender, DragEventArgs e)
        {
            var slotTag = FindParamSlotTag(sender as Control);
            var drag = GetDragData(e);
            if (slotTag == null || drag == null || !CanDropIntoParamSlot(drag, slotTag))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            e.Effect = drag.ExistingBlock == null ? DragDropEffects.Copy : DragDropEffects.Move;
        }

        private void ValueSlot_DragDrop(object? sender, DragEventArgs e)
        {
            var drag = GetDragData(e);
            if (drag == null || !TryAttachDragDataToTarget(drag, sender as Control))
                return;

            RebuildCanvas();
        }

        private static ParamSlotTag? FindParamSlotTag(Control? control)
        {
            while (control != null)
            {
                if (control.Tag is ParamSlotTag tag)
                    return tag;

                control = control.Parent;
            }

            return null;
        }

        private static BranchTag? FindBranchTag(Control? control)
        {
            while (control != null)
            {
                if (control.Tag is BranchTag tag)
                    return tag;

                control = control.Parent;
            }

            return null;
        }

        private static bool CanDropIntoParamSlot(BlockDragData drag, ParamSlotTag slot)
        {
            var definition = drag.Definition ?? (drag.ExistingBlock != null ? BlockRegistry.Get(drag.ExistingBlock.BlockDefId) : null);
            if (definition?.Category != BlockCategory.Value)
                return false;

            return drag.ExistingBlock == null ||
                   !ReferenceEquals(drag.ExistingBlock, slot.Owner) &&
                   !ContainsBlock(drag.ExistingBlock, slot.Owner);
        }

        private void AddBranchNode_Click(object? sender, EventArgs e)
        {
            if ((sender as Control)?.Tag is not BranchTag tag)
                return;

            var menu = new ContextMenuStrip
            {
                BackColor = ClrPanel,
                ForeColor = ClrText,
                Font = FontBody
            };

            AddMenuGroup(menu, "添加动作", BlockCategory.Action, tag);
            AddMenuGroup(menu, "添加条件", BlockCategory.Condition, tag);

            if (sender is Control control)
                menu.Show(control, new Point(0, control.Height + 2));
        }

        private void Branch_DragEnter(object? sender, DragEventArgs e)
        {
            var drag = GetDragData(e);
            var branch = FindBranchTag(sender as Control);
            if (drag == null || branch == null || !CanDropIntoBranch(drag, branch))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            e.Effect = drag.ExistingBlock == null ? DragDropEffects.Copy : DragDropEffects.Move;
        }

        private void Branch_DragDrop(object? sender, DragEventArgs e)
        {
            var drag = GetDragData(e);
            if (drag == null || !TryAttachDragDataToTarget(drag, sender as Control))
                return;

            RebuildCanvas();
        }

        private static bool CanDropIntoBranch(BlockDragData drag, BranchTag branch)
        {
            var definition = drag.Definition ?? (drag.ExistingBlock != null ? BlockRegistry.Get(drag.ExistingBlock.BlockDefId) : null);
            if (definition?.Category is not (BlockCategory.Action or BlockCategory.Condition))
                return false;

            return drag.ExistingBlock == null || !ReferenceEquals(drag.ExistingBlock, branch.Block) && !ContainsBlock(drag.ExistingBlock, branch.Block);
        }

        private void AddMenuGroup(ContextMenuStrip menu, string title, BlockCategory category, BranchTag tag)
        {
            var group = new ToolStripMenuItem(title);
            foreach (var definition in BlockRegistry.GetByCategory(category))
            {
                var item = new ToolStripMenuItem(definition.Name) { Tag = new BranchMenuTag { Branch = tag, Definition = definition } };
                item.Click += BranchMenuItem_Click;
                group.DropDownItems.Add(item);
            }

            menu.Items.Add(group);
        }

        private void BranchMenuItem_Click(object? sender, EventArgs e)
        {
            if ((sender as ToolStripMenuItem)?.Tag is not BranchMenuTag tag)
                return;

            var branch = GetBranchList(tag.Branch);
            branch.Add(CreateBlockInstance(tag.Definition));
            RebuildCanvas();
        }

        private static List<BlockInstance> GetBranchList(BranchTag tag)
        {
            if (tag.IsEventBody)
                return tag.Block.TrueBranch;

            return tag.IsTrue ? tag.Block.TrueBranch : tag.Block.FalseBranch;
        }

        private static BlockInstance CreateBlockInstance(BlockDefinition definition)
        {
            var instance = new BlockInstance { BlockDefId = definition.Id };
            foreach (var parameter in definition.Params)
            {
                instance.ParamValues[parameter.Name] = parameter.DefaultValue;
            }

            return instance;
        }

        private void RemoveBlock(BlockInstance target)
        {
            if (RemoveBlockFromList(_topBlocks, target))
                return;

            foreach (var block in _topBlocks)
            {
                if (RemoveBlockRecursive(block, target))
                    return;
            }
        }

        private static bool RemoveBlockRecursive(BlockInstance parent, BlockInstance target)
        {
            foreach (var param in parent.ParamBlocks.ToArray())
            {
                if (ReferenceEquals(param.Value, target))
                {
                    parent.ParamBlocks.Remove(param.Key);
                    return true;
                }

                if (RemoveBlockRecursive(param.Value, target))
                    return true;
            }

            return RemoveBlockFromList(parent.TrueBranch, target) ||
                   parent.TrueBranch.Any(child => RemoveBlockRecursive(child, target)) ||
                   RemoveBlockFromList(parent.FalseBranch, target) ||
                   parent.FalseBranch.Any(child => RemoveBlockRecursive(child, target));
        }

        private static bool RemoveBlockFromList(List<BlockInstance> list, BlockInstance target)
        {
            int index = list.IndexOf(target);
            if (index < 0)
                return false;

            list.RemoveAt(index);
            return true;
        }

        private void ParamCombo_Changed(object? sender, EventArgs e)
        {
            if (sender is not UIComboBox combo || combo.Tag is not ParamTag tag)
                return;

            string value = combo.SelectedItem?.ToString() ?? "";
            int spaceIndex = value.IndexOf(' ');
            tag.Block.ParamValues[tag.ParamName] = spaceIndex > 0 ? value[..spaceIndex] : value;
        }

        private void ParamText_Changed(object? sender, EventArgs e)
        {
            if (sender is not UITextBox textBox || textBox.Tag is not ParamTag tag)
                return;

            if (tag.IsNumber && !IsPartialNumber(textBox.Text))
            {
                textBox.ForeColor = ClrDanger;
                return;
            }

            textBox.ForeColor = ClrText;
            tag.Block.ParamValues[tag.ParamName] = textBox.Text;
        }

        private void ParamProjectile_Changed(object? sender, EventArgs e)
        {
            if (sender is not System.Windows.Forms.TextBox textBox || textBox.Tag is not ParamTag tag)
                return;

            tag.Block.ParamValues[tag.ParamName] = textBox.Text.Trim();
        }

        private static bool IsPartialNumber(string text)
        {
            return string.IsNullOrWhiteSpace(text) ||
                   text == "-" ||
                   decimal.TryParse(text, out _);
        }

        private static BlockInstance CloneBlock(BlockInstance source)
        {
            return new BlockInstance
            {
                Id = source.Id,
                BlockDefId = source.BlockDefId,
                CanvasX = source.CanvasX,
                CanvasY = source.CanvasY,
                ParamValues = new Dictionary<string, string>(source.ParamValues),
                ParamBlocks = source.ParamBlocks.ToDictionary(pair => pair.Key, pair => CloneBlock(pair.Value)),
                TrueBranch = source.TrueBranch.Select(CloneBlock).ToList(),
                FalseBranch = source.FalseBranch.Select(CloneBlock).ToList()
            };
        }

        private void UpdateSummary()
        {
            int total = CountBlocks(_topBlocks);
            int events = CountBlocks(_topBlocks, BlockCategory.Event);
            int conditions = CountBlocks(_topBlocks, BlockCategory.Condition);
            _summaryLabel.Text = $"{total} 个节点 / {events} 个事件 / {conditions} 个条件 / 缩放 {Math.Round(_canvasZoom * 100)}%";
        }

        private static int CountBlocks(IEnumerable<BlockInstance> blocks, BlockCategory? category = null)
        {
            int count = 0;
            foreach (var block in blocks)
            {
                var definition = BlockRegistry.Get(block.BlockDefId);
                if (category == null || definition?.Category == category)
                    count++;

                count += CountBlocks(block.ParamBlocks.Values, category);
                count += CountBlocks(block.TrueBranch, category);
                count += CountBlocks(block.FalseBranch, category);
            }

            return count;
        }

        private static bool ContainsBlock(BlockInstance root, BlockInstance target)
        {
            return root.ParamBlocks.Values.Any(child => ReferenceEquals(child, target) || ContainsBlock(child, target)) ||
                   root.TrueBranch.Any(child => ReferenceEquals(child, target) || ContainsBlock(child, target)) ||
                   root.FalseBranch.Any(child => ReferenceEquals(child, target) || ContainsBlock(child, target));
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
                BackColor = Color.Transparent
            };
        }

        private static UIComboBox CreateCombo(int x, int y, int width)
        {
            return new UIComboBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 28),
                Font = FontBody,
                Style = UIStyle.Black,
                FillColor = ClrInput,
                RectColor = ClrBorder,
                ForeColor = ClrText,
                BackColor = ClrInput
            };
        }

        private System.Windows.Forms.TextBox CreateProjectileTextBox(int x, int y, int width)
        {
            var textBox = new System.Windows.Forms.TextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 26),
                Font = FontBody,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.CustomSource,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = ClrInput,
                ForeColor = ClrText
            };

            var autoComplete = new AutoCompleteStringCollection();
            autoComplete.AddRange(_projectileReferences.ToArray());
            textBox.AutoCompleteCustomSource = autoComplete;
            return textBox;
        }

        private static UITextBox CreateTextBox(int x, int y, int width)
        {
            return new UITextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 28),
                Font = FontBody,
                Style = UIStyle.Black,
                FillColor = ClrInput,
                RectColor = ClrBorder,
                ForeColor = ClrText,
                BackColor = ClrInput
            };
        }

        private static void StyleButton(UIButton button, Color fill, Color border)
        {
            button.Style = UIStyle.Black;
            button.FillColor = fill;
            button.RectColor = border;
            button.ForeColor = Color.White;
        }

        private static string CategoryName(BlockCategory category) => category switch
        {
            BlockCategory.Event => "事件 Events",
            BlockCategory.Condition => "条件 Conditions",
            BlockCategory.Action => "动作 Actions",
            BlockCategory.Value => "数值 Values",
            _ => category.ToString()
        };

        private static string CategoryShortName(BlockCategory category) => category switch
        {
            BlockCategory.Event => "事件",
            BlockCategory.Condition => "条件",
            BlockCategory.Action => "动作",
            BlockCategory.Value => "数值",
            _ => category.ToString()
        };

        private static Color GetCategoryColor(BlockCategory category) => category switch
        {
            BlockCategory.Event => Color.FromArgb(241, 193, 70),
            BlockCategory.Condition => Color.FromArgb(238, 145, 57),
            BlockCategory.Action => Color.FromArgb(74, 166, 229),
            BlockCategory.Value => Color.FromArgb(160, 122, 235),
            _ => ClrMuted
        };

        private static readonly string[] TargetOptions =
        {
            "npc (被命中的 NPC)",
            "player (玩家)",
            "owner (使用者)",
            "all_npc (所有 NPC)",
            "all_player (所有玩家)"
        };

        private static readonly string[] ParticleOptions =
        {
            "15 Smoke",
            "6 Torch",
            "31 Fire",
            "43 Magic",
            "57 Spark",
            "58 Blue Spark",
            "59 Gold Flame",
            "60 Shadowflame",
            "62 Ice",
            "66 Cursed Flame",
            "68 Blood",
            "33 Water",
            "32 Sand",
            "3 Grass",
            "1 Dirt",
            "0 Stone",
            "7 Wood",
            "73 Rainbow",
            "74 Electric",
            "127 Solar",
            "229 Vortex",
            "242 Nebula",
            "269 Stardust"
        };

        private static void Header_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel) return;

            e.Graphics.Clear(ClrBg);
            using var bottom = new Pen(ClrSoftBorder);
            e.Graphics.DrawLine(bottom, 0, panel.Height - 1, panel.Width, panel.Height - 1);
            using var accent = new Pen(ClrAccent, 3);
            e.Graphics.DrawLine(accent, 0, panel.Height - 1, 170, panel.Height - 1);
        }

        private static void Panel_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel) return;

            using var border = new Pen(ClrBorder);
            e.Graphics.DrawRectangle(border, 0, 0, panel.Width - 1, panel.Height - 1);
        }

        private void Canvas_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel) return;

            e.Graphics.Clear(ClrCanvas);
            int grid = Math.Max(10, Zoom(CanvasGridSize));
            using var gridPen = new Pen(Color.FromArgb(22, ClrBorder));
            for (int x = 0; x < panel.Width; x += grid)
                e.Graphics.DrawLine(gridPen, x, 0, x, panel.Height);
            for (int y = 0; y < panel.Height; y += grid)
                e.Graphics.DrawLine(gridPen, 0, y, panel.Width, y);
            using var border = new Pen(ClrSoftBorder);
            e.Graphics.DrawRectangle(border, 0, 0, panel.Width - 1, panel.Height - 1);
        }

        private static void PaletteBlock_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel || panel.Tag is not BlockDefinition definition) return;

            using var border = new Pen(Color.FromArgb(34, GetCategoryColor(definition.Category)));
            e.Graphics.DrawRectangle(border, 0, 0, panel.Width - 1, panel.Height - 1);

            using var accent = new SolidBrush(GetCategoryColor(definition.Category));
            e.Graphics.FillRectangle(accent, 0, 0, 3, panel.Height);
        }

        private static void ValueSlot_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel) return;

            using var border = new Pen(Color.FromArgb(88, GetCategoryColor(BlockCategory.Value))) { DashStyle = DashStyle.Dash };
            e.Graphics.DrawRectangle(border, 0, 0, panel.Width - 1, panel.Height - 1);
        }

        private static void DrawAccentPanel(Graphics graphics, Rectangle bounds, Color accent, Color fill)
        {
            using var brush = new SolidBrush(fill);
            graphics.FillRectangle(brush, bounds);
            using var accentBrush = new SolidBrush(accent);
            graphics.FillRectangle(accentBrush, 0, 0, 4, bounds.Height);
            using var border = new Pen(ClrSoftBorder);
            graphics.DrawRectangle(border, 0, 0, bounds.Width - 1, bounds.Height - 1);
        }

        private static void DrawDashedPanel(Graphics graphics, Rectangle bounds)
        {
            using var border = new Pen(Color.FromArgb(80, ClrBorder)) { DashStyle = DashStyle.Dash };
            graphics.DrawRectangle(border, 0, 0, bounds.Width - 1, bounds.Height - 1);
        }

        private static void DrawBlockCard(Graphics graphics, Rectangle bounds, Color accent)
        {
            using var border = new Pen(Color.FromArgb(90, accent));
            graphics.DrawRectangle(border, 0, 0, bounds.Width - 1, bounds.Height - 1);
            using var left = new SolidBrush(accent);
            graphics.FillRectangle(left, 0, 0, 4, bounds.Height);
        }

        private static void DrawHeaderAccent(Graphics graphics, Rectangle bounds, Color accent)
        {
            using var glow = new LinearGradientBrush(bounds, Color.FromArgb(34, accent), Color.FromArgb(0, accent), LinearGradientMode.Horizontal);
            graphics.FillRectangle(glow, bounds);
            using var line = new Pen(Color.FromArgb(80, accent));
            graphics.DrawLine(line, 0, bounds.Height - 1, bounds.Width, bounds.Height - 1);
        }

        private static void DrawBranchHeader(Graphics graphics, Rectangle bounds, Color accent)
        {
            using var accentBrush = new SolidBrush(accent);
            graphics.FillRectangle(accentBrush, 0, 0, 4, bounds.Height);
            using var border = new Pen(Color.FromArgb(70, accent));
            graphics.DrawRectangle(border, 0, 0, bounds.Width - 1, bounds.Height - 1);
        }

        private class ParamTag
        {
            public BlockInstance Block { get; set; } = null!;
            public string ParamName { get; set; } = "";
            public bool IsNumber { get; set; }
        }

        private class ParamSlotTag
        {
            public BlockInstance Owner { get; set; } = null!;
            public BlockParam Param { get; set; } = null!;
        }

        private class BranchTag
        {
            public BlockInstance Block { get; set; } = null!;
            public bool IsTrue { get; set; }
            public bool IsEventBody { get; set; }
        }

        private class BlockDragData
        {
            public BlockDefinition? Definition { get; set; }
            public BlockInstance? ExistingBlock { get; set; }
            public Point DropOffset { get; set; } = new(24, 18);
        }

        private sealed class TopLevelBlockTag
        {
            public BlockInstance Block { get; set; } = null!;
        }

        private sealed class TopLevelDragState
        {
            public BlockInstance Block { get; set; } = null!;
            public Control Source { get; set; } = null!;
            public Control Container { get; set; } = null!;
            public Point Offset { get; set; }
            public Point OriginalLocation { get; set; }
            public bool Started { get; set; }
            public bool IsDetachedPreview { get; set; }
        }

        private class BranchMenuTag
        {
            public BranchTag Branch { get; set; } = null!;
            public BlockDefinition Definition { get; set; } = null!;
        }

        private class BufferedPanel : Panel
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

        private sealed class DarkScrollBar : Control
        {
            private int _minimum;
            private int _maximum = 100;
            private int _value;
            private int _largeChange = 100;
            private int _smallChange = 24;
            private bool _dragging;
            private int _dragOffset;

            public event EventHandler? ValueChanged;

            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public int Minimum
            {
                get => _minimum;
                set
                {
                    _minimum = value;
                    if (_maximum < _minimum)
                        _maximum = _minimum;
                    Value = _value;
                    Invalidate();
                }
            }

            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public int Maximum
            {
                get => _maximum;
                set
                {
                    _maximum = Math.Max(_minimum, value);
                    Value = _value;
                    Invalidate();
                }
            }

            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public int LargeChange
            {
                get => _largeChange;
                set
                {
                    _largeChange = Math.Max(1, value);
                    Value = _value;
                    Invalidate();
                }
            }

            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public int SmallChange
            {
                get => _smallChange;
                set => _smallChange = Math.Max(1, value);
            }

            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public int Value
            {
                get => _value;
                set
                {
                    int next = Math.Clamp(value, _minimum, EffectiveMaximum);
                    if (_value == next)
                        return;

                    _value = next;
                    Invalidate();
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }

            private int EffectiveMaximum => Math.Max(_minimum, _maximum - _largeChange + 1);

            public DarkScrollBar()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
                Width = 8;
                Cursor = Cursors.Hand;
                BackColor = Color.FromArgb(14, 20, 28);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                e.Graphics.Clear(Parent?.BackColor ?? BackColor);
                using var track = new SolidBrush(Color.FromArgb(18, 27, 38));
                e.Graphics.FillRectangle(track, ClientRectangle);

                Rectangle thumb = GetThumbRectangle();
                using var thumbBrush = new SolidBrush(_dragging ? Color.FromArgb(95, 121, 148) : Color.FromArgb(68, 90, 114));
                e.Graphics.FillRectangle(thumbBrush, thumb);
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);
                if (e.Button != MouseButtons.Left)
                    return;

                Rectangle thumb = GetThumbRectangle();
                if (thumb.Contains(e.Location))
                {
                    _dragging = true;
                    _dragOffset = e.Y - thumb.Top;
                    Capture = true;
                }
                else
                {
                    Value += e.Y < thumb.Top ? -LargeChange : LargeChange;
                }
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                if (!_dragging)
                    return;

                int thumbHeight = GetThumbRectangle().Height;
                int track = Math.Max(1, Height - thumbHeight - 4);
                int top = Math.Clamp(e.Y - _dragOffset - 2, 0, track);
                Value = _minimum + (int)Math.Round(top / (double)track * Math.Max(0, EffectiveMaximum - _minimum));
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);
                _dragging = false;
                Capture = false;
                Invalidate();
            }

            protected override void OnMouseWheel(MouseEventArgs e)
            {
                base.OnMouseWheel(e);
                Value += e.Delta > 0 ? -SmallChange * 3 : SmallChange * 3;
            }

            private Rectangle GetThumbRectangle()
            {
                int trackHeight = Math.Max(1, Height - 4);
                int range = Math.Max(1, _maximum - _minimum + 1);
                int thumbHeight = Math.Clamp((int)(trackHeight * (_largeChange / (float)Math.Max(_largeChange, range))), 34, trackHeight);
                int travel = Math.Max(1, trackHeight - thumbHeight);
                int top = EffectiveMaximum == _minimum
                    ? 2
                    : 2 + (int)(travel * ((_value - _minimum) / (float)(EffectiveMaximum - _minimum)));
                return new Rectangle(2, top, Math.Max(2, Width - 4), thumbHeight);
            }
        }
    }
}
