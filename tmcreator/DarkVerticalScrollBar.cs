using System.ComponentModel;

namespace tmcreator
{
    internal sealed class DarkVerticalScrollBar : Control
    {
        private bool _draggingThumb;
        private int _dragStartValue;
        private int _dragStartY;
        private int _maximum;
        private int _value;

        public DarkVerticalScrollBar()
        {
            DoubleBuffered = true;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);
        }

        public event EventHandler? ValueChanged;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color BorderColor { get; set; } = Color.FromArgb(35, 55, 77);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ContentSize { get; private set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int LargeChange { get; private set; } = 1;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Maximum
        {
            get => _maximum;
            private set => _maximum = Math.Max(0, value);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ThumbColor { get; set; } = Color.FromArgb(73, 105, 132);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ThumbHoverColor { get; set; } = Color.FromArgb(87, 129, 157);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color TrackColor { get; set; } = Color.FromArgb(13, 21, 31);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Value
        {
            get => _value;
            set => SetValue(value, true);
        }

        public void Configure(int value, int maximum, int largeChange, int contentSize)
        {
            Maximum = maximum;
            LargeChange = Math.Max(1, largeChange);
            ContentSize = Math.Max(LargeChange, contentSize);
            SetValue(value, false);
            Visible = Maximum > 0;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left || Maximum <= 0)
                return;

            var thumb = GetThumbRectangle();
            if (thumb.Contains(e.Location))
            {
                _draggingThumb = true;
                _dragStartY = e.Y;
                _dragStartValue = Value;
                Capture = true;
                return;
            }

            Value = GetValueFromTrackY(e.Y);
            _draggingThumb = true;
            _dragStartY = e.Y;
            _dragStartValue = Value;
            Capture = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!_draggingThumb || Maximum <= 0)
                return;

            var thumb = GetThumbRectangle();
            int travel = Math.Max(1, ClientSize.Height - thumb.Height - 4);
            int delta = (int)Math.Round((e.Y - _dragStartY) * Maximum / (double)travel);
            Value = _dragStartValue + delta;
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
            Value -= Math.Sign(e.Delta) * 54;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using var trackBrush = new SolidBrush(TrackColor);
            e.Graphics.FillRectangle(trackBrush, ClientRectangle);

            using var borderPen = new Pen(BorderColor);
            e.Graphics.DrawLine(borderPen, 0, 0, 0, ClientSize.Height);
            e.Graphics.DrawLine(borderPen, ClientSize.Width - 1, 0, ClientSize.Width - 1, ClientSize.Height);

            if (Maximum <= 0)
                return;

            var thumb = GetThumbRectangle();
            using var thumbBrush = new SolidBrush(_draggingThumb ? ThumbHoverColor : ThumbColor);
            e.Graphics.FillRectangle(thumbBrush, thumb);
        }

        private Rectangle GetThumbRectangle()
        {
            if (Maximum <= 0 || ContentSize <= 0 || ClientSize.Height <= 0)
                return Rectangle.Empty;

            int availableHeight = Math.Max(1, ClientSize.Height - 4);
            int thumbHeight = Math.Max(28, (int)Math.Round(availableHeight * (LargeChange / (double)ContentSize)));
            thumbHeight = Math.Min(availableHeight, thumbHeight);

            int travel = Math.Max(1, availableHeight - thumbHeight);
            int y = 2 + (int)Math.Round(travel * (Value / (double)Maximum));
            int width = Math.Max(4, Math.Min(7, ClientSize.Width - 5));
            int x = Math.Max(2, (ClientSize.Width - width) / 2);
            return new Rectangle(x, y, width, thumbHeight);
        }

        private int GetValueFromTrackY(int y)
        {
            var thumb = GetThumbRectangle();
            int availableHeight = Math.Max(1, ClientSize.Height - 4);
            int travel = Math.Max(1, availableHeight - thumb.Height);
            int thumbTop = Math.Clamp(y - thumb.Height / 2, 2, 2 + travel);
            return (int)Math.Round((thumbTop - 2) * Maximum / (double)travel);
        }

        private void SetValue(int value, bool raiseEvent)
        {
            int clamped = Math.Clamp(value, 0, Maximum);
            if (_value == clamped)
                return;

            _value = clamped;
            Invalidate();

            if (raiseEvent)
                ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
