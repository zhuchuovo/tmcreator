using Sunny.UI;
using tmcreator.Models;

namespace tmcreator
{
    public partial class RecipeEditorForm : UIForm
    {
        private readonly RecipeData _recipe;
        private readonly List<VanillaItemInfo> _allItems;
        private readonly DarkItemListBox<VanillaItemInfo> _lstSearch = new(item => item.ToString());
        private readonly DarkItemListBox<RecipeIngredientData> _lstIngredients = new(item => item.ToString());
        private readonly UITextBox _txtSearch = new();
        private readonly NumericUpDown _numStack = new();
        private readonly UIComboBox _cmbStation = new();
        private readonly PictureBox _picPreview = new();
        private readonly Label _lblSelected = new();

        private static readonly Color ClrBg = Color.FromArgb(14, 20, 28);
        private static readonly Color ClrPanel = Color.FromArgb(22, 31, 43);
        private static readonly Color ClrCard = Color.FromArgb(28, 39, 54);
        private static readonly Color ClrInput = Color.FromArgb(12, 18, 27);
        private static readonly Color ClrBorder = Color.FromArgb(60, 78, 100);
        private static readonly Color ClrText = Color.FromArgb(238, 244, 252);
        private static readonly Color ClrSubText = Color.FromArgb(154, 171, 193);
        private static readonly Color ClrAccent = Color.FromArgb(53, 199, 183);
        private static readonly Font FontTitle = new("Microsoft YaHei UI", 15F, FontStyle.Bold);
        private static readonly Font FontBody = new("Microsoft YaHei UI", 9F);
        private static readonly Font FontBodyBold = new("Microsoft YaHei UI", 9F, FontStyle.Bold);

        public RecipeEditorForm(RecipeData recipe)
        {
            _recipe = CloneRecipe(recipe);
            _allItems = VanillaItemIndex.LoadItems();

            Text = "合成配方编辑";
            ClientSize = new Size(920, 680);
            MinimumSize = new Size(920, 680);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = ClrBg;
            Style = UIStyle.Black;

            BuildLayout();
            RefreshSearch();
            RefreshIngredients();
        }

        public RecipeData Recipe => CloneRecipe(_recipe);

        private void BuildLayout()
        {
            var title = CreateLabel("合成配方", 24, 32, 200, 30, FontTitle, ClrText);
            var hint = CreateLabel("搜索原版物品 ID 或文件名，设置数量，再加入材料列表。", 26, 68, 620, 24, FontBody, ClrSubText);
            Controls.Add(title);
            Controls.Add(hint);

            var left = CreatePanel(24, 112, 426, 490);
            var right = CreatePanel(470, 112, 426, 490);
            Controls.Add(left);
            Controls.Add(right);

            left.Controls.Add(CreateLabel("原版物品库", 18, 16, 150, 24, FontBodyBold, ClrText));
            _txtSearch.SetBounds(18, 48, 270, 28);
            _txtSearch.Font = FontBody;
            StyleTextBox(_txtSearch);
            _txtSearch.TextChanged += (s, e) => RefreshSearch();
            left.Controls.Add(_txtSearch);

            _numStack.SetBounds(302, 48, 88, 28);
            _numStack.Minimum = 1;
            _numStack.Maximum = 9999;
            _numStack.Value = 1;
            _numStack.BackColor = ClrInput;
            _numStack.ForeColor = ClrText;
            _numStack.Font = FontBody;
            _numStack.TextAlign = HorizontalAlignment.Center;
            StyleNumericUpDown(_numStack);
            _numStack.ValueChanged += (s, e) => UpdatePreview();
            left.Controls.Add(_numStack);

            _lstSearch.SetBounds(18, 88, 372, 322);
            StyleList(_lstSearch);
            _lstSearch.SelectedItemChanged += (s, e) => UpdatePreview();
            _lstSearch.ItemDoubleClick += (s, e) => AddSelectedIngredient();
            left.Controls.Add(_lstSearch);

            _picPreview.SetBounds(18, 424, 28, 28);
            _picPreview.BackColor = ClrInput;
            _picPreview.SizeMode = PictureBoxSizeMode.Zoom;
            left.Controls.Add(_picPreview);

            _lblSelected.SetBounds(56, 427, 222, 24);
            _lblSelected.Font = FontBody;
            _lblSelected.ForeColor = ClrSubText;
            left.Controls.Add(_lblSelected);

            var btnAdd = CreateButton("加入材料", 292, 422, 98, 32, ClrAccent);
            btnAdd.Click += (s, e) => AddSelectedIngredient();
            left.Controls.Add(btnAdd);

            right.Controls.Add(CreateLabel("已选材料", 18, 16, 150, 24, FontBodyBold, ClrText));
            _lstIngredients.SetBounds(18, 48, 370, 286);
            StyleList(_lstIngredients);
            right.Controls.Add(_lstIngredients);

            var btnRemove = CreateButton("移除材料", 18, 348, 98, 32, Color.FromArgb(237, 94, 104));
            btnRemove.Click += (s, e) => RemoveSelectedIngredient();
            right.Controls.Add(btnRemove);

            right.Controls.Add(CreateLabel("制作站", 18, 400, 70, 24, FontBodyBold, ClrSubText));
            _cmbStation.SetBounds(118, 396, 250, 30);
            _cmbStation.DropDownStyle = UIDropDownStyle.DropDownList;
            _cmbStation.Font = FontBody;
            StyleComboBox(_cmbStation);
            _cmbStation.Items.AddRange(RecipeStationRegistry.Stations.Cast<object>().ToArray());
            int selectedStation = Math.Max(0, RecipeStationRegistry.Stations.FindIndex(station => station.Key == _recipe.CraftingStationKey));
            _cmbStation.SelectedIndex = selectedStation;
            right.Controls.Add(_cmbStation);

            var btnOk = CreateButton("保存配方", 668, 628, 110, 34, ClrAccent);
            btnOk.Click += (s, e) =>
            {
                if (_cmbStation.SelectedItem is RecipeStationInfo station)
                {
                    _recipe.CraftingStationKey = station.Key;
                    _recipe.CraftingStationDisplay = station.DisplayName;
                }

                _recipe.Enabled = _recipe.Ingredients.Count > 0;
                DialogResult = DialogResult.OK;
                Close();
            };
            Controls.Add(btnOk);

            var btnCancel = CreateButton("取消", 790, 628, 86, 34, Color.FromArgb(67, 83, 105));
            btnCancel.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };
            Controls.Add(btnCancel);
        }

        private void RefreshSearch()
        {
            string query = _txtSearch.Text.Trim();
            IEnumerable<VanillaItemInfo> items = _allItems;
            if (!string.IsNullOrWhiteSpace(query))
            {
                items = items.Where(item =>
                    item.Id.ToString().Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    item.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    item.FileName.Contains(query, StringComparison.OrdinalIgnoreCase));
            }

            _lstSearch.SetItems(items);
            if (_lstSearch.Count > 0)
                _lstSearch.SelectedIndex = 0;
        }

        private void UpdatePreview()
        {
            _picPreview.Image?.Dispose();
            _picPreview.Image = null;

            if (_lstSearch.SelectedItem is not VanillaItemInfo item)
            {
                _lblSelected.Text = "未选择物品";
                return;
            }

            _lblSelected.Text = $"{item.DisplayName} x{_numStack.Value}";
            try
            {
                using var image = Image.FromFile(item.ImagePath);
                _picPreview.Image = new Bitmap(image);
            }
            catch
            {
                _picPreview.Image = null;
            }
        }

        private void AddSelectedIngredient()
        {
            if (_lstSearch.SelectedItem is not VanillaItemInfo item)
                return;

            var existing = _recipe.Ingredients.FirstOrDefault(ingredient => ingredient.ItemId == item.Id);
            if (existing == null)
            {
                _recipe.Ingredients.Add(new RecipeIngredientData
                {
                    ItemId = item.Id,
                    DisplayName = item.DisplayName,
                    Stack = (int)_numStack.Value
                });
            }
            else
            {
                existing.Stack += (int)_numStack.Value;
            }

            _recipe.Enabled = _recipe.Ingredients.Count > 0;
            RefreshIngredients();
        }

        private void RemoveSelectedIngredient()
        {
            if (_lstIngredients.SelectedItem is not RecipeIngredientData ingredient)
                return;

            _recipe.Ingredients.Remove(ingredient);
            _recipe.Enabled = _recipe.Ingredients.Count > 0;
            RefreshIngredients();
        }

        private void RefreshIngredients()
        {
            _lstIngredients.SetItems(_recipe.Ingredients);
        }

        private static RecipeData CloneRecipe(RecipeData source)
        {
            return new RecipeData
            {
                Enabled = source.Enabled,
                CraftingStationKey = source.CraftingStationKey,
                CraftingStationDisplay = source.CraftingStationDisplay,
                Ingredients = source.Ingredients.Select(item => new RecipeIngredientData
                {
                    ItemId = item.ItemId,
                    DisplayName = item.DisplayName,
                    Stack = item.Stack
                }).ToList()
            };
        }

        private static Panel CreatePanel(int x, int y, int width, int height)
        {
            return new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = ClrPanel,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private static Label CreateLabel(string text, int x, int y, int width, int height, Font font, Color color)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                Font = font,
                ForeColor = color,
                BackColor = Color.Transparent
            };
        }

        private static UIButton CreateButton(string text, int x, int y, int width, int height, Color fill)
        {
            return new UIButton
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                Font = FontBodyBold,
                Style = UIStyle.Black,
                FillColor = fill,
                RectColor = ClrBorder,
                ForeColor = Color.White
            };
        }

        private static void StyleTextBox(UITextBox textBox)
        {
            textBox.Style = UIStyle.Black;
            textBox.FillColor = ClrInput;
            textBox.RectColor = ClrBorder;
            textBox.ForeColor = ClrText;
            textBox.BackColor = ClrInput;
        }

        private static void StyleComboBox(UIComboBox comboBox)
        {
            comboBox.Style = UIStyle.Black;
            comboBox.FillColor = ClrInput;
            comboBox.RectColor = ClrBorder;
            comboBox.ForeColor = ClrText;
            comboBox.BackColor = ClrInput;
        }

        private static void StyleList<T>(DarkItemListBox<T> listBox)
        {
            listBox.BackColor = ClrInput;
            listBox.ForeColor = ClrText;
            listBox.Font = FontBody;
            listBox.BorderColor = ClrBorder;
            listBox.SelectionColor = Color.FromArgb(45, 84, 92);
            listBox.ScrollTrackColor = Color.FromArgb(13, 21, 31);
            listBox.ScrollThumbColor = Color.FromArgb(73, 105, 132);
            listBox.ScrollThumbHoverColor = Color.FromArgb(87, 129, 157);
        }

        private static void StyleNumericUpDown(NumericUpDown number)
        {
            HideNativeSpinButtons(number);
            number.HandleCreated += (s, e) => HideNativeSpinButtons(number);
            number.Resize += (s, e) => HideNativeSpinButtons(number);
        }

        private static void HideNativeSpinButtons(NumericUpDown number)
        {
            foreach (Control child in number.Controls)
            {
                if (child.GetType().Name.Contains("UpDownButtons", StringComparison.OrdinalIgnoreCase))
                {
                    child.Visible = false;
                    child.Width = 0;
                }
            }
        }
    }

    internal sealed class DarkItemListBox<T> : Control
    {
        private readonly Func<T, string> _formatter;
        private readonly List<T> _items = new();
        private int _dragStartOffset;
        private int _dragStartY;
        private bool _draggingThumb;
        private int _scrollOffset;
        private int _selectedIndex = -1;

        private const int ScrollbarWidth = 12;

        public DarkItemListBox(Func<T, string> formatter)
        {
            _formatter = formatter;
            DoubleBuffered = true;
            TabStop = true;
            ItemHeight = 24;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.Selectable |
                ControlStyles.UserPaint,
                true);
        }

        public event EventHandler? ItemDoubleClick;
        public event EventHandler? SelectedItemChanged;

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Color BorderColor { get; set; } = Color.FromArgb(60, 78, 100);
        public int Count => _items.Count;

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int ItemHeight { get; set; }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Color ScrollThumbColor { get; set; } = Color.FromArgb(69, 92, 116);

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Color ScrollThumbHoverColor { get; set; } = Color.FromArgb(53, 199, 183);

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Color ScrollTrackColor { get; set; } = Color.FromArgb(18, 27, 39);

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Color SelectionColor { get; set; } = Color.FromArgb(45, 84, 92);

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex
        {
            get => _selectedIndex;
            set => SelectIndex(value);
        }

        public T? SelectedItem => _selectedIndex >= 0 && _selectedIndex < _items.Count
            ? _items[_selectedIndex]
            : default;

        private int MaxScrollOffset => Math.Max(0, _items.Count - VisibleRowCount);
        private int VisibleRowCount => Math.Max(1, ClientSize.Height / Math.Max(1, ItemHeight));

        public void SetItems(IEnumerable<T> items)
        {
            object? previousSelection = SelectedItem;

            _items.Clear();
            _items.AddRange(items);

            if (_items.Count == 0)
                _selectedIndex = -1;
            else if (_selectedIndex < 0 || _selectedIndex >= _items.Count)
                _selectedIndex = 0;

            SetScrollOffset(_scrollOffset);
            EnsureSelectedVisible();
            Invalidate();

            if (!Equals(previousSelection, SelectedItem))
                SelectedItemChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return keyData is Keys.Up or Keys.Down or Keys.PageUp or Keys.PageDown or Keys.Home or Keys.End
                || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (_items.Count == 0)
                return;

            int target = _selectedIndex;
            switch (e.KeyCode)
            {
                case Keys.Up:
                    target--;
                    break;
                case Keys.Down:
                    target++;
                    break;
                case Keys.PageUp:
                    target -= VisibleRowCount;
                    break;
                case Keys.PageDown:
                    target += VisibleRowCount;
                    break;
                case Keys.Home:
                    target = 0;
                    break;
                case Keys.End:
                    target = _items.Count - 1;
                    break;
                default:
                    return;
            }

            SelectIndex(target);
            e.Handled = true;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left)
                return;

            Focus();

            if (IsScrollbarHit(e.Location))
            {
                var thumb = GetThumbRectangle();
                if (thumb.Contains(e.Location))
                {
                    _draggingThumb = true;
                    _dragStartY = e.Y;
                    _dragStartOffset = _scrollOffset;
                    Capture = true;
                }
                else
                {
                    SetScrollOffset(_scrollOffset + (e.Y < thumb.Y ? -VisibleRowCount : VisibleRowCount));
                }

                return;
            }

            int index = _scrollOffset + e.Y / Math.Max(1, ItemHeight);
            if (index >= 0 && index < _items.Count)
                SelectIndex(index);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (e.Button == MouseButtons.Left && !IsScrollbarHit(e.Location) && _selectedIndex >= 0)
                ItemDoubleClick?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!_draggingThumb)
                return;

            var track = GetTrackRectangle();
            var thumb = GetThumbRectangle();
            int travel = Math.Max(1, track.Height - thumb.Height);
            int deltaRows = (int)Math.Round((e.Y - _dragStartY) * MaxScrollOffset / (double)travel);
            SetScrollOffset(_dragStartOffset + deltaRows);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button != MouseButtons.Left)
                return;

            _draggingThumb = false;
            Capture = false;
            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            SetScrollOffset(_scrollOffset - Math.Sign(e.Delta) * 3);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.Clear(BackColor);

            int listWidth = Math.Max(0, ClientSize.Width - ScrollbarWidth - 1);
            int visibleRows = Math.Min(VisibleRowCount + 1, Math.Max(0, _items.Count - _scrollOffset));

            using var selectionBrush = new SolidBrush(SelectionColor);
            using var separatorPen = new Pen(Color.FromArgb(26, BorderColor));

            for (int row = 0; row < visibleRows; row++)
            {
                int itemIndex = _scrollOffset + row;
                var rowBounds = new Rectangle(1, row * ItemHeight, listWidth, ItemHeight);

                if (itemIndex == _selectedIndex)
                    e.Graphics.FillRectangle(selectionBrush, rowBounds);

                var textBounds = new Rectangle(rowBounds.X + 8, rowBounds.Y, Math.Max(0, rowBounds.Width - 12), rowBounds.Height);
                TextRenderer.DrawText(
                    e.Graphics,
                    _formatter(_items[itemIndex]),
                    Font,
                    textBounds,
                    ForeColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

                e.Graphics.DrawLine(separatorPen, rowBounds.Left + 6, rowBounds.Bottom - 1, rowBounds.Right - 6, rowBounds.Bottom - 1);
            }

            DrawScrollbar(e.Graphics);

            using var borderPen = new Pen(BorderColor);
            e.Graphics.DrawRectangle(borderPen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            SetScrollOffset(_scrollOffset);
        }

        private void DrawScrollbar(Graphics graphics)
        {
            var track = GetTrackRectangle();
            using var trackBrush = new SolidBrush(ScrollTrackColor);
            graphics.FillRectangle(trackBrush, track);
            using var borderPen = new Pen(Color.FromArgb(35, 55, 77));
            graphics.DrawLine(borderPen, track.Left, track.Top, track.Left, track.Bottom);

            var thumb = GetThumbRectangle();
            if (thumb.IsEmpty)
                return;

            using var thumbBrush = new SolidBrush(_draggingThumb ? ScrollThumbHoverColor : ScrollThumbColor);
            graphics.FillRectangle(thumbBrush, thumb);
        }

        private void EnsureSelectedVisible()
        {
            if (_selectedIndex < 0)
                return;

            if (_selectedIndex < _scrollOffset)
                SetScrollOffset(_selectedIndex);
            else if (_selectedIndex >= _scrollOffset + VisibleRowCount)
                SetScrollOffset(_selectedIndex - VisibleRowCount + 1);
        }

        private Rectangle GetThumbRectangle()
        {
            if (MaxScrollOffset == 0)
                return Rectangle.Empty;

            var track = GetTrackRectangle();
            int thumbHeight = Math.Max(28, (int)Math.Round(track.Height * (VisibleRowCount / (double)Math.Max(1, _items.Count))));
            thumbHeight = Math.Min(track.Height, thumbHeight);
            int travel = Math.Max(1, track.Height - thumbHeight);
            int thumbY = track.Y + (int)Math.Round(travel * (_scrollOffset / (double)MaxScrollOffset));
            int thumbWidth = Math.Max(4, Math.Min(7, track.Width - 5));
            return new Rectangle(track.X + Math.Max(2, (track.Width - thumbWidth) / 2), thumbY + 2, thumbWidth, Math.Max(4, thumbHeight - 4));
        }

        private Rectangle GetTrackRectangle()
        {
            return new Rectangle(Math.Max(0, ClientSize.Width - ScrollbarWidth), 1, ScrollbarWidth - 1, Math.Max(0, ClientSize.Height - 2));
        }

        private bool IsScrollbarHit(Point location)
        {
            return location.X >= ClientSize.Width - ScrollbarWidth && MaxScrollOffset > 0;
        }

        private void SelectIndex(int index)
        {
            int target = _items.Count == 0 ? -1 : Math.Clamp(index, 0, _items.Count - 1);
            if (_selectedIndex == target)
                return;

            _selectedIndex = target;
            EnsureSelectedVisible();
            Invalidate();
            SelectedItemChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SetScrollOffset(int offset)
        {
            int clamped = Math.Clamp(offset, 0, MaxScrollOffset);
            if (_scrollOffset == clamped)
                return;

            _scrollOffset = clamped;
            Invalidate();
        }
    }

    public class VanillaItemInfo
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = "";
        public string FileName { get; set; } = "";
        public string ImagePath { get; set; } = "";

        public override string ToString() => $"{Id} - {DisplayName}";
    }

    public static class VanillaItemIndex
    {
        public static List<VanillaItemInfo> LoadItems()
        {
            string? root = FindImageRoot();
            if (root == null)
                return new List<VanillaItemInfo>();

            return Directory.GetFiles(root, "Item_*.png", SearchOption.TopDirectoryOnly)
                .Select(path => new { Path = path, Match = System.Text.RegularExpressions.Regex.Match(Path.GetFileNameWithoutExtension(path), @"^Item_(\d+)$") })
                .Where(item => item.Match.Success)
                .Select(item =>
                {
                    int id = int.Parse(item.Match.Groups[1].Value);
                    return new VanillaItemInfo
                    {
                        Id = id,
                        DisplayName = GetKnownItemName(id),
                        FileName = Path.GetFileName(item.Path),
                        ImagePath = item.Path
                    };
                })
                .OrderBy(item => item.Id)
                .ToList();
        }

        private static string? FindImageRoot()
        {
            foreach (string start in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
            {
                var dir = new DirectoryInfo(start);
                while (dir != null)
                {
                    string candidate = Path.Combine(dir.FullName, "Terraria-Images-1.4.4.9", "Terraria_Images_1.4.4.9");
                    if (Directory.Exists(candidate))
                        return candidate;
                    dir = dir.Parent;
                }
            }

            return null;
        }

        private static string GetKnownItemName(int id) => id switch
        {
            8 => "火把 Torch",
            9 => "木材 Wood",
            22 => "铁锭 Iron Bar",
            23 => "凝胶 Gel",
            30 => "木剑 Wooden Sword",
            31 => "木锤 Wooden Hammer",
            32 => "木弓 Wooden Bow",
            41 => "铜锭 Copper Bar",
            42 => "银锭 Silver Bar",
            43 => "金锭 Gold Bar",
            44 => "恶魔弓 Demon Bow",
            57 => "陨石锭 Meteorite Bar",
            60 => "魔矿锭 Demonite Bar",
            75 => "坠落之星 Fallen Star",
            86 => "暗影鳞片 Shadow Scale",
            88 => "火枪 Musket",
            89 => "火枪子弹 Musket Ball",
            105 => "火焰花 Fireblossom",
            166 => "地狱熔炉 Hellforge",
            175 => "丝绸 Silk",
            178 => "玻璃 Glass",
            179 => "太阳花 Daybloom",
            180 => "月光草 Moonglow",
            181 => "闪耀根 Blinkroot",
            182 => "死亡草 Deathweed",
            183 => "水叶草 Waterleaf",
            193 => "黑曜石 Obsidian",
            231 => "非法枪械部件 Illegal Gun Parts",
            259 => "鲨鱼鳍 Shark Fin",
            331 => "晶状体 Lens",
            345 => "铁砧 Iron Anvil",
            346 => "铜短剑 Copper Shortsword",
            350 => "生命水晶 Life Crystal",
            509 => "骨头 Bone",
            520 => "羽毛 Feather",
            521 => "墓碑 Tombstone",
            549 => "魔法镜 Magic Mirror",
            703 => "晶状体 Black Lens",
            _ => $"Item_{id}"
        };
    }

    public class RecipeStationInfo
    {
        public string Key { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string TileIdExpression { get; set; } = "";

        public override string ToString() => DisplayName;
    }

    public static class RecipeStationRegistry
    {
        public static List<RecipeStationInfo> Stations { get; } = new()
        {
            new() { Key = "", DisplayName = "徒手 / 不需要制作站", TileIdExpression = "" },
            new() { Key = "WorkBenches", DisplayName = "工作台", TileIdExpression = "TileID.WorkBenches" },
            new() { Key = "Anvils", DisplayName = "铁砧 / 铅砧", TileIdExpression = "TileID.Anvils" },
            new() { Key = "Furnaces", DisplayName = "熔炉", TileIdExpression = "TileID.Furnaces" },
            new() { Key = "Hellforge", DisplayName = "地狱熔炉", TileIdExpression = "TileID.Hellforge" },
            new() { Key = "DemonAltar", DisplayName = "恶魔祭坛 / 猩红祭坛", TileIdExpression = "TileID.DemonAltar" },
            new() { Key = "MythrilAnvil", DisplayName = "秘银砧 / 山铜砧", TileIdExpression = "TileID.MythrilAnvil" },
            new() { Key = "AdamantiteForge", DisplayName = "精金熔炉 / 钛金熔炉", TileIdExpression = "TileID.AdamantiteForge" },
            new() { Key = "TinkerersWorkbench", DisplayName = "工匠作坊", TileIdExpression = "TileID.TinkerersWorkbench" },
            new() { Key = "AlchemyTable", DisplayName = "炼药桌", TileIdExpression = "TileID.AlchemyTable" },
            new() { Key = "Bookcases", DisplayName = "书架", TileIdExpression = "TileID.Bookcases" },
            new() { Key = "CookingPots", DisplayName = "烹饪锅", TileIdExpression = "TileID.CookingPots" },
            new() { Key = "Loom", DisplayName = "织布机", TileIdExpression = "TileID.Loom" },
            new() { Key = "Sawmill", DisplayName = "锯木机", TileIdExpression = "TileID.Sawmill" },
            new() { Key = "HeavyWorkBench", DisplayName = "重型工作台", TileIdExpression = "TileID.HeavyWorkBench" },
            new() { Key = "CrystalBall", DisplayName = "水晶球", TileIdExpression = "TileID.CrystalBall" },
            new() { Key = "ImbuingStation", DisplayName = "灌注站", TileIdExpression = "TileID.ImbuingStation" },
        };

        public static RecipeStationInfo? Find(string key) => Stations.FirstOrDefault(station => station.Key == key);
    }
}
