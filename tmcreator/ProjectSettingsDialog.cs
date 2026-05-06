using Sunny.UI;

namespace tmcreator
{
    internal sealed class ProjectSettingsDialog : UIForm
    {
        private readonly UITextBox _txtName = new();
        private readonly TextBox _txtDescription = new();
        private readonly UITextBox _txtVersion = new();
        private readonly UITextBox _txtAuthor = new();
        private readonly UITextBox _txtHomepage = new();
        private readonly UITextBox _txtIconPath = new();
        private readonly PictureBox _picIcon = new();

        private static readonly Color ClrBg = Color.FromArgb(14, 20, 28);
        private static readonly Color ClrPanel = Color.FromArgb(22, 31, 43);
        private static readonly Color ClrInput = Color.FromArgb(12, 18, 27);
        private static readonly Color ClrBorder = Color.FromArgb(60, 78, 100);
        private static readonly Color ClrText = Color.FromArgb(238, 244, 252);
        private static readonly Color ClrSubText = Color.FromArgb(154, 171, 193);
        private static readonly Color ClrAccent = Color.FromArgb(53, 199, 183);
        private static readonly Font FontTitle = new("Microsoft YaHei UI", 12F, FontStyle.Bold);
        private static readonly Font FontBody = new("Microsoft YaHei UI", 9F);
        private static readonly Font FontBodyBold = new("Microsoft YaHei UI", 9F, FontStyle.Bold);

        public ProjectSettingsDialog(
            string projectName,
            string projectDescription,
            string buildVersion,
            string buildAuthor,
            string buildHomepage,
            string projectIconPath)
        {
            Text = "修改项目";
            ClientSize = new Size(620, 520);
            MinimumSize = new Size(620, 520);
            MaximumSize = new Size(620, 520);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = ClrBg;
            Style = UIStyle.Black;

            var titleLabel = new Label
            {
                Text = "修改项目",
                Location = new Point(24, 22),
                Size = new Size(240, 28),
                Font = FontTitle,
                ForeColor = ClrText,
                BackColor = Color.Transparent
            };
            Controls.Add(titleLabel);

            var hint = new Label
            {
                Text = "这些信息会保存到工程文件，并在导出 Mod 时写入 build.txt 与图标。",
                Location = new Point(24, 52),
                Size = new Size(540, 22),
                Font = FontBody,
                ForeColor = ClrSubText,
                BackColor = Color.Transparent
            };
            Controls.Add(hint);

            AddLabel("修改名称", 24, 94);
            ConfigureInput(_txtName, 132, 88, 430, 32, projectName);
            Controls.Add(_txtName);

            AddLabel("修改介绍", 24, 138);
            _txtDescription.SetBounds(132, 132, 430, 88);
            _txtDescription.Multiline = true;
            _txtDescription.ScrollBars = ScrollBars.Vertical;
            _txtDescription.Text = projectDescription;
            _txtDescription.Font = FontBody;
            _txtDescription.BorderStyle = BorderStyle.FixedSingle;
            _txtDescription.BackColor = ClrInput;
            _txtDescription.ForeColor = ClrText;
            Controls.Add(_txtDescription);

            AddLabel("build 版本号", 24, 242);
            ConfigureInput(_txtVersion, 132, 236, 160, 32, string.IsNullOrWhiteSpace(buildVersion) ? "0.1.0" : buildVersion);
            Controls.Add(_txtVersion);

            AddLabel("作者", 322, 242);
            ConfigureInput(_txtAuthor, 374, 236, 188, 32, buildAuthor);
            Controls.Add(_txtAuthor);

            AddLabel("主页", 24, 286);
            ConfigureInput(_txtHomepage, 132, 280, 430, 32, buildHomepage);
            Controls.Add(_txtHomepage);

            AddLabel("模组 icon", 24, 334);
            _picIcon.SetBounds(132, 326, 64, 64);
            _picIcon.BorderStyle = BorderStyle.FixedSingle;
            _picIcon.BackColor = ClrInput;
            _picIcon.SizeMode = PictureBoxSizeMode.Zoom;
            Controls.Add(_picIcon);

            ConfigureInput(_txtIconPath, 210, 326, 352, 32, projectIconPath);
            _txtIconPath.ReadOnly = true;
            Controls.Add(_txtIconPath);

            var btnChooseIcon = CreateButton("选择图标", 210, 358, 96, 32, Color.FromArgb(67, 83, 105));
            btnChooseIcon.Click += (s, e) => ChooseIcon();
            Controls.Add(btnChooseIcon);

            var btnClearIcon = CreateButton("清除", 316, 358, 76, 32, Color.FromArgb(67, 83, 105));
            btnClearIcon.Click += (s, e) => SetIconPath(string.Empty);
            Controls.Add(btnClearIcon);

            var iconHint = new Label
            {
                Text = "导出时会复制为 icon.png。",
                Location = new Point(405, 364),
                Size = new Size(160, 20),
                Font = FontBody,
                ForeColor = ClrSubText,
                BackColor = Color.Transparent
            };
            Controls.Add(iconHint);

            var btnOk = CreateButton("确定", 398, 456, 78, 34, ClrAccent);
            btnOk.Click += (s, e) => AcceptSettings();
            Controls.Add(btnOk);

            var btnCancel = CreateButton("取消", 486, 456, 78, 34, Color.FromArgb(67, 83, 105));
            btnCancel.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };
            Controls.Add(btnCancel);

            AcceptButton = btnOk;
            CancelButton = btnCancel;
            SetIconPath(projectIconPath);
        }

        public string ProjectName => _txtName.Text.Trim();
        public string ProjectDescription => _txtDescription.Text.Trim();
        public string BuildVersion => string.IsNullOrWhiteSpace(_txtVersion.Text) ? "0.1.0" : _txtVersion.Text.Trim();
        public string BuildAuthor => _txtAuthor.Text.Trim();
        public string BuildHomepage => _txtHomepage.Text.Trim();
        public string ProjectIconPath => _txtIconPath.Text.Trim();

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _txtName.Focus();
            _txtName.SelectAll();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _picIcon.Image?.Dispose();
            base.Dispose(disposing);
        }

        private void ChooseIcon()
        {
            using var dialog = new OpenFileDialog
            {
                Title = "选择模组 icon",
                Filter = "图片文件|*.png;*.jpg;*.jpeg;*.bmp|PNG 图片|*.png|所有文件|*.*"
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
                SetIconPath(dialog.FileName);
        }

        private void SetIconPath(string path)
        {
            _txtIconPath.Text = path ?? string.Empty;
            _picIcon.Image?.Dispose();
            _picIcon.Image = null;

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return;

            try
            {
                using var image = Image.FromFile(path);
                _picIcon.Image = new Bitmap(image);
            }
            catch
            {
                _txtIconPath.Text = string.Empty;
                UIMessageBox.Show("图标图片无法读取，请换一张图片。");
            }
        }

        private void AcceptSettings()
        {
            if (string.IsNullOrWhiteSpace(ProjectName))
            {
                UIMessageBox.Show("请输入工程名称。");
                return;
            }

            if (!string.IsNullOrWhiteSpace(ProjectIconPath) && !File.Exists(ProjectIconPath))
            {
                UIMessageBox.Show("模组 icon 文件不存在。");
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void AddLabel(string text, int x, int y)
        {
            Controls.Add(new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(96, 22),
                Font = FontBodyBold,
                ForeColor = ClrSubText,
                BackColor = Color.Transparent
            });
        }

        private static void ConfigureInput(UITextBox input, int x, int y, int width, int height, string text)
        {
            input.SetBounds(x, y, width, height);
            input.Text = text ?? string.Empty;
            input.Font = FontBody;
            input.Style = UIStyle.Black;
            input.FillColor = ClrInput;
            input.RectColor = ClrBorder;
            input.ForeColor = ClrText;
            input.BackColor = ClrPanel;
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
    }
}
