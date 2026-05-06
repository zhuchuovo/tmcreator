using Sunny.UI;

namespace tmcreator
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            pnlLeft = new UIPanel();
            lblTitle = new UILabel();
            lblType = new UILabel();
            cmbItemType = new UIComboBox();
            lblName = new UILabel();
            txtName = new UITextBox();
            lblDisplayName = new UILabel();
            txtDisplayName = new UITextBox();
            lblDescription = new UILabel();
            txtDescription = new UITextBox();
            lblTexture = new UILabel();
            picTexture = new PictureBox();
            btnSelectTexture = new UIButton();
            btnClearTexture = new UIButton();
            lblWidth = new UILabel();
            numWidth = new NumericUpDown();
            lblHeight = new UILabel();
            numHeight = new NumericUpDown();
            lblValue = new UILabel();
            numValue = new NumericUpDown();
            lblRarity = new UILabel();
            numRarity = new NumericUpDown();
            lblPickPower = new UILabel();
            numPickaxePower = new NumericUpDown();
            lblAxePower = new UILabel();
            numAxePower = new NumericUpDown();
            lblHammerPower = new UILabel();
            numHammerPower = new NumericUpDown();
            lblDamage = new UILabel();
            numDamage = new NumericUpDown();
            lblUseTime = new UILabel();
            numUseTime = new NumericUpDown();
            lblKnockback = new UILabel();
            numKnockback = new NumericUpDown();
            lblCrit = new UILabel();
            numCriticalChance = new NumericUpDown();
            chkAutoReuse = new UICheckBox();
            chkUseTurn = new UICheckBox();
            lblMinPick = new UILabel();
            numMinPick = new NumericUpDown();
            btnCreate = new UIButton();
            pnlRight = new UIPanel();
            btnExport = new UIButton();
            lblPreviewTitle = new UILabel();
            flowItems = new FlowLayoutPanel();
            pnlLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picTexture).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numWidth).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numHeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numValue).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numRarity).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numPickaxePower).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numAxePower).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numHammerPower).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numDamage).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numUseTime).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numKnockback).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numCriticalChance).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numMinPick).BeginInit();
            pnlRight.SuspendLayout();
            SuspendLayout();
            // 
            // pnlLeft
            // 
            pnlLeft.Controls.Add(lblTitle);
            pnlLeft.Controls.Add(lblType);
            pnlLeft.Controls.Add(cmbItemType);
            pnlLeft.Controls.Add(lblName);
            pnlLeft.Controls.Add(txtName);
            pnlLeft.Controls.Add(lblDisplayName);
            pnlLeft.Controls.Add(txtDisplayName);
            pnlLeft.Controls.Add(lblDescription);
            pnlLeft.Controls.Add(txtDescription);
            pnlLeft.Controls.Add(lblTexture);
            pnlLeft.Controls.Add(picTexture);
            pnlLeft.Controls.Add(btnSelectTexture);
            pnlLeft.Controls.Add(btnClearTexture);
            pnlLeft.Controls.Add(lblWidth);
            pnlLeft.Controls.Add(numWidth);
            pnlLeft.Controls.Add(lblHeight);
            pnlLeft.Controls.Add(numHeight);
            pnlLeft.Controls.Add(lblValue);
            pnlLeft.Controls.Add(numValue);
            pnlLeft.Controls.Add(lblRarity);
            pnlLeft.Controls.Add(numRarity);
            pnlLeft.Controls.Add(lblPickPower);
            pnlLeft.Controls.Add(numPickaxePower);
            pnlLeft.Controls.Add(lblAxePower);
            pnlLeft.Controls.Add(numAxePower);
            pnlLeft.Controls.Add(lblHammerPower);
            pnlLeft.Controls.Add(numHammerPower);
            pnlLeft.Controls.Add(lblDamage);
            pnlLeft.Controls.Add(numDamage);
            pnlLeft.Controls.Add(lblUseTime);
            pnlLeft.Controls.Add(numUseTime);
            pnlLeft.Controls.Add(lblKnockback);
            pnlLeft.Controls.Add(numKnockback);
            pnlLeft.Controls.Add(lblCrit);
            pnlLeft.Controls.Add(numCriticalChance);
            pnlLeft.Controls.Add(chkAutoReuse);
            pnlLeft.Controls.Add(chkUseTurn);
            pnlLeft.Controls.Add(lblMinPick);
            pnlLeft.Controls.Add(numMinPick);
            pnlLeft.Controls.Add(btnCreate);
            pnlLeft.FillColor = Color.FromArgb(45, 45, 45);
            pnlLeft.FillColor2 = Color.FromArgb(24, 24, 24);
            pnlLeft.Font = new Font("微软雅黑", 9F);
            pnlLeft.ForeColor = Color.White;
            pnlLeft.Location = new Point(15, 35);
            pnlLeft.Margin = new Padding(4, 5, 4, 5);
            pnlLeft.MinimumSize = new Size(1, 1);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.RectColor = Color.FromArgb(80, 80, 80);
            pnlLeft.Size = new Size(420, 720);
            pnlLeft.Style = UIStyle.Custom;
            pnlLeft.TabIndex = 0;
            pnlLeft.Text = "创建物品";
            pnlLeft.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("微软雅黑", 14F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(220, 220, 220);
            lblTitle.Location = new Point(15, 35);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(120, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "物品创建";
            // 
            // lblType
            // 
            lblType.Font = new Font("微软雅黑", 9F);
            lblType.ForeColor = Color.FromArgb(220, 220, 220);
            lblType.Location = new Point(15, 75);
            lblType.Name = "lblType";
            lblType.Size = new Size(60, 23);
            lblType.TabIndex = 1;
            lblType.Text = "类型";
            // 
            // cmbItemType
            // 
            cmbItemType.DataSource = null;
            cmbItemType.FillColor = Color.White;
            cmbItemType.Font = new Font("微软雅黑", 9F);
            cmbItemType.ItemHoverColor = Color.FromArgb(155, 200, 255);
            cmbItemType.Items.AddRange(new object[] { "工具", "武器", "方块", "物品" });
            cmbItemType.ItemSelectForeColor = Color.FromArgb(235, 243, 255);
            cmbItemType.Location = new Point(100, 72);
            cmbItemType.Margin = new Padding(4, 5, 4, 5);
            cmbItemType.MinimumSize = new Size(63, 0);
            cmbItemType.Name = "cmbItemType";
            cmbItemType.Padding = new Padding(0, 0, 30, 2);
            cmbItemType.Size = new Size(140, 26);
            cmbItemType.SymbolSize = 24;
            cmbItemType.TabIndex = 2;
            cmbItemType.Text = "物品";
            cmbItemType.TextAlignment = ContentAlignment.MiddleLeft;
            cmbItemType.Watermark = "";
            cmbItemType.SelectedIndexChanged += cmbItemType_SelectedIndexChanged;
            // 
            // lblName
            // 
            lblName.Font = new Font("微软雅黑", 9F);
            lblName.ForeColor = Color.FromArgb(220, 220, 220);
            lblName.Location = new Point(15, 110);
            lblName.Name = "lblName";
            lblName.Size = new Size(80, 23);
            lblName.TabIndex = 3;
            lblName.Text = "内部名称";
            // 
            // txtName
            // 
            txtName.Font = new Font("微软雅黑", 9F);
            txtName.Location = new Point(100, 107);
            txtName.Margin = new Padding(4, 5, 4, 5);
            txtName.MinimumSize = new Size(1, 16);
            txtName.Name = "txtName";
            txtName.Padding = new Padding(5);
            txtName.ShowText = false;
            txtName.Size = new Size(205, 26);
            txtName.TabIndex = 4;
            txtName.TextAlignment = ContentAlignment.MiddleLeft;
            txtName.Watermark = "";
            // 
            // lblDisplayName
            // 
            lblDisplayName.Font = new Font("微软雅黑", 9F);
            lblDisplayName.ForeColor = Color.FromArgb(220, 220, 220);
            lblDisplayName.Location = new Point(15, 145);
            lblDisplayName.Name = "lblDisplayName";
            lblDisplayName.Size = new Size(80, 23);
            lblDisplayName.TabIndex = 5;
            lblDisplayName.Text = "显示名称";
            // 
            // txtDisplayName
            // 
            txtDisplayName.Font = new Font("微软雅黑", 9F);
            txtDisplayName.Location = new Point(100, 142);
            txtDisplayName.Margin = new Padding(4, 5, 4, 5);
            txtDisplayName.MinimumSize = new Size(1, 16);
            txtDisplayName.Name = "txtDisplayName";
            txtDisplayName.Padding = new Padding(5);
            txtDisplayName.ShowText = false;
            txtDisplayName.Size = new Size(205, 26);
            txtDisplayName.TabIndex = 6;
            txtDisplayName.TextAlignment = ContentAlignment.MiddleLeft;
            txtDisplayName.Watermark = "";
            // 
            // lblDescription
            // 
            lblDescription.Font = new Font("微软雅黑", 9F);
            lblDescription.ForeColor = Color.FromArgb(220, 220, 220);
            lblDescription.Location = new Point(15, 180);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(60, 23);
            lblDescription.TabIndex = 7;
            lblDescription.Text = "描述";
            // 
            // txtDescription
            // 
            txtDescription.Font = new Font("微软雅黑", 9F);
            txtDescription.Location = new Point(100, 177);
            txtDescription.Margin = new Padding(4, 5, 4, 5);
            txtDescription.MinimumSize = new Size(1, 16);
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.Padding = new Padding(5);
            txtDescription.ShowText = false;
            txtDescription.Size = new Size(290, 50);
            txtDescription.TabIndex = 8;
            txtDescription.TextAlignment = ContentAlignment.MiddleLeft;
            txtDescription.Watermark = "";
            // 
            // lblTexture
            // 
            lblTexture.Font = new Font("微软雅黑", 9F);
            lblTexture.ForeColor = Color.FromArgb(220, 220, 220);
            lblTexture.Location = new Point(15, 242);
            lblTexture.Name = "lblTexture";
            lblTexture.Size = new Size(60, 23);
            lblTexture.TabIndex = 9;
            lblTexture.Text = "贴图";
            // 
            // picTexture
            // 
            picTexture.BackColor = Color.FromArgb(55, 55, 55);
            picTexture.BorderStyle = BorderStyle.FixedSingle;
            picTexture.Location = new Point(100, 240);
            picTexture.Name = "picTexture";
            picTexture.Size = new Size(64, 64);
            picTexture.SizeMode = PictureBoxSizeMode.Zoom;
            picTexture.TabIndex = 10;
            picTexture.TabStop = false;
            // 
            // btnSelectTexture
            // 
            btnSelectTexture.Font = new Font("微软雅黑", 9F);
            btnSelectTexture.Location = new Point(175, 242);
            btnSelectTexture.MinimumSize = new Size(1, 1);
            btnSelectTexture.Name = "btnSelectTexture";
            btnSelectTexture.Size = new Size(80, 28);
            btnSelectTexture.TabIndex = 11;
            btnSelectTexture.Text = "选择贴图";
            btnSelectTexture.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnSelectTexture.Click += btnSelectTexture_Click;
            // 
            // btnClearTexture
            // 
            btnClearTexture.Font = new Font("微软雅黑", 9F);
            btnClearTexture.Location = new Point(260, 242);
            btnClearTexture.MinimumSize = new Size(1, 1);
            btnClearTexture.Name = "btnClearTexture";
            btnClearTexture.Size = new Size(50, 28);
            btnClearTexture.TabIndex = 12;
            btnClearTexture.Text = "清除";
            btnClearTexture.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnClearTexture.Click += btnClearTexture_Click;
            // 
            // lblWidth
            // 
            lblWidth.Font = new Font("微软雅黑", 9F);
            lblWidth.ForeColor = Color.FromArgb(220, 220, 220);
            lblWidth.Location = new Point(15, 325);
            lblWidth.Name = "lblWidth";
            lblWidth.Size = new Size(80, 23);
            lblWidth.TabIndex = 13;
            lblWidth.Text = "宽度";
            // 
            // numWidth
            // 
            numWidth.BackColor = Color.FromArgb(55, 55, 55);
            numWidth.Font = new Font("微软雅黑", 9F);
            numWidth.ForeColor = Color.FromArgb(220, 220, 220);
            numWidth.Location = new Point(100, 325);
            numWidth.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
            numWidth.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numWidth.Name = "numWidth";
            numWidth.Size = new Size(70, 23);
            numWidth.TabIndex = 14;
            numWidth.Value = new decimal(new int[] { 20, 0, 0, 0 });
            // 
            // lblHeight
            // 
            lblHeight.Font = new Font("微软雅黑", 9F);
            lblHeight.ForeColor = Color.FromArgb(220, 220, 220);
            lblHeight.Location = new Point(200, 325);
            lblHeight.Name = "lblHeight";
            lblHeight.Size = new Size(60, 23);
            lblHeight.TabIndex = 15;
            lblHeight.Text = "高度";
            // 
            // numHeight
            // 
            numHeight.BackColor = Color.FromArgb(55, 55, 55);
            numHeight.Font = new Font("微软雅黑", 9F);
            numHeight.ForeColor = Color.FromArgb(220, 220, 220);
            numHeight.Location = new Point(260, 325);
            numHeight.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
            numHeight.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numHeight.Name = "numHeight";
            numHeight.Size = new Size(70, 23);
            numHeight.TabIndex = 16;
            numHeight.Value = new decimal(new int[] { 20, 0, 0, 0 });
            // 
            // lblValue
            // 
            lblValue.Font = new Font("微软雅黑", 9F);
            lblValue.ForeColor = Color.FromArgb(220, 220, 220);
            lblValue.Location = new Point(15, 325);
            lblValue.Name = "lblValue";
            lblValue.Size = new Size(80, 23);
            lblValue.TabIndex = 17;
            lblValue.Text = "价值";
            // 
            // numValue
            // 
            numValue.BackColor = Color.FromArgb(55, 55, 55);
            numValue.Font = new Font("微软雅黑", 9F);
            numValue.ForeColor = Color.FromArgb(220, 220, 220);
            numValue.Location = new Point(100, 325);
            numValue.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
            numValue.Name = "numValue";
            numValue.Size = new Size(70, 23);
            numValue.TabIndex = 18;
            // 
            // lblRarity
            // 
            lblRarity.Font = new Font("微软雅黑", 9F);
            lblRarity.ForeColor = Color.FromArgb(220, 220, 220);
            lblRarity.Location = new Point(200, 325);
            lblRarity.Name = "lblRarity";
            lblRarity.Size = new Size(100, 23);
            lblRarity.TabIndex = 19;
            lblRarity.Text = "稀有度(0-11)";
            // 
            // numRarity
            // 
            numRarity.BackColor = Color.FromArgb(55, 55, 55);
            numRarity.Font = new Font("微软雅黑", 9F);
            numRarity.ForeColor = Color.FromArgb(220, 220, 220);
            numRarity.Location = new Point(300, 325);
            numRarity.Maximum = new decimal(new int[] { 11, 0, 0, 0 });
            numRarity.Name = "numRarity";
            numRarity.Size = new Size(60, 23);
            numRarity.TabIndex = 20;
            // 
            // lblPickPower
            // 
            lblPickPower.Font = new Font("微软雅黑", 9F);
            lblPickPower.ForeColor = Color.FromArgb(220, 220, 220);
            lblPickPower.Location = new Point(15, 325);
            lblPickPower.Name = "lblPickPower";
            lblPickPower.Size = new Size(80, 23);
            lblPickPower.TabIndex = 21;
            lblPickPower.Text = "镐力";
            // 
            // numPickaxePower
            // 
            numPickaxePower.BackColor = Color.FromArgb(55, 55, 55);
            numPickaxePower.Font = new Font("微软雅黑", 9F);
            numPickaxePower.ForeColor = Color.FromArgb(220, 220, 220);
            numPickaxePower.Location = new Point(100, 325);
            numPickaxePower.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            numPickaxePower.Name = "numPickaxePower";
            numPickaxePower.Size = new Size(70, 23);
            numPickaxePower.TabIndex = 22;
            // 
            // lblAxePower
            // 
            lblAxePower.Font = new Font("微软雅黑", 9F);
            lblAxePower.ForeColor = Color.FromArgb(220, 220, 220);
            lblAxePower.Location = new Point(200, 325);
            lblAxePower.Name = "lblAxePower";
            lblAxePower.Size = new Size(60, 23);
            lblAxePower.TabIndex = 23;
            lblAxePower.Text = "斧力";
            // 
            // numAxePower
            // 
            numAxePower.BackColor = Color.FromArgb(55, 55, 55);
            numAxePower.Font = new Font("微软雅黑", 9F);
            numAxePower.ForeColor = Color.FromArgb(220, 220, 220);
            numAxePower.Location = new Point(260, 325);
            numAxePower.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            numAxePower.Name = "numAxePower";
            numAxePower.Size = new Size(70, 23);
            numAxePower.TabIndex = 24;
            // 
            // lblHammerPower
            // 
            lblHammerPower.Font = new Font("微软雅黑", 9F);
            lblHammerPower.ForeColor = Color.FromArgb(220, 220, 220);
            lblHammerPower.Location = new Point(15, 325);
            lblHammerPower.Name = "lblHammerPower";
            lblHammerPower.Size = new Size(80, 23);
            lblHammerPower.TabIndex = 25;
            lblHammerPower.Text = "锤力";
            // 
            // numHammerPower
            // 
            numHammerPower.BackColor = Color.FromArgb(55, 55, 55);
            numHammerPower.Font = new Font("微软雅黑", 9F);
            numHammerPower.ForeColor = Color.FromArgb(220, 220, 220);
            numHammerPower.Location = new Point(100, 325);
            numHammerPower.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            numHammerPower.Name = "numHammerPower";
            numHammerPower.Size = new Size(70, 23);
            numHammerPower.TabIndex = 26;
            // 
            // lblDamage
            // 
            lblDamage.Font = new Font("微软雅黑", 9F);
            lblDamage.ForeColor = Color.FromArgb(220, 220, 220);
            lblDamage.Location = new Point(15, 325);
            lblDamage.Name = "lblDamage";
            lblDamage.Size = new Size(80, 23);
            lblDamage.TabIndex = 27;
            lblDamage.Text = "伤害";
            // 
            // numDamage
            // 
            numDamage.BackColor = Color.FromArgb(55, 55, 55);
            numDamage.Font = new Font("微软雅黑", 9F);
            numDamage.ForeColor = Color.FromArgb(220, 220, 220);
            numDamage.Location = new Point(100, 325);
            numDamage.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
            numDamage.Name = "numDamage";
            numDamage.Size = new Size(70, 23);
            numDamage.TabIndex = 28;
            // 
            // lblUseTime
            // 
            lblUseTime.Font = new Font("微软雅黑", 9F);
            lblUseTime.ForeColor = Color.FromArgb(220, 220, 220);
            lblUseTime.Location = new Point(200, 325);
            lblUseTime.Name = "lblUseTime";
            lblUseTime.Size = new Size(80, 23);
            lblUseTime.TabIndex = 29;
            lblUseTime.Text = "使用时间";
            // 
            // numUseTime
            // 
            numUseTime.BackColor = Color.FromArgb(55, 55, 55);
            numUseTime.Font = new Font("微软雅黑", 9F);
            numUseTime.ForeColor = Color.FromArgb(220, 220, 220);
            numUseTime.Location = new Point(285, 325);
            numUseTime.Maximum = new decimal(new int[] { 999, 0, 0, 0 });
            numUseTime.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numUseTime.Name = "numUseTime";
            numUseTime.Size = new Size(70, 23);
            numUseTime.TabIndex = 30;
            numUseTime.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // lblKnockback
            // 
            lblKnockback.Font = new Font("微软雅黑", 9F);
            lblKnockback.ForeColor = Color.FromArgb(220, 220, 220);
            lblKnockback.Location = new Point(15, 325);
            lblKnockback.Name = "lblKnockback";
            lblKnockback.Size = new Size(80, 23);
            lblKnockback.TabIndex = 31;
            lblKnockback.Text = "击退";
            // 
            // numKnockback
            // 
            numKnockback.BackColor = Color.FromArgb(55, 55, 55);
            numKnockback.DecimalPlaces = 1;
            numKnockback.Font = new Font("微软雅黑", 9F);
            numKnockback.ForeColor = Color.FromArgb(220, 220, 220);
            numKnockback.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
            numKnockback.Location = new Point(100, 325);
            numKnockback.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            numKnockback.Name = "numKnockback";
            numKnockback.Size = new Size(70, 23);
            numKnockback.TabIndex = 32;
            // 
            // lblCrit
            // 
            lblCrit.Font = new Font("微软雅黑", 9F);
            lblCrit.ForeColor = Color.FromArgb(220, 220, 220);
            lblCrit.Location = new Point(200, 325);
            lblCrit.Name = "lblCrit";
            lblCrit.Size = new Size(80, 23);
            lblCrit.TabIndex = 33;
            lblCrit.Text = "暴击率%";
            // 
            // numCriticalChance
            // 
            numCriticalChance.BackColor = Color.FromArgb(55, 55, 55);
            numCriticalChance.Font = new Font("微软雅黑", 9F);
            numCriticalChance.ForeColor = Color.FromArgb(220, 220, 220);
            numCriticalChance.Location = new Point(285, 325);
            numCriticalChance.Name = "numCriticalChance";
            numCriticalChance.Size = new Size(70, 23);
            numCriticalChance.TabIndex = 34;
            numCriticalChance.Value = new decimal(new int[] { 4, 0, 0, 0 });
            // 
            // chkAutoReuse
            // 
            chkAutoReuse.Font = new Font("微软雅黑", 9F);
            chkAutoReuse.ForeColor = Color.FromArgb(48, 48, 48);
            chkAutoReuse.Location = new Point(15, 325);
            chkAutoReuse.MinimumSize = new Size(1, 1);
            chkAutoReuse.Name = "chkAutoReuse";
            chkAutoReuse.Size = new Size(100, 23);
            chkAutoReuse.TabIndex = 35;
            chkAutoReuse.Text = "自动挥舞";
            // 
            // chkUseTurn
            // 
            chkUseTurn.Font = new Font("微软雅黑", 9F);
            chkUseTurn.ForeColor = Color.FromArgb(48, 48, 48);
            chkUseTurn.Location = new Point(130, 325);
            chkUseTurn.MinimumSize = new Size(1, 1);
            chkUseTurn.Name = "chkUseTurn";
            chkUseTurn.Size = new Size(100, 23);
            chkUseTurn.TabIndex = 36;
            chkUseTurn.Text = "转身使用";
            // 
            // lblMinPick
            // 
            lblMinPick.Font = new Font("微软雅黑", 9F);
            lblMinPick.ForeColor = Color.FromArgb(220, 220, 220);
            lblMinPick.Location = new Point(15, 325);
            lblMinPick.Name = "lblMinPick";
            lblMinPick.Size = new Size(80, 23);
            lblMinPick.TabIndex = 37;
            lblMinPick.Text = "所需镐力";
            // 
            // numMinPick
            // 
            numMinPick.BackColor = Color.FromArgb(55, 55, 55);
            numMinPick.Font = new Font("微软雅黑", 9F);
            numMinPick.ForeColor = Color.FromArgb(220, 220, 220);
            numMinPick.Location = new Point(100, 325);
            numMinPick.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            numMinPick.Name = "numMinPick";
            numMinPick.Size = new Size(70, 23);
            numMinPick.TabIndex = 38;
            // 
            // btnCreate
            // 
            btnCreate.Font = new Font("微软雅黑", 12F, FontStyle.Bold);
            btnCreate.Location = new Point(100, 650);
            btnCreate.MinimumSize = new Size(1, 1);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(200, 40);
            btnCreate.TabIndex = 39;
            btnCreate.Text = "创建物品";
            btnCreate.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnCreate.Click += btnCreate_Click;
            // 
            // pnlRight
            // 
            pnlRight.Controls.Add(btnExport);
            pnlRight.Controls.Add(lblPreviewTitle);
            pnlRight.Controls.Add(flowItems);
            pnlRight.FillColor = Color.FromArgb(45, 45, 45);
            pnlRight.FillColor2 = Color.FromArgb(24, 24, 24);
            pnlRight.Font = new Font("微软雅黑", 9F);
            pnlRight.ForeColor = Color.White;
            pnlRight.Location = new Point(450, 35);
            pnlRight.Margin = new Padding(4, 5, 4, 5);
            pnlRight.MinimumSize = new Size(1, 1);
            pnlRight.Name = "pnlRight";
            pnlRight.RectColor = Color.FromArgb(80, 80, 80);
            pnlRight.Size = new Size(735, 720);
            pnlRight.Style = UIStyle.Custom;
            pnlRight.TabIndex = 1;
            pnlRight.Text = "物品预览";
            pnlRight.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // btnExport
            // 
            btnExport.FillColor = Color.FromArgb(220, 155, 40);
            btnExport.FillColor2 = Color.FromArgb(220, 155, 40);
            btnExport.FillHoverColor = Color.FromArgb(227, 175, 83);
            btnExport.FillPressColor = Color.FromArgb(176, 124, 32);
            btnExport.FillSelectedColor = Color.FromArgb(176, 124, 32);
            btnExport.Font = new Font("微软雅黑", 11F, FontStyle.Bold);
            btnExport.LightColor = Color.FromArgb(253, 249, 241);
            btnExport.Location = new Point(595, 35);
            btnExport.MinimumSize = new Size(1, 1);
            btnExport.Name = "btnExport";
            btnExport.RectColor = Color.FromArgb(220, 155, 40);
            btnExport.RectHoverColor = Color.FromArgb(227, 175, 83);
            btnExport.RectPressColor = Color.FromArgb(176, 124, 32);
            btnExport.RectSelectedColor = Color.FromArgb(176, 124, 32);
            btnExport.Size = new Size(120, 35);
            btnExport.Style = UIStyle.Custom;
            btnExport.TabIndex = 0;
            btnExport.Text = "导出Mod";
            btnExport.TipsFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            btnExport.Click += btnExport_Click;
            // 
            // lblPreviewTitle
            // 
            lblPreviewTitle.Font = new Font("微软雅黑", 12F, FontStyle.Bold);
            lblPreviewTitle.ForeColor = Color.FromArgb(220, 220, 220);
            lblPreviewTitle.Location = new Point(15, 40);
            lblPreviewTitle.Name = "lblPreviewTitle";
            lblPreviewTitle.Size = new Size(200, 25);
            lblPreviewTitle.TabIndex = 1;
            lblPreviewTitle.Text = "已创建物品";
            // 
            // flowItems
            // 
            flowItems.AutoScroll = true;
            flowItems.BackColor = Color.FromArgb(30, 30, 30);
            flowItems.FlowDirection = FlowDirection.TopDown;
            flowItems.Location = new Point(15, 80);
            flowItems.Name = "flowItems";
            flowItems.Size = new Size(700, 625);
            flowItems.TabIndex = 2;
            flowItems.WrapContents = false;
            // 
            // Form1
            // 
            BackColor = Color.FromArgb(24, 24, 24);
            ClientSize = new Size(1200, 750);
            ControlBoxFillHoverColor = Color.FromArgb(70, 70, 70);
            Controls.Add(pnlLeft);
            Controls.Add(pnlRight);
            ForeColor = Color.White;
            MinimumSize = new Size(1000, 600);
            Name = "Form1";
            RectColor = Color.FromArgb(18, 58, 92);
            Style = UIStyle.Custom;
            Text = "Terraria Mod制作器";
            TitleColor = Color.FromArgb(21, 21, 21);
            ZoomScaleRect = new Rectangle(15, 15, 1200, 750);
            pnlLeft.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picTexture).EndInit();
            ((System.ComponentModel.ISupportInitialize)numWidth).EndInit();
            ((System.ComponentModel.ISupportInitialize)numHeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)numValue).EndInit();
            ((System.ComponentModel.ISupportInitialize)numRarity).EndInit();
            ((System.ComponentModel.ISupportInitialize)numPickaxePower).EndInit();
            ((System.ComponentModel.ISupportInitialize)numAxePower).EndInit();
            ((System.ComponentModel.ISupportInitialize)numHammerPower).EndInit();
            ((System.ComponentModel.ISupportInitialize)numDamage).EndInit();
            ((System.ComponentModel.ISupportInitialize)numUseTime).EndInit();
            ((System.ComponentModel.ISupportInitialize)numKnockback).EndInit();
            ((System.ComponentModel.ISupportInitialize)numCriticalChance).EndInit();
            ((System.ComponentModel.ISupportInitialize)numMinPick).EndInit();
            pnlRight.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private UIPanel pnlLeft;
        private UIPanel pnlRight;
        private UILabel lblTitle;
        private UILabel lblType;
        private UILabel lblName;
        private UILabel lblDisplayName;
        private UILabel lblDescription;
        private UILabel lblTexture;
        private UILabel lblDamage;
        private UILabel lblUseTime;
        private UILabel lblKnockback;
        private UILabel lblCrit;
        private UILabel lblPickPower;
        private UILabel lblAxePower;
        private UILabel lblHammerPower;
        private UILabel lblWidth;
        private UILabel lblHeight;
        private UILabel lblValue;
        private UILabel lblRarity;
        private UILabel lblMinPick;
        private UILabel lblPreviewTitle;
        private UIComboBox cmbItemType;
        private UITextBox txtName;
        private UITextBox txtDisplayName;
        private UITextBox txtDescription;
        private PictureBox picTexture;
        private UIButton btnSelectTexture;
        private UIButton btnClearTexture;
        private UIButton btnCreate;
        private UIButton btnExport;
        private NumericUpDown numDamage;
        private NumericUpDown numUseTime;
        private NumericUpDown numKnockback;
        private NumericUpDown numCriticalChance;
        private NumericUpDown numPickaxePower;
        private NumericUpDown numAxePower;
        private NumericUpDown numHammerPower;
        private NumericUpDown numWidth;
        private NumericUpDown numHeight;
        private NumericUpDown numValue;
        private NumericUpDown numRarity;
        private NumericUpDown numMinPick;
        private UICheckBox chkAutoReuse;
        private UICheckBox chkUseTurn;
        private FlowLayoutPanel flowItems;
    }
}
