using Sunny.UI;

namespace tmcreator
{
    internal sealed class ProjectNameDialog : UIForm
    {
        private readonly UITextBox _txtName = new();

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

        public ProjectNameDialog(string title, string initialName)
        {
            Text = title;
            ClientSize = new Size(420, 190);
            MinimumSize = new Size(420, 190);
            MaximumSize = new Size(420, 190);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = ClrBg;
            Style = UIStyle.Black;

            var titleLabel = new Label
            {
                Text = title,
                Location = new Point(24, 24),
                Size = new Size(320, 28),
                Font = FontTitle,
                ForeColor = ClrText,
                BackColor = Color.Transparent
            };
            Controls.Add(titleLabel);

            var hint = new Label
            {
                Text = "设置工程名称，保存时会作为工程文件名使用。",
                Location = new Point(24, 56),
                Size = new Size(340, 22),
                Font = FontBody,
                ForeColor = ClrSubText,
                BackColor = Color.Transparent
            };
            Controls.Add(hint);

            _txtName.SetBounds(24, 90, 368, 32);
            _txtName.Text = initialName;
            _txtName.Font = FontBody;
            _txtName.Style = UIStyle.Black;
            _txtName.FillColor = ClrInput;
            _txtName.RectColor = ClrBorder;
            _txtName.ForeColor = ClrText;
            _txtName.BackColor = ClrPanel;
            Controls.Add(_txtName);

            var btnOk = CreateButton("确定", 218, 140, 82, 32, ClrAccent);
            btnOk.Click += (s, e) => AcceptName();
            Controls.Add(btnOk);

            var btnCancel = CreateButton("取消", 310, 140, 82, 32, Color.FromArgb(67, 83, 105));
            btnCancel.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };
            Controls.Add(btnCancel);

            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }

        public string ProjectName => _txtName.Text.Trim();

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _txtName.Focus();
            _txtName.SelectAll();
        }

        private void AcceptName()
        {
            if (string.IsNullOrWhiteSpace(ProjectName))
            {
                UIMessageBox.Show("请输入工程名称。");
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
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
