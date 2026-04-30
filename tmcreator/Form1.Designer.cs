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
            components = new System.ComponentModel.Container();

            pnlLeft = new UIPanel();
            lblTitle = new UILabel();
            cmbItemType = new UIComboBox();
            txtName = new UITextBox();
            txtDisplayName = new UITextBox();
            txtDescription = new UITextBox();
            btnCreate = new UIButton();
            lblName = new UILabel();
            lblDisplayName = new UILabel();
            lblDescription = new UILabel();
            lblType = new UILabel();

            lblTexture = new UILabel();
            btnSelectTexture = new UIButton();
            btnClearTexture = new UIButton();
            picTexture = new PictureBox();

            numDamage = new NumericUpDown();
            numUseTime = new NumericUpDown();
            numKnockback = new NumericUpDown();
            numCriticalChance = new NumericUpDown();
            numPickaxePower = new NumericUpDown();
            numAxePower = new NumericUpDown();
            numHammerPower = new NumericUpDown();
            numWidth = new NumericUpDown();
            numHeight = new NumericUpDown();
            numValue = new NumericUpDown();
            numRarity = new NumericUpDown();
            numMinPick = new NumericUpDown();
            chkAutoReuse = new UICheckBox();
            chkUseTurn = new UICheckBox();

            lblDamage = new UILabel();
            lblUseTime = new UILabel();
            lblKnockback = new UILabel();
            lblCrit = new UILabel();
            lblPickPower = new UILabel();
            lblAxePower = new UILabel();
            lblHammerPower = new UILabel();
            lblWidth = new UILabel();
            lblHeight = new UILabel();
            lblValue = new UILabel();
            lblRarity = new UILabel();
            lblMinPick = new UILabel();

            pnlRight = new UIPanel();
            btnExport = new UIButton();
            lblPreviewTitle = new UILabel();
            flowItems = new FlowLayoutPanel();

            ((System.ComponentModel.ISupportInitialize)picTexture).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numDamage).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numUseTime).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numKnockback).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numCriticalChance).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numPickaxePower).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numAxePower).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numHammerPower).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numWidth).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numHeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numValue).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numRarity).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numMinPick).BeginInit();
            pnlLeft.SuspendLayout();
            pnlRight.SuspendLayout();
            SuspendLayout();

            var font9 = new Font("微软雅黑", 9F, FontStyle.Regular, GraphicsUnit.Point);
            var font10b = new Font("微软雅黑", 10F, FontStyle.Bold, GraphicsUnit.Point);
            var font11b = new Font("微软雅黑", 11F, FontStyle.Bold, GraphicsUnit.Point);
            var font12b = new Font("微软雅黑", 12F, FontStyle.Bold, GraphicsUnit.Point);
            var font14b = new Font("微软雅黑", 14F, FontStyle.Bold, GraphicsUnit.Point);
            var font8 = new Font("微软雅黑", 8F, FontStyle.Regular, GraphicsUnit.Point);

            Color clrBg = Color.FromArgb(30, 30, 30);
            Color clrPanelBg = Color.FromArgb(45, 45, 45);
            Color clrText = Color.FromArgb(220, 220, 220);
            Color clrSubText = Color.FromArgb(160, 160, 160);
            Color clrInputBg = Color.FromArgb(55, 55, 55);

            //
            // Form1
            //
            this.ClientSize = new Size(1200, 750);
            this.Text = "Terraria Mod制作器";
            this.MinimumSize = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = clrBg;
            this.Style = UIStyle.Black;

            //
            // pnlLeft
            //
            pnlLeft.Location = new Point(15, 15);
            pnlLeft.Size = new Size(420, 720);
            pnlLeft.Text = "创建物品";
            pnlLeft.Font = font9;
            pnlLeft.Style = UIStyle.Black;
            pnlLeft.FillColor = clrPanelBg;
            pnlLeft.RectColor = Color.FromArgb(80, 80, 80);

            //
            // lblTitle
            //
            lblTitle = new UILabel();
            lblTitle.Text = "物品创建";
            lblTitle.Font = font14b;
            lblTitle.Location = new Point(15, 35);
            lblTitle.Size = new Size(120, 30);
            lblTitle.ForeColor = clrText;

            //
            // lblType
            //
            lblType.Text = "类型";
            lblType.Location = new Point(15, 75);
            lblType.Size = new Size(60, 23);
            lblType.Font = font9;
            lblType.ForeColor = clrText;

            //
            // cmbItemType
            //
            cmbItemType.Location = new Point(100, 72);
            cmbItemType.Size = new Size(140, 26);
            cmbItemType.Items.AddRange(new object[] { "工具", "武器", "方块", "物品" });
            cmbItemType.SelectedIndex = 3;
            cmbItemType.Font = font9;
            cmbItemType.SelectedIndexChanged += cmbItemType_SelectedIndexChanged;

            //
            // lblName
            //
            lblName.Text = "内部名称";
            lblName.Location = new Point(15, 110);
            lblName.Size = new Size(80, 23);
            lblName.Font = font9;
            lblName.ForeColor = clrText;

            //
            // txtName
            //
            txtName.Location = new Point(100, 107);
            txtName.Size = new Size(205, 26);
            txtName.Font = font9;

            //
            // lblDisplayName
            //
            lblDisplayName.Text = "显示名称";
            lblDisplayName.Location = new Point(15, 145);
            lblDisplayName.Size = new Size(80, 23);
            lblDisplayName.Font = font9;
            lblDisplayName.ForeColor = clrText;

            //
            // txtDisplayName
            //
            txtDisplayName.Location = new Point(100, 142);
            txtDisplayName.Size = new Size(205, 26);
            txtDisplayName.Font = font9;

            //
            // lblDescription
            //
            lblDescription.Text = "描述";
            lblDescription.Location = new Point(15, 180);
            lblDescription.Size = new Size(60, 23);
            lblDescription.Font = font9;
            lblDescription.ForeColor = clrText;

            //
            // txtDescription
            //
            txtDescription.Location = new Point(100, 177);
            txtDescription.Size = new Size(290, 50);
            txtDescription.Multiline = true;
            txtDescription.Font = font9;

            //
            // Texture Section
            //
            lblTexture.Text = "贴图";
            lblTexture.Location = new Point(15, 242);
            lblTexture.Size = new Size(60, 23);
            lblTexture.Font = font9;
            lblTexture.ForeColor = clrText;

            picTexture.Location = new Point(100, 240);
            picTexture.Size = new Size(64, 64);
            picTexture.BackColor = clrInputBg;
            picTexture.BorderStyle = BorderStyle.FixedSingle;
            picTexture.SizeMode = PictureBoxSizeMode.Zoom;

            btnSelectTexture.Text = "选择贴图";
            btnSelectTexture.Location = new Point(175, 242);
            btnSelectTexture.Size = new Size(80, 28);
            btnSelectTexture.Font = font9;
            btnSelectTexture.Click += btnSelectTexture_Click;

            btnClearTexture.Text = "清除";
            btnClearTexture.Location = new Point(260, 242);
            btnClearTexture.Size = new Size(50, 28);
            btnClearTexture.Font = font9;
            btnClearTexture.Click += btnClearTexture_Click;

            //
            // Basic Stats Section
            //
            int yBase = 325;

            // Width
            lblWidth.Text = "宽度";
            lblWidth.Location = new Point(15, yBase);
            lblWidth.Size = new Size(80, 23);
            lblWidth.Font = font9;
            lblWidth.ForeColor = clrText;
            numWidth.Location = new Point(100, yBase - 2);
            numWidth.Size = new Size(70, 23);
            numWidth.Minimum = 1;
            numWidth.Maximum = 200;
            numWidth.Value = 20;
            numWidth.BackColor = clrInputBg;
            numWidth.ForeColor = clrText;
            numWidth.Font = font9;

            // Height
            lblHeight.Text = "高度";
            lblHeight.Location = new Point(200, yBase);
            lblHeight.Size = new Size(60, 23);
            lblHeight.Font = font9;
            lblHeight.ForeColor = clrText;
            numHeight.Location = new Point(260, yBase - 2);
            numHeight.Size = new Size(70, 23);
            numHeight.Minimum = 1;
            numHeight.Maximum = 200;
            numHeight.Value = 20;
            numHeight.BackColor = clrInputBg;
            numHeight.ForeColor = clrText;
            numHeight.Font = font9;

            // Value
            lblValue.Text = "价值";
            lblValue.Location = new Point(15, yBase + 38);
            lblValue.Size = new Size(80, 23);
            lblValue.Font = font9;
            lblValue.ForeColor = clrText;
            numValue.Location = new Point(100, yBase + 36);
            numValue.Size = new Size(70, 23);
            numValue.Maximum = 999999;
            numValue.BackColor = clrInputBg;
            numValue.ForeColor = clrText;
            numValue.Font = font9;

            // Rarity
            lblRarity.Text = "稀有度(0-11)";
            lblRarity.Location = new Point(200, yBase + 38);
            lblRarity.Size = new Size(100, 23);
            lblRarity.Font = font9;
            lblRarity.ForeColor = clrText;
            numRarity.Location = new Point(300, yBase + 36);
            numRarity.Size = new Size(60, 23);
            numRarity.Maximum = 11;
            numRarity.BackColor = clrInputBg;
            numRarity.ForeColor = clrText;
            numRarity.Font = font9;

            //
            // Tool properties (镐力/斧力/锤力)
            //
            int yTool = yBase + 80;

            lblPickPower.Text = "镐力";
            lblPickPower.Location = new Point(15, yTool);
            lblPickPower.Size = new Size(80, 23);
            lblPickPower.Font = font9;
            lblPickPower.ForeColor = clrText;
            numPickaxePower.Location = new Point(100, yTool - 2);
            numPickaxePower.Size = new Size(70, 23);
            numPickaxePower.Maximum = 500;
            numPickaxePower.BackColor = clrInputBg;
            numPickaxePower.ForeColor = clrText;
            numPickaxePower.Font = font9;

            lblAxePower.Text = "斧力";
            lblAxePower.Location = new Point(200, yTool);
            lblAxePower.Size = new Size(60, 23);
            lblAxePower.Font = font9;
            lblAxePower.ForeColor = clrText;
            numAxePower.Location = new Point(260, yTool - 2);
            numAxePower.Size = new Size(70, 23);
            numAxePower.Maximum = 500;
            numAxePower.BackColor = clrInputBg;
            numAxePower.ForeColor = clrText;
            numAxePower.Font = font9;

            lblHammerPower.Text = "锤力";
            lblHammerPower.Location = new Point(15, yTool + 38);
            lblHammerPower.Size = new Size(80, 23);
            lblHammerPower.Font = font9;
            lblHammerPower.ForeColor = clrText;
            numHammerPower.Location = new Point(100, yTool + 36);
            numHammerPower.Size = new Size(70, 23);
            numHammerPower.Maximum = 500;
            numHammerPower.BackColor = clrInputBg;
            numHammerPower.ForeColor = clrText;
            numHammerPower.Font = font9;

            //
            // Weapon/Combat properties (伤害/使用时间/击退/暴击)
            //
            int yCombat = yTool + 80;

            lblDamage.Text = "伤害";
            lblDamage.Location = new Point(15, yCombat);
            lblDamage.Size = new Size(80, 23);
            lblDamage.Font = font9;
            lblDamage.ForeColor = clrText;
            numDamage.Location = new Point(100, yCombat - 2);
            numDamage.Size = new Size(70, 23);
            numDamage.Maximum = 999999;
            numDamage.BackColor = clrInputBg;
            numDamage.ForeColor = clrText;
            numDamage.Font = font9;

            lblUseTime.Text = "使用时间";
            lblUseTime.Location = new Point(200, yCombat);
            lblUseTime.Size = new Size(80, 23);
            lblUseTime.Font = font9;
            lblUseTime.ForeColor = clrText;
            numUseTime.Location = new Point(285, yCombat - 2);
            numUseTime.Size = new Size(70, 23);
            numUseTime.Minimum = 1;
            numUseTime.Maximum = 999;
            numUseTime.Value = 30;
            numUseTime.BackColor = clrInputBg;
            numUseTime.ForeColor = clrText;
            numUseTime.Font = font9;

            lblKnockback.Text = "击退";
            lblKnockback.Location = new Point(15, yCombat + 38);
            lblKnockback.Size = new Size(80, 23);
            lblKnockback.Font = font9;
            lblKnockback.ForeColor = clrText;
            numKnockback.Location = new Point(100, yCombat + 36);
            numKnockback.Size = new Size(70, 23);
            numKnockback.DecimalPlaces = 1;
            numKnockback.Increment = 0.5M;
            numKnockback.Maximum = 20;
            numKnockback.BackColor = clrInputBg;
            numKnockback.ForeColor = clrText;
            numKnockback.Font = font9;

            lblCrit.Text = "暴击率%";
            lblCrit.Location = new Point(200, yCombat + 38);
            lblCrit.Size = new Size(80, 23);
            lblCrit.Font = font9;
            lblCrit.ForeColor = clrText;
            numCriticalChance.Location = new Point(285, yCombat + 36);
            numCriticalChance.Size = new Size(70, 23);
            numCriticalChance.Value = 4;
            numCriticalChance.Maximum = 100;
            numCriticalChance.BackColor = clrInputBg;
            numCriticalChance.ForeColor = clrText;
            numCriticalChance.Font = font9;

            chkAutoReuse.Text = "自动挥舞";
            chkAutoReuse.Location = new Point(15, yCombat + 76);
            chkAutoReuse.Size = new Size(100, 23);
            chkAutoReuse.Font = font9;

            chkUseTurn.Text = "转身使用";
            chkUseTurn.Location = new Point(130, yCombat + 76);
            chkUseTurn.Size = new Size(100, 23);
            chkUseTurn.Font = font9;

            //
            // Block properties (MinPick)
            //
            int yBlock = yTool + 80;

            lblMinPick.Text = "所需镐力";
            lblMinPick.Location = new Point(15, yBlock);
            lblMinPick.Size = new Size(80, 23);
            lblMinPick.Font = font9;
            lblMinPick.ForeColor = clrText;
            numMinPick.Location = new Point(100, yBlock - 2);
            numMinPick.Size = new Size(70, 23);
            numMinPick.Maximum = 500;
            numMinPick.BackColor = clrInputBg;
            numMinPick.ForeColor = clrText;
            numMinPick.Font = font9;

            //
            // btnCreate
            //
            btnCreate.Text = "创建物品";
            btnCreate.Location = new Point(100, 650);
            btnCreate.Size = new Size(200, 40);
            btnCreate.Font = font12b;
            btnCreate.Click += btnCreate_Click;

            //
            // pnlRight
            //
            pnlRight.Location = new Point(450, 15);
            pnlRight.Size = new Size(735, 720);
            pnlRight.Text = "物品预览";
            pnlRight.Font = font9;
            pnlRight.Style = UIStyle.Black;
            pnlRight.FillColor = clrPanelBg;
            pnlRight.RectColor = Color.FromArgb(80, 80, 80);

            //
            // btnExport
            //
            btnExport.Text = "导出Mod";
            btnExport.Location = new Point(600, 40);
            btnExport.Size = new Size(120, 35);
            btnExport.Font = font11b;
            btnExport.Style = UIStyle.Orange;
            btnExport.Click += btnExport_Click;

            //
            // lblPreviewTitle
            //
            lblPreviewTitle.Text = "已创建物品";
            lblPreviewTitle.Font = font12b;
            lblPreviewTitle.Location = new Point(15, 40);
            lblPreviewTitle.Size = new Size(200, 25);
            lblPreviewTitle.ForeColor = clrText;

            //
            // flowItems
            //
            flowItems.Location = new Point(15, 80);
            flowItems.Size = new Size(700, 625);
            flowItems.AutoScroll = true;
            flowItems.FlowDirection = FlowDirection.TopDown;
            flowItems.WrapContents = false;
            flowItems.BackColor = clrBg;

            //
            // Add controls
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

            // Tool fields
            pnlLeft.Controls.Add(lblPickPower);
            pnlLeft.Controls.Add(numPickaxePower);
            pnlLeft.Controls.Add(lblAxePower);
            pnlLeft.Controls.Add(numAxePower);
            pnlLeft.Controls.Add(lblHammerPower);
            pnlLeft.Controls.Add(numHammerPower);

            // Weapon/Tool combat fields
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

            // Block fields
            pnlLeft.Controls.Add(lblMinPick);
            pnlLeft.Controls.Add(numMinPick);

            pnlLeft.Controls.Add(btnCreate);

            pnlRight.Controls.Add(btnExport);
            pnlRight.Controls.Add(lblPreviewTitle);
            pnlRight.Controls.Add(flowItems);

            this.Controls.Add(pnlLeft);
            this.Controls.Add(pnlRight);

            ((System.ComponentModel.ISupportInitialize)picTexture).EndInit();
            ((System.ComponentModel.ISupportInitialize)numDamage).EndInit();
            ((System.ComponentModel.ISupportInitialize)numUseTime).EndInit();
            ((System.ComponentModel.ISupportInitialize)numKnockback).EndInit();
            ((System.ComponentModel.ISupportInitialize)numCriticalChance).EndInit();
            ((System.ComponentModel.ISupportInitialize)numPickaxePower).EndInit();
            ((System.ComponentModel.ISupportInitialize)numAxePower).EndInit();
            ((System.ComponentModel.ISupportInitialize)numHammerPower).EndInit();
            ((System.ComponentModel.ISupportInitialize)numWidth).EndInit();
            ((System.ComponentModel.ISupportInitialize)numHeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)numValue).EndInit();
            ((System.ComponentModel.ISupportInitialize)numRarity).EndInit();
            ((System.ComponentModel.ISupportInitialize)numMinPick).EndInit();
            pnlLeft.ResumeLayout(false);
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
