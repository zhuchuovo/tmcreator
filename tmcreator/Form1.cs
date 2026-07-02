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
        private Label? _lblCombatUseTime;
        private Label? _lblCombatCritical;
        private Label? _lblCombatDamageKind;
        private Label? _lblProjectileReference;
        private Label? _lblProjectileSpeed;
        private Label? _lblUseStyleId;
        private Label? _lblManaCost;
        private Label? _lblAmmoType;
        private Label? _lblUseSound;
        private Label? _lblWeaponOffsetX;
        private Label? _lblWeaponOffsetY;
        private Label? _rightSubtitleLabel;
        private Label? _emptyStateLabel;
        private readonly UIComboBox cmbDamageKind = new();
        private readonly UIComboBox cmbBuffIconSource = new();
        private readonly UICheckBox chkUsesProjectile = new();
        private readonly UICheckBox chkConsumeOnUse = new();
        private readonly UICheckBox chkMultiFrameTexture = new();
        private readonly UICheckBox chkWhipProjectile = new();
        private readonly UICheckBox chkUsesAmmo = new();
        private readonly System.Windows.Forms.TextBox txtProjectileId = new();
        private readonly System.Windows.Forms.TextBox txtAmmoType = new();
        private readonly System.Windows.Forms.TextBox txtUseSound = new();
        private readonly NumericUpDown numWeaponOffsetX = new();
        private readonly NumericUpDown numWeaponOffsetY = new();
        private readonly NumericUpDown numProjectileSpeed = new();
        private readonly NumericUpDown numVanillaBuffIconId = new();
        private readonly NumericUpDown numUseStyleId = new();
        private readonly NumericUpDown numManaCost = new();
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
        private readonly UIButton btnImportBlock = new();
        private readonly UIButton btnEffectPreview = new();
        private RecipeData _currentRecipe = new();
        private string _projectName = "未命名工程";
        private string _projectDescription = string.Empty;
        private string _projectIconPath = string.Empty;
        private string _buildVersion = "1.4";
        private string _buildAuthor = string.Empty;
        private string _buildHomepage = string.Empty;
        private string? _projectFilePath;

        private static readonly JsonSerializerOptions ProjectJsonOptions = new()
        {
            WriteIndented = true
        };

        private static readonly HashSet<string> BuffFlowEventIds = FlowCodeGenerator.BuffFlowEventIds;
        private static readonly HashSet<string> ProjectileFlowEventIds = FlowCodeGenerator.ProjectileFlowEventIds;
        private static readonly HashSet<string> AccessoryFlowEventIds = FlowCodeGenerator.AccessoryFlowEventIds;
        private static readonly string BuffDefaultFlowEventId = FlowCodeGenerator.BuffDefaultEventId;
        private static readonly string ProjectileDefaultFlowEventId = FlowCodeGenerator.ProjectileDefaultEventId;
        private static readonly string AccessoryDefaultFlowEventId = FlowCodeGenerator.AccessoryDefaultEventId;

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
            ConfigureHeaderButton(btnRenameProject, "修改项目", RenameProject_Click);

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
            _lblCombatUseTime = AddNumericField(_combatSection, "使用时间", numUseTime, 196, 58, 72);
            AddNumericField(_combatSection, "击退", numKnockback, 18, 98, 72);
            _lblCombatCritical = AddNumericField(_combatSection, "暴击率", numCriticalChance, 196, 98, 72);

            _lblCombatDamageKind = AddFieldLabel(_combatSection, "伤害类型", 18, 134, 80);
            cmbDamageKind.Location = new Point(96, 130);
            cmbDamageKind.Size = new Size(114, 30);
            cmbDamageKind.Font = FontBody;
            cmbDamageKind.Items.Clear();
            cmbDamageKind.Items.AddRange(new object[] { "近战", "远程", "魔法", "召唤", "普通伤害" });
            cmbDamageKind.SelectedIndex = 0;
            StyleComboBox(cmbDamageKind);
            cmbDamageKind.SelectedIndexChanged += (s, e) => UpdateFieldVisibility();
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

            _lblProjectileReference = AddFieldLabel(_combatSection, "弹幕ID", 18, 206, 72);
            txtProjectileId.Location = new Point(96, 202);
            txtProjectileId.Size = new Size(92, 26);
            txtProjectileId.Font = FontBody;
            ConfigureProjectileReferenceTextBox(txtProjectileId);
            _combatSection.Controls.Add(txtProjectileId);
            _lblProjectileSpeed = AddNumericField(_combatSection, "速度", numProjectileSpeed, 196, 206, 72);
            txtProjectileId.Text = "1";
            numProjectileSpeed.DecimalPlaces = 1;
            numProjectileSpeed.Increment = 0.5M;
            numProjectileSpeed.Maximum = 99;
            numProjectileSpeed.Value = 10;

            _lblUseStyleId = AddNumericField(_combatSection, "动作ID", numUseStyleId, 18, 242, 72);
            numUseStyleId.Minimum = 0;
            numUseStyleId.Maximum = 9999;
            numUseStyleId.Value = 1;

            chkWhipProjectile.Text = "鞭子弹幕";
            chkWhipProjectile.Location = new Point(196, 246);
            chkWhipProjectile.Size = new Size(128, 24);
            chkWhipProjectile.Font = FontBody;
            chkWhipProjectile.ForeColor = ClrText;
            chkWhipProjectile.BackColor = Color.Transparent;
            _combatSection.Controls.Add(chkWhipProjectile);

            _lblManaCost = AddNumericField(_combatSection, "魔力消耗", numManaCost, 18, 278, 72);
            numManaCost.Minimum = 0;
            numManaCost.Maximum = 9999;
            numManaCost.Value = 0;

            chkUsesAmmo.Text = "消耗弹药";
            chkUsesAmmo.Location = new Point(196, 282);
            chkUsesAmmo.Size = new Size(128, 24);
            chkUsesAmmo.Font = FontBody;
            chkUsesAmmo.ForeColor = ClrText;
            chkUsesAmmo.BackColor = Color.Transparent;
            chkUsesAmmo.CheckedChanged += (s, e) => UpdateFieldVisibility();
            _combatSection.Controls.Add(chkUsesAmmo);

            _lblAmmoType = AddFieldLabel(_combatSection, "弹药类型", 18, 318, 72);
            txtAmmoType.Location = new Point(96, 314);
            txtAmmoType.Size = new Size(196, 26);
            txtAmmoType.Font = FontBody;
            ConfigureAmmoTypeTextBox(txtAmmoType);
            txtAmmoType.Text = "Bullet";
            _combatSection.Controls.Add(txtAmmoType);

            _lblUseSound = AddFieldLabel(_combatSection, "音效ID", 18, 354, 72);
            txtUseSound.Location = new Point(96, 350);
            txtUseSound.Size = new Size(196, 26);
            txtUseSound.Font = FontBody;
            ConfigureUseSoundTextBox(txtUseSound);
            txtUseSound.Text = "Auto";
            _combatSection.Controls.Add(txtUseSound);

            _lblWeaponOffsetX = AddNumericField(_combatSection, "武器偏移X", numWeaponOffsetX, 18, 390, 72);
            numWeaponOffsetX.Minimum = -999;
            numWeaponOffsetX.Maximum = 999;
            numWeaponOffsetX.DecimalPlaces = 1;
            numWeaponOffsetX.Increment = 0.5M;
            numWeaponOffsetX.Value = 0;

            _lblWeaponOffsetY = AddNumericField(_combatSection, "武器偏移Y", numWeaponOffsetY, 196, 390, 72);
            numWeaponOffsetY.Minimum = -999;
            numWeaponOffsetY.Maximum = 999;
            numWeaponOffsetY.DecimalPlaces = 1;
            numWeaponOffsetY.Increment = 0.5M;
            numWeaponOffsetY.Value = 0;

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

            btnImportBlock.Text = "导入块";
            btnImportBlock.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            btnImportBlock.Size = new Size(104, 36);
            StyleButton(btnImportBlock, Color.FromArgb(92, 116, 236), Color.FromArgb(68, 86, 175));
            btnImportBlock.Click -= ImportBlock_Click;
            btnImportBlock.Click += ImportBlock_Click;
            pnlRight.Controls.Add(btnImportBlock);

            btnEffectPreview.Text = "特效预览";
            btnEffectPreview.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            btnEffectPreview.Size = new Size(104, 36);
            StyleButton(btnEffectPreview, Color.FromArgb(160, 122, 235), Color.FromArgb(120, 90, 175));
            btnEffectPreview.Click -= EffectPreview_Click;
            btnEffectPreview.Click += EffectPreview_Click;
            pnlRight.Controls.Add(btnEffectPreview);

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

        private static Label AddFieldLabel(Control parent, string text, int x, int y, int width)
        {
            var fieldLabel = new Label
            {
                Text = text,
                AutoSize = false,
                Location = new Point(x, y),
                Size = new Size(width, 22),
                Font = FontBodyBold,
                ForeColor = ClrSubText,
                BackColor = Color.Transparent
            };
            parent.Controls.Add(fieldLabel);
            return fieldLabel;
        }

        private static Label AddNumericField(Control parent, string label, NumericUpDown number, int x, int y, int inputWidth)
        {
            var fieldLabel = AddFieldLabel(parent, label, x, y + 3, 72);
            AddNumericBox(parent, number, x + 78, y, inputWidth);
            return fieldLabel;
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

        private void ConfigureProjectileReferenceTextBox(System.Windows.Forms.TextBox textBox)
        {
            textBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            textBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.BackColor = ClrInputBg;
            textBox.ForeColor = ClrText;
            UpdateProjectileReferenceOptions();
        }

        private static void ConfigureAmmoTypeTextBox(System.Windows.Forms.TextBox textBox)
        {
            textBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            textBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.BackColor = ClrInputBg;
            textBox.ForeColor = ClrText;
            var autoComplete = new AutoCompleteStringCollection();
            autoComplete.AddRange(new[] { "Arrow", "Bullet", "Rocket", "Dart", "Solution", "Sand", "Gel", "FallenStar", "None" });
            textBox.AutoCompleteCustomSource = autoComplete;
        }

        private static void ConfigureUseSoundTextBox(System.Windows.Forms.TextBox textBox)
        {
            textBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            textBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.BackColor = ClrInputBg;
            textBox.ForeColor = ClrText;

            var values = new List<string> { "Auto", "None" };
            values.AddRange(Enumerable.Range(1, 200).Select(id => $"Item{id}"));
            values.AddRange(Enumerable.Range(1, 200).Select(id => $"SoundID.Item{id}"));

            var autoComplete = new AutoCompleteStringCollection();
            autoComplete.AddRange(values.ToArray());
            textBox.AutoCompleteCustomSource = autoComplete;
        }

        private void UpdateProjectileReferenceOptions()
        {
            var references = _items
                .Where(item => item.Type == ItemType.Projectile)
                .Select(item => item.Name.Trim())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            string current = txtProjectileId.Text;

            var autoComplete = new AutoCompleteStringCollection();
            autoComplete.AddRange(references);
            txtProjectileId.AutoCompleteCustomSource = autoComplete;
            txtProjectileId.Text = current;
        }

        private string GetProjectileReferenceInput()
        {
            return NormalizeProjectileReference(txtProjectileId.Text, GetProjectileIdFallback(txtProjectileId.Text));
        }

        private static int GetProjectileIdFallback(string? reference)
        {
            if (int.TryParse(reference, out int intValue))
                return Math.Max(0, intValue);

            if (double.TryParse(reference, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double doubleValue))
                return Math.Max(0, (int)Math.Round(doubleValue));

            return 1;
        }

        private static string NormalizeProjectileReference(string? reference, int projectileId)
        {
            string trimmed = reference?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(trimmed))
                return Math.Max(0, projectileId).ToString(System.Globalization.CultureInfo.InvariantCulture);

            if (trimmed == "1" && projectileId != 1)
                return Math.Max(0, projectileId).ToString(System.Globalization.CultureInfo.InvariantCulture);

            return trimmed;
        }

        private static string NormalizeAmmoType(string? ammoType)
        {
            string value = ammoType?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(value) || value.Equals("None", StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            return value;
        }

        private static string NormalizeUseSound(string? useSound)
        {
            string value = useSound?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(value) ||
                value.Equals("Auto", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("Default", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("自动", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("默认", StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            return value;
        }

        private static string GetUseSoundText(string? useSound)
        {
            string value = NormalizeUseSound(useSound);
            return string.IsNullOrWhiteSpace(value) ? "Auto" : value;
        }

        private static string GetAmmoExpression(string? ammoType)
        {
            string value = NormalizeAmmoType(ammoType);
            if (string.IsNullOrWhiteSpace(value))
                return "AmmoID.Bullet";

            if (int.TryParse(value, out int id))
                return Math.Max(0, id).ToString(System.Globalization.CultureInfo.InvariantCulture);

            string normalized = value;
            int spaceIndex = normalized.IndexOf(' ');
            if (spaceIndex >= 0)
                normalized = normalized[..spaceIndex];
            if (normalized.StartsWith("AmmoID.", StringComparison.OrdinalIgnoreCase))
                normalized = normalized["AmmoID.".Length..];

            normalized = normalized switch
            {
                "箭" => "Arrow",
                "子弹" => "Bullet",
                "火箭" => "Rocket",
                "飞镖" => "Dart",
                "溶液" => "Solution",
                "沙" => "Sand",
                "凝胶" => "Gel",
                "坠星" or "落星" => "FallenStar",
                _ => normalized
            };

            string identifier = SanitizeIdentifier(normalized);
            return string.IsNullOrWhiteSpace(identifier) ? "AmmoID.Bullet" : $"AmmoID.{identifier}";
        }

        private static string SanitizeIdentifier(string value)
        {
            var sb = new System.Text.StringBuilder();
            foreach (char c in value)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                    sb.Append(c);
            }

            return sb.ToString();
        }

        private bool IsDuplicateInternalName(string name, ModItemData? allowedItem = null)
        {
            return _items.Any(item =>
                !ReferenceEquals(item, allowedItem) &&
                string.Equals(item.Name.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private bool TryGetDuplicateInternalName(out string duplicateName)
        {
            duplicateName = _items
                .Select(item => item.Name.Trim())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .GroupBy(name => name, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(group => group.Count() > 1)
                ?.Key ?? string.Empty;

            return !string.IsNullOrWhiteSpace(duplicateName);
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
            btnImportBlock.Location = new Point(Math.Max(24, btnExport.Left - btnImportBlock.Width - 12), 24);
            btnEffectPreview.Location = new Point(Math.Max(24, btnImportBlock.Left - btnEffectPreview.Width - 12), 24);
            if (_rightSubtitleLabel != null)
                _rightSubtitleLabel.Width = Math.Max(260, btnEffectPreview.Left - _rightSubtitleLabel.Left - 24);
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
            bool isProjectile = type == ItemType.Projectile;

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
                UseStyleId = isProjectile ? 1 : (int)numUseStyleId.Value,
                Knockback = (int)numKnockback.Value,
                CriticalChance = isProjectile ? 0 : (int)numCriticalChance.Value,
                ManaCost = type == ItemType.Weapon ? (int)numManaCost.Value : 0,
                UsesAmmo = type == ItemType.Weapon && chkUsesAmmo.Checked,
                AmmoType = type == ItemType.Weapon && chkUsesAmmo.Checked ? NormalizeAmmoType(txtAmmoType.Text) : string.Empty,
                UseSound = type == ItemType.Weapon ? NormalizeUseSound(txtUseSound.Text) : string.Empty,
                WeaponOffsetX = type == ItemType.Weapon ? numWeaponOffsetX.Value : 0,
                WeaponOffsetY = type == ItemType.Weapon ? numWeaponOffsetY.Value : 0,
                UsesProjectile = !isProjectile && chkUsesProjectile.Checked,
                ProjectileId = isProjectile ? 1 : GetProjectileIdFallback(txtProjectileId.Text),
                ProjectileReference = isProjectile ? "1" : GetProjectileReferenceInput(),
                ProjectileSpeed = isProjectile ? 10 : numProjectileSpeed.Value,
                ConsumeOnUse = !isProjectile && chkConsumeOnUse.Checked,
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
                AutoReuse = !isProjectile && chkAutoReuse.Checked,
                UseTurn = !isProjectile && chkUseTurn.Checked,
                TexturePath = type == ItemType.Buff && GetSelectedBuffIconSource() == BuffIconSource.Vanilla ? string.Empty : _selectedTexturePath,
                IsMultiFrameTexture = chkMultiFrameTexture.Checked,
                TextureFrameCount = (int)numTextureFrameCount.Value,
                IsWhipProjectile = IsWhipProjectileOptionAvailable() && chkWhipProjectile.Checked,
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
            if (IsDuplicateInternalName(txtName.Text.Trim(), editingItem))
            {
                UIMessageBox.Show("内部名称不能重名，请换一个唯一的内部名称。");
                return;
            }

            var item = CaptureItemFromForm(editingItem?.Flow);

            if (editingItem != null)
            {
                int index = _items.IndexOf(editingItem);
                if (index >= 0)
                {
                    _items[index] = item;
                    ClearInputs();
                    RefreshItemCards();
                    UpdateProjectileReferenceOptions();
                    return;
                }

                _editingItem = null;
            }

            _items.Add(item);
            AddItemCardModern(item);
            UpdateProjectileReferenceOptions();
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
                var noTexture = CreatePlainLabel(item.IsWhipProjectile ? "骨鞭" : "无贴图", new Point(0, 27), new Size(76, 22), FontSmall, item.IsWhipProjectile ? accent : ClrMuted);
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
                Size = new Size(Math.Max(120, card.Width - 340), 24),
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
                Size = new Size(Math.Max(120, card.Width - 340), 18),
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
                Size = new Size(Math.Max(220, card.Width - 340), 22),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                Font = FontSmall,
                ForeColor = ClrSubText,
                BackColor = Color.Transparent
            };
            card.Controls.Add(statsLabel);

            var editBtn = new UIButton
            {
                Text = "编辑",
                Size = new Size(86, 28),
                Location = new Point(card.Width - 204, 18),
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
                Size = new Size(86, 28),
                Location = new Point(card.Width - 112, 18),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = FontBodyBold,
                Tag = item
            };
            StyleButton(flowBtn, Color.FromArgb(54, 161, 124), Color.FromArgb(38, 112, 87));
            flowBtn.Click += (s, e) =>
            {
                using var form = new VisualScriptForm(item, _items);
                form.ShowDialog();
            };
            card.Controls.Add(flowBtn);

            var exportBtn = new UIButton
            {
                Text = "导出块",
                Size = new Size(86, 28),
                Location = new Point(card.Width - 204, 52),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = FontBodyBold,
                Tag = item
            };
            StyleButton(exportBtn, Color.FromArgb(92, 116, 236), Color.FromArgb(68, 86, 175));
            exportBtn.Click += (s, e) => ExportItemBlock(item);
            card.Controls.Add(exportBtn);

            var deleteBtn = new UIButton
            {
                Text = "删除",
                Size = new Size(86, 28),
                Location = new Point(card.Width - 112, 52),
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
                UpdateProjectileReferenceOptions();
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
            SetNumericValue(numManaCost, item.ManaCost);
            chkUsesAmmo.Checked = item.UsesAmmo;
            txtAmmoType.Text = string.IsNullOrWhiteSpace(item.AmmoType) ? "Bullet" : item.AmmoType;
            txtUseSound.Text = GetUseSoundText(item.UseSound);
            SetNumericValue(numWeaponOffsetX, item.WeaponOffsetX);
            SetNumericValue(numWeaponOffsetY, item.WeaponOffsetY);
            chkConsumeOnUse.Checked = item.ConsumeOnUse;
            txtProjectileId.Text = NormalizeProjectileReference(item.ProjectileReference, item.ProjectileId);
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
            chkWhipProjectile.Checked = item.IsWhipProjectile;
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
            if (!ValidateModernExportReady())
                return;

            using var dialog = CreateExportFolderDialog("选择导出目录 (可选 ModSources 或已有 Mod 文件夹)");

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                string modDir = ResolveExportModDirectory(dialog.SelectedPath);
                UIMessageBox.Show(ExportModernToDirectory(modDir));
            }
            catch (Exception ex)
            {
                UIMessageBox.Show($"导出失败: {ex.Message}");
            }
        }

        private void ImportBlock_Click(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Title = "导入块",
                Filter = "TMCreator 块|*.tmcreator-block.json;*.tmblock.json|JSON 文件|*.json|所有文件|*.*"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                ModItemData item = ReadItemBlock(dialog.FileName);
                string sourceDir = Path.GetDirectoryName(dialog.FileName) ?? AppContext.BaseDirectory;
                NormalizeItem(item, sourceDir);
                item.Name = GetUniqueInternalName(item.Name);
                if (string.IsNullOrWhiteSpace(item.DisplayName))
                    item.DisplayName = item.Name;

                _items.Add(item);
                AddItemCardModern(item);
                UpdateProjectileReferenceOptions();
                UpdateEmptyState();
                UIMessageBox.Show($"已导入块：{item.DisplayName}");
            }
            catch (Exception ex)
            {
                UIMessageBox.Show($"导入块失败：{ex.Message}");
            }
        }

        private void EffectPreview_Click(object? sender, EventArgs e)
        {
            using var form = new EffectPreviewForm();
            form.ShowDialog(this);
        }

        private void ExportItemBlock(ModItemData item)
        {
            using var dialog = new SaveFileDialog
            {
                Title = "导出块",
                Filter = "TMCreator 块|*.tmcreator-block.json|JSON 文件|*.json|所有文件|*.*",
                FileName = $"{SanitizeFileName(item.Name)}.tmcreator-block.json"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                var blockFile = CreateItemBlockFile(item);
                string json = JsonSerializer.Serialize(blockFile, ProjectJsonOptions);
                File.WriteAllText(dialog.FileName, json, System.Text.Encoding.UTF8);
                UIMessageBox.Show($"已导出块：\n{dialog.FileName}");
            }
            catch (Exception ex)
            {
                UIMessageBox.Show($"导出块失败：{ex.Message}");
            }
        }

        private static ModItemBlockFile CreateItemBlockFile(ModItemData item)
        {
            var blockItem = CloneItem(item);
            var blockFile = new ModItemBlockFile { Item = blockItem };

            if (!string.IsNullOrWhiteSpace(item.TexturePath) && File.Exists(item.TexturePath))
            {
                blockFile.TextureExtension = Path.GetExtension(item.TexturePath);
                blockFile.TextureBase64 = Convert.ToBase64String(File.ReadAllBytes(item.TexturePath));
                blockItem.TexturePath = Path.GetFileName(item.TexturePath);
            }

            return blockFile;
        }

        private static ModItemData ReadItemBlock(string fileName)
        {
            string json = File.ReadAllText(fileName);
            var blockFile = JsonSerializer.Deserialize<ModItemBlockFile>(json, ProjectJsonOptions);
            ModItemData? item = blockFile?.Item;

            if (item == null)
                item = JsonSerializer.Deserialize<ModItemData>(json, ProjectJsonOptions);
            if (item == null)
                throw new InvalidOperationException("块文件内容为空或格式不正确。");

            if (blockFile != null && !string.IsNullOrWhiteSpace(blockFile.TextureBase64))
                item.TexturePath = WriteImportedBlockTexture(item.Name, blockFile.TextureExtension, blockFile.TextureBase64);

            return item;
        }

        private static string WriteImportedBlockTexture(string itemName, string? extension, string base64)
        {
            string safeExtension = string.IsNullOrWhiteSpace(extension) ? ".png" : extension;
            if (!safeExtension.StartsWith('.'))
                safeExtension = "." + safeExtension;

            string dir = Path.Combine(Path.GetTempPath(), "tmcreator-imported-blocks");
            Directory.CreateDirectory(dir);
            string target = Path.Combine(dir, $"{SanitizeFileName(itemName)}_{Guid.NewGuid():N}{safeExtension}");
            File.WriteAllBytes(target, Convert.FromBase64String(base64));
            return target;
        }

        private string GetUniqueInternalName(string name)
        {
            string baseName = string.IsNullOrWhiteSpace(name) ? "ImportedBlock" : name.Trim();
            string candidate = baseName;
            int index = 2;
            while (_items.Any(item => string.Equals(item.Name, candidate, StringComparison.OrdinalIgnoreCase)))
            {
                candidate = $"{baseName}_{index}";
                index++;
            }

            return candidate;
        }

        private bool ValidateModernExportReady()
        {
            if (_items.Count == 0)
            {
                UIMessageBox.Show("没有可导出的物品。");
                return false;
            }

            if (TryGetDuplicateInternalName(out string duplicateName))
            {
                UIMessageBox.Show($"内部名称不能重名：{duplicateName}");
                return false;
            }

            return true;
        }

        private static FolderBrowserDialog CreateExportFolderDialog(string description)
        {
            return new FolderBrowserDialog
            {
                Description = description
            };
        }

        private string ResolveExportModDirectory(string selectedDir)
        {
            string projectCodeName = GetProjectCodeName();
            string selectedName = Path.GetFileName(selectedDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            bool selectedIsExistingMod = string.Equals(selectedName, projectCodeName, StringComparison.OrdinalIgnoreCase) ||
                                         File.Exists(Path.Combine(selectedDir, "build.txt")) ||
                                         Directory.GetFiles(selectedDir, "*.csproj", SearchOption.TopDirectoryOnly).Length > 0 ||
                                         File.Exists(Path.Combine(selectedDir, $"{projectCodeName}.cs")) ||
                                         Directory.Exists(Path.Combine(selectedDir, "Items")) ||
                                         Directory.Exists(Path.Combine(selectedDir, "Localization"));

            return selectedIsExistingMod
                ? selectedDir
                : Path.Combine(selectedDir, projectCodeName);
        }

        private string ExportModernToDirectory(string baseDir)
        {
            Directory.CreateDirectory(baseDir);

            string projectCodeName = GetProjectCodeName();
            string itemsDir = Path.Combine(baseDir, "Items");
            string buffsDir = Path.Combine(baseDir, "Buffs");
            string projectilesDir = Path.Combine(baseDir, "Projectiles");
            string locDir = Path.Combine(baseDir, "Localization");
            string locFile = Path.Combine(locDir, "en-US.hjson");

            Directory.CreateDirectory(itemsDir);
            Directory.CreateDirectory(buffsDir);
            Directory.CreateDirectory(projectilesDir);
            Directory.CreateDirectory(locDir);

            var sbItems = new System.Text.StringBuilder();
            var sbBuffs = new System.Text.StringBuilder();
            var sbProjectiles = new System.Text.StringBuilder();
            var sbLoc = new System.Text.StringBuilder();
            var itemEntries = _items.Where(item => item.Type != ItemType.Buff && item.Type != ItemType.Projectile).ToList();
            var buffEntries = _items.Where(item => item.Type == ItemType.Buff).ToList();
            var projectileEntries = _items.Where(item => item.Type == ItemType.Projectile).ToList();

            WriteProjectModFiles(baseDir, projectCodeName, sbItems);

            if (_items.Any(HasFlowScript))
            {
                string flowVariablesFile = Path.Combine(baseDir, "TMCreatorFlowVariables.cs");
                File.WriteAllText(flowVariablesFile, GenerateFlowVariablesCode(projectCodeName), System.Text.Encoding.UTF8);
                sbItems.AppendLine("  - 导出全局流程变量: TMCreatorFlowVariables.cs");
            }

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
                    else if (projectile.IsWhipProjectile)
                    {
                        sbProjectiles.AppendLine($"  - 导出弹幕: {className}.cs (默认骨鞭贴图)");
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

            return $"成功导出 {_items.Count} 个内容到:\n{baseDir}\n\n生成内容:\n{sbItems}{sbBuffs}{sbProjectiles}";
        }

        private void WriteProjectModFiles(string baseDir, string projectCodeName, System.Text.StringBuilder summary)
        {
            string modClassFile = Path.Combine(baseDir, $"{projectCodeName}.cs");
            File.WriteAllText(modClassFile, GenerateModClassCode(projectCodeName), System.Text.Encoding.UTF8);
            summary.AppendLine($"  - 导出模组主类: {projectCodeName}.cs");

            string projectFile = Path.Combine(baseDir, $"{projectCodeName}.csproj");
            File.WriteAllText(projectFile, GenerateModProjectFile(), System.Text.Encoding.UTF8);
            summary.AppendLine($"  - 导出工程文件: {projectCodeName}.csproj");

            WriteProjectDescriptionFiles(baseDir, summary);
            WriteProjectBuildFile(baseDir, summary);
            WriteProjectIconFiles(baseDir, projectCodeName, summary);
        }

        private static string GenerateModClassCode(string projectCodeName)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("using Terraria.ModLoader;");
            sb.AppendLine();
            sb.AppendLine($"namespace {projectCodeName}");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {projectCodeName} : Mod");
            sb.AppendLine("    {");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string GenerateModProjectFile()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
            sb.AppendLine();
            sb.AppendLine("  <Import Project=\"..\\tModLoader.targets\" />");
            sb.AppendLine();
            sb.AppendLine("  <PropertyGroup>");
            sb.AppendLine("  </PropertyGroup>");
            sb.AppendLine();
            sb.AppendLine("  <ItemGroup>");
            sb.AppendLine("  </ItemGroup>");
            sb.AppendLine();
            sb.AppendLine("</Project>");
            return sb.ToString();
        }

        private void WriteProjectDescriptionFiles(string baseDir, System.Text.StringBuilder summary)
        {
            string description = string.IsNullOrWhiteSpace(_projectDescription)
                ? $"由 TMCreator 导出的 {_projectName} 模组。"
                : _projectDescription.Trim();

            File.WriteAllText(Path.Combine(baseDir, "description.txt"), description + Environment.NewLine, System.Text.Encoding.UTF8);
            File.WriteAllText(Path.Combine(baseDir, "description_workshop.txt"), description + Environment.NewLine, System.Text.Encoding.UTF8);
            summary.AppendLine("  - 导出 description.txt");
            summary.AppendLine("  - 导出 description_workshop.txt");
        }

        private void WriteProjectBuildFile(string baseDir, System.Text.StringBuilder summary)
        {
            string buildFile = Path.Combine(baseDir, "build.txt");
            File.WriteAllText(buildFile, GenerateBuildTxt(), System.Text.Encoding.UTF8);
            summary.AppendLine("  - 导出 build.txt");
        }

        private void WriteProjectIconFiles(string baseDir, string projectCodeName, System.Text.StringBuilder summary)
        {
            string iconSource = _projectIconPath;
            using Bitmap sourceIcon = !string.IsNullOrWhiteSpace(iconSource) && File.Exists(iconSource)
                ? LoadBitmapCopy(iconSource)
                : CreateDefaultModIcon(80, projectCodeName);
            using Bitmap icon = ResizeImage(sourceIcon, 80, 80);
            using Bitmap smallIcon = ResizeImage(icon, 30, 30);

            string iconTarget = Path.Combine(baseDir, "icon.png");
            string smallIconTarget = Path.Combine(baseDir, "icon_small.png");
            SavePng(icon, iconTarget);
            SavePng(smallIcon, smallIconTarget);
            summary.AppendLine("  - 导出 icon.png");
            summary.AppendLine("  - 导出 icon_small.png");
        }

        private string GenerateBuildTxt()
        {
            var lines = new List<string>
            {
                $"displayName = {EscapeBuildValue(_projectName)}",
                $"author = {EscapeBuildValue(string.IsNullOrWhiteSpace(_buildAuthor) ? Environment.UserName : _buildAuthor)}",
                $"version = {EscapeBuildValue(_buildVersion)}"
            };

            if (!string.IsNullOrWhiteSpace(_buildHomepage))
                lines.Add($"homepage = {EscapeBuildValue(_buildHomepage)}");

            return string.Join(Environment.NewLine, lines) + Environment.NewLine;
        }

        private static string EscapeBuildValue(string value)
        {
            return (value ?? string.Empty)
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Trim();
        }

        private static Bitmap LoadBitmapCopy(string path)
        {
            using var image = Image.FromFile(path);
            return new Bitmap(image);
        }

        private static Bitmap ResizeImage(Image source, int width, int height)
        {
            var bitmap = new Bitmap(width, height);
            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.Clear(Color.Transparent);
            graphics.DrawImage(source, new Rectangle(0, 0, width, height));
            return bitmap;
        }

        private static Bitmap CreateDefaultModIcon(int size, string projectCodeName)
        {
            var bitmap = new Bitmap(size, size);
            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var bg = new LinearGradientBrush(new Rectangle(0, 0, size, size), Color.FromArgb(34, 211, 193), Color.FromArgb(245, 179, 53), 135F);
            graphics.FillRectangle(bg, 0, 0, size, size);
            using var overlay = new SolidBrush(Color.FromArgb(72, 12, 18, 27));
            graphics.FillEllipse(overlay, size * 0.12F, size * 0.12F, size * 0.76F, size * 0.76F);
            using var font = new Font("Microsoft YaHei UI", Math.Max(9F, size * 0.34F), FontStyle.Bold, GraphicsUnit.Point);
            using var textBrush = new SolidBrush(Color.White);
            string text = string.IsNullOrWhiteSpace(projectCodeName)
                ? "TM"
                : projectCodeName[..Math.Min(2, projectCodeName.Length)].ToUpperInvariant();
            using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            graphics.DrawString(text, font, textBrush, new RectangleF(0, 0, size, size), format);
            return bitmap;
        }

        private static void SavePng(Image image, string targetPath)
        {
            string tempPath = targetPath + ".tmp";
            image.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);
            File.Copy(tempPath, targetPath, true);
            File.Delete(tempPath);
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
                if (item.ManaCost > 0) parts.Add($"魔力 {item.ManaCost}");
                if (item.UsesAmmo) parts.Add($"弹药 {NormalizeAmmoType(item.AmmoType)}");
                if (item.Type == ItemType.Weapon && !string.IsNullOrWhiteSpace(NormalizeUseSound(item.UseSound))) parts.Add($"音效 {NormalizeUseSound(item.UseSound)}");
                if (item.Type == ItemType.Weapon && (item.WeaponOffsetX != 0 || item.WeaponOffsetY != 0)) parts.Add($"武器偏移 X {item.WeaponOffsetX:0.#} / Y {item.WeaponOffsetY:0.#}");
                if (item.UsesProjectile) parts.Add($"弹幕 {NormalizeProjectileReference(item.ProjectileReference, item.ProjectileId)} / 速度 {item.ProjectileSpeed:0.#}");
                if (item.ConsumeOnUse) parts.Add("使用消耗");
            }

            if (item.Type == ItemType.Projectile)
            {
                if (item.IsWhipProjectile)
                    parts.Add("鞭子弹幕");
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
            using var dialog = new ProjectSettingsDialog(
                _projectName,
                _projectDescription,
                _buildVersion,
                _buildAuthor,
                _buildHomepage,
                _projectIconPath);
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            _projectName = dialog.ProjectName;
            _projectDescription = dialog.ProjectDescription;
            _buildVersion = dialog.BuildVersion;
            _buildAuthor = dialog.BuildAuthor;
            _buildHomepage = dialog.BuildHomepage;
            _projectIconPath = dialog.ProjectIconPath;
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
            _projectDescription = string.Empty;
            _projectIconPath = string.Empty;
            _buildVersion = "1.4";
            _buildAuthor = string.Empty;
            _buildHomepage = string.Empty;
            _projectFilePath = null;
            _items.Clear();
            ClearItemCards();
            ClearInputs();
            UpdateProjectileReferenceOptions();
            UpdateProjectStatus();
            UpdateEmptyState();
        }

        private void LoadProject(ProjectData project, string filePath)
        {
            string projectDir = Path.GetDirectoryName(filePath) ?? AppContext.BaseDirectory;

            _projectName = string.IsNullOrWhiteSpace(project.ProjectName) ? "未命名工程" : project.ProjectName.Trim();
            _projectDescription = project.ProjectDescription ?? string.Empty;
            _projectIconPath = ResolveProjectPath(project.ProjectIconPath, projectDir);
            _buildVersion = string.IsNullOrWhiteSpace(project.BuildVersion) ? "1.4" : project.BuildVersion.Trim();
            _buildAuthor = project.BuildAuthor ?? string.Empty;
            _buildHomepage = project.BuildHomepage ?? string.Empty;
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

            UpdateProjectileReferenceOptions();
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
                ProjectDescription = _projectDescription,
                ProjectIconPath = _projectIconPath,
                BuildVersion = _buildVersion,
                BuildAuthor = _buildAuthor,
                BuildHomepage = _buildHomepage,
                Items = _items.Select(CloneItem).ToList(),
                Draft = CaptureDraft()
            };
        }

        private ProjectDraftData CaptureDraft()
        {
            var type = cmbItemType.SelectedIndex >= 0 ? (ItemType)cmbItemType.SelectedIndex : ItemType.Item;
            bool isProjectile = type == ItemType.Projectile;

            return new ProjectDraftData
            {
                Name = txtName.Text.Trim(),
                DisplayName = txtDisplayName.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                Type = type,
                Width = (int)numWidth.Value,
                Height = (int)numHeight.Value,
                Value = (int)numValue.Value,
                Rarity = (int)numRarity.Value,
                Damage = (int)numDamage.Value,
                DamageKind = GetSelectedDamageKind(),
                UseTime = (int)numUseTime.Value,
                UseStyleId = isProjectile ? 1 : (int)numUseStyleId.Value,
                Knockback = (int)numKnockback.Value,
                CriticalChance = isProjectile ? 0 : (int)numCriticalChance.Value,
                ManaCost = type == ItemType.Weapon ? (int)numManaCost.Value : 0,
                UsesAmmo = type == ItemType.Weapon && chkUsesAmmo.Checked,
                AmmoType = type == ItemType.Weapon && chkUsesAmmo.Checked ? NormalizeAmmoType(txtAmmoType.Text) : string.Empty,
                UseSound = type == ItemType.Weapon ? NormalizeUseSound(txtUseSound.Text) : string.Empty,
                WeaponOffsetX = type == ItemType.Weapon ? numWeaponOffsetX.Value : 0,
                WeaponOffsetY = type == ItemType.Weapon ? numWeaponOffsetY.Value : 0,
                UsesProjectile = !isProjectile && chkUsesProjectile.Checked,
                ProjectileId = isProjectile ? 1 : GetProjectileIdFallback(txtProjectileId.Text),
                ProjectileReference = isProjectile ? "1" : GetProjectileReferenceInput(),
                ProjectileSpeed = isProjectile ? 10 : numProjectileSpeed.Value,
                ConsumeOnUse = !isProjectile && chkConsumeOnUse.Checked,
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
                AutoReuse = !isProjectile && chkAutoReuse.Checked,
                UseTurn = !isProjectile && chkUseTurn.Checked,
                TexturePath = (ItemType)cmbItemType.SelectedIndex == ItemType.Buff && GetSelectedBuffIconSource() == BuffIconSource.Vanilla ? string.Empty : _selectedTexturePath,
                IsMultiFrameTexture = chkMultiFrameTexture.Checked,
                TextureFrameCount = (int)numTextureFrameCount.Value,
                IsWhipProjectile = IsWhipProjectileOptionAvailable() && chkWhipProjectile.Checked,
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
            SetNumericValue(numManaCost, draft.ManaCost);
            chkUsesAmmo.Checked = draft.UsesAmmo;
            txtAmmoType.Text = string.IsNullOrWhiteSpace(draft.AmmoType) ? "Bullet" : draft.AmmoType;
            txtUseSound.Text = GetUseSoundText(draft.UseSound);
            SetNumericValue(numWeaponOffsetX, draft.WeaponOffsetX);
            SetNumericValue(numWeaponOffsetY, draft.WeaponOffsetY);
            chkUsesProjectile.Checked = draft.UsesProjectile;
            chkConsumeOnUse.Checked = draft.ConsumeOnUse;
            txtProjectileId.Text = NormalizeProjectileReference(draft.ProjectileReference, draft.ProjectileId);
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
            chkWhipProjectile.Checked = draft.IsWhipProjectile;
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
                   string.IsNullOrWhiteSpace(NormalizeUseSound(txtUseSound.Text)) &&
                   numWeaponOffsetX.Value == 0 &&
                   numWeaponOffsetY.Value == 0 &&
                   _currentRecipe.Ingredients.Count == 0 &&
                   !chkWhipProjectile.Checked &&
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
            UpdateProjectileReferenceOptions();
            UpdateEmptyState();
        }

        private void UpdateProjectStatus()
        {
            if (_projectStatusLabel == null)
                return;

            string fileState = string.IsNullOrWhiteSpace(_projectFilePath)
                ? "未保存"
                : Path.GetFileName(_projectFilePath);
            _projectStatusLabel.Text = $"项目：{_projectName}    版本：{_buildVersion}    文件：{fileState}";
        }

        private void PrepareProjectAssets(ProjectData project, string projectDir)
        {
            string assetsDir = Path.Combine(projectDir, "assets");
            project.ProjectIconPath = CopyTextureIntoProject(project.ProjectIconPath, projectDir, assetsDir, "project_icon");
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
            item.ProjectileReference = NormalizeProjectileReference(item.ProjectileReference, item.ProjectileId);
            item.AmmoType = NormalizeAmmoType(item.AmmoType);
            item.UseSound = item.Type == ItemType.Weapon ? NormalizeUseSound(item.UseSound) : string.Empty;
            if (item.Type != ItemType.Weapon)
            {
                item.WeaponOffsetX = 0;
                item.WeaponOffsetY = 0;
            }
            if (item.Type != ItemType.Weapon)
            {
                item.ManaCost = 0;
                item.UsesAmmo = false;
                item.AmmoType = string.Empty;
            }
            if (item.TextureFrameCount <= 0)
                item.TextureFrameCount = 1;
            item.IsWhipProjectile = item.Type == ItemType.Projectile && item.DamageKind == ModDamageKind.Summon && item.IsWhipProjectile;
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
                ManaCost = item.ManaCost,
                UsesAmmo = item.UsesAmmo,
                AmmoType = item.AmmoType,
                UseSound = item.UseSound,
                WeaponOffsetX = item.WeaponOffsetX,
                WeaponOffsetY = item.WeaponOffsetY,
                UsesProjectile = item.UsesProjectile,
                ProjectileId = item.ProjectileId,
                ProjectileReference = NormalizeProjectileReference(item.ProjectileReference, item.ProjectileId),
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
                IsWhipProjectile = item.IsWhipProjectile,
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
                CanvasX = source.CanvasX,
                CanvasY = source.CanvasY,
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

            if (_lblCombatUseTime != null)
                _lblCombatUseTime.Text = isProjectile ? "存在时间" : "使用时间";

            SetControlsVisible(!isProjectile, _lblCombatCritical, numCriticalChance, chkAutoReuse, chkUseTurn, chkUsesProjectile, chkConsumeOnUse, _lblProjectileReference, txtProjectileId, _lblProjectileSpeed, numProjectileSpeed, _lblUseStyleId, numUseStyleId);
            if (isProjectile)
            {
                chkAutoReuse.Checked = false;
                chkUseTurn.Checked = false;
                chkUsesProjectile.Checked = false;
                chkConsumeOnUse.Checked = false;
                txtProjectileId.Text = "1";
                numProjectileSpeed.Value = 10;
                numUseStyleId.Value = 1;
            }

            bool showWeaponResourceFields = isWeapon;
            bool showAmmoType = showWeaponResourceFields && chkUsesAmmo.Checked;
            SetControlsVisible(showWeaponResourceFields, _lblManaCost, numManaCost, chkUsesAmmo, _lblUseSound, txtUseSound, _lblWeaponOffsetX, numWeaponOffsetX, _lblWeaponOffsetY, numWeaponOffsetY);
            SetControlsVisible(showAmmoType, _lblAmmoType, txtAmmoType);
            if (_lblUseSound != null)
                _lblUseSound.Location = showAmmoType ? new Point(18, 354) : new Point(18, 318);
            txtUseSound.Location = showAmmoType ? new Point(96, 350) : new Point(96, 314);
            if (!showWeaponResourceFields)
            {
                numManaCost.Value = 0;
                chkUsesAmmo.Checked = false;
                txtAmmoType.Text = "Bullet";
                txtUseSound.Text = "Auto";
                numWeaponOffsetX.Value = 0;
                numWeaponOffsetY.Value = 0;
            }

            bool canUseWhipProjectile = IsWhipProjectileOptionAvailable();
            chkWhipProjectile.Location = canUseWhipProjectile ? new Point(226, 134) : new Point(196, 246);
            chkWhipProjectile.Visible = canUseWhipProjectile;
            if (!canUseWhipProjectile)
                chkWhipProjectile.Checked = false;

            _combatSection.Height = isProjectile ? 216 : showWeaponResourceFields ? 452 : 300;
            _combatSection.Invalidate();

            btnCreate.Text = _editingItem != null ? "保存修改" : isBuff ? "创建 Buff" : isProjectile ? "创建弹幕" : isAccessory ? "创建饰品" : "创建物品";

            LayoutShell();
        }

        private static void SetControlsVisible(bool visible, params Control?[] controls)
        {
            foreach (Control? control in controls)
            {
                if (control == null)
                    continue;

                control.Visible = visible;
                if (control.Parent is Panel host && host.Controls.Count == 1 && ReferenceEquals(host.Controls[0], control))
                    host.Visible = visible;
            }
        }

        private bool IsWhipProjectileOptionAvailable()
        {
            return cmbItemType.SelectedIndex >= 0 &&
                   (ItemType)cmbItemType.SelectedIndex == ItemType.Projectile &&
                   GetSelectedDamageKind() == ModDamageKind.Summon;
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

            if (IsDuplicateInternalName(txtName.Text.Trim()))
            {
                UIMessageBox.Show("内部名称不能重名，请换一个唯一的内部名称。");
                return;
            }

            var type = (ItemType)cmbItemType.SelectedIndex;
            var item = new ModItemData
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
                UseSound = type == ItemType.Weapon ? NormalizeUseSound(txtUseSound.Text) : string.Empty,
                WeaponOffsetX = type == ItemType.Weapon ? numWeaponOffsetX.Value : 0,
                WeaponOffsetY = type == ItemType.Weapon ? numWeaponOffsetY.Value : 0,
                TexturePath = _selectedTexturePath
            };

            _items.Add(item);
            AddItemCard(item);
            UpdateProjectileReferenceOptions();
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
            numManaCost.Value = 0;
            chkUsesAmmo.Checked = false;
            txtAmmoType.Text = "Bullet";
            txtUseSound.Text = "Auto";
            numWeaponOffsetX.Value = 0;
            numWeaponOffsetY.Value = 0;
            chkUsesProjectile.Checked = false;
            chkConsumeOnUse.Checked = false;
            txtProjectileId.Text = "1";
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
            chkWhipProjectile.Checked = false;
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
                if (item.Type == ItemType.Weapon && !string.IsNullOrWhiteSpace(NormalizeUseSound(item.UseSound))) parts.Add($"音效: {NormalizeUseSound(item.UseSound)}");
                if (item.Type == ItemType.Weapon && (item.WeaponOffsetX != 0 || item.WeaponOffsetY != 0)) parts.Add($"武器偏移: X {item.WeaponOffsetX:0.#} / Y {item.WeaponOffsetY:0.#}");
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

            if (TryGetDuplicateInternalName(out string duplicateName))
            {
                UIMessageBox.Show($"内部名称不能重名：{duplicateName}");
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
                if (_items.Any(HasFlowScript))
                {
                    string flowVariablesFile = Path.Combine(baseDir, "TMCreatorFlowVariables.cs");
                    File.WriteAllText(flowVariablesFile, GenerateFlowVariablesCode(projectCodeName), System.Text.Encoding.UTF8);
                    sbItems.AppendLine("  - 导出全局流程变量: TMCreatorFlowVariables.cs");
                }

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

        private static bool HasFlowScript(ModItemData item) => item.Flow?.Blocks.Count > 0;

        private static string GenerateFlowVariablesCode(string projectCodeName)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using Terraria.ModLoader;");
            sb.AppendLine();
            sb.AppendLine($"namespace {projectCodeName}");
            sb.AppendLine("{");
            sb.AppendLine("    internal static class TMCreatorFlowVariables");
            sb.AppendLine("    {");
            sb.AppendLine("        private static readonly Dictionary<string, float> Values = new(StringComparer.OrdinalIgnoreCase);");
            sb.AppendLine();
            sb.AppendLine("        public static float Get(string name)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (string.IsNullOrWhiteSpace(name))");
            sb.AppendLine("                return 0f;");
            sb.AppendLine();
            sb.AppendLine("            lock (Values)");
            sb.AppendLine("                return Values.TryGetValue(name.Trim(), out float value) ? value : 0f;");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public static void Set(string name, float value)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (string.IsNullOrWhiteSpace(name))");
            sb.AppendLine("                return;");
            sb.AppendLine();
            sb.AppendLine("            lock (Values)");
            sb.AppendLine("                Values[name.Trim()] = value;");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public static void Clear()");
            sb.AppendLine("        {");
            sb.AppendLine("            lock (Values)");
            sb.AppendLine("                Values.Clear();");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    internal sealed class TMCreatorFlowVariableSystem : ModSystem");
            sb.AppendLine("    {");
            sb.AppendLine("        public override void OnWorldUnload()");
            sb.AppendLine("        {");
            sb.AppendLine("            TMCreatorFlowVariables.Clear();");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private string GenerateItemCode(ModItemData item, string className)
        {
            var sb = new System.Text.StringBuilder();
            bool hasFlow = item.Flow?.Blocks.Count > 0;
            bool hasAccessoryFlow = item.Type == ItemType.Accessory && HasFlowEvents(item.Flow, AccessoryFlowEventIds, AccessoryDefaultFlowEventId);
            bool hasAnimation = item.IsMultiFrameTexture && item.TextureFrameCount > 1;
            bool hasBaseWeaponOffset = item.Type == ItemType.Weapon && (item.WeaponOffsetX != 0 || item.WeaponOffsetY != 0);
            bool hasFlowWeaponOffset = item.Type is ItemType.Tool or ItemType.Weapon or ItemType.Block or ItemType.Item &&
                                       FlowCodeGenerator.HasBlock(item.Flow, "set_weapon_holdout_offset");
            bool hasWeaponOffset = hasBaseWeaponOffset || hasFlowWeaponOffset;
            if (hasFlow)
            {
                sb.AppendLine("using System;");
            }
            if (hasFlow || hasWeaponOffset)
            {
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
            if (hasFlowWeaponOffset)
            {
                sb.AppendLine("        private Vector2 _flowWeaponHoldoutOffset;");
                sb.AppendLine("        private int _flowWeaponHoldoutOffsetTicks;");
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
                bool usesAmmoFallbackProjectile = item.Type == ItemType.Weapon && item.UsesAmmo && !item.UsesProjectile;

                if (item.Damage > 0)
                {
                    sb.AppendLine($"            Item.damage = {item.Damage};");
                    sb.AppendLine($"            Item.DamageType = {GetDamageClassExpression(item.DamageKind)};");
                }
                if (item.Type == ItemType.Weapon && item.ManaCost > 0)
                    sb.AppendLine($"            Item.mana = {item.ManaCost};");
                if (item.Type == ItemType.Weapon && item.UsesAmmo)
                    sb.AppendLine($"            Item.useAmmo = {GetAmmoExpression(item.AmmoType)};");
                sb.AppendLine($"            Item.useTime = {item.UseTime};");
                sb.AppendLine($"            Item.useAnimation = {item.UseAnimation};");
                sb.AppendLine($"            Item.useStyle = {GetUseStyleId(item)};");
                string useSoundExpression = GetUseSoundExpression(item, usesAmmoFallbackProjectile, GetProjectCodeName());
                if (!string.IsNullOrWhiteSpace(useSoundExpression))
                    sb.AppendLine($"            Item.UseSound = {useSoundExpression};");

                if (item.Knockback > 0)
                    sb.AppendLine($"            Item.knockBack = {item.Knockback};");

                if (item.CriticalChance > 0)
                    sb.AppendLine($"            Item.crit = {item.CriticalChance};");

                if (item.AutoReuse)
                    sb.AppendLine("            Item.autoReuse = true;");

                if (item.UseTurn)
                    sb.AppendLine("            Item.useTurn = true;");

                if (item.UsesProjectile || usesAmmoFallbackProjectile)
                {
                    string shootExpression = item.UsesProjectile
                        ? GetProjectileTypeExpression(item.ProjectileReference, item.ProjectileId.ToString(System.Globalization.CultureInfo.InvariantCulture), GetProjectCodeName())
                        : "ProjectileID.Bullet";
                    sb.AppendLine($"            Item.shoot = {shootExpression};");
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

            if (hasWeaponOffset)
            {
                AppendWeaponHoldoutOffsetCode(sb, item, hasFlowWeaponOffset);
            }

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
                AppendFlowCode(sb, item.Flow!, $"{className}FlowStatsPlayer", $"{className}FlowStatsNpc", GetProjectCodeName());
            }

            sb.AppendLine("    }");

            if (hasAccessoryFlow)
            {
                AppendAccessoryFlowCode(sb, item.Flow!, $"{className}AccessoryPlayer", $"{className}FlowStatsPlayer", $"{className}FlowStatsNpc", GetProjectCodeName());
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
            bool hasFlow = HasFlowEvents(item.Flow, BuffFlowEventIds, BuffDefaultFlowEventId);

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
                AppendBuffFlowCode(sb, item.Flow!, className, GetProjectCodeName());

            sb.AppendLine("}");
            return sb.ToString();
        }

        private string GenerateProjectileCode(ModItemData item, string className)
        {
            var sb = new System.Text.StringBuilder();
            bool hasFlow = HasFlowEvents(item.Flow, ProjectileFlowEventIds, ProjectileDefaultFlowEventId);
            bool isWhipProjectile = item.IsWhipProjectile && item.DamageKind == ModDamageKind.Summon;
            bool usesBoneWhipTexture = isWhipProjectile && string.IsNullOrWhiteSpace(item.TexturePath);
            bool hasAnimation = !isWhipProjectile && item.IsMultiFrameTexture && item.TextureFrameCount > 1;

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
            if (usesBoneWhipTexture)
                sb.AppendLine("        public override string Texture => \"Terraria/Images/Projectile_\" + ProjectileID.BoneWhip;");
            if (usesBoneWhipTexture && (hasAnimation || isWhipProjectile))
                sb.AppendLine();

            if (hasAnimation || isWhipProjectile)
            {
                sb.AppendLine("        public override void SetStaticDefaults()");
                sb.AppendLine("        {");
                if (isWhipProjectile)
                    sb.AppendLine("            ProjectileID.Sets.IsAWhip[Type] = true;");
                if (hasAnimation)
                    sb.AppendLine($"            Main.projFrames[Type] = {Math.Max(2, item.TextureFrameCount)};");
                sb.AppendLine("        }");
                sb.AppendLine();
            }

            sb.AppendLine("        public override void SetDefaults()");
            sb.AppendLine("        {");
            if (isWhipProjectile)
            {
                sb.AppendLine("            Projectile.DefaultToWhip();");
                sb.AppendLine($"            Projectile.width = {item.Width};");
                sb.AppendLine($"            Projectile.height = {item.Height};");
                sb.AppendLine("            Projectile.DamageType = DamageClass.Summon;");
                sb.AppendLine("            Projectile.WhipSettings.Segments = 20;");
                sb.AppendLine("            Projectile.WhipSettings.RangeMultiplier = 1f;");
                if (item.Damage > 0)
                    sb.AppendLine($"            Projectile.damage = {item.Damage};");
                if (item.Knockback > 0)
                    sb.AppendLine($"            Projectile.knockBack = {item.Knockback};");
            }
            else
            {
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
            }
            sb.AppendLine("        }");

            if (hasFlow || hasAnimation)
                AppendProjectileFlowCode(sb, item.Flow, hasAnimation, Math.Max(2, item.TextureFrameCount), $"{className}FlowStatsPlayer", $"{className}FlowStatsNpc", GetProjectCodeName());

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

        private static void AppendWeaponHoldoutOffsetCode(System.Text.StringBuilder sb, ModItemData item, bool hasFlowWeaponOffset)
        {
            string baseX = FormatFloatLiteral(item.WeaponOffsetX);
            string baseY = FormatFloatLiteral(item.WeaponOffsetY);

            sb.AppendLine();
            sb.AppendLine("        public override Vector2? HoldoutOffset()");
            sb.AppendLine("        {");
            sb.AppendLine($"            Vector2 offset = new Vector2({baseX}, {baseY});");
            if (hasFlowWeaponOffset)
            {
                sb.AppendLine("            if (_flowWeaponHoldoutOffsetTicks > 0)");
                sb.AppendLine("                offset = _flowWeaponHoldoutOffset;");
            }
            sb.AppendLine("            return offset.LengthSquared() <= 0.0001f ? null : offset;");
            sb.AppendLine("        }");

            if (!hasFlowWeaponOffset)
                return;

            sb.AppendLine();
            sb.AppendLine("        public override void UpdateInventory(Player player)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (_flowWeaponHoldoutOffsetTicks <= 0)");
            sb.AppendLine("                return;");
            sb.AppendLine();
            sb.AppendLine("            _flowWeaponHoldoutOffsetTicks--;");
            sb.AppendLine("            if (_flowWeaponHoldoutOffsetTicks <= 0)");
            sb.AppendLine("                _flowWeaponHoldoutOffset = Vector2.Zero;");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        private void Flow_SetWeaponHoldoutOffset(float x, float y)");
            sb.AppendLine("        {");
            sb.AppendLine("            _flowWeaponHoldoutOffset = new Vector2(x, y);");
            sb.AppendLine("            _flowWeaponHoldoutOffsetTicks = 2;");
            sb.AppendLine("        }");
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
            return FlowCodeGenerator.HasFlowEvents(flow, eventIds, defaultEventId);
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
            int? flowUseStyleId = FlowCodeGenerator.ResolveItemUseStyleId(item.Flow);
            if (flowUseStyleId.HasValue)
                return flowUseStyleId.Value;

            return item.UseStyleId > 0
                ? item.UseStyleId
                : item.UsesProjectile ? 5 : 1;
        }

        private static string GetUseSoundExpression(ModItemData item, bool usesAmmoFallbackProjectile, string projectCodeName)
        {
            string useSound = item.Type == ItemType.Weapon ? NormalizeUseSound(item.UseSound) : string.Empty;
            if (string.IsNullOrWhiteSpace(useSound))
                return item.UsesProjectile || usesAmmoFallbackProjectile ? "SoundID.Item5" : "SoundID.Item1";

            if (IsSilentUseSound(useSound))
                return string.Empty;

            return GetSoundStyleExpression(useSound, projectCodeName);
        }

        private static bool IsSilentUseSound(string useSound)
        {
            return useSound.Equals("None", StringComparison.OrdinalIgnoreCase) ||
                   useSound.Equals("Silent", StringComparison.OrdinalIgnoreCase) ||
                   useSound.Equals("无", StringComparison.OrdinalIgnoreCase) ||
                   useSound.Equals("静音", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetSoundStyleExpression(string useSound, string projectCodeName)
        {
            string value = useSound.Trim();
            if (int.TryParse(value, out int itemSoundId))
                return $"SoundID.Item{Math.Max(1, itemSoundId)}";

            if (value.StartsWith("SoundID.", StringComparison.OrdinalIgnoreCase))
            {
                string identifier = SanitizeIdentifier(value["SoundID.".Length..]);
                return string.IsNullOrWhiteSpace(identifier) ? "SoundID.Item1" : $"SoundID.{identifier}";
            }

            if (value.StartsWith("Item", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(value[4..], out itemSoundId))
                return $"SoundID.Item{Math.Max(1, itemSoundId)}";

            if (value.StartsWith("new Terraria.Audio.SoundStyle(", StringComparison.Ordinal))
                return value;

            if (value.StartsWith("new SoundStyle(", StringComparison.Ordinal))
                return "new Terraria.Audio.SoundStyle(" + value["new SoundStyle(".Length..];

            if (value.StartsWith("SoundStyle(", StringComparison.Ordinal))
                return "new Terraria.Audio." + value;

            if (IsQuotedString(value))
                return $"new Terraria.Audio.SoundStyle({value})";

            if (value.Contains('/') || value.Contains('\\'))
            {
                string path = value.Trim().Trim('"').Replace('\\', '/').TrimStart('/');
                if (!string.IsNullOrWhiteSpace(projectCodeName) &&
                    !path.StartsWith(projectCodeName + "/", StringComparison.OrdinalIgnoreCase) &&
                    !path.StartsWith("Terraria/", StringComparison.OrdinalIgnoreCase))
                    path = $"{projectCodeName}/{path}";

                return $"new Terraria.Audio.SoundStyle(\"{FlowCodeUtility.EscapeString(path)}\")";
            }

            string soundIdName = SanitizeIdentifier(value);
            return string.IsNullOrWhiteSpace(soundIdName) ? "SoundID.Item1" : $"SoundID.{soundIdName}";
        }

        private static bool IsQuotedString(string value)
        {
            return value.Length >= 2 && value[0] == '"' && value[^1] == '"';
        }

        private static string GetProjectileTypeExpression(string? reference, string fallback, string projectCodeName)
        {
            return FlowCodeUtility.GetProjectileTypeExpression(reference, fallback, projectCodeName);
        }

        private static void AppendFlowCode(System.Text.StringBuilder sb, FlowScript flow, string tempStatPlayerClassName, string tempStatNpcClassName, string projectCodeName)
        {
            FlowCodeGenerator.AppendItemFlowCode(sb, flow, tempStatPlayerClassName, tempStatNpcClassName, projectCodeName);
        }

        private static void AppendAccessoryFlowCode(System.Text.StringBuilder sb, FlowScript flow, string accessoryPlayerClassName, string tempStatPlayerClassName, string tempStatNpcClassName, string projectCodeName)
        {
            FlowCodeGenerator.AppendAccessoryFlowCode(sb, flow, accessoryPlayerClassName, tempStatPlayerClassName, tempStatNpcClassName, projectCodeName);
        }

        private static void AppendBuffFlowCode(System.Text.StringBuilder sb, FlowScript flow, string buffClassName, string projectCodeName)
        {
            FlowCodeGenerator.AppendBuffFlowCode(sb, flow, buffClassName, projectCodeName);
        }

        private static void AppendProjectileFlowCode(System.Text.StringBuilder sb, FlowScript? flow, bool hasAnimation, int frameCount, string tempStatPlayerClassName, string tempStatNpcClassName, string projectCodeName)
        {
            FlowCodeGenerator.AppendProjectileFlowCode(sb, flow, hasAnimation, frameCount, tempStatPlayerClassName, tempStatNpcClassName, projectCodeName);
        }

        private static void AppendFlowTempStatsPlayerClass(System.Text.StringBuilder sb, string className)
        {
            FlowCodeGenerator.AppendFlowTempStatsPlayerClass(sb, className);
        }

        private static void AppendFlowTempStatsNpcClass(System.Text.StringBuilder sb, string className)
        {
            FlowCodeGenerator.AppendFlowTempStatsNpcClass(sb, className);
        }

        private sealed class ModItemBlockFile
        {
            public int Version { get; set; } = 1;
            public ModItemData? Item { get; set; }
            public string TextureExtension { get; set; } = string.Empty;
            public string TextureBase64 { get; set; } = string.Empty;
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
