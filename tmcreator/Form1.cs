using Sunny.UI;
using System.Drawing.Drawing2D;
using System.Text.Json;
using tmcreator.Models;
using tmcreator.Models.Flow;

namespace tmcreator
{
    public partial class Form1 : UIForm
    {
        private readonly List<ModItemData> _items = new();
        private ModItemData? _editingItem;

        private Panel? _headerPanel;
        private Label? _projectStatusLabel;
        private FlowLayoutPanel? _projectToolbar;
        private Panel? _formViewport;
        private FlowLayoutPanel? _formStack;
        private DarkVerticalScrollBar? _formScrollBar;
        private int _formScrollOffset;
        private Panel? _identitySection;
        private Panel? _textureSection;
        private Panel? _basicSection;
        private Panel? _toolSection;
        private Panel? _combatSection;
        private Panel? _blockSection;
        private Panel? _accessorySection;
        private Panel? _buffSection;
        private Panel? _recipeSection;
        private Label? _rightSubtitleLabel;
        private Label? _emptyStateLabel;
        private readonly UIComboBox cmbDamageKind = new();
        private readonly UIComboBox cmbBuffIconSource = new();
        private readonly UICheckBox chkUsesProjectile = new();
        private readonly UICheckBox chkConsumeOnUse = new();
        private readonly UICheckBox chkMultiFrameTexture = new();
        private readonly NumericUpDown numProjectileId = new();
        private readonly NumericUpDown numProjectileSpeed = new();
        private readonly NumericUpDown numVanillaBuffIconId = new();
        private readonly NumericUpDown numUseStyleId = new();
        private readonly NumericUpDown numTextureFrameCount = new();
        private readonly NumericUpDown numAccessoryMeleeDamage = new();
        private readonly NumericUpDown numAccessoryMagicDamage = new();
        private readonly NumericUpDown numAccessoryRangedDamage = new();
        private readonly NumericUpDown numAccessorySummonDamage = new();
        private readonly NumericUpDown numAccessoryMeleeSpeed = new();
        private readonly NumericUpDown numAccessoryMagicSpeed = new();
        private readonly NumericUpDown numAccessoryRangedSpeed = new();
        private readonly NumericUpDown numAccessorySummonSpeed = new();
        private readonly NumericUpDown numAccessoryMeleeCrit = new();
        private readonly NumericUpDown numAccessoryMagicCrit = new();
        private readonly NumericUpDown numAccessoryRangedCrit = new();
        private readonly NumericUpDown numAccessorySummonCrit = new();
        private readonly NumericUpDown numAccessoryDefense = new();
        private readonly NumericUpDown numAccessoryDamageReduction = new();
        private readonly UIButton btnEditRecipe = new();
        private readonly Label lblRecipeSummary = new();
        private readonly UIButton btnNewProject = new();
        private readonly UIButton btnOpenProject = new();
        private readonly UIButton btnSaveProject = new();
        private readonly UIButton btnRenameProject = new();
        private RecipeData _currentRecipe = new();
        private string _projectName = "未命名工程";
        private string? _projectFilePath;

        private static readonly JsonSerializerOptions ProjectJsonOptions = new()
        {
            WriteIndented = true
        };

        private static readonly HashSet<string> BuffFlowEventIds = new()
        {
            "buff_on_gain",
            "buff_update",
            "buff_on_end"
        };

        private static readonly HashSet<string> ProjectileFlowEventIds = new()
        {
            "projectile_on_spawn",
            "projectile_update",
            "projectile_on_hit_npc",
            "projectile_on_hit_player"
        };

        private static readonly HashSet<string> AccessoryFlowEventIds = new()
        {
            "accessory_wearing",
            "accessory_attack",
            "accessory_unequip",
            "accessory_equip"
        };

        private static readonly Color ClrBg = Color.FromArgb(14, 20, 28);
        private static readonly Color ClrPanelBg = Color.FromArgb(22, 31, 43);
        private static readonly Color ClrSectionBg = Color.FromArgb(28, 39, 54);
        private static readonly Color ClrSectionAlt = Color.FromArgb(20, 28, 38);
        private static readonly Color ClrInputBg = Color.FromArgb(12, 18, 27);
        private static readonly Color ClrBorder = Color.FromArgb(60, 78, 100);
        private static readonly Color ClrSoftBorder = Color.FromArgb(42, 57, 75);
        private static readonly Color ClrText = Color.FromArgb(238, 244, 252);
        private static readonly Color ClrSubText = Color.FromArgb(154, 171, 193);
        private static readonly Color ClrMuted = Color.FromArgb(104, 123, 146);
        private static readonly Color ClrAccent = Color.FromArgb(53, 199, 183);
        private static readonly Color ClrAccent2 = Color.FromArgb(247, 180, 64);
        private static readonly Color ClrDanger = Color.FromArgb(237, 94, 104);
        private static readonly Color ClrCardBg = Color.FromArgb(25, 35, 49);

        private static readonly Font FontTitle = new("Microsoft YaHei UI", 18F, FontStyle.Bold, GraphicsUnit.Point);
        private static readonly Font FontSectionTitle = new("Microsoft YaHei UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
        private static readonly Font FontBody = new("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        private static readonly Font FontBodyBold = new("Microsoft YaHei UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        private static readonly Font FontSmall = new("Microsoft YaHei UI", 8F, FontStyle.Regular, GraphicsUnit.Point);

        public Form1()
        {
            InitializeComponent();
            BuildModernLayout();
            UpdateFieldVisibility();
        }

        private string _selectedTexturePath = string.Empty;

        private void BuildModernLayout()
        {
            SuspendLayout();

            Text = string.Empty;
            ShowIcon = false;
            ClientSize = new Size(1280, 780);
            MinimumSize = new Size(1080, 680);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = ClrBg;
            Style = UIStyle.Black;

            Controls.Clear();

            _headerPanel = new Panel
            {
                BackColor = ClrBg,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            _headerPanel.Paint += HeaderPanel_Paint;
            BuildHeader();

            BuildLeftPanel();
            BuildRightPanel();

            Controls.Add(_headerPanel);
            Controls.Add(pnlLeft);
            Controls.Add(pnlRight);

            Resize -= Form1_Resize;
            Resize += Form1_Resize;
            LayoutShell();

            ResumeLayout(false);
        }

        private void BuildHeader()
        {
            if (_headerPanel == null) return;

            _headerPanel.Controls.Clear();

            var title = new Label
            {
                Text = "Terraria Mod 制作器",
                AutoSize = false,
                Location = new Point(26, 14),
                Size = new Size(360, 34),
                Font = FontTitle,
                ForeColor = ClrText,
                BackColor = Color.Transparent
            };

            var subtitle = new Label
            {
                Text = "",
                AutoSize = false,
                Location = new Point(28, 54),
                Size = new Size(520, 22),
                Font = FontBody,
                ForeColor = ClrSubText,
                BackColor = Color.Transparent
            };
            _projectStatusLabel = subtitle;

            _headerPanel.Controls.Add(title);
            _headerPanel.Controls.Add(subtitle);

            _projectToolbar = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = false,
                BackColor = Color.Transparent,
                Padding = new Padding(0),
                Margin = new Padding(0),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            ConfigureHeaderButton(btnNewProject, "创建文件", NewProject_Click);
            ConfigureHeaderButton(btnOpenProject, "打开工程", OpenProject_Click);
            ConfigureHeaderButton(btnSaveProject, "保存工程", SaveProject_Click);
            ConfigureHeaderButton(btnRenameProject, "修改名称", RenameProject_Click);

            _projectToolbar.Controls.Add(btnNewProject);
            _projectToolbar.Controls.Add(btnOpenProject);
            _projectToolbar.Controls.Add(btnSaveProject);
            _projectToolbar.Controls.Add(btnRenameProject);
            _headerPanel.Controls.Add(_projectToolbar);

            UpdateProjectStatus();
        }

        private void BuildLeftPanel()
        {
            pnlLeft.SuspendLayout();
            pnlLeft.Controls.Clear();
            pnlLeft.Text = "";
            pnlLeft.Style = UIStyle.Black;
            pnlLeft.FillColor = ClrPanelBg;
            pnlLeft.RectColor = ClrBorder;
            pnlLeft.BackColor = ClrPanelBg;

            var title = CreatePlainLabel("创建物品", new Point(24, 18), new Size(180, 26), FontSectionTitle, ClrText);
            var subtitle = CreatePlainLabel("先填核心信息，再按类型展开专属属性。", new Point(24, 44), new Size(330, 22), FontBody, ClrSubText);
            pnlLeft.Controls.Add(title);
            pnlLeft.Controls.Add(subtitle);

            _formViewport = new Panel
            {
                BackColor = ClrPanelBg,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            _formViewport.MouseWheel += FormScroller_MouseWheel;
            pnlLeft.Controls.Add(_formViewport);

            _formStack = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = false,
                AutoSize = false,
                BackColor = ClrPanelBg,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            _formStack.MouseWheel += FormScroller_MouseWheel;
            _formViewport.Controls.Add(_formStack);

            _formScrollBar = new DarkVerticalScrollBar
            {
                TrackColor = Color.FromArgb(11, 18, 27),
                ThumbColor = Color.FromArgb(78, 112, 142),
                ThumbHoverColor = Color.FromArgb(99, 146, 177),
                BorderColor = Color.FromArgb(38, 62, 86),
                BackColor = ClrPanelBg
            };
            _formScrollBar.ValueChanged += FormScrollBar_ValueChanged;
            pnlLeft.Controls.Add(_formScrollBar);

            BuildIdentitySection();
            BuildTextureSection();
            BuildBasicSection();
            BuildToolSection();
            BuildCombatSection();
            BuildAccessorySection();
            BuildBlockSection();
            BuildBuffSection();
            BuildRecipeSection();
            HookFormMouseWheel(_formStack);

            btnCreate.Text = "创建物品";
            btnCreate.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
            StyleButton(btnCreate, ClrAccent, Color.FromArgb(35, 144, 134));
            btnCreate.Click -= btnCreate_Click;
            btnCreate.Click -= CreateItemModern_Click;
            btnCreate.Click += CreateItemModern_Click;
            pnlLeft.Controls.Add(btnCreate);

            pnlLeft.ResumeLayout(false);
        }

        private void BuildIdentitySection()
        {
            _identitySection = CreateSection("基础身份", "分类、内部名称与展示文案", 202);

            AddFieldLabel(_identitySection, "类型", 18, 55, 80);
            cmbItemType.Location = new Point(112, 49);
            cmbItemType.Size = new Size(230, 30);
            cmbItemType.Font = FontBody;
            cmbItemType.Items.Clear();
            cmbItemType.Items.AddRange(new object[] { "工具", "武器", "方块", "物品", "Buff", "弹幕", "饰品" });
            cmbItemType.SelectedIndex = 3;
            StyleComboBox(cmbItemType);
            _identitySection.Controls.Add(cmbItemType);

            AddFieldLabel(_identitySection, "内部名称", 18, 89, 80);
            txtName.Location = new Point(112, 83);
            txtName.Size = new Size(230, 30);
            txtName.Font = FontBody;
            StyleTextBox(txtName);
            _identitySection.Controls.Add(txtName);

            AddFieldLabel(_identitySection, "显示名称", 18, 123, 80);
            txtDisplayName.Location = new Point(112, 117);
            txtDisplayName.Size = new Size(230, 30);
            txtDisplayName.Font = FontBody;
            StyleTextBox(txtDisplayName);
            _identitySection.Controls.Add(txtDisplayName);

            AddFieldLabel(_identitySection, "描述", 18, 157, 80);
            txtDescription.Location = new Point(112, 151);
            txtDescription.Size = new Size(230, 38);
            txtDescription.Multiline = true;
            txtDescription.Font = FontBody;
            StyleTextBox(txtDescription);
            _identitySection.Controls.Add(txtDescription);

            _formStack?.Controls.Add(_identitySection);
        }

        private void BuildTextureSection()
        {
            _textureSection = CreateSection("贴图", "可选 PNG，会随内容一起导出", 156);

            var texturePreview = new Panel
            {
                Location = new Point(18, 54),
                Size = new Size(62, 62),
                BackColor = ClrInputBg,
                Padding = new Padding(6)
            };
            texturePreview.Paint += TexturePreview_Paint;
            _textureSection.Controls.Add(texturePreview);

            picTexture.Dock = DockStyle.Fill;
            picTexture.Margin = new Padding(0);
            picTexture.BackColor = ClrInputBg;
            picTexture.BorderStyle = BorderStyle.None;
            picTexture.SizeMode = PictureBoxSizeMode.Zoom;
            texturePreview.Controls.Add(picTexture);

            AddFieldLabel(_textureSection, "预览", 100, 55, 70);

            btnSelectTexture.Text = "选择贴图";
            btnSelectTexture.Location = new Point(100, 78);
            btnSelectTexture.Size = new Size(98, 30);
            btnSelectTexture.Font = FontBodyBold;
            StyleButton(btnSelectTexture, Color.FromArgb(62, 142, 241), Color.FromArgb(43, 102, 178));
            btnSelectTexture.Click -= btnSelectTexture_Click;
            btnSelectTexture.Click -= SelectTextureModern_Click;
            btnSelectTexture.Click += SelectTextureModern_Click;
            _textureSection.Controls.Add(btnSelectTexture);

            btnClearTexture.Text = "清除";
            btnClearTexture.Location = new Point(208, 78);
            btnClearTexture.Size = new Size(70, 30);
            btnClearTexture.Font = FontBody;
            StyleButton(btnClearTexture, Color.FromArgb(67, 83, 105), ClrBorder);
            _textureSection.Controls.Add(btnClearTexture);

            chkMultiFrameTexture.Text = "多帧图";
            chkMultiFrameTexture.Location = new Point(18, 122);
            chkMultiFrameTexture.Size = new Size(82, 24);
            chkMultiFrameTexture.Font = FontBody;
            chkMultiFrameTexture.ForeColor = ClrText;
            chkMultiFrameTexture.BackColor = Color.Transparent;
            _textureSection.Controls.Add(chkMultiFrameTexture);

            AddNumericField(_textureSection, "播放帧数", numTextureFrameCount, 118, 120, 72);
            numTextureFrameCount.Minimum = 1;
            numTextureFrameCount.Maximum = 999;
            numTextureFrameCount.Value = 1;

            _formStack?.Controls.Add(_textureSection);
        }

        private void BuildBasicSection()
        {
            _basicSection = CreateSection("基础数值", "尺寸、价值与稀有度", 122);
            AddNumericField(_basicSection, "宽度", numWidth, 18, 56, 72);
            AddNumericField(_basicSection, "高度", numHeight, 196, 56, 72);
            AddNumericField(_basicSection, "价值", numValue, 18, 90, 72);
            AddNumericField(_basicSection, "稀有度", numRarity, 196, 90, 72);
            _formStack?.Controls.Add(_basicSection);
        }

        private void BuildToolSection()
        {
            _toolSection = CreateSection("工具能力", "镐、斧、锤等采掘参数", 142);
            AddNumericField(_toolSection, "镐力", numPickaxePower, 18, 58, 72);
            AddNumericField(_toolSection, "斧力", numAxePower, 196, 58, 72);
            AddNumericField(_toolSection, "锤力", numHammerPower, 18, 98, 72);
            _formStack?.Controls.Add(_toolSection);
        }

        private void BuildCombatSection()
        {
            _combatSection = CreateSection("战斗手感", "伤害、职业、攻击动作与投掷物", 300);
            AddNumericField(_combatSection, "伤害", numDamage, 18, 58, 72);
            AddNumericField(_combatSection, "使用时间", numUseTime, 196, 58, 72);
            AddNumericField(_combatSection, "击退", numKnockback, 18, 98, 72);
            AddNumericField(_combatSection, "暴击率", numCriticalChance, 196, 98, 72);

            AddFieldLabel(_combatSection, "伤害类型", 18, 134, 80);
            cmbDamageKind.Location = new Point(96, 130);
            cmbDamageKind.Size = new Size(114, 30);
            cmbDamageKind.Font = FontBody;
            cmbDamageKind.Items.Clear();
            cmbDamageKind.Items.AddRange(new object[] { "近战", "远程", "魔法", "召唤", "普通伤害" });
            cmbDamageKind.SelectedIndex = 0;
            StyleComboBox(cmbDamageKind);
            _combatSection.Controls.Add(cmbDamageKind);

            chkAutoReuse.Text = "自动挥舞";
            chkAutoReuse.Location = new Point(226, 134);
            chkAutoReuse.Size = new Size(116, 24);
            chkAutoReuse.Font = FontBody;
            chkAutoReuse.ForeColor = ClrText;
            chkAutoReuse.BackColor = Color.Transparent;
            _combatSection.Controls.Add(chkAutoReuse);

            chkUseTurn.Text = "转身使用";
            chkUseTurn.Location = new Point(18, 168);
            chkUseTurn.Size = new Size(116, 24);
            chkUseTurn.Font = FontBody;
            chkUseTurn.ForeColor = ClrText;
            chkUseTurn.BackColor = Color.Transparent;
            _combatSection.Controls.Add(chkUseTurn);

            chkUsesProjectile.Text = "使用投掷物";
            chkUsesProjectile.Location = new Point(140, 168);
            chkUsesProjectile.Size = new Size(116, 24);
            chkUsesProjectile.Font = FontBody;
            chkUsesProjectile.ForeColor = ClrText;
            chkUsesProjectile.BackColor = Color.Transparent;
            chkUsesProjectile.CheckedChanged += (s, e) =>
            {
                if (chkUsesProjectile.Checked && numUseStyleId.Value == 1)
                    numUseStyleId.Value = 5;
            };
            _combatSection.Controls.Add(chkUsesProjectile);

            chkConsumeOnUse.Text = "使用时消耗本体";
            chkConsumeOnUse.Location = new Point(256, 168);
            chkConsumeOnUse.Size = new Size(128, 24);
            chkConsumeOnUse.Font = FontBody;
            chkConsumeOnUse.ForeColor = ClrText;
            chkConsumeOnUse.BackColor = Color.Transparent;
            _combatSection.Controls.Add(chkConsumeOnUse);

            AddNumericField(_combatSection, "弹幕ID", numProjectileId, 18, 206, 72);
            AddNumericField(_combatSection, "速度", numProjectileSpeed, 196, 206, 72);
            numProjectileId.Minimum = 0;
            numProjectileId.Maximum = 9999;
            numProjectileId.Value = 1;
            numProjectileSpeed.DecimalPlaces = 1;
            numProjectileSpeed.Increment = 0.5M;
            numProjectileSpeed.Maximum = 99;
            numProjectileSpeed.Value = 10;

            AddNumericField(_combatSection, "动作ID", numUseStyleId, 18, 242, 72);
            numUseStyleId.Minimum = 0;
            numUseStyleId.Maximum = 9999;
            numUseStyleId.Value = 1;

            _formStack?.Controls.Add(_combatSection);
        }

        private void BuildAccessorySection()
        {
            _accessorySection = CreateSection("饰品加成", "职业伤害、攻速、暴击、防御与减伤", 290);

            _accessorySection.Controls.Add(CreatePlainLabel("类型", new Point(18, 56), new Size(52, 20), FontBodyBold, ClrSubText));
            _accessorySection.Controls.Add(CreatePlainLabel("伤害%", new Point(86, 56), new Size(58, 20), FontBodyBold, ClrSubText));
            _accessorySection.Controls.Add(CreatePlainLabel("攻速%", new Point(190, 56), new Size(58, 20), FontBodyBold, ClrSubText));
            _accessorySection.Controls.Add(CreatePlainLabel("暴击%", new Point(294, 56), new Size(58, 20), FontBodyBold, ClrSubText));

            AddAccessoryStatRow("近战", numAccessoryMeleeDamage, numAccessoryMeleeSpeed, numAccessoryMeleeCrit, 84);
            AddAccessoryStatRow("魔法", numAccessoryMagicDamage, numAccessoryMagicSpeed, numAccessoryMagicCrit, 120);
            AddAccessoryStatRow("远程", numAccessoryRangedDamage, numAccessoryRangedSpeed, numAccessoryRangedCrit, 156);
            AddAccessoryStatRow("召唤", numAccessorySummonDamage, numAccessorySummonSpeed, numAccessorySummonCrit, 192);

            AddNumericField(_accessorySection, "防御", numAccessoryDefense, 18, 238, 64);
            AddNumericField(_accessorySection, "减伤%", numAccessoryDamageReduction, 196, 238, 64);

            foreach (var number in GetAccessoryNumbers())
            {
                number.Minimum = -999;
                number.Maximum = 999;
            }
            numAccessoryDamageReduction.Minimum = 0;
            numAccessoryDamageReduction.Maximum = 100;

            _formStack?.Controls.Add(_accessorySection);
        }

        private void AddAccessoryStatRow(string label, NumericUpDown damage, NumericUpDown speed, NumericUpDown crit, int y)
        {
            if (_accessorySection == null)
                return;

            _accessorySection.Controls.Add(CreatePlainLabel(label, new Point(18, y + 3), new Size(52, 20), FontBodyBold, ClrText));
            AddNumericBox(_accessorySection, damage, 86, y, 58);
            AddNumericBox(_accessorySection, speed, 190, y, 58);
            AddNumericBox(_accessorySection, crit, 294, y, 58);
        }

        private IEnumerable<NumericUpDown> GetAccessoryNumbers()
        {
            yield return numAccessoryMeleeDamage;
            yield return numAccessoryMagicDamage;
            yield return numAccessoryRangedDamage;
            yield return numAccessorySummonDamage;
            yield return numAccessoryMeleeSpeed;
            yield return numAccessoryMagicSpeed;
            yield return numAccessoryRangedSpeed;
            yield return numAccessorySummonSpeed;
            yield return numAccessoryMeleeCrit;
            yield return numAccessoryMagicCrit;
            yield return numAccessoryRangedCrit;
            yield return numAccessorySummonCrit;
            yield return numAccessoryDefense;
            yield return numAccessoryDamageReduction;
        }

        private void BuildBlockSection()
        {
            _blockSection = CreateSection("方块条件", "挖掘门槛与放置设定", 100);
            AddNumericField(_blockSection, "所需镐力", numMinPick, 18, 58, 72);
            _formStack?.Controls.Add(_blockSection);
        }

        private void BuildBuffSection()
        {
            _buffSection = CreateSection("Buff 效果", "图标来源与效果流程", 112);

            AddFieldLabel(_buffSection, "图标", 18, 58, 72);
            cmbBuffIconSource.Location = new Point(96, 52);
            cmbBuffIconSource.Size = new Size(130, 30);
            cmbBuffIconSource.Font = FontBody;
            cmbBuffIconSource.Items.Clear();
            cmbBuffIconSource.Items.AddRange(new object[] { "DIY 图标", "原版图标" });
            cmbBuffIconSource.SelectedIndex = 0;
            StyleComboBox(cmbBuffIconSource);
            cmbBuffIconSource.SelectedIndexChanged += (s, e) => UpdateBuffIconVisibility();
            _buffSection.Controls.Add(cmbBuffIconSource);

            AddNumericField(_buffSection, "原版ID", numVanillaBuffIconId, 18, 88, 72);
            numVanillaBuffIconId.Minimum = 1;
            numVanillaBuffIconId.Maximum = 9999;
            numVanillaBuffIconId.Value = 1;

            var hint = CreatePlainLabel("DIY 图标使用上方贴图 PNG；原版图标填 Buff ID。", new Point(196, 88), new Size(170, 22), FontSmall, ClrSubText);
            _buffSection.Controls.Add(hint);

            _formStack?.Controls.Add(_buffSection);
        }

        private void BuildRecipeSection()
        {
            _recipeSection = CreateSection("合成配方", "选择材料、数量和制作站", 116);
            btnEditRecipe.Text = "编辑配方";
            btnEditRecipe.Location = new Point(18, 58);
            btnEditRecipe.Size = new Size(104, 32);
            btnEditRecipe.Font = FontBodyBold;
            StyleButton(btnEditRecipe, Color.FromArgb(92, 116, 236), Color.FromArgb(68, 86, 175));
            btnEditRecipe.Click += (s, e) =>
            {
                using var form = new RecipeEditorForm(_currentRecipe);
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    _currentRecipe = form.Recipe;
                    UpdateRecipeSummary();
                }
            };
            _recipeSection.Controls.Add(btnEditRecipe);

            lblRecipeSummary.Location = new Point(136, 58);
            lblRecipeSummary.Size = new Size(220, 42);
            lblRecipeSummary.Font = FontSmall;
            lblRecipeSummary.ForeColor = ClrSubText;
            lblRecipeSummary.BackColor = Color.Transparent;
            _recipeSection.Controls.Add(lblRecipeSummary);
            UpdateRecipeSummary();

            _formStack?.Controls.Add(_recipeSection);
        }

        private void UpdateRecipeSummary()
        {
            if (lblRecipeSummary == null)
                return;

            lblRecipeSummary.Text = _currentRecipe.Ingredients.Count == 0
                ? "未设置配方"
                : $"{_currentRecipe.Ingredients.Count} 种材料\r\n制作站: {_currentRecipe.CraftingStationDisplay}";
        }

        private ModDamageKind GetSelectedDamageKind()
        {
            return cmbDamageKind.SelectedIndex switch
            {
                1 => ModDamageKind.Ranged,
                2 => ModDamageKind.Magic,
                3 => ModDamageKind.Summon,
                4 => ModDamageKind.Generic,
                _ => ModDamageKind.Melee
            };
        }

        private BuffIconSource GetSelectedBuffIconSource()
        {
            return cmbBuffIconSource.SelectedIndex == 1
                ? BuffIconSource.Vanilla
                : BuffIconSource.Custom;
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

        private void BuildRightPanel()
        {
            pnlRight.SuspendLayout();
            pnlRight.Controls.Clear();
            pnlRight.Text = "";
            pnlRight.Style = UIStyle.Black;
            pnlRight.FillColor = ClrPanelBg;
            pnlRight.RectColor = ClrBorder;
            pnlRight.BackColor = ClrPanelBg;

            lblPreviewTitle.Text = "已创建物品";
            lblPreviewTitle.Font = FontSectionTitle;
            lblPreviewTitle.Location = new Point(24, 20);
            lblPreviewTitle.Size = new Size(180, 28);
            lblPreviewTitle.ForeColor = ClrText;
            lblPreviewTitle.BackColor = Color.Transparent;
            pnlRight.Controls.Add(lblPreviewTitle);

            _rightSubtitleLabel = CreatePlainLabel("创建后的内容会以卡片形式显示，方便继续编辑属性、流程或删除。", new Point(24, 47), new Size(560, 22), FontBody, ClrSubText);
            pnlRight.Controls.Add(_rightSubtitleLabel);

            btnExport.Text = "导出 Mod";
            btnExport.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            btnExport.Size = new Size(124, 36);
            StyleButton(btnExport, ClrAccent2, Color.FromArgb(188, 126, 24));
            btnExport.Click -= btnExport_Click;
            btnExport.Click -= ExportModern_Click;
            btnExport.Click += ExportModern_Click;
            pnlRight.Controls.Add(btnExport);

            flowItems.AutoScroll = true;
            flowItems.FlowDirection = FlowDirection.TopDown;
            flowItems.WrapContents = false;
            flowItems.Padding = new Padding(8);
            flowItems.Margin = new Padding(0);
            flowItems.BackColor = ClrSectionAlt;
            flowItems.Resize -= flowItems_Resize;
            flowItems.Resize += flowItems_Resize;
            pnlRight.Controls.Add(flowItems);

            _emptyStateLabel = new Label
            {
                Text = "还没有创建物品\r\n填写左侧参数后点击「创建物品」，这里会出现预览卡片。",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = ClrMuted,
                BackColor = ClrSectionAlt
            };
            pnlRight.Controls.Add(_emptyStateLabel);

            pnlRight.ResumeLayout(false);
            UpdateEmptyState();
        }

        private static Panel CreateSection(string title, string subtitle, int height)
        {
            var section = new Panel
            {
                Size = new Size(382, height),
                BackColor = ClrSectionBg,
                Margin = new Padding(0, 0, 0, 10)
            };
            section.Paint += SectionPanel_Paint;

            section.Controls.Add(CreatePlainLabel(title, new Point(18, 14), new Size(170, 24), FontSectionTitle, ClrText));
            section.Controls.Add(CreatePlainLabel(subtitle, new Point(18, 36), new Size(320, 18), FontSmall, ClrSubText));

            return section;
        }

        private static Label CreatePlainLabel(string text, Point location, Size size, Font font, Color color)
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

        private static void AddFieldLabel(Control parent, string text, int x, int y, int width)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                AutoSize = false,
                Location = new Point(x, y),
                Size = new Size(width, 22),
                Font = FontBodyBold,
                ForeColor = ClrSubText,
                BackColor = Color.Transparent
            });
        }

        private static void AddNumericField(Control parent, string label, NumericUpDown number, int x, int y, int inputWidth)
        {
            AddFieldLabel(parent, label, x, y + 3, 72);
            AddNumericBox(parent, number, x + 78, y, inputWidth);
        }

        private static void AddNumericBox(Control parent, NumericUpDown number, int x, int y, int inputWidth)
        {
            var host = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(inputWidth, 26),
                BackColor = ClrInputBg,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            host.Paint += NumericHost_Paint;

            number.Location = new Point(0, 2);
            number.Size = new Size(inputWidth + SystemInformation.VerticalScrollBarWidth, 22);
            number.Font = FontBody;
            number.BackColor = ClrInputBg;
            number.ForeColor = ClrText;
            number.BorderStyle = BorderStyle.None;
            number.TextAlign = HorizontalAlignment.Center;
            StyleNumericUpDown(number);
            host.Controls.Add(number);
            parent.Controls.Add(host);
        }

        private static void StyleNumericUpDown(NumericUpDown number)
        {
            StyleNativeNumericChildren(number);
            number.HandleCreated -= NumericUpDown_HandleCreated;
            number.HandleCreated += NumericUpDown_HandleCreated;
            number.Resize -= NumericUpDown_Resize;
            number.Resize += NumericUpDown_Resize;
        }

        private static void NumericUpDown_HandleCreated(object? sender, EventArgs e)
        {
            if (sender is NumericUpDown number)
                StyleNativeNumericChildren(number);
        }

        private static void NumericUpDown_Resize(object? sender, EventArgs e)
        {
            if (sender is NumericUpDown number)
                StyleNativeNumericChildren(number);
        }

        private static void StyleNativeNumericChildren(NumericUpDown number)
        {
            foreach (Control child in number.Controls)
            {
                child.BackColor = ClrInputBg;
                child.ForeColor = ClrText;
            }

            number.Invalidate(true);
        }

        private static void NumericHost_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel) return;

            using var borderPen = new Pen(ClrBorder);
            e.Graphics.DrawRectangle(borderPen, 0, 0, panel.Width - 1, panel.Height - 1);
        }

        private static void StyleTextBox(UITextBox textBox)
        {
            textBox.Style = UIStyle.Black;
            textBox.FillColor = ClrInputBg;
            textBox.RectColor = ClrBorder;
            textBox.ForeColor = ClrText;
            textBox.BackColor = ClrInputBg;
        }

        private static void StyleComboBox(UIComboBox comboBox)
        {
            comboBox.Style = UIStyle.Black;
            comboBox.FillColor = ClrInputBg;
            comboBox.RectColor = ClrBorder;
            comboBox.ForeColor = ClrText;
            comboBox.BackColor = ClrInputBg;
        }

        private static void StyleButton(UIButton button, Color fill, Color border)
        {
            button.Style = UIStyle.Black;
            button.FillColor = fill;
            button.RectColor = border;
            button.ForeColor = Color.White;
        }

        private static void ConfigureHeaderButton(UIButton button, string text, EventHandler clickHandler)
        {
            button.Text = text;
            button.Size = new Size(86, 30);
            button.Margin = new Padding(8, 0, 0, 0);
            button.Font = FontBodyBold;
            button.Style = UIStyle.Black;
            button.FillColor = Color.FromArgb(32, 45, 61);
            button.RectColor = ClrBorder;
            button.ForeColor = ClrText;
            button.Click -= clickHandler;
            button.Click += clickHandler;
        }

        private void Form1_Resize(object? sender, EventArgs e)
        {
            LayoutShell();
            ResizeItemCards();
        }

        private void LayoutShell()
        {
            if (_headerPanel == null || _formStack == null || _formViewport == null || _formScrollBar == null || _emptyStateLabel == null)
                return;

            int margin = 18;
            int headerTop = 38;
            int headerHeight = 76;
            int panelTop = headerTop + headerHeight + 12;
            int panelHeight = Math.Max(500, ClientSize.Height - panelTop - margin);
            int leftWidth = 430;
            int gap = 16;

            _headerPanel.SetBounds(margin, headerTop, ClientSize.Width - margin * 2, headerHeight);
            if (_projectToolbar != null)
                _projectToolbar.SetBounds(Math.Max(430, _headerPanel.Width - 390), 22, 382, 32);

            pnlLeft.SetBounds(margin, panelTop, leftWidth, panelHeight);
            pnlRight.SetBounds(margin + leftWidth + gap, panelTop, ClientSize.Width - (margin * 2 + leftWidth + gap), panelHeight);

            int createButtonTop = pnlLeft.Height - 58;
            int scrollWidth = 14;
            int scrollGap = 4;
            int viewportWidth = Math.Max(260, pnlLeft.Width - 48 - scrollWidth - scrollGap);
            int viewportHeight = Math.Max(300, createButtonTop - 82);

            _formViewport.SetBounds(24, 76, viewportWidth, viewportHeight);
            _formScrollBar.SetBounds(24 + viewportWidth + scrollGap, 76, scrollWidth, viewportHeight);
            _formStack.Width = _formViewport.ClientSize.Width;
            foreach (Control section in _formStack.Controls)
                section.Width = _formStack.Width;
            UpdateFormScrollLayout();

            btnCreate.SetBounds(24, createButtonTop, pnlLeft.Width - 48, 40);
            _formScrollBar.BringToFront();

            btnExport.Location = new Point(Math.Max(24, pnlRight.Width - btnExport.Width - 24), 24);
            if (_rightSubtitleLabel != null)
                _rightSubtitleLabel.Width = Math.Max(260, btnExport.Left - _rightSubtitleLabel.Left - 24);
            flowItems.SetBounds(24, 84, pnlRight.Width - 48, pnlRight.Height - 108);
            _emptyStateLabel.Bounds = flowItems.Bounds;
            _emptyStateLabel.BringToFront();
        }

        private void FormScrollBar_ValueChanged(object? sender, EventArgs e)
        {
            if (_formScrollBar == null)
                return;

            _formScrollOffset = _formScrollBar.Value;
            UpdateFormScrollLayout();
        }

        private void FormScroller_MouseWheel(object? sender, MouseEventArgs e)
        {
            ScrollFormStack(-Math.Sign(e.Delta) * 54);
        }

        private void HookFormMouseWheel(Control parent)
        {
            parent.MouseWheel -= FormScroller_MouseWheel;
            parent.MouseWheel += FormScroller_MouseWheel;

            foreach (Control child in parent.Controls)
                HookFormMouseWheel(child);
        }

        private void ScrollFormStack(int delta)
        {
            if (_formScrollBar == null)
                return;

            _formScrollBar.Value = _formScrollOffset + delta;
        }

        private void UpdateFormScrollLayout()
        {
            if (_formStack == null || _formViewport == null || _formScrollBar == null)
                return;

            _formStack.PerformLayout();

            int contentHeight = GetFlowContentHeight(_formStack);
            int viewportHeight = _formViewport.ClientSize.Height;
            int maxScroll = Math.Max(0, contentHeight - viewportHeight);
            _formScrollOffset = Math.Clamp(_formScrollOffset, 0, maxScroll);

            _formStack.SetBounds(0, -_formScrollOffset, _formViewport.ClientSize.Width, Math.Max(viewportHeight, contentHeight));

            _formScrollBar.Configure(_formScrollOffset, maxScroll, viewportHeight, contentHeight);
            _formScrollBar.BringToFront();
        }

        private static int GetFlowContentHeight(FlowLayoutPanel panel)
        {
            int height = panel.Padding.Vertical;
            foreach (Control control in panel.Controls)
            {
                if (!control.Visible)
                    continue;

                height += control.Margin.Top + control.Height + control.Margin.Bottom;
            }

            return height;
        }

        private static void HeaderPanel_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel) return;

            e.Graphics.Clear(ClrBg);

            using var bottomPen = new Pen(ClrSoftBorder);
            e.Graphics.DrawLine(bottomPen, 0, panel.Height - 1, panel.Width, panel.Height - 1);

            using var accentPen = new Pen(ClrAccent, 3);
            e.Graphics.DrawLine(accentPen, 0, panel.Height - 1, 170, panel.Height - 1);
        }

        private static void SectionPanel_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var borderPen = new Pen(ClrSoftBorder);
            e.Graphics.DrawRectangle(borderPen, 0, 0, panel.Width - 1, panel.Height - 1);

            using var accentPen = new Pen(ClrAccent, 3);
            e.Graphics.DrawLine(accentPen, 12, 14, 12, 38);
        }

        private void SelectTextureModern_Click(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "选择物品贴图",
                Filter = "PNG 图片|*.png|所有文件|*.*"
            };

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            _selectedTexturePath = dlg.FileName;
            try
            {
                picTexture.Image?.Dispose();
                picTexture.Image = LoadImageCopy(_selectedTexturePath);
            }
            catch
            {
                UIMessageBox.Show("无法加载图片，请选择有效的 PNG 文件。");
                _selectedTexturePath = string.Empty;
            }
        }

        private ModItemData CaptureItemFromForm(FlowScript? flow = null)
        {
            var type = cmbItemType.SelectedIndex >= 0 ? (ItemType)cmbItemType.SelectedIndex : ItemType.Item;

            return new ModItemData
            {
                Name = txtName.Text.Trim(),
                DisplayName = string.IsNullOrWhiteSpace(txtDisplayName.Text) ? txtName.Text.Trim() : txtDisplayName.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                Type = type,
                Width = (int)numWidth.Value,
                Height = (int)numHeight.Value,
                Value = (int)numValue.Value,
                Rarity = (int)numRarity.Value,
                Damage = (int)numDamage.Value,
                DamageKind = GetSelectedDamageKind(),
                UseTime = (int)numUseTime.Value,
                UseAnimation = (int)numUseTime.Value,
                UseStyleId = (int)numUseStyleId.Value,
                Knockback = (int)numKnockback.Value,
                CriticalChance = (int)numCriticalChance.Value,
                UsesProjectile = chkUsesProjectile.Checked,
                ProjectileId = (int)numProjectileId.Value,
                ProjectileSpeed = numProjectileSpeed.Value,
                ConsumeOnUse = chkConsumeOnUse.Checked,
                PickaxePower = (int)numPickaxePower.Value,
                AxePower = (int)numAxePower.Value,
                HammerPower = (int)numHammerPower.Value,
                MinPick = (int)numMinPick.Value,
                AccessoryMeleeDamage = (int)numAccessoryMeleeDamage.Value,
                AccessoryMagicDamage = (int)numAccessoryMagicDamage.Value,
                AccessoryRangedDamage = (int)numAccessoryRangedDamage.Value,
                AccessorySummonDamage = (int)numAccessorySummonDamage.Value,
                AccessoryMeleeSpeed = (int)numAccessoryMeleeSpeed.Value,
                AccessoryMagicSpeed = (int)numAccessoryMagicSpeed.Value,
                AccessoryRangedSpeed = (int)numAccessoryRangedSpeed.Value,
                AccessorySummonSpeed = (int)numAccessorySummonSpeed.Value,
                AccessoryMeleeCrit = (int)numAccessoryMeleeCrit.Value,
                AccessoryMagicCrit = (int)numAccessoryMagicCrit.Value,
                AccessoryRangedCrit = (int)numAccessoryRangedCrit.Value,
                AccessorySummonCrit = (int)numAccessorySummonCrit.Value,
                AccessoryDefense = (int)numAccessoryDefense.Value,
                AccessoryDamageReduction = (int)numAccessoryDamageReduction.Value,
                AutoReuse = chkAutoReuse.Checked,
                UseTurn = chkUseTurn.Checked,
                TexturePath = type == ItemType.Buff && GetSelectedBuffIconSource() == BuffIconSource.Vanilla ? string.Empty : _selectedTexturePath,
                IsMultiFrameTexture = chkMultiFrameTexture.Checked,
                TextureFrameCount = (int)numTextureFrameCount.Value,
                BuffIconSource = GetSelectedBuffIconSource(),
                VanillaBuffIconId = (int)numVanillaBuffIconId.Value,
                Flow = flow,
                Recipe = CloneRecipe(_currentRecipe)
            };
        }

        private void CreateItemModern_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                UIMessageBox.Show("请输入内部名称。");
                return;
            }

            var editingItem = _editingItem;
            var item = CaptureItemFromForm(editingItem?.Flow);

            if (editingItem != null)
            {
                int index = _items.IndexOf(editingItem);
                if (index >= 0)
                {
                    _items[index] = item;
                    ClearInputs();
                    RefreshItemCards();
                    return;
                }

                _editingItem = null;
            }

            _items.Add(item);
            AddItemCardModern(item);
            ClearInputs();
            UpdateEmptyState();
        }

        private void AddItemCardModern(ModItemData item)
        {
            var accent = GetItemAccent(item.Type);
            var card = new Panel
            {
                Size = new Size(GetCardWidth(), 124),
                Margin = new Padding(0, 0, 0, 12),
                BackColor = ClrCardBg,
                Tag = item
            };
            card.Paint += CardPanel_Paint;

            card.Controls.Add(new Panel
            {
                Dock = DockStyle.Left,
                Width = 5,
                BackColor = accent
            });

            var preview = new Panel
            {
                Location = new Point(18, 22),
                Size = new Size(76, 76),
                BackColor = ClrInputBg
            };
            preview.Paint += TexturePreview_Paint;
            card.Controls.Add(preview);

            if (item.Type == ItemType.Buff && item.BuffIconSource == BuffIconSource.Vanilla)
            {
                var vanillaIcon = CreatePlainLabel($"Buff\r\n{item.VanillaBuffIconId}", new Point(0, 17), new Size(76, 40), FontBodyBold, accent);
                vanillaIcon.TextAlign = ContentAlignment.MiddleCenter;
                preview.Controls.Add(vanillaIcon);
            }
            else if (!string.IsNullOrEmpty(item.TexturePath) && File.Exists(item.TexturePath))
            {
                try
                {
                    preview.Controls.Add(new PictureBox
                    {
                        Dock = DockStyle.Fill,
                        Padding = new Padding(6),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        BackColor = Color.Transparent,
                        Image = LoadImageCopy(item.TexturePath)
                    });
                }
                catch
                {
                    preview.Controls.Add(CreatePlainLabel("PNG", new Point(0, 25), new Size(76, 24), FontBodyBold, ClrMuted));
                }
            }
            else
            {
                var noTexture = CreatePlainLabel("无贴图", new Point(0, 27), new Size(76, 22), FontSmall, ClrMuted);
                noTexture.TextAlign = ContentAlignment.MiddleCenter;
                preview.Controls.Add(noTexture);
            }

            var typeBadge = new Label
            {
                Text = item.TypeDisplay,
                Location = new Point(112, 18),
                Size = new Size(70, 24),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = FontBodyBold,
                ForeColor = accent,
                BackColor = Color.FromArgb(35, 48, 64)
            };
            card.Controls.Add(typeBadge);

            var nameLabel = new Label
            {
                Text = item.DisplayName,
                Location = new Point(112, 44),
                Size = new Size(Math.Max(120, card.Width - 260), 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = ClrText,
                BackColor = Color.Transparent
            };
            card.Controls.Add(nameLabel);

            var idLabel = new Label
            {
                Text = item.Name,
                Location = new Point(112, 68),
                Size = new Size(Math.Max(120, card.Width - 260), 18),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Font = FontSmall,
                ForeColor = ClrMuted,
                BackColor = Color.Transparent
            };
            card.Controls.Add(idLabel);

            var statsLabel = new Label
            {
                Text = BuildStatsTextModern(item),
                Location = new Point(112, 91),
                Size = new Size(Math.Max(220, card.Width - 260), 22),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                Font = FontSmall,
                ForeColor = ClrSubText,
                BackColor = Color.Transparent
            };
            card.Controls.Add(statsLabel);

            var editBtn = new UIButton
            {
                Text = "编辑",
                Size = new Size(88, 28),
                Location = new Point(card.Width - 112, 18),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = FontBodyBold,
                Tag = item
            };
            StyleButton(editBtn, Color.FromArgb(62, 142, 241), Color.FromArgb(43, 102, 178));
            editBtn.Click += (s, e) => LoadItemForEditing(item);
            card.Controls.Add(editBtn);

            var flowBtn = new UIButton
            {
                Text = "流程",
                Size = new Size(88, 28),
                Location = new Point(card.Width - 112, 50),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = FontBodyBold,
                Tag = item
            };
            StyleButton(flowBtn, Color.FromArgb(54, 161, 124), Color.FromArgb(38, 112, 87));
            flowBtn.Click += (s, e) =>
            {
                using var form = new VisualScriptForm(item);
                form.ShowDialog();
            };
            card.Controls.Add(flowBtn);

            var deleteBtn = new UIButton
            {
                Text = "删除",
                Size = new Size(88, 28),
                Location = new Point(card.Width - 112, 82),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = FontBodyBold,
                Tag = item
            };
            StyleButton(deleteBtn, ClrDanger, Color.FromArgb(180, 58, 70));
            deleteBtn.Click += (s, e) =>
            {
                _items.Remove(item);
                if (ReferenceEquals(_editingItem, item))
                    ClearInputs();

                DisposeCardImages(card);
                flowItems.Controls.Remove(card);
                card.Dispose();
                UpdateEmptyState();
            };
            card.Controls.Add(deleteBtn);

            flowItems.Controls.Add(card);
            flowItems.Controls.SetChildIndex(card, 0);
            ResizeItemCards();
        }

        private void LoadItemForEditing(ModItemData item)
        {
            _editingItem = item;

            cmbItemType.SelectedIndex = ClampComboIndex((int)item.Type, cmbItemType.Items.Count, 3);
            txtName.Text = item.Name;
            txtDisplayName.Text = item.DisplayName;
            txtDescription.Text = item.Description;

            SetNumericValue(numWidth, item.Width);
            SetNumericValue(numHeight, item.Height);
            SetNumericValue(numValue, item.Value);
            SetNumericValue(numRarity, item.Rarity);
            SetNumericValue(numDamage, item.Damage);
            SetDamageKind(item.DamageKind);
            SetNumericValue(numUseTime, item.UseTime);
            chkUsesProjectile.Checked = item.UsesProjectile;
            SetNumericValue(numUseStyleId, item.UseStyleId <= 0 ? (item.UsesProjectile ? 5 : 1) : item.UseStyleId);
            SetNumericValue(numKnockback, item.Knockback);
            SetNumericValue(numCriticalChance, item.CriticalChance);
            chkConsumeOnUse.Checked = item.ConsumeOnUse;
            SetNumericValue(numProjectileId, item.ProjectileId);
            SetNumericValue(numProjectileSpeed, item.ProjectileSpeed);
            SetNumericValue(numPickaxePower, item.PickaxePower);
            SetNumericValue(numAxePower, item.AxePower);
            SetNumericValue(numHammerPower, item.HammerPower);
            SetNumericValue(numMinPick, item.MinPick);
            SetNumericValue(numAccessoryMeleeDamage, item.AccessoryMeleeDamage);
            SetNumericValue(numAccessoryMagicDamage, item.AccessoryMagicDamage);
            SetNumericValue(numAccessoryRangedDamage, item.AccessoryRangedDamage);
            SetNumericValue(numAccessorySummonDamage, item.AccessorySummonDamage);
            SetNumericValue(numAccessoryMeleeSpeed, item.AccessoryMeleeSpeed);
            SetNumericValue(numAccessoryMagicSpeed, item.AccessoryMagicSpeed);
            SetNumericValue(numAccessoryRangedSpeed, item.AccessoryRangedSpeed);
            SetNumericValue(numAccessorySummonSpeed, item.AccessorySummonSpeed);
            SetNumericValue(numAccessoryMeleeCrit, item.AccessoryMeleeCrit);
            SetNumericValue(numAccessoryMagicCrit, item.AccessoryMagicCrit);
            SetNumericValue(numAccessoryRangedCrit, item.AccessoryRangedCrit);
            SetNumericValue(numAccessorySummonCrit, item.AccessorySummonCrit);
            SetNumericValue(numAccessoryDefense, item.AccessoryDefense);
            SetNumericValue(numAccessoryDamageReduction, item.AccessoryDamageReduction);
            chkAutoReuse.Checked = item.AutoReuse;
            chkUseTurn.Checked = item.UseTurn;
            chkMultiFrameTexture.Checked = item.IsMultiFrameTexture;
            SetNumericValue(numTextureFrameCount, item.TextureFrameCount <= 0 ? 1 : item.TextureFrameCount);
            SetBuffIconSource(item.BuffIconSource);
            SetNumericValue(numVanillaBuffIconId, item.VanillaBuffIconId <= 0 ? 1 : item.VanillaBuffIconId);

            _currentRecipe = CloneRecipe(item.Recipe ?? new RecipeData());
            UpdateRecipeSummary();

            _selectedTexturePath = item.TexturePath ?? string.Empty;
            picTexture.Image?.Dispose();
            picTexture.Image = null;
            if (!string.IsNullOrWhiteSpace(_selectedTexturePath) && File.Exists(_selectedTexturePath))
            {
                try
                {
                    picTexture.Image = LoadImageCopy(_selectedTexturePath);
                }
                catch
                {
                    _selectedTexturePath = string.Empty;
                }
            }

            UpdateFieldVisibility();
            if (_formScrollBar != null)
                _formScrollBar.Value = 0;
        }

        private void ExportModern_Click(object? sender, EventArgs e)
        {
            if (_items.Count == 0)
            {
                UIMessageBox.Show("没有可导出的物品。");
                return;
            }

            using var dialog = new FolderBrowserDialog
            {
                Description = "选择导出目录 (tModLoader Mod 源码文件夹)"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            string baseDir = dialog.SelectedPath;
            string itemsDir = Path.Combine(baseDir, "Items");
            string buffsDir = Path.Combine(baseDir, "Buffs");
            string projectilesDir = Path.Combine(baseDir, "Projectiles");
            string locDir = Path.Combine(baseDir, "Localization");
            string locFile = Path.Combine(locDir, "en-US.hjson");

            try
            {
                Directory.CreateDirectory(itemsDir);
                Directory.CreateDirectory(buffsDir);
                Directory.CreateDirectory(projectilesDir);
                Directory.CreateDirectory(locDir);

                var sbItems = new System.Text.StringBuilder();
                var sbBuffs = new System.Text.StringBuilder();
                var sbProjectiles = new System.Text.StringBuilder();
                var sbLoc = new System.Text.StringBuilder();
                string projectCodeName = GetProjectCodeName();
                var itemEntries = _items.Where(item => item.Type != ItemType.Buff && item.Type != ItemType.Projectile).ToList();
                var buffEntries = _items.Where(item => item.Type == ItemType.Buff).ToList();
                var projectileEntries = _items.Where(item => item.Type == ItemType.Projectile).ToList();

                sbLoc.AppendLine("Mods: {");
                sbLoc.AppendLine($"  {projectCodeName}: {{");

                if (itemEntries.Count > 0)
                {
                    sbLoc.AppendLine("    Items: {");
                    foreach (var item in itemEntries)
                    {
                        string className = SanitizeClassName(item.Name);
                        string csFile = Path.Combine(itemsDir, $"{className}.cs");
                        string code = GenerateItemCode(item, className);

                        File.WriteAllText(csFile, code, System.Text.Encoding.UTF8);

                        if (!string.IsNullOrEmpty(item.TexturePath) && File.Exists(item.TexturePath))
                        {
                            string texFile = Path.Combine(itemsDir, $"{className}.png");
                            File.Copy(item.TexturePath, texFile, true);
                            sbItems.AppendLine($"  - 导出物品: {className}.cs + {className}.png");
                        }
                        else
                        {
                            sbItems.AppendLine($"  - 导出物品: {className}.cs");
                        }

                        sbLoc.AppendLine($"      {className}.DisplayName: {item.DisplayName}");
                        if (!string.IsNullOrEmpty(item.Description))
                            sbLoc.AppendLine($"      {className}.Tooltip: {item.Description}");
                    }
                    sbLoc.AppendLine("    }");
                }

                if (buffEntries.Count > 0)
                {
                    sbLoc.AppendLine("    Buffs: {");
                    foreach (var buff in buffEntries)
                    {
                        string className = SanitizeClassName(buff.Name);
                        string csFile = Path.Combine(buffsDir, $"{className}.cs");
                        string code = GenerateBuffCode(buff, className);

                        File.WriteAllText(csFile, code, System.Text.Encoding.UTF8);

                        if (buff.BuffIconSource == BuffIconSource.Custom && !string.IsNullOrEmpty(buff.TexturePath) && File.Exists(buff.TexturePath))
                        {
                            string texFile = Path.Combine(buffsDir, $"{className}.png");
                            File.Copy(buff.TexturePath, texFile, true);
                            sbBuffs.AppendLine($"  - 导出 Buff: {className}.cs + {className}.png");
                        }
                        else
                        {
                            sbBuffs.AppendLine(buff.BuffIconSource == BuffIconSource.Vanilla
                                ? $"  - 导出 Buff: {className}.cs (原版图标 {buff.VanillaBuffIconId})"
                                : $"  - 导出 Buff: {className}.cs");
                        }

                        sbLoc.AppendLine($"      {className}.DisplayName: {buff.DisplayName}");
                        if (!string.IsNullOrEmpty(buff.Description))
                            sbLoc.AppendLine($"      {className}.Description: {buff.Description}");
                    }
                    sbLoc.AppendLine("    }");
                }

                if (projectileEntries.Count > 0)
                {
                    sbLoc.AppendLine("    Projectiles: {");
                    foreach (var projectile in projectileEntries)
                    {
                        string className = SanitizeClassName(projectile.Name);
                        string csFile = Path.Combine(projectilesDir, $"{className}.cs");
                        string code = GenerateProjectileCode(projectile, className);

                        File.WriteAllText(csFile, code, System.Text.Encoding.UTF8);

                        if (!string.IsNullOrEmpty(projectile.TexturePath) && File.Exists(projectile.TexturePath))
                        {
                            string texFile = Path.Combine(projectilesDir, $"{className}.png");
                            File.Copy(projectile.TexturePath, texFile, true);
                            sbProjectiles.AppendLine($"  - 导出弹幕: {className}.cs + {className}.png");
                        }
                        else
                        {
                            sbProjectiles.AppendLine($"  - 导出弹幕: {className}.cs");
                        }

                        sbLoc.AppendLine($"      {className}.DisplayName: {projectile.DisplayName}");
                        if (!string.IsNullOrEmpty(projectile.Description))
                            sbLoc.AppendLine($"      {className}.Description: {projectile.Description}");
                    }
                    sbLoc.AppendLine("    }");
                }

                sbLoc.AppendLine("  }");
                sbLoc.AppendLine("}");

                File.WriteAllText(locFile, sbLoc.ToString(), System.Text.Encoding.UTF8);

                string msg = $"成功导出 {_items.Count} 个内容到:\n{baseDir}\n\n生成内容:\n{sbItems}{sbBuffs}{sbProjectiles}";
                UIMessageBox.Show(msg);
            }
            catch (Exception ex)
            {
                UIMessageBox.Show($"导出失败: {ex.Message}");
            }
        }

        private static string BuildStatsTextModern(ModItemData item)
        {
            var parts = new List<string>();

            if (item.Type == ItemType.Tool || item.Type == ItemType.Weapon)
            {
                if (item.Damage > 0) parts.Add($"{item.DamageKindDisplay}伤害 {item.Damage}");
                parts.Add($"使用时间 {item.UseTime}");
                parts.Add($"动作 {item.UseStyleId}");
                if (item.Knockback > 0) parts.Add($"击退 {item.Knockback}");
                if (item.CriticalChance > 0) parts.Add($"暴击 {item.CriticalChance}%");
                if (item.UsesProjectile) parts.Add($"弹幕 {item.ProjectileId} / 速度 {item.ProjectileSpeed:0.#}");
                if (item.ConsumeOnUse) parts.Add("使用消耗");
            }

            if (item.Type == ItemType.Projectile)
            {
                if (item.Damage > 0) parts.Add($"{item.DamageKindDisplay}伤害 {item.Damage}");
                if (item.Knockback > 0) parts.Add($"击退 {item.Knockback}");
                if (item.Flow?.Blocks.Count > 0)
                    parts.Add($"流程 {item.Flow.Blocks.Count} 节点");
            }

            if (item.Type == ItemType.Tool)
            {
                if (item.PickaxePower > 0) parts.Add($"镐力 {item.PickaxePower}%");
                if (item.AxePower > 0) parts.Add($"斧力 {item.AxePower}%");
                if (item.HammerPower > 0) parts.Add($"锤力 {item.HammerPower}%");
            }

            if (item.Type == ItemType.Block && item.MinPick > 0)
                parts.Add($"所需镐力 {item.MinPick}%");

            if (item.Type == ItemType.Buff)
            {
                parts.Add(item.BuffIconSource == BuffIconSource.Vanilla
                    ? $"原版图标 Buff {item.VanillaBuffIconId}"
                    : "DIY 图标");
                if (item.Flow?.Blocks.Count > 0)
                    parts.Add($"流程 {item.Flow.Blocks.Count} 节点");
            }

            if (item.Type == ItemType.Accessory)
            {
                AppendAccessoryStatSummary(parts, "近战", item.AccessoryMeleeDamage, item.AccessoryMeleeSpeed, item.AccessoryMeleeCrit);
                AppendAccessoryStatSummary(parts, "魔法", item.AccessoryMagicDamage, item.AccessoryMagicSpeed, item.AccessoryMagicCrit);
                AppendAccessoryStatSummary(parts, "远程", item.AccessoryRangedDamage, item.AccessoryRangedSpeed, item.AccessoryRangedCrit);
                AppendAccessoryStatSummary(parts, "召唤", item.AccessorySummonDamage, item.AccessorySummonSpeed, item.AccessorySummonCrit);
                if (item.AccessoryDefense != 0) parts.Add($"防御 {item.AccessoryDefense:+#;-#;0}");
                if (item.AccessoryDamageReduction > 0) parts.Add($"减伤 {item.AccessoryDamageReduction}%");
            }

            if (item.Type != ItemType.Buff)
            {
                if (item.Value > 0) parts.Add($"价值 {item.Value}");
                if (item.Type != ItemType.Projectile && item.Recipe.Enabled && item.Recipe.Ingredients.Count > 0)
                    parts.Add($"配方 {item.Recipe.Ingredients.Count} 材料 / {item.Recipe.CraftingStationDisplay}");
                if (item.IsMultiFrameTexture && item.TextureFrameCount > 1)
                    parts.Add($"多帧 {item.TextureFrameCount}");
                parts.Add($"尺寸 {item.Width}x{item.Height}");
            }

            return string.Join("   /   ", parts);
        }

        private static void AppendAccessoryStatSummary(List<string> parts, string label, int damage, int speed, int crit)
        {
            var stats = new List<string>();
            if (damage != 0) stats.Add($"伤害 {damage:+#;-#;0}%");
            if (speed != 0) stats.Add($"攻速 {speed:+#;-#;0}%");
            if (crit != 0) stats.Add($"暴击 {crit:+#;-#;0}%");
            if (stats.Count > 0)
                parts.Add($"{label} {string.Join(" ", stats)}");
        }

        private static Color GetItemAccent(ItemType type) => type switch
        {
            ItemType.Tool => Color.FromArgb(247, 180, 64),
            ItemType.Weapon => Color.FromArgb(237, 94, 104),
            ItemType.Block => Color.FromArgb(88, 204, 132),
            ItemType.Item => Color.FromArgb(85, 160, 255),
            ItemType.Buff => Color.FromArgb(164, 122, 235),
            ItemType.Projectile => Color.FromArgb(80, 210, 220),
            ItemType.Accessory => Color.FromArgb(255, 204, 92),
            _ => ClrMuted
        };

        private int GetCardWidth()
        {
            int scrollbarAllowance = flowItems.VerticalScroll.Visible ? SystemInformation.VerticalScrollBarWidth : 0;
            return Math.Max(430, flowItems.ClientSize.Width - flowItems.Padding.Horizontal - scrollbarAllowance - 4);
        }

        private void flowItems_Resize(object? sender, EventArgs e)
        {
            ResizeItemCards();
        }

        private void ResizeItemCards()
        {
            int width = GetCardWidth();
            foreach (Control control in flowItems.Controls)
            {
                if (control is Panel card)
                    card.Width = width;
            }
        }

        private void UpdateEmptyState()
        {
            if (_emptyStateLabel == null)
                return;

            bool isEmpty = _items.Count == 0;
            _emptyStateLabel.Visible = isEmpty;
            flowItems.Visible = !isEmpty;
        }

        private void NewProject_Click(object? sender, EventArgs e)
        {
            if (!ConfirmDiscardCurrentProject())
                return;

            using var dialog = new ProjectNameDialog("创建工程文件", _projectName);
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            using var folderDialog = new FolderBrowserDialog
            {
                Description = "选择新工程文件保存到哪个文件夹"
            };

            if (folderDialog.ShowDialog(this) != DialogResult.OK)
                return;

            ResetProject(dialog.ProjectName);
            _projectFilePath = Path.Combine(folderDialog.SelectedPath, $"{SanitizeFileName(_projectName)}.tmcreator.json");

            try
            {
                SaveProject(_projectFilePath);
                UpdateProjectStatus();
                UIMessageBox.Show($"已创建工程文件：\n{_projectFilePath}");
            }
            catch (Exception ex)
            {
                _projectFilePath = null;
                UpdateProjectStatus();
                UIMessageBox.Show($"创建工程文件失败：{ex.Message}");
            }
        }

        private void OpenProject_Click(object? sender, EventArgs e)
        {
            if (!ConfirmDiscardCurrentProject())
                return;

            using var dialog = new OpenFileDialog
            {
                Title = "打开工程文件",
                Filter = "tmcreator 工程|*.tmcreator.json;*.tmcreator|JSON 文件|*.json|所有文件|*.*"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                string json = File.ReadAllText(dialog.FileName);
                var project = JsonSerializer.Deserialize<ProjectData>(json, ProjectJsonOptions);
                if (project == null)
                {
                    UIMessageBox.Show("工程文件内容为空或格式不正确。");
                    return;
                }

                LoadProject(project, dialog.FileName);
                UIMessageBox.Show($"已打开工程：{_projectName}");
            }
            catch (Exception ex)
            {
                UIMessageBox.Show($"打开工程失败：{ex.Message}");
            }
        }

        private void SaveProject_Click(object? sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_projectName))
                {
                    using var nameDialog = new ProjectNameDialog("设置工程名称", "未命名工程");
                    if (nameDialog.ShowDialog(this) != DialogResult.OK)
                        return;
                    _projectName = nameDialog.ProjectName;
                }

                string filePath = _projectFilePath ?? "";
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    using var folderDialog = new FolderBrowserDialog
                    {
                        Description = "选择保存工程文件的文件夹"
                    };

                    if (folderDialog.ShowDialog(this) != DialogResult.OK)
                        return;

                    filePath = Path.Combine(folderDialog.SelectedPath, $"{SanitizeFileName(_projectName)}.tmcreator.json");
                }

                SaveProject(filePath);
                _projectFilePath = filePath;
                UpdateProjectStatus();
                UIMessageBox.Show($"工程已保存：\n{filePath}");
            }
            catch (Exception ex)
            {
                UIMessageBox.Show($"保存工程失败：{ex.Message}");
            }
        }

        private void RenameProject_Click(object? sender, EventArgs e)
        {
            using var dialog = new ProjectNameDialog("修改项目名称", _projectName);
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            _projectName = dialog.ProjectName;
            UpdateProjectStatus();
        }

        private bool ConfirmDiscardCurrentProject()
        {
            if (_items.Count == 0 && IsDraftEmpty())
                return true;

            var result = MessageBox.Show(
                "当前界面里的工程内容会被新工程或打开的工程替换。\n请确认已经保存需要保留的内容。",
                "替换当前工程",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning);

            return result == DialogResult.OK;
        }

        private void ResetProject(string projectName)
        {
            _projectName = string.IsNullOrWhiteSpace(projectName) ? "未命名工程" : projectName.Trim();
            _projectFilePath = null;
            _items.Clear();
            ClearItemCards();
            ClearInputs();
            UpdateProjectStatus();
            UpdateEmptyState();
        }

        private void LoadProject(ProjectData project, string filePath)
        {
            string projectDir = Path.GetDirectoryName(filePath) ?? AppContext.BaseDirectory;

            _projectName = string.IsNullOrWhiteSpace(project.ProjectName) ? "未命名工程" : project.ProjectName.Trim();
            _projectFilePath = filePath;

            _editingItem = null;
            _items.Clear();
            ClearItemCards();

            foreach (var item in project.Items ?? new List<ModItemData>())
            {
                NormalizeItem(item, projectDir);
                _items.Add(item);
            }

            foreach (var item in _items)
                AddItemCardModern(item);

            ApplyDraft(project.Draft ?? new ProjectDraftData(), projectDir);
            UpdateProjectStatus();
            UpdateEmptyState();
        }

        private void SaveProject(string filePath)
        {
            string projectDir = Path.GetDirectoryName(filePath) ?? AppContext.BaseDirectory;
            Directory.CreateDirectory(projectDir);

            var project = CaptureProject();
            PrepareProjectAssets(project, projectDir);

            string json = JsonSerializer.Serialize(project, ProjectJsonOptions);
            File.WriteAllText(filePath, json, System.Text.Encoding.UTF8);
        }

        private ProjectData CaptureProject()
        {
            return new ProjectData
            {
                ProjectName = _projectName,
                Items = _items.Select(CloneItem).ToList(),
                Draft = CaptureDraft()
            };
        }

        private ProjectDraftData CaptureDraft()
        {
            return new ProjectDraftData
            {
                Name = txtName.Text.Trim(),
                DisplayName = txtDisplayName.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                Type = (ItemType)cmbItemType.SelectedIndex,
                Width = (int)numWidth.Value,
                Height = (int)numHeight.Value,
                Value = (int)numValue.Value,
                Rarity = (int)numRarity.Value,
                Damage = (int)numDamage.Value,
                DamageKind = GetSelectedDamageKind(),
                UseTime = (int)numUseTime.Value,
                UseStyleId = (int)numUseStyleId.Value,
                Knockback = (int)numKnockback.Value,
                CriticalChance = (int)numCriticalChance.Value,
                UsesProjectile = chkUsesProjectile.Checked,
                ProjectileId = (int)numProjectileId.Value,
                ProjectileSpeed = numProjectileSpeed.Value,
                ConsumeOnUse = chkConsumeOnUse.Checked,
                PickaxePower = (int)numPickaxePower.Value,
                AxePower = (int)numAxePower.Value,
                HammerPower = (int)numHammerPower.Value,
                MinPick = (int)numMinPick.Value,
                AccessoryMeleeDamage = (int)numAccessoryMeleeDamage.Value,
                AccessoryMagicDamage = (int)numAccessoryMagicDamage.Value,
                AccessoryRangedDamage = (int)numAccessoryRangedDamage.Value,
                AccessorySummonDamage = (int)numAccessorySummonDamage.Value,
                AccessoryMeleeSpeed = (int)numAccessoryMeleeSpeed.Value,
                AccessoryMagicSpeed = (int)numAccessoryMagicSpeed.Value,
                AccessoryRangedSpeed = (int)numAccessoryRangedSpeed.Value,
                AccessorySummonSpeed = (int)numAccessorySummonSpeed.Value,
                AccessoryMeleeCrit = (int)numAccessoryMeleeCrit.Value,
                AccessoryMagicCrit = (int)numAccessoryMagicCrit.Value,
                AccessoryRangedCrit = (int)numAccessoryRangedCrit.Value,
                AccessorySummonCrit = (int)numAccessorySummonCrit.Value,
                AccessoryDefense = (int)numAccessoryDefense.Value,
                AccessoryDamageReduction = (int)numAccessoryDamageReduction.Value,
                AutoReuse = chkAutoReuse.Checked,
                UseTurn = chkUseTurn.Checked,
                TexturePath = (ItemType)cmbItemType.SelectedIndex == ItemType.Buff && GetSelectedBuffIconSource() == BuffIconSource.Vanilla ? string.Empty : _selectedTexturePath,
                IsMultiFrameTexture = chkMultiFrameTexture.Checked,
                TextureFrameCount = (int)numTextureFrameCount.Value,
                BuffIconSource = GetSelectedBuffIconSource(),
                VanillaBuffIconId = (int)numVanillaBuffIconId.Value,
                Recipe = CloneRecipe(_currentRecipe)
            };
        }

        private void ApplyDraft(ProjectDraftData draft, string projectDir)
        {
            cmbItemType.SelectedIndex = ClampComboIndex((int)draft.Type, cmbItemType.Items.Count, 3);
            txtName.Text = draft.Name ?? string.Empty;
            txtDisplayName.Text = draft.DisplayName ?? string.Empty;
            txtDescription.Text = draft.Description ?? string.Empty;

            SetNumericValue(numWidth, draft.Width);
            SetNumericValue(numHeight, draft.Height);
            SetNumericValue(numValue, draft.Value);
            SetNumericValue(numRarity, draft.Rarity);
            SetNumericValue(numDamage, draft.Damage);
            SetDamageKind(draft.DamageKind);
            SetNumericValue(numUseTime, draft.UseTime);
            SetNumericValue(numUseStyleId, draft.UseStyleId <= 0 ? 1 : draft.UseStyleId);
            SetNumericValue(numKnockback, draft.Knockback);
            SetNumericValue(numCriticalChance, draft.CriticalChance);
            chkUsesProjectile.Checked = draft.UsesProjectile;
            chkConsumeOnUse.Checked = draft.ConsumeOnUse;
            SetNumericValue(numProjectileId, draft.ProjectileId);
            SetNumericValue(numProjectileSpeed, draft.ProjectileSpeed);
            SetNumericValue(numPickaxePower, draft.PickaxePower);
            SetNumericValue(numAxePower, draft.AxePower);
            SetNumericValue(numHammerPower, draft.HammerPower);
            SetNumericValue(numMinPick, draft.MinPick);
            SetNumericValue(numAccessoryMeleeDamage, draft.AccessoryMeleeDamage);
            SetNumericValue(numAccessoryMagicDamage, draft.AccessoryMagicDamage);
            SetNumericValue(numAccessoryRangedDamage, draft.AccessoryRangedDamage);
            SetNumericValue(numAccessorySummonDamage, draft.AccessorySummonDamage);
            SetNumericValue(numAccessoryMeleeSpeed, draft.AccessoryMeleeSpeed);
            SetNumericValue(numAccessoryMagicSpeed, draft.AccessoryMagicSpeed);
            SetNumericValue(numAccessoryRangedSpeed, draft.AccessoryRangedSpeed);
            SetNumericValue(numAccessorySummonSpeed, draft.AccessorySummonSpeed);
            SetNumericValue(numAccessoryMeleeCrit, draft.AccessoryMeleeCrit);
            SetNumericValue(numAccessoryMagicCrit, draft.AccessoryMagicCrit);
            SetNumericValue(numAccessoryRangedCrit, draft.AccessoryRangedCrit);
            SetNumericValue(numAccessorySummonCrit, draft.AccessorySummonCrit);
            SetNumericValue(numAccessoryDefense, draft.AccessoryDefense);
            SetNumericValue(numAccessoryDamageReduction, draft.AccessoryDamageReduction);
            chkAutoReuse.Checked = draft.AutoReuse;
            chkUseTurn.Checked = draft.UseTurn;
            chkMultiFrameTexture.Checked = draft.IsMultiFrameTexture;
            SetNumericValue(numTextureFrameCount, draft.TextureFrameCount <= 0 ? 1 : draft.TextureFrameCount);
            SetBuffIconSource(draft.BuffIconSource);
            SetNumericValue(numVanillaBuffIconId, draft.VanillaBuffIconId <= 0 ? 1 : draft.VanillaBuffIconId);

            _currentRecipe = CloneRecipe(draft.Recipe ?? new RecipeData());
            UpdateRecipeSummary();

            _selectedTexturePath = ResolveProjectPath(draft.TexturePath, projectDir);
            picTexture.Image?.Dispose();
            picTexture.Image = null;
            if (!string.IsNullOrWhiteSpace(_selectedTexturePath) && File.Exists(_selectedTexturePath))
            {
                try
                {
                    picTexture.Image = LoadImageCopy(_selectedTexturePath);
                }
                catch
                {
                    _selectedTexturePath = string.Empty;
                }
            }

            UpdateFieldVisibility();
        }

        private bool IsDraftEmpty()
        {
            return string.IsNullOrWhiteSpace(txtName.Text) &&
                   string.IsNullOrWhiteSpace(txtDisplayName.Text) &&
                   string.IsNullOrWhiteSpace(txtDescription.Text) &&
                   string.IsNullOrWhiteSpace(_selectedTexturePath) &&
                   _currentRecipe.Ingredients.Count == 0 &&
                   numDamage.Value == 0 &&
                   GetAccessoryNumbers().All(number => number.Value == 0) &&
                   numWidth.Value == 20 &&
                   numHeight.Value == 20 &&
                   numValue.Value == 0 &&
                   numRarity.Value == 0;
        }

        private void ClearItemCards()
        {
            foreach (Control control in flowItems.Controls)
                DisposeCardImages(control);
            flowItems.Controls.Clear();
        }

        private void RefreshItemCards()
        {
            ClearItemCards();
            foreach (var item in _items)
                AddItemCardModern(item);
            UpdateEmptyState();
        }

        private void UpdateProjectStatus()
        {
            if (_projectStatusLabel == null)
                return;

            string fileState = string.IsNullOrWhiteSpace(_projectFilePath)
                ? "未保存"
                : Path.GetFileName(_projectFilePath);
            _projectStatusLabel.Text = $"项目：{_projectName}    文件：{fileState}";
        }

        private void PrepareProjectAssets(ProjectData project, string projectDir)
        {
            string assetsDir = Path.Combine(projectDir, "assets");
            foreach (var item in project.Items)
            {
                item.TexturePath = item.Type == ItemType.Buff && item.BuffIconSource == BuffIconSource.Vanilla
                    ? string.Empty
                    : CopyTextureIntoProject(item.TexturePath, projectDir, assetsDir, item.Name);
            }

            project.Draft.TexturePath = project.Draft.Type == ItemType.Buff && project.Draft.BuffIconSource == BuffIconSource.Vanilla
                ? string.Empty
                : CopyTextureIntoProject(project.Draft.TexturePath, projectDir, assetsDir, "draft");
        }

        private static string CopyTextureIntoProject(string texturePath, string projectDir, string assetsDir, string prefix)
        {
            if (string.IsNullOrWhiteSpace(texturePath))
                return string.Empty;

            string source = ResolveProjectPath(texturePath, projectDir);
            if (!File.Exists(source))
                return texturePath;

            Directory.CreateDirectory(assetsDir);
            string extension = Path.GetExtension(source);
            if (string.IsNullOrWhiteSpace(extension))
                extension = ".png";

            string targetName = $"{SanitizeFileName(prefix)}{extension}";
            string target = Path.Combine(assetsDir, targetName);
            if (!string.Equals(Path.GetFullPath(source), Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase))
                File.Copy(source, target, true);
            return Path.GetRelativePath(projectDir, target);
        }

        private static string ResolveProjectPath(string? path, string projectDir)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            return Path.IsPathRooted(path)
                ? path
                : Path.GetFullPath(Path.Combine(projectDir, path));
        }

        private static void NormalizeItem(ModItemData item, string projectDir)
        {
            item.TexturePath = ResolveProjectPath(item.TexturePath, projectDir);
            item.Recipe ??= new RecipeData();
            if (item.VanillaBuffIconId <= 0)
                item.VanillaBuffIconId = 1;
            if (item.UseStyleId <= 0)
                item.UseStyleId = item.UsesProjectile ? 5 : 1;
            if (item.TextureFrameCount <= 0)
                item.TextureFrameCount = 1;
        }

        private static ModItemData CloneItem(ModItemData item)
        {
            return new ModItemData
            {
                Name = item.Name,
                DisplayName = item.DisplayName,
                Description = item.Description,
                Type = item.Type,
                Damage = item.Damage,
                DamageKind = item.DamageKind,
                UseTime = item.UseTime,
                UseStyleId = item.UseStyleId,
                Knockback = item.Knockback,
                CriticalChance = item.CriticalChance,
                UsesProjectile = item.UsesProjectile,
                ProjectileId = item.ProjectileId,
                ProjectileSpeed = item.ProjectileSpeed,
                ConsumeOnUse = item.ConsumeOnUse,
                PickaxePower = item.PickaxePower,
                AxePower = item.AxePower,
                HammerPower = item.HammerPower,
                Width = item.Width,
                Height = item.Height,
                Value = item.Value,
                Rarity = item.Rarity,
                MinPick = item.MinPick,
                AccessoryMeleeDamage = item.AccessoryMeleeDamage,
                AccessoryMagicDamage = item.AccessoryMagicDamage,
                AccessoryRangedDamage = item.AccessoryRangedDamage,
                AccessorySummonDamage = item.AccessorySummonDamage,
                AccessoryMeleeSpeed = item.AccessoryMeleeSpeed,
                AccessoryMagicSpeed = item.AccessoryMagicSpeed,
                AccessoryRangedSpeed = item.AccessoryRangedSpeed,
                AccessorySummonSpeed = item.AccessorySummonSpeed,
                AccessoryMeleeCrit = item.AccessoryMeleeCrit,
                AccessoryMagicCrit = item.AccessoryMagicCrit,
                AccessoryRangedCrit = item.AccessoryRangedCrit,
                AccessorySummonCrit = item.AccessorySummonCrit,
                AccessoryDefense = item.AccessoryDefense,
                AccessoryDamageReduction = item.AccessoryDamageReduction,
                TexturePath = item.TexturePath,
                IsMultiFrameTexture = item.IsMultiFrameTexture,
                TextureFrameCount = item.TextureFrameCount,
                BuffIconSource = item.BuffIconSource,
                VanillaBuffIconId = item.VanillaBuffIconId,
                Flow = CloneFlow(item.Flow),
                AutoReuse = item.AutoReuse,
                UseTurn = item.UseTurn,
                UseAnimation = item.UseAnimation,
                Recipe = CloneRecipe(item.Recipe)
            };
        }

        private static FlowScript? CloneFlow(FlowScript? flow)
        {
            if (flow == null)
                return null;

            return new FlowScript { Blocks = flow.Blocks.Select(CloneBlock).ToList() };
        }

        private static BlockInstance CloneBlock(BlockInstance source)
        {
            return new BlockInstance
            {
                Id = source.Id,
                BlockDefId = source.BlockDefId,
                ParamValues = source.ParamValues.ToDictionary(pair => pair.Key, pair => pair.Value),
                ParamBlocks = source.ParamBlocks.ToDictionary(pair => pair.Key, pair => CloneBlock(pair.Value)),
                TrueBranch = source.TrueBranch.Select(CloneBlock).ToList(),
                FalseBranch = source.FalseBranch.Select(CloneBlock).ToList()
            };
        }

        private static void SetNumericValue(NumericUpDown number, decimal value)
        {
            number.Value = Math.Min(number.Maximum, Math.Max(number.Minimum, value));
        }

        private void SetDamageKind(ModDamageKind kind)
        {
            cmbDamageKind.SelectedIndex = kind switch
            {
                ModDamageKind.Ranged => 1,
                ModDamageKind.Magic => 2,
                ModDamageKind.Summon => 3,
                ModDamageKind.Generic => 4,
                _ => 0
            };
        }

        private void SetBuffIconSource(BuffIconSource source)
        {
            cmbBuffIconSource.SelectedIndex = source == BuffIconSource.Vanilla ? 1 : 0;
        }

        private static int ClampComboIndex(int index, int count, int fallback)
        {
            if (count <= 0)
                return -1;

            if (index < 0 || index >= count)
                return Math.Min(Math.Max(fallback, 0), count - 1);

            return index;
        }

        private static void CardPanel_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel) return;

            using var borderPen = new Pen(ClrSoftBorder);
            e.Graphics.DrawRectangle(borderPen, 0, 0, panel.Width - 1, panel.Height - 1);
        }

        private static void TexturePreview_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel) return;

            using var borderPen = new Pen(ClrSoftBorder);
            e.Graphics.DrawRectangle(borderPen, 0, 0, panel.Width - 1, panel.Height - 1);
        }

        private static Image LoadImageCopy(string path)
        {
            using var stream = File.OpenRead(path);
            using var image = Image.FromStream(stream);
            return new Bitmap(image);
        }

        private static void DisposeCardImages(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                if (child is PictureBox pictureBox)
                {
                    pictureBox.Image?.Dispose();
                    pictureBox.Image = null;
                }

                if (child.HasChildren)
                    DisposeCardImages(child);
            }
        }

        private void btnSelectTexture_Click(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "选择物品贴图",
                Filter = "PNG图片|*.png|所有文件|*.*"
            };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _selectedTexturePath = dlg.FileName;
                try
                {
                    picTexture.Image?.Dispose();
                    picTexture.Image = Image.FromFile(_selectedTexturePath);
                }
                catch
                {
                    UIMessageBox.Show("无法加载图片，请选择有效的PNG文件");
                    _selectedTexturePath = string.Empty;
                }
            }
        }

        private void btnClearTexture_Click(object? sender, EventArgs e)
        {
            _selectedTexturePath = string.Empty;
            picTexture.Image?.Dispose();
            picTexture.Image = null;
        }

        private void cmbItemType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateFieldVisibility();
        }

        private void UpdateFieldVisibility()
        {
            var type = (ItemType)cmbItemType.SelectedIndex;

            bool isTool = type == ItemType.Tool;
            bool isWeapon = type == ItemType.Weapon;
            bool isBlock = type == ItemType.Block;
            bool isBuff = type == ItemType.Buff;
            bool isProjectile = type == ItemType.Projectile;
            bool isAccessory = type == ItemType.Accessory;

            if (_toolSection == null || _combatSection == null || _blockSection == null || _accessorySection == null || _buffSection == null || _basicSection == null || _textureSection == null || _recipeSection == null)
                return;

            _basicSection.Visible = !isBuff;
            _textureSection.Visible = !isBuff || GetSelectedBuffIconSource() == BuffIconSource.Custom;
            _toolSection.Visible = isTool;
            _combatSection.Visible = isTool || isWeapon || isProjectile;
            _accessorySection.Visible = isAccessory;
            _blockSection.Visible = isBlock;
            _buffSection.Visible = isBuff;
            _recipeSection.Visible = !isBuff && !isProjectile;
            btnCreate.Text = _editingItem != null ? "保存修改" : isBuff ? "创建 Buff" : isProjectile ? "创建弹幕" : isAccessory ? "创建饰品" : "创建物品";

            LayoutShell();
        }

        private void UpdateBuffIconVisibility()
        {
            if (cmbItemType.SelectedIndex >= 0 && (ItemType)cmbItemType.SelectedIndex == ItemType.Buff)
                UpdateFieldVisibility();
        }

        private void btnCreate_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                UIMessageBox.Show("请输入内部名称");
                return;
            }

            var item = new ModItemData
            {
                Name = txtName.Text.Trim(),
                DisplayName = string.IsNullOrWhiteSpace(txtDisplayName.Text) ? txtName.Text.Trim() : txtDisplayName.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                Type = (ItemType)cmbItemType.SelectedIndex,
                Width = (int)numWidth.Value,
                Height = (int)numHeight.Value,
                Value = (int)numValue.Value,
                Rarity = (int)numRarity.Value,
                Damage = (int)numDamage.Value,
                UseTime = (int)numUseTime.Value,
                UseAnimation = (int)numUseTime.Value,
                Knockback = (int)numKnockback.Value,
                CriticalChance = (int)numCriticalChance.Value,
                PickaxePower = (int)numPickaxePower.Value,
                AxePower = (int)numAxePower.Value,
                HammerPower = (int)numHammerPower.Value,
                MinPick = (int)numMinPick.Value,
                AutoReuse = chkAutoReuse.Checked,
                UseTurn = chkUseTurn.Checked,
                TexturePath = _selectedTexturePath
            };

            _items.Add(item);
            AddItemCard(item);
            ClearInputs();
        }

        private void ClearInputs()
        {
            _editingItem = null;
            txtName.Text = "";
            txtDisplayName.Text = "";
            txtDescription.Text = "";
            numDamage.Value = 0;
            cmbDamageKind.SelectedIndex = 0;
            numUseTime.Value = 30;
            numUseStyleId.Value = 1;
            numKnockback.Value = 0;
            numCriticalChance.Value = 4;
            chkUsesProjectile.Checked = false;
            chkConsumeOnUse.Checked = false;
            numProjectileId.Value = 1;
            numProjectileSpeed.Value = 10;
            numPickaxePower.Value = 0;
            numAxePower.Value = 0;
            numHammerPower.Value = 0;
            numWidth.Value = 20;
            numHeight.Value = 20;
            numValue.Value = 0;
            numRarity.Value = 0;
            numMinPick.Value = 0;
            foreach (var number in GetAccessoryNumbers())
                number.Value = 0;
            chkAutoReuse.Checked = false;
            chkUseTurn.Checked = false;
            chkMultiFrameTexture.Checked = false;
            numTextureFrameCount.Value = 1;
            cmbBuffIconSource.SelectedIndex = 0;
            numVanillaBuffIconId.Value = 1;
            _currentRecipe = new RecipeData();
            UpdateRecipeSummary();
            _selectedTexturePath = string.Empty;
            picTexture.Image?.Dispose();
            picTexture.Image = null;
            UpdateFieldVisibility();
        }

        private void AddItemCard(ModItemData item)
        {
            var card = new UIPanel
            {
                Size = new Size(680, 90),
                Margin = new Padding(0, 0, 0, 5),
                Text = "",
                Tag = item,
                Style = UIStyle.Black,
                FillColor = ClrCardBg,
                RectColor = Color.FromArgb(70, 70, 70)
            };

            var color = item.Type switch
            {
                ItemType.Tool => Color.FromArgb(255, 180, 80),
                ItemType.Weapon => Color.FromArgb(255, 80, 80),
                ItemType.Block => Color.FromArgb(80, 220, 80),
                ItemType.Item => Color.FromArgb(80, 160, 255),
                _ => Color.Gray
            };

            int xOffset = 10;

            if (!string.IsNullOrEmpty(item.TexturePath))
            {
                try
                {
                    var pic = new PictureBox
                    {
                        Location = new Point(xOffset, 8),
                        Size = new Size(74, 74),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        BackColor = Color.FromArgb(30, 30, 30),
                        Image = Image.FromFile(item.TexturePath)
                    };
                    card.Controls.Add(pic);
                    xOffset = 90;
                }
                catch { }
            }

            var colorBar = new Panel
            {
                Location = new Point(xOffset, 10),
                Size = new Size(4, 70),
                BackColor = color
            };
            card.Controls.Add(colorBar);

            var typeLabel = new UILabel
            {
                Text = $"[{item.TypeDisplay}]",
                Location = new Point(xOffset + 12, 6),
                Size = new Size(60, 20),
                ForeColor = color,
                Font = new Font("微软雅黑", 10F, FontStyle.Bold)
            };

            var nameLabel = new UILabel
            {
                Text = item.DisplayName,
                Location = new Point(xOffset + 12, 28),
                Size = new Size(200, 22),
                ForeColor = ClrText,
                Font = new Font("微软雅黑", 11F, FontStyle.Bold)
            };

            var idLabel = new UILabel
            {
                Text = $"({item.Name})",
                Location = new Point(xOffset + 12, 50),
                Size = new Size(200, 18),
                ForeColor = ClrSubText,
                Font = new Font("微软雅黑", 8F)
            };

            string stats = BuildStatsText(item);
            var statsLabel = new UILabel
            {
                Text = stats,
                Location = new Point(220, 10),
                Size = new Size(350, 70),
                ForeColor = ClrSubText,
                Font = new Font("微软雅黑", 9F)
            };

            var flowBtn = new UIButton
            {
                Text = "流程",
                Location = new Point(510, 30),
                Size = new Size(60, 28),
                Font = new Font("微软雅黑", 9F, FontStyle.Bold),
                Style = UIStyle.Green,
                Tag = item
            };
            flowBtn.Click += (s, e) =>
            {
                var form = new VisualScriptForm(item);
                form.ShowDialog();
            };

            var deleteBtn = new UIButton
            {
                Text = "删除",
                Location = new Point(580, 30),
                Size = new Size(70, 28),
                Tag = item,
                Font = new Font("微软雅黑", 9F)
            };
            deleteBtn.Click += (s, e) =>
            {
                _items.Remove(item);
                flowItems.Controls.Remove(card);
            };

            card.Controls.Add(typeLabel);
            card.Controls.Add(nameLabel);
            card.Controls.Add(idLabel);
            card.Controls.Add(statsLabel);
            card.Controls.Add(flowBtn);
            card.Controls.Add(deleteBtn);

            flowItems.Controls.Add(card);
            flowItems.Controls.SetChildIndex(card, 0);
        }

        private static string BuildStatsText(ModItemData item)
        {
            var parts = new List<string>();

            if (item.Type == ItemType.Tool || item.Type == ItemType.Weapon)
            {
                if (item.Damage > 0) parts.Add($"伤害: {item.Damage}");
                parts.Add($"使用时间: {item.UseTime}");
                if (item.Knockback > 0) parts.Add($"击退: {item.Knockback}");
                if (item.CriticalChance > 0) parts.Add($"暴击: {item.CriticalChance}%");
            }

            if (item.Type == ItemType.Tool)
            {
                if (item.PickaxePower > 0) parts.Add($"镐力: {item.PickaxePower}%");
                if (item.AxePower > 0) parts.Add($"斧力: {item.AxePower}%");
                if (item.HammerPower > 0) parts.Add($"锤力: {item.HammerPower}%");
            }

            if (item.Type == ItemType.Block)
            {
                if (item.MinPick > 0) parts.Add($"所需镐力: {item.MinPick}%");
            }

            if (item.Value > 0) parts.Add($"价值: {item.Value}");
            parts.Add($"尺寸: {item.Width}x{item.Height}");

            return string.Join(" | ", parts);
        }

        private void btnExport_Click(object? sender, EventArgs e)
        {
            if (_items.Count == 0)
            {
                UIMessageBox.Show("没有可导出的物品");
                return;
            }

            var dialog = new FolderBrowserDialog
            {
                Description = "选择导出目录 (tmodloader mod源码文件夹)"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            string baseDir = dialog.SelectedPath;
            string itemsDir = Path.Combine(baseDir, "Items");
            string locDir = Path.Combine(baseDir, "Localization");
            string locFile = Path.Combine(locDir, "en-US.hjson");

            try
            {
                Directory.CreateDirectory(itemsDir);
                Directory.CreateDirectory(locDir);

                var sbItems = new System.Text.StringBuilder();
                var sbLoc = new System.Text.StringBuilder();
                string projectCodeName = GetProjectCodeName();
                sbLoc.AppendLine("Mods: {");
                sbLoc.AppendLine($"  {projectCodeName}: {{");
                sbLoc.AppendLine("    Items: {");

                foreach (var item in _items)
                {
                    string className = SanitizeClassName(item.Name);
                    string csFile = Path.Combine(itemsDir, $"{className}.cs");
                    string code = GenerateItemCode(item, className);

                    File.WriteAllText(csFile, code, System.Text.Encoding.UTF8);

                    if (!string.IsNullOrEmpty(item.TexturePath) && File.Exists(item.TexturePath))
                    {
                        string texFile = Path.Combine(itemsDir, $"{className}.png");
                        File.Copy(item.TexturePath, texFile, true);
                        sbItems.AppendLine($"  - 导出: {className}.cs + {className}.png");
                    }
                    else
                    {
                        sbItems.AppendLine($"  - 导出: {className}.cs");
                    }

                    sbLoc.AppendLine($"      {className}.DisplayName: {item.DisplayName}");
                    if (!string.IsNullOrEmpty(item.Description))
                        sbLoc.AppendLine($"      {className}.Tooltip: {item.Description}");
                }

                sbLoc.AppendLine("    }");
                sbLoc.AppendLine("  }");
                sbLoc.AppendLine("}");

                File.WriteAllText(locFile, sbLoc.ToString(), System.Text.Encoding.UTF8);

                string msg = $"成功导出 {_items.Count} 个物品到:\n{baseDir}\n\n生成的物品:\n{sbItems}";
                UIMessageBox.Show(msg);
            }
            catch (Exception ex)
            {
                UIMessageBox.Show($"导出失败: {ex.Message}");
            }
        }

        private static string SanitizeClassName(string name)
        {
            var sb = new System.Text.StringBuilder();
            bool capitalize = true;

            foreach (char c in name)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    sb.Append(capitalize ? char.ToUpperInvariant(c) : c);
                    capitalize = false;
                }
                else if (c == ' ')
                {
                    capitalize = true;
                }
            }

            string result = sb.ToString();
            if (result.Length == 0) return "CustomItem";
            if (char.IsDigit(result[0])) result = "Item" + result;
            return result;
        }

        private static string SanitizeFileName(string name)
        {
            string cleaned = string.Join("_", name.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).Trim();
            if (string.IsNullOrWhiteSpace(cleaned))
                return "tmcreator_project";

            return cleaned;
        }

        private string GetProjectCodeName() => SanitizeClassName(_projectName);

        private string GenerateItemCode(ModItemData item, string className)
        {
            var sb = new System.Text.StringBuilder();
            bool hasFlow = item.Flow?.Blocks.Count > 0;
            bool hasAccessoryFlow = item.Type == ItemType.Accessory && HasFlowEvents(item.Flow, AccessoryFlowEventIds, "accessory_wearing");
            bool hasAnimation = item.IsMultiFrameTexture && item.TextureFrameCount > 1;
            if (hasFlow)
            {
                sb.AppendLine("using System;");
                sb.AppendLine("using Microsoft.Xna.Framework;");
            }
            sb.AppendLine("using Terraria;");
            if (hasFlow || hasAnimation)
            {
                sb.AppendLine("using Terraria.DataStructures;");
            }
            if (hasFlow)
            {
                sb.AppendLine("using Terraria.Audio;");
                sb.AppendLine("using Terraria.Localization;");
            }
            sb.AppendLine("using Terraria.ID;");
            sb.AppendLine("using Terraria.ModLoader;");
            sb.AppendLine();
            sb.AppendLine($"namespace {GetProjectCodeName()}.Items");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {className} : ModItem");
            sb.AppendLine("    {");
            if (hasFlow)
            {
                sb.AppendLine("        private readonly System.Collections.Generic.Dictionary<string, float> _flowVariables = new();");
                sb.AppendLine();
            }

            if (hasAnimation)
            {
                sb.AppendLine("        public override void SetStaticDefaults()");
                sb.AppendLine("        {");
                sb.AppendLine($"            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(5, {Math.Max(2, item.TextureFrameCount)}));");
                sb.AppendLine("            ItemID.Sets.AnimatesAsSoul[Type] = true;");
                sb.AppendLine("        }");
                sb.AppendLine();
            }

            sb.AppendLine("        public override void SetDefaults()");
            sb.AppendLine("        {");

            sb.AppendLine($"            Item.width = {item.Width};");
            sb.AppendLine($"            Item.height = {item.Height};");
            sb.AppendLine($"            Item.value = {item.Value};");
            sb.AppendLine(GetRarityLine(item.Rarity));
            sb.AppendLine($"            Item.maxStack = {(item.Type == ItemType.Block ? "9999" : "1")};");

            if (item.Type == ItemType.Tool || item.Type == ItemType.Weapon)
            {
                if (item.Damage > 0)
                {
                    sb.AppendLine($"            Item.damage = {item.Damage};");
                    sb.AppendLine($"            Item.DamageType = {GetDamageClassExpression(item.DamageKind)};");
                }
                sb.AppendLine($"            Item.useTime = {item.UseTime};");
                sb.AppendLine($"            Item.useAnimation = {item.UseAnimation};");
                sb.AppendLine($"            Item.useStyle = {GetUseStyleId(item)};");
                sb.AppendLine(item.UsesProjectile
                    ? "            Item.UseSound = SoundID.Item5;"
                    : "            Item.UseSound = SoundID.Item1;");

                if (item.Knockback > 0)
                    sb.AppendLine($"            Item.knockBack = {item.Knockback};");

                if (item.CriticalChance > 0)
                    sb.AppendLine($"            Item.crit = {item.CriticalChance};");

                if (item.AutoReuse)
                    sb.AppendLine("            Item.autoReuse = true;");

                if (item.UseTurn)
                    sb.AppendLine("            Item.useTurn = true;");

                if (item.UsesProjectile)
                {
                    sb.AppendLine($"            Item.shoot = {item.ProjectileId};");
                    sb.AppendLine($"            Item.shootSpeed = {FormatFloatLiteral(item.ProjectileSpeed)};");
                    sb.AppendLine("            Item.noMelee = true;");
                }

                if (item.ConsumeOnUse)
                {
                    sb.AppendLine("            Item.consumable = true;");
                    sb.AppendLine("            Item.maxStack = 999;");
                }
            }

            if (item.Type == ItemType.Tool)
            {
                if (item.PickaxePower > 0)
                    sb.AppendLine($"            Item.pick = {item.PickaxePower};");
                if (item.AxePower > 0)
                    sb.AppendLine($"            Item.axe = {item.AxePower};");
                if (item.HammerPower > 0)
                    sb.AppendLine($"            Item.hammer = {item.HammerPower};");
            }

            if (item.Type == ItemType.Block)
            {
                sb.AppendLine("            Item.useTime = 10;");
                sb.AppendLine("            Item.useAnimation = 15;");
                sb.AppendLine("            Item.useStyle = ItemUseStyleID.Swing;");
                sb.AppendLine("            Item.consumable = true;");
                sb.AppendLine("            Item.maxStack = 9999;");
                sb.AppendLine("            Item.createTile = TileID.Dirt; // TODO: 替换为你的方块ID");
            }

            if (item.Type == ItemType.Item)
            {
                sb.AppendLine("            Item.maxStack = 999;");
            }

            if (item.Type == ItemType.Accessory)
            {
                sb.AppendLine("            Item.accessory = true;");
            }

            sb.AppendLine("        }");

            if (item.Type == ItemType.Accessory && (HasAccessoryStats(item) || hasAccessoryFlow))
            {
                AppendAccessoryUpdateCode(sb, item, hasAccessoryFlow ? $"{className}AccessoryPlayer" : null);
            }

            if (HasRecipe(item))
            {
                AppendRecipeCode(sb, item);
            }

            if (hasFlow && item.Type != ItemType.Accessory)
            {
                AppendFlowCode(sb, item.Flow!, $"{className}FlowStatsPlayer", $"{className}FlowStatsNpc");
            }

            sb.AppendLine("    }");

            if (hasAccessoryFlow)
            {
                AppendAccessoryFlowCode(sb, item.Flow!, $"{className}AccessoryPlayer", $"{className}FlowStatsPlayer", $"{className}FlowStatsNpc");
                AppendFlowTempStatsPlayerClass(sb, $"{className}FlowStatsPlayer");
                AppendFlowTempStatsNpcClass(sb, $"{className}FlowStatsNpc");
            }
            else if (hasFlow && item.Type != ItemType.Accessory)
            {
                AppendFlowTempStatsPlayerClass(sb, $"{className}FlowStatsPlayer");
                AppendFlowTempStatsNpcClass(sb, $"{className}FlowStatsNpc");
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GenerateBuffCode(ModItemData item, string className)
        {
            var sb = new System.Text.StringBuilder();
            bool hasFlow = HasFlowEvents(item.Flow, BuffFlowEventIds, "buff_update");

            if (hasFlow)
            {
                sb.AppendLine("using System;");
                sb.AppendLine("using Microsoft.Xna.Framework;");
            }
            sb.AppendLine("using Terraria;");
            if (hasFlow)
            {
                sb.AppendLine("using Terraria.Audio;");
                sb.AppendLine("using Terraria.DataStructures;");
                sb.AppendLine("using Terraria.Localization;");
            }
            sb.AppendLine("using Terraria.ID;");
            sb.AppendLine("using Terraria.ModLoader;");
            sb.AppendLine();
            sb.AppendLine($"namespace {GetProjectCodeName()}.Buffs");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {className} : ModBuff");
            sb.AppendLine("    {");
            if (item.BuffIconSource == BuffIconSource.Vanilla)
                sb.AppendLine($"        public override string Texture => \"Terraria/Images/Buff_{Math.Max(1, item.VanillaBuffIconId)}\";");
            sb.AppendLine("        public override void SetStaticDefaults()");
            sb.AppendLine("        {");
            sb.AppendLine("            Main.buffNoSave[Type] = false;");
            sb.AppendLine("            Main.debuff[Type] = false;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");

            if (hasFlow)
                AppendBuffFlowCode(sb, item.Flow!, className);

            sb.AppendLine("}");
            return sb.ToString();
        }

        private string GenerateProjectileCode(ModItemData item, string className)
        {
            var sb = new System.Text.StringBuilder();
            bool hasFlow = HasFlowEvents(item.Flow, ProjectileFlowEventIds, "projectile_update");
            bool hasAnimation = item.IsMultiFrameTexture && item.TextureFrameCount > 1;

            if (hasFlow)
            {
                sb.AppendLine("using System;");
                sb.AppendLine("using Microsoft.Xna.Framework;");
            }
            sb.AppendLine("using Terraria;");
            if (hasFlow)
            {
                sb.AppendLine("using Terraria.Audio;");
                sb.AppendLine("using Terraria.DataStructures;");
                sb.AppendLine("using Terraria.Localization;");
            }
            sb.AppendLine("using Terraria.ID;");
            sb.AppendLine("using Terraria.ModLoader;");
            sb.AppendLine();
            sb.AppendLine($"namespace {GetProjectCodeName()}.Projectiles");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {className} : ModProjectile");
            sb.AppendLine("    {");
            if (hasFlow)
            {
                sb.AppendLine("        private readonly System.Collections.Generic.Dictionary<string, float> _flowVariables = new();");
                sb.AppendLine();
            }

            if (hasAnimation)
            {
                sb.AppendLine("        public override void SetStaticDefaults()");
                sb.AppendLine("        {");
                sb.AppendLine($"            Main.projFrames[Type] = {Math.Max(2, item.TextureFrameCount)};");
                sb.AppendLine("        }");
                sb.AppendLine();
            }

            sb.AppendLine("        public override void SetDefaults()");
            sb.AppendLine("        {");
            sb.AppendLine($"            Projectile.width = {item.Width};");
            sb.AppendLine($"            Projectile.height = {item.Height};");
            sb.AppendLine("            Projectile.friendly = true;");
            sb.AppendLine("            Projectile.hostile = false;");
            sb.AppendLine($"            Projectile.DamageType = {GetDamageClassExpression(item.DamageKind)};");
            if (item.Damage > 0)
                sb.AppendLine($"            Projectile.damage = {item.Damage};");
            sb.AppendLine($"            Projectile.timeLeft = {Math.Max(1, item.UseTime <= 0 ? 600 : item.UseTime * 60)};");
            sb.AppendLine("            Projectile.penetrate = 1;");
            if (item.Knockback > 0)
                sb.AppendLine($"            Projectile.knockBack = {item.Knockback};");
            sb.AppendLine("        }");

            if (hasFlow || hasAnimation)
                AppendProjectileFlowCode(sb, item.Flow, hasAnimation, Math.Max(2, item.TextureFrameCount), $"{className}FlowStatsPlayer", $"{className}FlowStatsNpc");

            sb.AppendLine("    }");

            if (hasFlow)
            {
                AppendFlowTempStatsPlayerClass(sb, $"{className}FlowStatsPlayer");
                AppendFlowTempStatsNpcClass(sb, $"{className}FlowStatsNpc");
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        private static bool HasRecipe(ModItemData item)
        {
            return item.Recipe.Enabled && item.Recipe.Ingredients.Count > 0;
        }

        private static bool HasAccessoryStats(ModItemData item)
        {
            return item.AccessoryMeleeDamage != 0 ||
                   item.AccessoryMagicDamage != 0 ||
                   item.AccessoryRangedDamage != 0 ||
                   item.AccessorySummonDamage != 0 ||
                   item.AccessoryMeleeSpeed != 0 ||
                   item.AccessoryMagicSpeed != 0 ||
                   item.AccessoryRangedSpeed != 0 ||
                   item.AccessorySummonSpeed != 0 ||
                   item.AccessoryMeleeCrit != 0 ||
                   item.AccessoryMagicCrit != 0 ||
                   item.AccessoryRangedCrit != 0 ||
                   item.AccessorySummonCrit != 0 ||
                   item.AccessoryDefense != 0 ||
                   item.AccessoryDamageReduction > 0;
        }

        private static void AppendAccessoryUpdateCode(System.Text.StringBuilder sb, ModItemData item, string? accessoryFlowPlayerClassName)
        {
            sb.AppendLine();
            sb.AppendLine("        public override void UpdateAccessory(Player player, bool hideVisual)");
            sb.AppendLine("        {");
            AppendAccessoryDamageClassCode(sb, "DamageClass.Melee", item.AccessoryMeleeDamage, item.AccessoryMeleeSpeed, item.AccessoryMeleeCrit);
            AppendAccessoryDamageClassCode(sb, "DamageClass.Magic", item.AccessoryMagicDamage, item.AccessoryMagicSpeed, item.AccessoryMagicCrit);
            AppendAccessoryDamageClassCode(sb, "DamageClass.Ranged", item.AccessoryRangedDamage, item.AccessoryRangedSpeed, item.AccessoryRangedCrit);
            AppendAccessoryDamageClassCode(sb, "DamageClass.Summon", item.AccessorySummonDamage, item.AccessorySummonSpeed, item.AccessorySummonCrit);

            if (item.AccessoryDefense != 0)
                sb.AppendLine($"            player.statDefense += {item.AccessoryDefense};");
            if (item.AccessoryDamageReduction > 0)
                sb.AppendLine($"            player.endurance += {FormatPercentLiteral(item.AccessoryDamageReduction)};");

            if (!string.IsNullOrWhiteSpace(accessoryFlowPlayerClassName))
                sb.AppendLine($"            player.GetModPlayer<{accessoryFlowPlayerClassName}>().UpdateEquippedAccessory();");

            sb.AppendLine("        }");
        }

        private static void AppendAccessoryDamageClassCode(System.Text.StringBuilder sb, string damageClass, int damage, int speed, int crit)
        {
            if (damage != 0)
                sb.AppendLine($"            player.GetDamage({damageClass}) += {FormatPercentLiteral(damage)};");
            if (speed != 0)
                sb.AppendLine($"            player.GetAttackSpeed({damageClass}) += {FormatPercentLiteral(speed)};");
            if (crit != 0)
                sb.AppendLine($"            player.GetCritChance({damageClass}) += {crit};");
        }

        private static bool HasFlowEvents(FlowScript? flow, HashSet<string> eventIds, string defaultEventId)
        {
            if (flow == null || flow.Blocks.Count == 0)
                return false;

            return BuildFlowEventGroups(flow.Blocks, defaultEventId)
                .Any(group => eventIds.Contains(group.EventId));
        }

        private static void AppendRecipeCode(System.Text.StringBuilder sb, ModItemData item)
        {
            sb.AppendLine();
            sb.AppendLine("        public override void AddRecipes()");
            sb.AppendLine("        {");
            sb.AppendLine("            Recipe recipe = CreateRecipe();");

            foreach (var ingredient in item.Recipe.Ingredients)
            {
                int stack = Math.Max(1, ingredient.Stack);
                sb.AppendLine($"            recipe.AddIngredient({ingredient.ItemId}, {stack});");
            }

            var station = RecipeStationRegistry.Find(item.Recipe.CraftingStationKey);
            if (!string.IsNullOrWhiteSpace(station?.TileIdExpression))
                sb.AppendLine($"            recipe.AddTile({station.TileIdExpression});");

            sb.AppendLine("            recipe.Register();");
            sb.AppendLine("        }");
        }

        private static string GetDamageClassExpression(ModDamageKind damageKind)
        {
            return damageKind switch
            {
                ModDamageKind.Ranged => "DamageClass.Ranged",
                ModDamageKind.Magic => "DamageClass.Magic",
                ModDamageKind.Summon => "DamageClass.Summon",
                ModDamageKind.Generic => "DamageClass.Generic",
                _ => "DamageClass.Melee"
            };
        }

        private static string FormatFloatLiteral(decimal value)
        {
            return $"{value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}f";
        }

        private static string FormatPercentLiteral(int percent)
        {
            return (percent / 100f).ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + "f";
        }

        private static int GetUseStyleId(ModItemData item)
        {
            return item.UseStyleId > 0
                ? item.UseStyleId
                : item.UsesProjectile ? 5 : 1;
        }

        private static void AppendFlowCode(System.Text.StringBuilder sb, FlowScript flow, string tempStatPlayerClassName, string tempStatNpcClassName)
        {
            var eventGroups = BuildFlowEventGroups(flow.Blocks, "on_use");
            if (eventGroups.Count == 0)
                return;

            sb.AppendLine();
            sb.AppendLine("        // Generated visual flow logic.");
            AppendUseItemFlow(sb, eventGroups.Where(group => group.EventId == "on_use").ToList());
            AppendHoldItemFlow(sb, eventGroups.Where(group => group.EventId == "while_held").ToList());
            AppendOnHitNpcFlow(sb, eventGroups.Where(group => group.EventId == "on_hit_npc").ToList());
            AppendOnHitPvpFlow(sb, eventGroups.Where(group => group.EventId == "on_hit_pvp").ToList());
            AppendFlowHelpers(sb, tempStatPlayerClassName, tempStatNpcClassName);
        }

        private static List<FlowEventGroup> BuildFlowEventGroups(IEnumerable<BlockInstance> blocks, string defaultEventId)
        {
            var groups = new List<FlowEventGroup>();
            FlowEventGroup? current = null;

            foreach (var block in blocks)
            {
                var definition = BlockRegistry.Get(block.BlockDefId);
                if (definition?.Category == BlockCategory.Event)
                {
                    current = new FlowEventGroup { EventId = block.BlockDefId, EventBlock = block };
                    current.Blocks.AddRange(block.TrueBranch);
                    groups.Add(current);
                    continue;
                }

                if (current == null)
                {
                    current = new FlowEventGroup { EventId = defaultEventId };
                    groups.Add(current);
                }

                current.Blocks.Add(block);
            }

            return groups.Where(group => group.Blocks.Count > 0).ToList();
        }

        private static void AppendUseItemFlow(System.Text.StringBuilder sb, List<FlowEventGroup> groups)
        {
            if (groups.Count == 0)
                return;

            sb.AppendLine();
            sb.AppendLine("        public override bool? UseItem(Player player)");
            sb.AppendLine("        {");
            sb.AppendLine("            NPC npc = null;");
            sb.AppendLine("            Player targetPlayer = null;");
            AppendFlowGroupBodies(sb, groups, 12);
            sb.AppendLine("            return true;");
            sb.AppendLine("        }");
        }

        private static void AppendHoldItemFlow(System.Text.StringBuilder sb, List<FlowEventGroup> groups)
        {
            if (groups.Count == 0)
                return;

            sb.AppendLine();
            sb.AppendLine("        public override void HoldItem(Player player)");
            sb.AppendLine("        {");
            sb.AppendLine("            NPC npc = null;");
            sb.AppendLine("            Player targetPlayer = null;");
            AppendFlowGroupBodies(sb, groups, 12);
            sb.AppendLine("        }");
        }

        private static void AppendOnHitNpcFlow(System.Text.StringBuilder sb, List<FlowEventGroup> groups)
        {
            if (groups.Count == 0)
                return;

            sb.AppendLine();
            sb.AppendLine("        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)");
            sb.AppendLine("        {");
            sb.AppendLine("            NPC npc = target;");
            sb.AppendLine("            Player targetPlayer = null;");
            AppendFlowGroupBodies(sb, groups, 12);
            sb.AppendLine("        }");
        }

        private static void AppendOnHitPvpFlow(System.Text.StringBuilder sb, List<FlowEventGroup> groups)
        {
            if (groups.Count == 0)
                return;

            sb.AppendLine();
            sb.AppendLine("        public override void OnHitPvp(Player player, Player target, Player.HurtInfo hurtInfo)");
            sb.AppendLine("        {");
            sb.AppendLine("            NPC npc = null;");
            sb.AppendLine("            Player targetPlayer = target;");
            AppendFlowGroupBodies(sb, groups, 12);
            sb.AppendLine("        }");
        }

        private static void AppendAccessoryFlowCode(System.Text.StringBuilder sb, FlowScript flow, string accessoryPlayerClassName, string tempStatPlayerClassName, string tempStatNpcClassName)
        {
            var eventGroups = BuildFlowEventGroups(flow.Blocks, "accessory_wearing");
            var wearingGroups = eventGroups.Where(group => group.EventId == "accessory_wearing").ToList();
            var attackGroups = eventGroups.Where(group => group.EventId == "accessory_attack").ToList();
            var unequipGroups = eventGroups.Where(group => group.EventId == "accessory_unequip").ToList();
            var equipGroups = eventGroups.Where(group => group.EventId == "accessory_equip").ToList();
            if (wearingGroups.Count == 0 && attackGroups.Count == 0 && unequipGroups.Count == 0 && equipGroups.Count == 0)
                return;

            sb.AppendLine();
            sb.AppendLine($"    public class {accessoryPlayerClassName} : ModPlayer");
            sb.AppendLine("    {");
            sb.AppendLine("        private bool _equippedThisTick;");
            sb.AppendLine("        private bool _wasEquipped;");
            sb.AppendLine("        private readonly System.Collections.Generic.Dictionary<string, float> _flowVariables = new();");
            sb.AppendLine();
            sb.AppendLine("        public void UpdateEquippedAccessory()");
            sb.AppendLine("        {");
            sb.AppendLine("            Player player = Player;");
            sb.AppendLine("            NPC npc = null;");
            sb.AppendLine("            Player targetPlayer = player;");
            sb.AppendLine();
            sb.AppendLine("            bool firstEquipTick = !_wasEquipped && !_equippedThisTick;");
            sb.AppendLine("            _equippedThisTick = true;");
            if (equipGroups.Count > 0)
            {
                sb.AppendLine("            if (firstEquipTick)");
                sb.AppendLine("            {");
                AppendFlowGroupBodies(sb, equipGroups, 16, "player.GetSource_FromThis()");
                sb.AppendLine("            }");
            }
            if (wearingGroups.Count > 0)
                AppendFlowGroupBodies(sb, wearingGroups, 12, "player.GetSource_FromThis()");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public override void ResetEffects()");
            sb.AppendLine("        {");
            sb.AppendLine("            _equippedThisTick = false;");
            sb.AppendLine("        }");

            if (attackGroups.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)");
                sb.AppendLine("        {");
                sb.AppendLine("            if (!_equippedThisTick && !_wasEquipped)");
                sb.AppendLine("                return;");
                sb.AppendLine();
                sb.AppendLine("            Player player = Player;");
                sb.AppendLine("            NPC npc = target;");
                sb.AppendLine("            Player targetPlayer = null;");
                AppendFlowGroupBodies(sb, attackGroups, 12, "player.GetSource_FromThis()");
                sb.AppendLine("        }");
            }

            sb.AppendLine();
            sb.AppendLine("        public override void PostUpdate()");
            sb.AppendLine("        {");
            if (unequipGroups.Count > 0)
            {
                sb.AppendLine("            if (!_equippedThisTick && _wasEquipped)");
                sb.AppendLine("            {");
                sb.AppendLine("                Player player = Player;");
                sb.AppendLine("                NPC npc = null;");
                sb.AppendLine("                Player targetPlayer = player;");
                AppendFlowGroupBodies(sb, unequipGroups, 16, "player.GetSource_FromThis()");
                sb.AppendLine("            }");
                sb.AppendLine();
            }
            sb.AppendLine("            _wasEquipped = _equippedThisTick;");
            sb.AppendLine("        }");
            AppendFlowHelpers(sb, tempStatPlayerClassName, tempStatNpcClassName);
            sb.AppendLine("    }");
        }

        private static void AppendBuffFlowCode(System.Text.StringBuilder sb, FlowScript flow, string buffClassName)
        {
            string tempStatPlayerClassName = $"{buffClassName}FlowStatsPlayer";
            string tempStatNpcClassName = $"{buffClassName}FlowStatsNpc";
            var eventGroups = BuildFlowEventGroups(flow.Blocks, "buff_update");
            var gainGroups = eventGroups.Where(group => group.EventId == "buff_on_gain").ToList();
            var updateGroups = eventGroups.Where(group => group.EventId == "buff_update").ToList();
            var endGroups = eventGroups.Where(group => group.EventId == "buff_on_end").ToList();
            if (gainGroups.Count == 0 && updateGroups.Count == 0 && endGroups.Count == 0)
                return;

            sb.AppendLine();
            sb.AppendLine($"    public class {buffClassName}Player : ModPlayer");
            sb.AppendLine("    {");
            sb.AppendLine("        private bool _hadBuff;");
            sb.AppendLine("        private readonly System.Collections.Generic.Dictionary<string, float> _flowVariables = new();");
            sb.AppendLine();
            sb.AppendLine("        public override void PostUpdateBuffs()");
            sb.AppendLine("        {");
            sb.AppendLine($"            bool hasBuff = Player.HasBuff(ModContent.BuffType<{buffClassName}>());");
            sb.AppendLine("            Player player = Player;");
            sb.AppendLine("            NPC npc = null;");
            sb.AppendLine("            Player targetPlayer = player;");
            sb.AppendLine();
            sb.AppendLine("            if (hasBuff)");
            sb.AppendLine("            {");
            if (gainGroups.Count > 0)
            {
                sb.AppendLine("                if (!_hadBuff)");
                sb.AppendLine("                {");
                AppendFlowGroupBodies(sb, gainGroups, 20, "player.GetSource_FromThis()");
                sb.AppendLine("                }");
            }
            if (updateGroups.Count > 0)
                AppendFlowGroupBodies(sb, updateGroups, 16, "player.GetSource_FromThis()");
            sb.AppendLine("            }");

            if (endGroups.Count > 0)
            {
                sb.AppendLine("            else if (_hadBuff)");
                sb.AppendLine("            {");
                AppendFlowGroupBodies(sb, endGroups, 16, "player.GetSource_FromThis()");
                sb.AppendLine("            }");
            }

            sb.AppendLine();
            sb.AppendLine("            _hadBuff = hasBuff;");
            sb.AppendLine("        }");
            AppendFlowHelpers(sb, tempStatPlayerClassName, tempStatNpcClassName);
            sb.AppendLine("    }");

            AppendFlowTempStatsPlayerClass(sb, tempStatPlayerClassName);
            AppendFlowTempStatsNpcClass(sb, tempStatNpcClassName);
        }

        private static void AppendProjectileFlowCode(System.Text.StringBuilder sb, FlowScript? flow, bool hasAnimation, int frameCount, string tempStatPlayerClassName, string tempStatNpcClassName)
        {
            var eventGroups = flow == null
                ? new List<FlowEventGroup>()
                : BuildFlowEventGroups(flow.Blocks, "projectile_update");
            var spawnGroups = eventGroups.Where(group => group.EventId == "projectile_on_spawn").ToList();
            var updateGroups = eventGroups.Where(group => group.EventId == "projectile_update").ToList();
            var hitNpcGroups = eventGroups.Where(group => group.EventId == "projectile_on_hit_npc").ToList();
            var hitPlayerGroups = eventGroups.Where(group => group.EventId == "projectile_on_hit_player").ToList();

            if (spawnGroups.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("        public override void OnSpawn(IEntitySource source)");
                sb.AppendLine("        {");
                sb.AppendLine("            Player player = Main.player[Math.Clamp(Projectile.owner, 0, Main.maxPlayers - 1)];");
                sb.AppendLine("            NPC npc = null;");
                sb.AppendLine("            Player targetPlayer = null;");
                sb.AppendLine("            Projectile projectile = Projectile;");
                AppendFlowGroupBodies(sb, spawnGroups, 12, "player.GetSource_FromThis()", "projectile");
                sb.AppendLine("        }");
            }

            if (hasAnimation || updateGroups.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("        public override void AI()");
                sb.AppendLine("        {");
                if (hasAnimation)
                {
                    sb.AppendLine("            Projectile.frameCounter++;");
                    sb.AppendLine("            if (Projectile.frameCounter >= 5)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                Projectile.frameCounter = 0;");
                    sb.AppendLine($"                Projectile.frame = (Projectile.frame + 1) % {frameCount};");
                    sb.AppendLine("            }");
                    if (updateGroups.Count > 0)
                        sb.AppendLine();
                }
                if (updateGroups.Count > 0)
                {
                    sb.AppendLine("            Player player = Main.player[Math.Clamp(Projectile.owner, 0, Main.maxPlayers - 1)];");
                    sb.AppendLine("            NPC npc = null;");
                    sb.AppendLine("            Player targetPlayer = null;");
                    sb.AppendLine("            Projectile projectile = Projectile;");
                    AppendFlowGroupBodies(sb, updateGroups, 12, "player.GetSource_FromThis()", "projectile");
                }
                sb.AppendLine("        }");
            }

            if (hitNpcGroups.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)");
                sb.AppendLine("        {");
                sb.AppendLine("            Player player = Main.player[Math.Clamp(Projectile.owner, 0, Main.maxPlayers - 1)];");
                sb.AppendLine("            NPC npc = target;");
                sb.AppendLine("            Player targetPlayer = null;");
                sb.AppendLine("            Projectile projectile = Projectile;");
                AppendFlowGroupBodies(sb, hitNpcGroups, 12, "player.GetSource_FromThis()", "projectile");
                sb.AppendLine("        }");
            }

            if (hitPlayerGroups.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("        public override void OnHitPlayer(Player target, Player.HurtInfo info)");
                sb.AppendLine("        {");
                sb.AppendLine("            Player player = Main.player[Math.Clamp(Projectile.owner, 0, Main.maxPlayers - 1)];");
                sb.AppendLine("            NPC npc = null;");
                sb.AppendLine("            Player targetPlayer = target;");
                sb.AppendLine("            Projectile projectile = Projectile;");
                AppendFlowGroupBodies(sb, hitPlayerGroups, 12, "player.GetSource_FromThis()", "projectile");
                sb.AppendLine("        }");
            }

            if (eventGroups.Count > 0)
                AppendFlowHelpers(sb, tempStatPlayerClassName, tempStatNpcClassName);
        }

        private static void AppendFlowGroupBodies(System.Text.StringBuilder sb, IEnumerable<FlowEventGroup> groups, int indent, string sourceExpression = "player.GetSource_ItemUse(Item)", string projectileExpression = "null")
        {
            var context = new FlowGenerationContext(sourceExpression, projectileExpression);
            foreach (var group in groups)
            {
                AppendLine(sb, indent, $"// {GetFlowEventComment(group.EventId)}");
                AppendFlowStatements(sb, group.Blocks, indent, context);
            }
        }

        private static void AppendFlowStatements(System.Text.StringBuilder sb, IEnumerable<BlockInstance> blocks, int indent, FlowGenerationContext context)
        {
            foreach (var block in blocks)
            {
                var definition = BlockRegistry.Get(block.BlockDefId);
                if (definition == null)
                {
                    AppendLine(sb, indent, $"// Unknown flow block: {EscapeComment(block.BlockDefId)}");
                    continue;
                }

                switch (definition.Category)
                {
                    case BlockCategory.Action:
                        AppendActionStatement(sb, block, indent, context);
                        break;
                    case BlockCategory.Condition:
                        AppendConditionStatement(sb, block, indent, context);
                        break;
                    case BlockCategory.Value:
                        AppendLine(sb, indent, $"// Value block \"{EscapeComment(definition.Name)}\" is only used when dropped into a numeric parameter slot.");
                        break;
                    case BlockCategory.Event:
                        AppendLine(sb, indent, $"// Nested event \"{EscapeComment(definition.Name)}\" is ignored. Place event blocks at the top level.");
                        break;
                }
            }
        }

        private static void AppendActionStatement(System.Text.StringBuilder sb, BlockInstance block, int indent, FlowGenerationContext context)
        {
            switch (block.BlockDefId)
            {
                case "deal_damage":
                {
                    string selector = GetParamString(block, "target", "npc");
                    string amount = GetIntExpression(block, "amount", "100");
                    AppendLine(sb, indent, $"Flow_ForEachNpc(player, npc, targetPlayer, \"{EscapeString(selector)}\", flowNpc => Flow_DamageNpc(player, flowNpc, {amount}));");
                    AppendLine(sb, indent, $"Flow_ForEachPlayer(player, targetPlayer, \"{EscapeString(selector)}\", flowPlayer => Flow_DamagePlayer(player, flowPlayer, {amount}));");
                    break;
                }
                case "heal_player":
                {
                    string amount = GetIntExpression(block, "amount", "20");
                    AppendLine(sb, indent, $"Flow_HealPlayer(player, {amount});");
                    break;
                }
                case "restore_mana":
                {
                    string selector = GetParamString(block, "target", "player");
                    string amount = GetIntExpression(block, "amount", "20");
                    AppendLine(sb, indent, $"Flow_ForEachPlayer(player, targetPlayer, \"{EscapeString(selector)}\", flowPlayer => Flow_RestoreMana(flowPlayer, {amount}));");
                    break;
                }
                case "spawn_projectile":
                {
                    string projectile = GetIntExpression(block, "projectile", "1");
                    string damage = GetIntExpression(block, "damage", "50");
                    string speed = GetFloatExpression(block, "speed", "10");
                    string directionVar = context.Next("flowDirection");
                    AppendLine(sb, indent, $"Vector2 {directionVar} = Main.MouseWorld - player.Center;");
                    AppendLine(sb, indent, $"if ({directionVar}.LengthSquared() < 0.01f)");
                    AppendLine(sb, indent, "{");
                    AppendLine(sb, indent + 4, $"{directionVar} = new Vector2(player.direction == 0 ? 1 : player.direction, 0f);");
                    AppendLine(sb, indent, "}");
                    AppendLine(sb, indent, $"{directionVar}.Normalize();");
                    AppendLine(sb, indent, $"Projectile.NewProjectile({context.SourceExpression}, player.Center, {directionVar} * {speed}, {projectile}, {damage}, 0f, player.whoAmI);");
                    break;
                }
                case "set_projectile_speed":
                {
                    string speed = GetFloatExpression(block, "speed", "10");
                    AppendLine(sb, indent, $"Flow_SetProjectileSpeed({context.ProjectileExpression}, player, {speed});");
                    break;
                }
                case "spawn_particles":
                {
                    string selector = GetParamString(block, "target", "player");
                    string particle = GetIntExpression(block, "particle", "15");
                    string amount = GetIntExpression(block, "amount", "12");
                    string speed = GetFloatExpression(block, "speed", "2");
                    AppendLine(sb, indent, $"Flow_ForEachNpc(player, npc, targetPlayer, \"{EscapeString(selector)}\", flowNpc => Flow_SpawnParticles(flowNpc.Center, {particle}, {amount}, {speed}));");
                    AppendLine(sb, indent, $"Flow_ForEachPlayer(player, targetPlayer, \"{EscapeString(selector)}\", flowPlayer => Flow_SpawnParticles(flowPlayer.Center, {particle}, {amount}, {speed}));");
                    break;
                }
                case "add_buff":
                {
                    string selector = GetParamString(block, "target", "player");
                    string buff = GetIntExpression(block, "buff", "1");
                    string duration = GetIntExpression(block, "duration", "60");
                    AppendLine(sb, indent, $"Flow_ForEachNpc(player, npc, targetPlayer, \"{EscapeString(selector)}\", flowNpc => flowNpc.AddBuff({buff}, {duration} * 60));");
                    AppendLine(sb, indent, $"Flow_ForEachPlayer(player, targetPlayer, \"{EscapeString(selector)}\", flowPlayer => flowPlayer.AddBuff({buff}, {duration} * 60));");
                    break;
                }
                case "kill_npc":
                {
                    string selector = GetParamString(block, "target", "npc");
                    AppendLine(sb, indent, $"Flow_ForEachNpc(player, npc, targetPlayer, \"{EscapeString(selector)}\", flowNpc => Flow_DamageNpc(player, flowNpc, 999999));");
                    AppendLine(sb, indent, $"Flow_ForEachPlayer(player, targetPlayer, \"{EscapeString(selector)}\", flowPlayer => Flow_DamagePlayer(player, flowPlayer, 999999));");
                    break;
                }
                case "set_value":
                {
                    string selector = GetParamString(block, "target", "npc");
                    string stat = GetParamString(block, "stat", "life");
                    string value = GetIntExpression(block, "value", "0");
                    AppendLine(sb, indent, $"Flow_ForEachNpc(player, npc, targetPlayer, \"{EscapeString(selector)}\", flowNpc => Flow_SetNpcValue(flowNpc, \"{EscapeString(stat)}\", {value}));");
                    AppendLine(sb, indent, $"Flow_ForEachPlayer(player, targetPlayer, \"{EscapeString(selector)}\", flowPlayer => Flow_SetPlayerValue(flowPlayer, \"{EscapeString(stat)}\", {value}));");
                    break;
                }
                case "temporary_stat":
                {
                    string selector = GetParamString(block, "target", "player");
                    string operation = GetParamString(block, "operation", "add");
                    string amount = GetFloatExpression(block, "amount", "10");
                    string stat = GetParamString(block, "stat", "damage");
                    string damageType = GetParamString(block, "damage_type", "generic");
                    string duration = GetIntExpression(block, "duration", "5");
                    string signedAmount = operation == "subtract" ? $"-({amount})" : amount;
                    string statKey = stat is "damage" or "crit" ? $"{damageType}_{stat}" : stat;
                    AppendLine(sb, indent, $"Flow_ForEachNpc(player, npc, targetPlayer, \"{EscapeString(selector)}\", flowNpc => Flow_AddTemporaryNpcStat(flowNpc, \"{EscapeString(statKey)}\", {signedAmount}, {duration} * 60));");
                    AppendLine(sb, indent, $"Flow_ForEachPlayer(player, targetPlayer, \"{EscapeString(selector)}\", flowPlayer => Flow_AddTemporaryPlayerStat(flowPlayer, \"{EscapeString(statKey)}\", {signedAmount}, {duration} * 60));");
                    break;
                }
                case "create_variable":
                {
                    string name = GetParamString(block, "name", "myValue");
                    string value = GetFloatExpression(block, "value", "0");
                    AppendLine(sb, indent, $"Flow_SetVariable(\"{EscapeString(name)}\", {value});");
                    break;
                }
                case "broadcast":
                {
                    string message = GetParamString(block, "message", "Hello World!");
                    AppendLine(sb, indent, $"Flow_Broadcast(\"{EscapeString(message)}\");");
                    break;
                }
                case "play_sound":
                {
                    string sound = GetIntExpression(block, "sound", "1");
                    AppendLine(sb, indent, $"Flow_PlaySound(player, {sound});");
                    break;
                }
                case "give_item":
                {
                    string itemId = GetIntExpression(block, "item_id", "1");
                    string amount = GetIntExpression(block, "amount", "1");
                    AppendLine(sb, indent, $"player.QuickSpawnItem({context.SourceExpression}, {itemId}, {amount});");
                    break;
                }
                case "teleport":
                {
                    string selector = GetParamString(block, "target", "player");
                    string x = GetFloatExpression(block, "x", "0");
                    string y = GetFloatExpression(block, "y", "0");
                    AppendLine(sb, indent, $"Flow_ForEachNpc(player, npc, targetPlayer, \"{EscapeString(selector)}\", flowNpc => flowNpc.position = new Vector2({x}, {y}));");
                    AppendLine(sb, indent, $"Flow_ForEachPlayer(player, targetPlayer, \"{EscapeString(selector)}\", flowPlayer => flowPlayer.Teleport(new Vector2({x}, {y})));");
                    break;
                }
                default:
                    AppendLine(sb, indent, $"// Unsupported action block: {EscapeComment(block.BlockDefId)}");
                    break;
            }
        }

        private static void AppendConditionStatement(System.Text.StringBuilder sb, BlockInstance block, int indent, FlowGenerationContext context)
        {
            string condition = block.BlockDefId switch
            {
                "if_hp" => BuildHpCondition(block),
                "if_buff" => BuildBuffCondition(block),
                "if_random" => BuildRandomCondition(block),
                "if_value_compare" => BuildValueCompareCondition(block),
                "if_value_equal" => BuildValueEqualCondition(block),
                _ => "false"
            };

            AppendLine(sb, indent, $"if ({condition})");
            AppendLine(sb, indent, "{");
            AppendFlowStatements(sb, block.TrueBranch, indent + 4, context);
            AppendLine(sb, indent, "}");

            if (block.FalseBranch.Count > 0)
            {
                AppendLine(sb, indent, "else");
                AppendLine(sb, indent, "{");
                AppendFlowStatements(sb, block.FalseBranch, indent + 4, context);
                AppendLine(sb, indent, "}");
            }
        }

        private static string BuildHpCondition(BlockInstance block)
        {
            string selector = GetParamString(block, "target", "npc");
            string compare = GetParamString(block, "compare", "<");
            string value = GetIntExpression(block, "value", "50");
            return $"Flow_Compare(Flow_GetLife(player, npc, targetPlayer, \"{EscapeString(selector)}\"), \"{EscapeString(compare)}\", {value})";
        }

        private static string BuildBuffCondition(BlockInstance block)
        {
            string selector = GetParamString(block, "target", "npc");
            string buff = GetIntExpression(block, "buff", "1");
            return $"Flow_HasBuff(player, npc, targetPlayer, \"{EscapeString(selector)}\", {buff})";
        }

        private static string BuildRandomCondition(BlockInstance block)
        {
            string chance = GetFloatExpression(block, "chance", "50");
            return $"Main.rand.NextDouble() * 100.0 < {chance}";
        }

        private static string BuildValueCompareCondition(BlockInstance block)
        {
            string left = GetFloatExpression(block, "left", "0");
            string compare = GetParamString(block, "compare", ">");
            string right = GetFloatExpression(block, "right", "0");
            return $"Flow_CompareFloat({left}, \"{EscapeString(compare)}\", {right})";
        }

        private static string BuildValueEqualCondition(BlockInstance block)
        {
            string left = GetFloatExpression(block, "left", "0");
            string compare = GetParamString(block, "compare", "=");
            string right = GetFloatExpression(block, "right", "0");
            return $"Flow_CompareFloat({left}, \"{EscapeString(compare)}\", {right})";
        }

        private static string GetIntExpression(BlockInstance block, string paramName, string fallback)
        {
            if (block.ParamBlocks.TryGetValue(paramName, out var nestedBlock))
                return GetIntValueBlockExpression(nestedBlock);

            return ToIntLiteral(GetParamString(block, paramName, fallback), fallback);
        }

        private static string GetFloatExpression(BlockInstance block, string paramName, string fallback)
        {
            if (block.ParamBlocks.TryGetValue(paramName, out var nestedBlock))
                return GetValueBlockExpression(nestedBlock);

            return ToFloatLiteral(GetParamString(block, paramName, fallback), fallback);
        }

        private static string GetIntValueBlockExpression(BlockInstance block)
        {
            return $"((int)Math.Round((double)({GetValueBlockExpression(block)})))";
        }

        private static string GetValueBlockExpression(BlockInstance block)
        {
            return block.BlockDefId switch
            {
                "value_constant" => GetFloatExpression(block, "value", "0"),
                "value_math" => GetMathValueBlockExpression(block),
                "value_npc_hp" => $"Flow_GetLife(player, npc, targetPlayer, \"{EscapeString(GetParamString(block, "target", "npc"))}\")",
                "value_player_hp" => "player.statLife",
                "value_player_mana" => "player.statMana",
                "value_variable" => $"Flow_GetVariable(\"{EscapeString(GetParamString(block, "name", "myValue"))}\")",
                _ => "0"
            };
        }

        private static string GetMathValueBlockExpression(BlockInstance block)
        {
            string left = GetFloatExpression(block, "left", "0");
            string right = GetFloatExpression(block, "right", "1");
            string op = GetParamString(block, "operator", "*");

            return op switch
            {
                "+" => $"(({left}) + ({right}))",
                "-" => $"(({left}) - ({right}))",
                "*" => $"(({left}) * ({right}))",
                "/" => $"(({right}) == 0 ? 0 : ({left}) / ({right}))",
                "%" => $"(({left}) * ({right}) / 100f)",
                "=" => $"(({left}) == ({right}) ? 1 : 0)",
                _ => $"(({left}) * ({right}))"
            };
        }

        private static string GetParamString(BlockInstance block, string paramName, string fallback)
        {
            return block.ParamValues.TryGetValue(paramName, out var value) && !string.IsNullOrWhiteSpace(value)
                ? value
                : fallback;
        }

        private static string ToIntLiteral(string value, string fallback)
        {
            if (int.TryParse(value, out var intValue))
                return intValue.ToString(System.Globalization.CultureInfo.InvariantCulture);

            if (double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var doubleValue))
                return ((int)Math.Round(doubleValue)).ToString(System.Globalization.CultureInfo.InvariantCulture);

            return int.TryParse(fallback, out var fallbackValue)
                ? fallbackValue.ToString(System.Globalization.CultureInfo.InvariantCulture)
                : "0";
        }

        private static string ToFloatLiteral(string value, string fallback)
        {
            if (!double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var doubleValue))
                double.TryParse(fallback, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out doubleValue);

            return doubleValue.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + "f";
        }

        private static void AppendFlowHelpers(System.Text.StringBuilder sb, string tempStatPlayerClassName, string tempStatNpcClassName)
        {
            sb.AppendLine();
            sb.AppendLine("        private void Flow_ForEachNpc(Player player, NPC npc, Player targetPlayer, string selector, Action<NPC> action)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (selector == \"npc\")");
            sb.AppendLine("            {");
            sb.AppendLine("                if (npc != null && npc.active)");
            sb.AppendLine("                    action(npc);");
            sb.AppendLine("                return;");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            if (selector == \"all_npc\")");
            sb.AppendLine("            {");
            sb.AppendLine("                for (int i = 0; i < Main.maxNPCs; i++)");
            sb.AppendLine("                {");
            sb.AppendLine("                    NPC candidate = Main.npc[i];");
            sb.AppendLine("                    if (candidate.active && !candidate.friendly && candidate.life > 0)");
            sb.AppendLine("                        action(candidate);");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine("        }");

            sb.AppendLine();
            sb.AppendLine("        private void Flow_ForEachPlayer(Player player, Player targetPlayer, string selector, Action<Player> action)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (selector == \"owner\")");
            sb.AppendLine("            {");
            sb.AppendLine("                action(player);");
            sb.AppendLine("                return;");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            if (selector == \"player\")");
            sb.AppendLine("            {");
            sb.AppendLine("                action(targetPlayer ?? player);");
            sb.AppendLine("                return;");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            if (selector == \"all_player\")");
            sb.AppendLine("            {");
            sb.AppendLine("                for (int i = 0; i < Main.maxPlayers; i++)");
            sb.AppendLine("                {");
            sb.AppendLine("                    Player candidate = Main.player[i];");
            sb.AppendLine("                    if (candidate.active && !candidate.dead)");
            sb.AppendLine("                        action(candidate);");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine("        }");

            sb.AppendLine();
            sb.AppendLine("        private int Flow_GetLife(Player player, NPC npc, Player targetPlayer, string selector)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (selector == \"npc\")");
            sb.AppendLine("                return npc != null && npc.active ? npc.life : 0;");
            sb.AppendLine("            if (selector == \"owner\")");
            sb.AppendLine("                return player.statLife;");
            sb.AppendLine("            if (selector == \"player\")");
            sb.AppendLine("                return (targetPlayer ?? player).statLife;");
            sb.AppendLine("            if (selector == \"all_npc\")");
            sb.AppendLine("            {");
            sb.AppendLine("                int total = 0;");
            sb.AppendLine("                for (int i = 0; i < Main.maxNPCs; i++)");
            sb.AppendLine("                {");
            sb.AppendLine("                    NPC candidate = Main.npc[i];");
            sb.AppendLine("                    if (candidate.active && !candidate.friendly && candidate.life > 0)");
            sb.AppendLine("                        total += candidate.life;");
            sb.AppendLine("                }");
            sb.AppendLine("                return total;");
            sb.AppendLine("            }");
            sb.AppendLine("            if (selector == \"all_player\")");
            sb.AppendLine("            {");
            sb.AppendLine("                int total = 0;");
            sb.AppendLine("                for (int i = 0; i < Main.maxPlayers; i++)");
            sb.AppendLine("                {");
            sb.AppendLine("                    Player candidate = Main.player[i];");
            sb.AppendLine("                    if (candidate.active && !candidate.dead)");
            sb.AppendLine("                        total += candidate.statLife;");
            sb.AppendLine("                }");
            sb.AppendLine("                return total;");
            sb.AppendLine("            }");
            sb.AppendLine("            return 0;");
            sb.AppendLine("        }");

            sb.AppendLine();
            sb.AppendLine("        private bool Flow_HasBuff(Player player, NPC npc, Player targetPlayer, string selector, int buff)");
            sb.AppendLine("        {");
            sb.AppendLine("            bool result = false;");
            sb.AppendLine("            Flow_ForEachNpc(player, npc, targetPlayer, selector, flowNpc => result |= flowNpc.HasBuff(buff));");
            sb.AppendLine("            Flow_ForEachPlayer(player, targetPlayer, selector, flowPlayer => result |= flowPlayer.HasBuff(buff));");
            sb.AppendLine("            return result;");
            sb.AppendLine("        }");

            sb.AppendLine();
            sb.AppendLine("        private float Flow_GetVariable(string name)");
            sb.AppendLine("        {");
            sb.AppendLine("            return _flowVariables.TryGetValue(name, out float value) ? value : 0f;");
            sb.AppendLine("        }");

            sb.AppendLine();
            sb.AppendLine("        private void Flow_SetVariable(string name, float value)");
            sb.AppendLine("        {");
            sb.AppendLine("            _flowVariables[name] = value;");
            sb.AppendLine("        }");

            sb.AppendLine();
            sb.AppendLine("        private static bool Flow_Compare(int left, string op, int right)");
            sb.AppendLine("        {");
            sb.AppendLine("            return op switch");
            sb.AppendLine("            {");
            sb.AppendLine("                \">\" => left > right,");
            sb.AppendLine("                \"=\" => left == right,");
            sb.AppendLine("                \">=\" or \"≥\" => left >= right,");
            sb.AppendLine("                \"<=\" or \"≤\" => left <= right,");
            sb.AppendLine("                _ => left < right");
            sb.AppendLine("            };");
            sb.AppendLine("        }");

            sb.AppendLine();
            sb.AppendLine("        private static bool Flow_CompareFloat(float left, string op, float right)");
            sb.AppendLine("        {");
            sb.AppendLine("            return op switch");
            sb.AppendLine("            {");
            sb.AppendLine("                \">\" => left > right,");
            sb.AppendLine("                \"=\" => Math.Abs(left - right) <= 0.0001f,");
            sb.AppendLine("                \"!=\" => Math.Abs(left - right) > 0.0001f,");
            sb.AppendLine("                \">=\" => left >= right,");
            sb.AppendLine("                \"<=\" => left <= right,");
            sb.AppendLine("                _ => left < right");
            sb.AppendLine("            };");
            sb.AppendLine("        }");

            sb.AppendLine();
            sb.AppendLine("        private void Flow_DamageNpc(Player player, NPC target, int damage)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (target == null || !target.active || damage <= 0)");
            sb.AppendLine("                return;");
            sb.AppendLine();
            sb.AppendLine("            int hitDirection = target.Center.X >= player.Center.X ? 1 : -1;");
            sb.AppendLine("            var hitInfo = new NPC.HitInfo");
            sb.AppendLine("            {");
            sb.AppendLine("                Damage = damage,");
            sb.AppendLine("                Knockback = 0f,");
            sb.AppendLine("                HitDirection = hitDirection");
            sb.AppendLine("            };");
            sb.AppendLine("            target.StrikeNPC(hitInfo);");
            sb.AppendLine("        }");

            sb.AppendLine();
            sb.AppendLine("        private void Flow_DamagePlayer(Player player, Player target, int damage)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (target == null || !target.active || target.dead || damage <= 0)");
            sb.AppendLine("                return;");
            sb.AppendLine();
            sb.AppendLine("            int hitDirection = target.Center.X >= player.Center.X ? 1 : -1;");
            sb.AppendLine("            target.Hurt(PlayerDeathReason.ByCustomReason(NetworkText.FromLiteral($\"{target.name} was hit by {player.name}.\")), damage, hitDirection, pvp: true);");
            sb.AppendLine("        }");

            sb.AppendLine();
            sb.AppendLine("        private static void Flow_HealPlayer(Player player, int amount)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (amount <= 0)");
            sb.AppendLine("                return;");
            sb.AppendLine();
            sb.AppendLine("            int before = player.statLife;");
            sb.AppendLine("            player.statLife = Math.Min(player.statLifeMax2, player.statLife + amount);");
            sb.AppendLine("            player.HealEffect(player.statLife - before);");
            sb.AppendLine("        }");

            sb.AppendLine();
            sb.AppendLine("        private static void Flow_RestoreMana(Player player, int amount)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (player == null || !player.active || player.dead || amount <= 0)");
            sb.AppendLine("                return;");
            sb.AppendLine();
            sb.AppendLine("            int before = player.statMana;");
            sb.AppendLine("            player.statMana = Math.Min(player.statManaMax2, player.statMana + amount);");
            sb.AppendLine("            player.ManaEffect(player.statMana - before);");
            sb.AppendLine("        }");

            sb.AppendLine();
            sb.AppendLine("        private static void Flow_SetNpcValue(NPC target, string stat, int value)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (target == null || !target.active)");
            sb.AppendLine("                return;");
            sb.AppendLine();
            sb.AppendLine("            switch (stat)");
            sb.AppendLine("            {");
            sb.AppendLine("                case \"life\": target.life = Math.Clamp(value, 0, target.lifeMax); break;");
            sb.AppendLine("                case \"damage\": target.damage = Math.Max(0, value); break;");
            sb.AppendLine("                case \"defense\": target.defense = Math.Max(0, value); break;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");

            sb.AppendLine();
            sb.AppendLine("        private static void Flow_SetPlayerValue(Player target, string stat, int value)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (target == null || !target.active)");
            sb.AppendLine("                return;");
            sb.AppendLine();
            sb.AppendLine("            switch (stat)");
            sb.AppendLine("            {");
            sb.AppendLine("                case \"life\": target.statLife = Math.Clamp(value, 0, target.statLifeMax2); break;");
            sb.AppendLine("                case \"damage\": break;");
            sb.AppendLine("                case \"defense\": target.statDefense = Player.DefenseStat.Default + Math.Max(0, value); break;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");

            sb.AppendLine();
            sb.AppendLine("        private static void Flow_AddTemporaryPlayerStat(Player target, string stat, float amount, int durationTicks)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (target == null || !target.active || target.dead)");
            sb.AppendLine("                return;");
            sb.AppendLine();
            sb.AppendLine($"            target.GetModPlayer<{tempStatPlayerClassName}>().AddModifier(stat, amount, durationTicks);");
            sb.AppendLine("        }");

            sb.AppendLine();
            sb.AppendLine("        private static void Flow_AddTemporaryNpcStat(NPC target, string stat, float amount, int durationTicks)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (target == null || !target.active)");
            sb.AppendLine("                return;");
            sb.AppendLine();
            sb.AppendLine($"            target.GetGlobalNPC<{tempStatNpcClassName}>().AddModifier(stat, amount, durationTicks);");
            sb.AppendLine("        }");

            sb.AppendLine();
            sb.AppendLine("        private static void Flow_Broadcast(string message)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (Main.netMode != NetmodeID.Server)");
            sb.AppendLine("                Main.NewText(message);");
            sb.AppendLine("        }");

            sb.AppendLine();
            sb.AppendLine("        private static void Flow_PlaySound(Player player, int soundId)");
            sb.AppendLine("        {");
            sb.AppendLine("            switch (soundId)");
            sb.AppendLine("            {");
            sb.AppendLine("                case 2: SoundEngine.PlaySound(SoundID.Item2, player.Center); break;");
            sb.AppendLine("                case 3: SoundEngine.PlaySound(SoundID.Item3, player.Center); break;");
            sb.AppendLine("                case 4: SoundEngine.PlaySound(SoundID.Item4, player.Center); break;");
            sb.AppendLine("                case 5: SoundEngine.PlaySound(SoundID.Item5, player.Center); break;");
            sb.AppendLine("                default: SoundEngine.PlaySound(SoundID.Item1, player.Center); break;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");

            sb.AppendLine();
            sb.AppendLine("        private static void Flow_SetProjectileSpeed(Projectile projectile, Player player, float speed)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (projectile == null || !projectile.active)");
            sb.AppendLine("                return;");
            sb.AppendLine();
            sb.AppendLine("            Vector2 direction = projectile.velocity;");
            sb.AppendLine("            if (direction.LengthSquared() < 0.001f)");
            sb.AppendLine("                direction = new Vector2(player != null && player.direction < 0 ? -1f : 1f, 0f);");
            sb.AppendLine("            direction.Normalize();");
            sb.AppendLine("            projectile.velocity = direction * Math.Max(0f, speed);");
            sb.AppendLine("        }");

            sb.AppendLine();
            sb.AppendLine("        private static void Flow_SpawnParticles(Vector2 center, int dustId, int count, float speed)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (Main.netMode == NetmodeID.Server)");
            sb.AppendLine("                return;");
            sb.AppendLine();
            sb.AppendLine("            count = Math.Clamp(count, 1, 200);");
            sb.AppendLine("            speed = Math.Max(0f, speed);");
            sb.AppendLine("            for (int i = 0; i < count; i++)");
            sb.AppendLine("            {");
            sb.AppendLine("                float angle = (float)(Main.rand.NextDouble() * MathHelper.TwoPi);");
            sb.AppendLine("                float magnitude = (float)Main.rand.NextDouble() * speed;");
            sb.AppendLine("                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * magnitude;");
            sb.AppendLine("                Dust.NewDustPerfect(center, dustId, velocity, 150, Color.White, 1f);");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
        }

        private static void AppendFlowTempStatsPlayerClass(System.Text.StringBuilder sb, string className)
        {
            sb.AppendLine();
            sb.AppendLine($"    public class {className} : ModPlayer");
            sb.AppendLine("    {");
            sb.AppendLine("        private readonly System.Collections.Generic.List<FlowTimedStatModifier> _modifiers = new();");
            sb.AppendLine();
            sb.AppendLine("        public void AddModifier(string stat, float amount, int durationTicks)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (string.IsNullOrWhiteSpace(stat) || Math.Abs(amount) <= 0.0001f)");
            sb.AppendLine("                return;");
            sb.AppendLine();
            sb.AppendLine("            for (int i = 0; i < _modifiers.Count; i++)");
            sb.AppendLine("            {");
            sb.AppendLine("                FlowTimedStatModifier existing = _modifiers[i];");
            sb.AppendLine("                if (existing.Stat == stat && Math.Abs(existing.Amount - amount) <= 0.0001f)");
            sb.AppendLine("                {");
            sb.AppendLine("                    existing.TimeLeft = Math.Max(existing.TimeLeft, Math.Max(1, durationTicks));");
            sb.AppendLine("                    _modifiers[i] = existing;");
            sb.AppendLine("                    return;");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            _modifiers.Add(new FlowTimedStatModifier");
            sb.AppendLine("            {");
            sb.AppendLine("                Stat = stat,");
            sb.AppendLine("                Amount = amount,");
            sb.AppendLine("                TimeLeft = Math.Max(1, durationTicks)");
            sb.AppendLine("            });");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public override void ResetEffects()");
            sb.AppendLine("        {");
            sb.AppendLine("            for (int i = _modifiers.Count - 1; i >= 0; i--)");
            sb.AppendLine("            {");
            sb.AppendLine("                FlowTimedStatModifier modifier = _modifiers[i];");
            sb.AppendLine("                ApplyModifier(modifier.Stat, modifier.Amount);");
            sb.AppendLine("                modifier.TimeLeft--;");
            sb.AppendLine();
            sb.AppendLine("                if (modifier.TimeLeft <= 0)");
            sb.AppendLine("                    _modifiers.RemoveAt(i);");
            sb.AppendLine("                else");
            sb.AppendLine("                    _modifiers[i] = modifier;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        private void ApplyModifier(string stat, float amount)");
            sb.AppendLine("        {");
            sb.AppendLine("            switch (stat)");
            sb.AppendLine("            {");
            sb.AppendLine("                case \"defense\": Player.statDefense += (int)Math.Round(amount); break;");
            sb.AppendLine("                case \"damage_reduction\": Player.endurance += amount / 100f; break;");
            sb.AppendLine("                case \"melee_damage\": Player.GetDamage(DamageClass.Melee) += amount / 100f; break;");
            sb.AppendLine("                case \"magic_damage\": Player.GetDamage(DamageClass.Magic) += amount / 100f; break;");
            sb.AppendLine("                case \"ranged_damage\": Player.GetDamage(DamageClass.Ranged) += amount / 100f; break;");
            sb.AppendLine("                case \"summon_damage\": Player.GetDamage(DamageClass.Summon) += amount / 100f; break;");
            sb.AppendLine("                case \"generic_damage\": Player.GetDamage(DamageClass.Generic) += amount / 100f; break;");
            sb.AppendLine("                case \"melee_crit\": Player.GetCritChance(DamageClass.Melee) += amount; break;");
            sb.AppendLine("                case \"magic_crit\": Player.GetCritChance(DamageClass.Magic) += amount; break;");
            sb.AppendLine("                case \"ranged_crit\": Player.GetCritChance(DamageClass.Ranged) += amount; break;");
            sb.AppendLine("                case \"summon_crit\": Player.GetCritChance(DamageClass.Summon) += amount; break;");
            sb.AppendLine("                case \"generic_crit\": Player.GetCritChance(DamageClass.Generic) += amount; break;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        private struct FlowTimedStatModifier");
            sb.AppendLine("        {");
            sb.AppendLine("            public string Stat;");
            sb.AppendLine("            public float Amount;");
            sb.AppendLine("            public int TimeLeft;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
        }

        private static void AppendFlowTempStatsNpcClass(System.Text.StringBuilder sb, string className)
        {
            sb.AppendLine();
            sb.AppendLine($"    public class {className} : GlobalNPC");
            sb.AppendLine("    {");
            sb.AppendLine("        private readonly System.Collections.Generic.List<FlowTimedStatModifier> _modifiers = new();");
            sb.AppendLine();
            sb.AppendLine("        public override bool InstancePerEntity => true;");
            sb.AppendLine();
            sb.AppendLine("        public void AddModifier(string stat, float amount, int durationTicks)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (string.IsNullOrWhiteSpace(stat) || Math.Abs(amount) <= 0.0001f)");
            sb.AppendLine("                return;");
            sb.AppendLine();
            sb.AppendLine("            for (int i = 0; i < _modifiers.Count; i++)");
            sb.AppendLine("            {");
            sb.AppendLine("                FlowTimedStatModifier existing = _modifiers[i];");
            sb.AppendLine("                if (existing.Stat == stat && Math.Abs(existing.Amount - amount) <= 0.0001f)");
            sb.AppendLine("                {");
            sb.AppendLine("                    existing.TimeLeft = Math.Max(existing.TimeLeft, Math.Max(1, durationTicks));");
            sb.AppendLine("                    _modifiers[i] = existing;");
            sb.AppendLine("                    return;");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            _modifiers.Add(new FlowTimedStatModifier");
            sb.AppendLine("            {");
            sb.AppendLine("                Stat = stat,");
            sb.AppendLine("                Amount = amount,");
            sb.AppendLine("                TimeLeft = Math.Max(1, durationTicks)");
            sb.AppendLine("            });");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public override void ResetEffects(NPC npc)");
            sb.AppendLine("        {");
            sb.AppendLine("            for (int i = _modifiers.Count - 1; i >= 0; i--)");
            sb.AppendLine("            {");
            sb.AppendLine("                FlowTimedStatModifier modifier = _modifiers[i];");
            sb.AppendLine("                ApplyModifier(npc, modifier.Stat, modifier.Amount);");
            sb.AppendLine("                modifier.TimeLeft--;");
            sb.AppendLine();
            sb.AppendLine("                if (modifier.TimeLeft <= 0)");
            sb.AppendLine("                    _modifiers.RemoveAt(i);");
            sb.AppendLine("                else");
            sb.AppendLine("                    _modifiers[i] = modifier;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        private static void ApplyModifier(NPC npc, string stat, float amount)");
            sb.AppendLine("        {");
            sb.AppendLine("            switch (stat)");
            sb.AppendLine("            {");
            sb.AppendLine("                case \"defense\": npc.defense += (int)Math.Round(amount); break;");
            sb.AppendLine("                case \"melee_damage\":");
            sb.AppendLine("                case \"magic_damage\":");
            sb.AppendLine("                case \"ranged_damage\":");
            sb.AppendLine("                case \"summon_damage\":");
            sb.AppendLine("                case \"generic_damage\":");
            sb.AppendLine("                    npc.damage = Math.Max(0, npc.damage + (int)Math.Round(amount));");
            sb.AppendLine("                    break;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        private struct FlowTimedStatModifier");
            sb.AppendLine("        {");
            sb.AppendLine("            public string Stat;");
            sb.AppendLine("            public float Amount;");
            sb.AppendLine("            public int TimeLeft;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
        }

        private static string GetFlowEventComment(string eventId) => eventId switch
        {
            "on_hit_npc" => "When this item hits an NPC",
            "on_hit_pvp" => "When this item hits a player",
            "while_held" => "While this item is held",
            "buff_on_gain" => "When this buff is gained",
            "buff_update" => "While this buff is active",
            "buff_on_end" => "When this buff ends",
            "projectile_on_spawn" => "When this projectile is fired",
            "projectile_on_hit_npc" => "When this projectile hits an NPC",
            "projectile_on_hit_player" => "When this projectile hits a player",
            "projectile_update" => "While this projectile exists",
            "accessory_wearing" => "While this accessory is worn",
            "accessory_attack" => "When attacking while this accessory is worn",
            "accessory_unequip" => "When this accessory is removed",
            "accessory_equip" => "When this accessory is equipped",
            _ => "When this item is used"
        };

        private static void AppendLine(System.Text.StringBuilder sb, int spaces, string text)
        {
            sb.Append(' ', spaces);
            sb.AppendLine(text);
        }

        private static string EscapeString(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static string EscapeComment(string value)
        {
            return value.Replace("\r", " ").Replace("\n", " ");
        }

        private sealed class FlowEventGroup
        {
            public string EventId { get; set; } = "on_use";
            public BlockInstance? EventBlock { get; set; }
            public List<BlockInstance> Blocks { get; } = new();
        }

        private sealed class FlowGenerationContext
        {
            private int _index;

            public FlowGenerationContext(string sourceExpression, string projectileExpression)
            {
                SourceExpression = sourceExpression;
                ProjectileExpression = projectileExpression;
            }

            public string SourceExpression { get; }
            public string ProjectileExpression { get; }

            public string Next(string prefix)
            {
                _index++;
                return $"{prefix}{_index}";
            }
        }

        private static string GetRarityLine(int rarity)
        {
            return rarity switch
            {
                -1 => "            Item.rare = ItemRarityID.Expert;",
                0 => "            Item.rare = ItemRarityID.White;",
                1 => "            Item.rare = ItemRarityID.Blue;",
                2 => "            Item.rare = ItemRarityID.Green;",
                3 => "            Item.rare = ItemRarityID.Orange;",
                4 => "            Item.rare = ItemRarityID.LightRed;",
                5 => "            Item.rare = ItemRarityID.Pink;",
                6 => "            Item.rare = ItemRarityID.LightPurple;",
                7 => "            Item.rare = ItemRarityID.Lime;",
                8 => "            Item.rare = ItemRarityID.Yellow;",
                9 => "            Item.rare = ItemRarityID.Cyan;",
                10 => "            Item.rare = ItemRarityID.Red;",
                11 => "            Item.rare = ItemRarityID.Purple;",
                _ => "            Item.rare = ItemRarityID.White;"
            };
        }
    }
}
