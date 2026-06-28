using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace tmcreator.Models.EffectPreview;

public sealed class EffectEngine
{
    private readonly Random _rng = new();
    private readonly List<TrailSample> _trail = new();
    private readonly List<ImpactParticle> _impactParticles = new();
    private float _time;
    private float _phase;
    private float _launchAge;
    private float _impactAge;
    private float _impactDuration = 0.65f;
    private int _impactSeed;
    private bool _impactActive;
    private PointF _projectilePosition;
    private PointF _projectileDirection = new(1f, 0f);
    private PointF _impactCenter;
    private Image? _previewTexture;
    private string _previewTexturePath = "";

    public EffectPreset Preset { get; }

    public EffectEngine(EffectPreset preset)
    {
        Preset = preset;
        Reset();
    }

    public void Reset()
    {
        _time = 0f;
        _phase = 0.04f;
        _launchAge = 0f;
        _trail.Clear();
        _impactParticles.Clear();
        _impactAge = 0f;
        _impactActive = false;
        UpdateProjectilePose();
        _trail.Add(new TrailSample(_projectilePosition, _time));
    }

    public void Update(float deltaTime)
    {
        deltaTime = Math.Clamp(deltaTime, 0.001f, 0.08f);
        _time += deltaTime;

        var settings = Preset.Projectile;
        float pathLength = Math.Max(80f, settings.PreviewPathLength);
        float oldPhase = _phase;
        _launchAge += deltaTime;
        _phase += GetCurrentPreviewSpeed(settings) * deltaTime / pathLength;

        if (_phase >= 1f)
        {
            if (settings.AutoImpactOnLoop && settings.ShowImpact)
            {
                UpdateProjectilePose(0.98f);
                TriggerImpact();
            }

            _phase %= 1f;
            _trail.Clear();
            _launchAge = 0f;
        }

        UpdateProjectilePose();
        AddTrailPoint(_projectilePosition);
        UpdateImpactParticles(deltaTime);

        if (oldPhase > _phase && _trail.Count == 0)
            _trail.Add(new TrailSample(_projectilePosition, _time));
    }

    public void TriggerImpact()
    {
        var settings = Preset.Projectile;
        _impactCenter = _projectilePosition;
        _impactAge = 0f;
        _impactDuration = Math.Max(0.12f, settings.ImpactPreviewLife);
        _impactSeed = _rng.Next();
        _impactActive = true;

        CreateImpactParticles(settings);
    }

    private float GetCurrentPreviewSpeed(ProjectileEffectSettings settings)
    {
        if (!settings.UseSpeedCurve)
            return settings.PreviewSpeed;

        float rampSeconds = Math.Max(0.02f, settings.SpeedRampSeconds);
        float amount = Math.Clamp(_launchAge / rampSeconds, 0f, 1f);
        amount = amount * amount * (3f - 2f * amount);
        return Lerp(settings.LaunchSpeed, settings.TargetSpeed, amount);
    }

    public void Render(Graphics g, Rectangle viewport)
    {
        var settings = Preset.Projectile;
        g.Clear(Preset.BackgroundColor);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        DrawGrid(g, viewport, Preset.BackgroundColor);

        var state = g.Save();
        float zoom = Math.Clamp(settings.PreviewZoom, 0.35f, 2.5f);
        g.TranslateTransform(viewport.Width / 2f + Preset.WorldCenterX, viewport.Height / 2f + Preset.WorldCenterY);
        g.ScaleTransform(zoom, zoom);

        switch (settings.EffectKind)
        {
            case ProjectileEffectKind.NovaBurst:
                DrawNovaBurst(g, settings);
                break;
            case ProjectileEffectKind.ScatterShard:
                DrawScatterShards(g, settings);
                break;
            default:
                if (settings.ShowMuzzleFlame)
                    DrawMuzzleFlame(g, settings);
                if (settings.ShowTrail)
                    DrawSmoothFlowTrail(g, settings);
                if (settings.EffectKind == ProjectileEffectKind.SpiralComet)
                    DrawSpiralStrands(g, settings);
                DrawProjectileVisual(g, settings);
                if (settings.EffectKind == ProjectileEffectKind.OrbitingOrbs)
                    DrawOrbitingOrbs(g, settings);
                break;
        }

        if (settings.ShowHead && settings.EffectKind is not (ProjectileEffectKind.NovaBurst or ProjectileEffectKind.ScatterShard))
            DrawHeadGlow(g, settings);

        if (settings.ShowImpact)
            DrawImpactParticles(g, settings);

        g.Restore(state);
    }

    private void UpdateProjectilePose(float? phaseOverride = null)
    {
        var settings = Preset.Projectile;
        float phase = phaseOverride ?? _phase;

        if (settings.EffectKind is ProjectileEffectKind.NovaBurst or ProjectileEffectKind.ScatterShard)
        {
            _projectilePosition = PointF.Empty;
            _projectileDirection = new PointF(1f, 0f);
            return;
        }

        float pathLength = Math.Max(80f, settings.PreviewPathLength);
        float x = -pathLength * 0.5f + pathLength * phase;
        float y = MathF.Sin(phase * MathF.PI * 2f * settings.PreviewArcFrequency) * settings.PreviewArcAmplitude;

        float nextPhase = Math.Clamp(phase + 0.01f, 0f, 1f);
        float nextX = -pathLength * 0.5f + pathLength * nextPhase;
        float nextY = MathF.Sin(nextPhase * MathF.PI * 2f * settings.PreviewArcFrequency) * settings.PreviewArcAmplitude;

        _projectilePosition = new PointF(x, y);
        _projectileDirection = Normalize(new PointF(nextX - x, nextY - y));
    }

    private void AddTrailPoint(PointF point)
    {
        if (_trail.Count > 0 && Distance(_trail[0].Position, point) < 1.25f)
            return;

        _trail.Insert(0, new TrailSample(point, _time));
        int max = Math.Max(2, Preset.Projectile.TrailCacheLength);
        while (_trail.Count > max)
            _trail.RemoveAt(_trail.Count - 1);

        float duration = Math.Max(0.02f, Preset.Projectile.TrailDurationSeconds);
        while (_trail.Count > 2 && _time - _trail[^1].Time > duration)
            _trail.RemoveAt(_trail.Count - 1);
    }

    private void CreateImpactParticles(ProjectileEffectSettings settings)
    {
        int count = Math.Clamp(settings.ImpactDustCount, 0, 140);
        if (count <= 0)
            return;

        switch (settings.ImpactKind)
        {
            case ImpactEffectKind.RingShockwave:
                CreateRingImpactParticles(settings, count);
                break;
            case ImpactEffectKind.CrossSlash:
                CreateCrossSlashImpactParticles(settings, count);
                break;
            case ImpactEffectKind.SmokeBloom:
                CreateSmokeImpactParticles(settings, count);
                break;
            case ImpactEffectKind.LightningFork:
                CreateLightningImpactParticles(settings, count);
                break;
            case ImpactEffectKind.ShardSpray:
                CreateShardImpactParticles(settings, count);
                break;
            default:
                CreateSparkImpactParticles(settings, count);
                break;
        }
    }

    private void CreateSparkImpactParticles(ProjectileEffectSettings settings, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = (float)(_rng.NextDouble() * Math.PI * 2);
            float speed = settings.ImpactDustSpeed * (0.5f + (float)_rng.NextDouble() * 1.15f) * 44f;
            float life = Math.Max(0.12f, settings.ImpactPreviewLife * (0.55f + (float)_rng.NextDouble() * 0.65f));
            float size = Math.Max(1f, settings.ImpactDustScale * (2.4f + (float)_rng.NextDouble() * 4.6f));
            AddImpactParticle(settings, angle, speed, life, size, _impactCenter);
        }
    }

    private void CreateRingImpactParticles(ProjectileEffectSettings settings, int count)
    {
        int rings = Math.Max(1, settings.ImpactRingCount);
        for (int i = 0; i < count; i++)
        {
            int ring = i % rings;
            float angle = i / (float)Math.Max(1, count) * MathF.PI * 2f + ring * 0.28f;
            float radius = (ring + 1f) / rings * Math.Max(10f, settings.ImpactRingRadius * 0.18f);
            PointF start = Add(_impactCenter, new PointF(MathF.Cos(angle) * radius, MathF.Sin(angle) * radius));
            float speed = settings.ImpactDustSpeed * (0.75f + ring * 0.2f) * 46f;
            float life = Math.Max(0.16f, settings.ImpactPreviewLife * (0.72f + ring * 0.08f));
            float size = Math.Max(1.2f, settings.ImpactDustScale * (2.1f + ring * 0.4f));
            AddImpactParticle(settings, angle, speed, life, size, start);
        }
    }

    private void CreateCrossSlashImpactParticles(ProjectileEffectSettings settings, int count)
    {
        for (int i = 0; i < count; i++)
        {
            int slash = i % 4;
            float angle = MathF.PI * 0.25f + slash * MathF.PI * 0.5f + ((float)_rng.NextDouble() - 0.5f) * 0.18f;
            PointF dir = new(MathF.Cos(angle), MathF.Sin(angle));
            PointF perp = new(-dir.Y, dir.X);
            float offset = ((float)_rng.NextDouble() - 0.5f) * settings.ImpactRingRadius * 0.5f;
            PointF start = Add(_impactCenter, Add(Scale(dir, offset), Scale(perp, ((float)_rng.NextDouble() - 0.5f) * 10f)));
            float speed = settings.ImpactDustSpeed * (0.45f + (float)_rng.NextDouble() * 0.8f) * 38f;
            float life = Math.Max(0.1f, settings.ImpactPreviewLife * (0.55f + (float)_rng.NextDouble() * 0.45f));
            float size = Math.Max(1.1f, settings.ImpactDustScale * (2.4f + (float)_rng.NextDouble() * 2.8f));
            AddImpactParticle(settings, angle, speed, life, size, start);
        }
    }

    private void CreateSmokeImpactParticles(ProjectileEffectSettings settings, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = (float)(_rng.NextDouble() * Math.PI * 2);
            float startRadius = (float)_rng.NextDouble() * settings.ImpactCloudSize * 0.28f;
            PointF start = Add(_impactCenter, new PointF(MathF.Cos(angle) * startRadius, MathF.Sin(angle) * startRadius));
            float speed = settings.ImpactDustSpeed * (0.12f + (float)_rng.NextDouble() * 0.42f) * 34f;
            float life = Math.Max(0.35f, settings.ImpactPreviewLife * (0.85f + (float)_rng.NextDouble() * 0.55f));
            float size = Math.Max(4f, settings.ImpactDustScale * (5.8f + (float)_rng.NextDouble() * 9.4f));
            AddImpactParticle(settings, angle, speed, life, size, start, spinScale: 1.1f);
        }
    }

    private void CreateLightningImpactParticles(ProjectileEffectSettings settings, int count)
    {
        int branches = Math.Max(1, settings.ImpactLightningBranches);
        for (int i = 0; i < count; i++)
        {
            int branch = i % branches;
            float angle = branch / (float)branches * MathF.PI * 2f + ((float)_rng.NextDouble() - 0.5f) * 0.42f;
            PointF dir = new(MathF.Cos(angle), MathF.Sin(angle));
            float step = (i / (float)Math.Max(1, count - 1)) * settings.ImpactRingRadius * 0.62f;
            PointF start = Add(_impactCenter, Scale(dir, step));
            float speed = settings.ImpactDustSpeed * (0.35f + (float)_rng.NextDouble() * 0.55f) * 42f;
            float life = Math.Max(0.08f, settings.ImpactPreviewLife * (0.35f + (float)_rng.NextDouble() * 0.38f));
            float size = Math.Max(1f, settings.ImpactDustScale * (1.7f + (float)_rng.NextDouble() * 2.2f));
            AddImpactParticle(settings, angle, speed, life, size, start, spinScale: 6f);
        }
    }

    private void CreateShardImpactParticles(ProjectileEffectSettings settings, int count)
    {
        PointF baseDir = _projectileDirection;
        float baseAngle = MathF.Atan2(baseDir.Y, baseDir.X);
        for (int i = 0; i < count; i++)
        {
            float angle = baseAngle + ((float)_rng.NextDouble() - 0.5f) * MathF.PI * 1.25f;
            float speed = settings.ImpactDustSpeed * (0.7f + (float)_rng.NextDouble() * 1.05f) * 48f;
            float life = Math.Max(0.16f, settings.ImpactPreviewLife * (0.62f + (float)_rng.NextDouble() * 0.48f));
            float size = Math.Max(1.4f, settings.ImpactDustScale * (3.1f + (float)_rng.NextDouble() * 4.8f));
            AddImpactParticle(settings, angle, speed, life, size, _impactCenter, spinScale: 5f);
        }
    }

    private void AddImpactParticle(ProjectileEffectSettings settings, float angle, float speed, float life, float size, PointF start, float spinScale = 4f)
    {
        _impactParticles.Add(new ImpactParticle
        {
            X = start.X,
            Y = start.Y,
            VX = MathF.Cos(angle) * speed,
            VY = MathF.Sin(angle) * speed,
            Life = life,
            MaxLife = life,
            Size = size,
            Angle = angle,
            Spin = ((float)_rng.NextDouble() * 2f - 1f) * spinScale,
            Color = LerpColor(settings.ImpactColorA, settings.ImpactColorB, (float)_rng.NextDouble())
        });
    }

    private void UpdateImpactParticles(float deltaTime)
    {
        if (_impactActive)
        {
            _impactAge += deltaTime;
            if (_impactAge > _impactDuration)
                _impactActive = false;
        }

        for (int i = _impactParticles.Count - 1; i >= 0; i--)
        {
            var p = _impactParticles[i];
            p.Life -= deltaTime;
            if (p.Life <= 0f)
            {
                _impactParticles.RemoveAt(i);
                continue;
            }

            p.X += p.VX * deltaTime;
            p.Y += p.VY * deltaTime;
            p.VX *= 1f - Math.Min(0.92f, deltaTime * 2.8f);
            p.VY *= 1f - Math.Min(0.92f, deltaTime * 2.8f);
            p.Angle += p.Spin * deltaTime;
        }
    }

    private void DrawSmoothFlowTrail(Graphics g, ProjectileEffectSettings settings)
    {
        var smooth = BuildSmoothPath(settings);
        if (smooth.Count < 2)
            return;

        float pulse = 0.95f + settings.TrailPulseAmount * MathF.Sin(_time * settings.TrailPulseSpeed);

        for (int i = 0; i < smooth.Count - 1; i++)
        {
            float progress = i / (float)Math.Max(1, smooth.Count - 1);
            float opacity = Lerp(settings.TrailOpacityStart, settings.TrailOpacityEnd, progress);

            DrawTrailSegment(g, smooth[i], smooth[i + 1],
                LerpColor(settings.OuterTrailStartColor, settings.OuterTrailEndColor, progress),
                Lerp(settings.OuterTrailWidthStart, settings.OuterTrailWidthEnd, progress) * pulse,
                opacity * settings.OuterOpacityScale);

            DrawTrailSegment(g, smooth[i], smooth[i + 1],
                LerpColor(settings.InnerTrailStartColor, settings.InnerTrailEndColor, progress),
                Lerp(settings.InnerTrailWidthStart, settings.InnerTrailWidthEnd, progress),
                opacity);

            DrawTrailSegment(g, smooth[i], smooth[i + 1],
                LerpColor(settings.CoreTrailStartColor, settings.CoreTrailEndColor, progress),
                Lerp(settings.CoreTrailWidthStart, settings.CoreTrailWidthEnd, progress),
                opacity * settings.CoreOpacityScale);

            if (i % 3 == 0)
            {
                float glowSize = Lerp(settings.TrailGlowScaleStart, settings.TrailGlowScaleEnd, progress) * 90f * pulse;
                Color glow = WithAlpha(LerpColor(settings.InnerTrailStartColor, settings.InnerTrailEndColor, progress), opacity * 0.24f);
                DrawRadialGlow(g, smooth[i], glow, glowSize);
            }
        }
    }

    private List<PointF> BuildSmoothPath(ProjectileEffectSettings settings)
    {
        if (_trail.Count < 2)
            return new List<PointF>();

        var raw = new List<PointF> { _projectilePosition };
        for (int i = 0; i < _trail.Count && raw.Count < Math.Max(3, settings.MaxTrailPoints); i++)
        {
            PointF position = _trail[i].Position;
            if (raw.Count > 0 && Distance(raw[^1], position) > settings.TrailBreakDistance)
                break;

            raw.Add(position);
        }

        if (raw.Count < 3)
            return new List<PointF>();

        int steps = Math.Max(1, settings.TrailSubdivisionSteps);
        var smooth = new List<PointF>();
        for (int i = 0; i < raw.Count - 1; i++)
        {
            PointF p1 = raw[i];
            PointF p2 = raw[i + 1];
            PointF p0 = i > 0 ? raw[i - 1] : Add(p1, Subtract(p1, p2));
            PointF p3 = i + 2 < raw.Count ? raw[i + 2] : Add(p2, Subtract(p2, p1));

            for (int j = 0; j < steps; j++)
                smooth.Add(CatmullRom(p0, p1, p2, p3, j / (float)steps));
        }

        return smooth;
    }

    private static void DrawTrailSegment(Graphics g, PointF from, PointF to, Color color, float width, float opacity)
    {
        int alpha = Math.Clamp((int)(color.A * opacity), 0, 255);
        if (alpha <= 0 || width <= 0.05f)
            return;

        using var pen = new Pen(Color.FromArgb(alpha, color), Math.Max(0.4f, width))
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        g.DrawLine(pen, from, to);
    }

    private void DrawHeadGlow(Graphics g, ProjectileEffectSettings settings)
    {
        float pulse = 1f + settings.HeadPulseAmount * MathF.Sin(_time * settings.HeadPulseSpeed);
        DrawRadialGlow(g, _projectilePosition, WithAlpha(settings.HeadGlowOuterColor, 0.78f), settings.HeadSize * settings.HeadOuterScale * pulse);
        DrawRadialGlow(g, _projectilePosition, WithAlpha(settings.HeadGlowInnerColor, 0.62f), settings.HeadSize * settings.HeadInnerScale * pulse);

        using var sparklePen = new Pen(WithAlpha(settings.HeadGlowInnerColor, 0.78f), 1.2f);
        float star = settings.HeadSize * 0.72f * pulse;
        g.DrawLine(sparklePen, _projectilePosition.X - star, _projectilePosition.Y, _projectilePosition.X + star, _projectilePosition.Y);
        g.DrawLine(sparklePen, _projectilePosition.X, _projectilePosition.Y - star, _projectilePosition.X, _projectilePosition.Y + star);
    }

    private void DrawMuzzleFlame(Graphics g, ProjectileEffectSettings settings)
    {
        float duration = Math.Max(0.02f, settings.MuzzleFlameDuration);
        float progress = Math.Clamp(_launchAge / duration, 0f, 1f);
        if (progress >= 1f)
            return;

        float fade = MathF.Pow(1f - progress, 1.35f);
        PointF dir = _projectileDirection;
        PointF perp = new(-dir.Y, dir.X);
        float pulse = 0.88f + 0.18f * MathF.Sin(_time * 44f);
        float length = settings.MuzzleFlameLength * (0.55f + 0.45f * fade) * pulse;
        float width = settings.MuzzleFlameWidth * (0.55f + 0.45f * fade);
        PointF basePoint = Add(_projectilePosition, Scale(dir, -settings.HeadSize * 0.8f));
        PointF tail = Add(basePoint, Scale(dir, -length));
        PointF left = Add(basePoint, Scale(perp, width));
        PointF right = Add(basePoint, Scale(perp, -width));

        DrawRadialGlow(g, tail, WithAlpha(settings.MuzzleFlameEndColor, fade * 0.28f), width * 3.4f);
        DrawRadialGlow(g, basePoint, WithAlpha(settings.MuzzleFlameStartColor, fade * 0.34f), width * 2.2f);

        using var path = new GraphicsPath();
        path.AddPolygon(new[] { tail, left, _projectilePosition, right });
        using var brush = new PathGradientBrush(path)
        {
            CenterPoint = basePoint,
            CenterColor = WithAlpha(settings.MuzzleFlameStartColor, fade * 0.82f),
            SurroundColors = new[] { WithAlpha(settings.MuzzleFlameEndColor, fade * 0.05f), WithAlpha(settings.MuzzleFlameEndColor, fade * 0.28f), WithAlpha(settings.MuzzleFlameStartColor, fade * 0.18f), WithAlpha(settings.MuzzleFlameEndColor, fade * 0.28f) }
        };
        g.FillPath(brush, path);
    }

    private void DrawProjectileVisual(Graphics g, ProjectileEffectSettings settings)
    {
        if (settings.VisualSource == ProjectileVisualSource.CustomTexture)
        {
            Image? texture = GetPreviewTexture(settings);
            if (texture is not null)
            {
                DrawPreviewTexture(g, texture, settings);
                return;
            }

            DrawMissingTextureMarker(g, settings);
            return;
        }

        if (settings.EffectKind == ProjectileEffectKind.EmptyProjectile)
        {
            DrawEmptyProjectileMarker(g, settings);
            return;
        }

        DrawProjectileCore(g, settings);
    }

    private Image? GetPreviewTexture(ProjectileEffectSettings settings)
    {
        string path = settings.PreviewTexturePath.Trim();
        if (path == _previewTexturePath)
            return _previewTexture;

        _previewTexture?.Dispose();
        _previewTexture = null;
        _previewTexturePath = path;

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return null;

        try
        {
            using Image loaded = Image.FromFile(path);
            _previewTexture = new Bitmap(loaded);
        }
        catch
        {
            _previewTexture = null;
        }

        return _previewTexture;
    }

    private void DrawPreviewTexture(Graphics g, Image texture, ProjectileEffectSettings settings)
    {
        float drawWidth = Math.Max(4f, settings.Width * 2f);
        float drawHeight = Math.Max(4f, settings.Height * 2f);
        float rotation = MathF.Atan2(_projectileDirection.Y, _projectileDirection.X) * 180f / MathF.PI + 90f;

        var state = g.Save();
        g.TranslateTransform(_projectilePosition.X, _projectilePosition.Y);
        g.RotateTransform(rotation);
        g.DrawImage(texture, -drawWidth * 0.5f, -drawHeight * 0.5f, drawWidth, drawHeight);
        g.Restore(state);
    }

    private void DrawMissingTextureMarker(Graphics g, ProjectileEffectSettings settings)
    {
        float size = Math.Max(6f, Math.Max(settings.Width, settings.Height) * 1.2f);
        var rect = new RectangleF(_projectilePosition.X - size * 0.5f, _projectilePosition.Y - size * 0.5f, size, size);
        using var pen = new Pen(WithAlpha(settings.LightColor, 0.72f), 1.2f);
        g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
        g.DrawLine(pen, rect.Left, rect.Top, rect.Right, rect.Bottom);
        g.DrawLine(pen, rect.Right, rect.Top, rect.Left, rect.Bottom);
    }

    private void DrawEmptyProjectileMarker(Graphics g, ProjectileEffectSettings settings)
    {
        float size = Math.Max(5f, Math.Max(settings.Width, settings.Height));
        DrawRadialGlow(g, _projectilePosition, WithAlpha(settings.LightColor, 0.16f), size * 1.8f);
        using var pen = new Pen(WithAlpha(settings.LightColor, 0.62f), 1.2f);
        g.DrawEllipse(pen, _projectilePosition.X - size * 0.5f, _projectilePosition.Y - size * 0.5f, size, size);
        g.DrawLine(pen, _projectilePosition.X - size * 0.42f, _projectilePosition.Y, _projectilePosition.X + size * 0.42f, _projectilePosition.Y);
        g.DrawLine(pen, _projectilePosition.X, _projectilePosition.Y - size * 0.42f, _projectilePosition.X, _projectilePosition.Y + size * 0.42f);
    }

    private void DrawProjectileCore(Graphics g, ProjectileEffectSettings settings)
    {
        PointF dir = _projectileDirection;
        PointF perp = new(-dir.Y, dir.X);
        float len = Math.Max(4f, settings.HeadSize * 1.15f);
        float width = Math.Max(3f, settings.HeadSize * 0.62f);

        PointF tip = Add(_projectilePosition, Scale(dir, len));
        PointF tail = Add(_projectilePosition, Scale(dir, -len * 0.68f));
        PointF left = Add(tail, Scale(perp, width));
        PointF right = Add(tail, Scale(perp, -width));

        using var path = new GraphicsPath();
        path.AddPolygon(new[] { tip, left, _projectilePosition, right });

        using var brush = new PathGradientBrush(path)
        {
            CenterPoint = _projectilePosition,
            CenterColor = settings.HeadCoreColor,
            SurroundColors = new[] { WithAlpha(settings.HeadGlowOuterColor, 0.72f), WithAlpha(settings.HeadGlowOuterColor, 0.25f), WithAlpha(settings.HeadGlowInnerColor, 0.92f), WithAlpha(settings.HeadGlowOuterColor, 0.25f) }
        };
        g.FillPath(brush, path);

        using var edge = new Pen(WithAlpha(settings.HeadGlowInnerColor, 0.48f), 1f);
        g.DrawPath(edge, path);
    }

    private void DrawSpiralStrands(Graphics g, ProjectileEffectSettings settings)
    {
        var smooth = BuildSmoothPath(settings);
        if (smooth.Count < 3)
            return;

        int strands = Math.Max(1, settings.SpiralStrands);
        for (int strand = 0; strand < strands; strand++)
        {
            float basePhase = strand / (float)strands * MathF.PI * 2f + _time * settings.OrbitSpeed;
            for (int i = 0; i < smooth.Count - 1; i += 2)
            {
                float t = i / (float)Math.Max(1, smooth.Count - 1);
                PointF current = smooth[i];
                PointF next = smooth[Math.Min(i + 1, smooth.Count - 1)];
                PointF dir = Normalize(Subtract(next, current));
                PointF perp = new(-dir.Y, dir.X);
                float wave = MathF.Sin(basePhase + t * settings.SpiralTwist * MathF.PI * 2f);
                PointF pos = Add(current, Scale(perp, wave * settings.SpiralRadius * (1f - t * 0.45f)));
                float alpha = (1f - t) * 0.72f;
                float size = settings.SpiralDotSize * (1f - t * 0.55f);
                Color color = LerpColor(settings.InnerTrailStartColor, settings.OuterTrailEndColor, t);
                DrawRadialGlow(g, pos, WithAlpha(color, alpha * 0.36f), size * 3f);
                using var brush = new SolidBrush(WithAlpha(color, alpha));
                g.FillEllipse(brush, pos.X - size * 0.5f, pos.Y - size * 0.5f, size, size);
            }
        }
    }

    private void DrawOrbitingOrbs(Graphics g, ProjectileEffectSettings settings)
    {
        float radius = Math.Max(4f, settings.OrbitRadius);
        using var ringPen = new Pen(WithAlpha(settings.OuterTrailStartColor, 0.35f), 1.2f);
        g.DrawEllipse(ringPen, _projectilePosition.X - radius, _projectilePosition.Y - radius, radius * 2f, radius * 2f);

        int count = Math.Max(1, settings.OrbitCount);
        for (int i = 0; i < count; i++)
        {
            float angle = _time * settings.OrbitSpeed + i / (float)count * MathF.PI * 2f;
            PointF pos = new(
                _projectilePosition.X + MathF.Cos(angle) * radius,
                _projectilePosition.Y + MathF.Sin(angle) * radius);
            Color color = LerpColor(settings.InnerTrailStartColor, settings.OuterTrailEndColor, i / (float)Math.Max(1, count - 1));
            DrawRadialGlow(g, pos, WithAlpha(color, 0.42f), settings.OrbitDotSize * 3.2f);
            using var brush = new SolidBrush(WithAlpha(color, 0.92f));
            g.FillEllipse(brush, pos.X - settings.OrbitDotSize * 0.5f, pos.Y - settings.OrbitDotSize * 0.5f, settings.OrbitDotSize, settings.OrbitDotSize);
        }
    }

    private void DrawNovaBurst(Graphics g, ProjectileEffectSettings settings)
    {
        PointF center = PointF.Empty;
        float pulse = (MathF.Sin(_time * settings.NovaSpinSpeed * 2.4f) + 1f) * 0.5f;
        DrawRadialGlow(g, center, WithAlpha(settings.HeadGlowOuterColor, 0.46f), settings.NovaRadius * (0.85f + pulse * 0.28f));
        DrawRadialGlow(g, center, WithAlpha(settings.HeadGlowInnerColor, 0.58f), settings.HeadSize * 3.2f);

        int rings = Math.Max(1, settings.NovaRingCount);
        for (int i = 0; i < rings; i++)
        {
            float t = (i + pulse) / rings;
            float radius = settings.NovaRadius * (0.28f + t * 0.72f);
            float alpha = 0.72f * (1f - i / (float)(rings + 1));
            Color color = LerpColor(settings.OuterTrailStartColor, settings.OuterTrailEndColor, i / (float)Math.Max(1, rings - 1));
            using var pen = new Pen(WithAlpha(color, alpha), 2f + i * 0.5f);
            g.DrawEllipse(pen, center.X - radius, center.Y - radius, radius * 2f, radius * 2f);
        }

        int spikes = 16;
        float spin = _time * settings.NovaSpinSpeed;
        for (int i = 0; i < spikes; i++)
        {
            float angle = spin + i / (float)spikes * MathF.PI * 2f;
            float inner = settings.NovaRadius * 0.25f;
            float outer = inner + settings.NovaSpikeLength * (0.55f + 0.45f * MathF.Sin(_time * 4f + i));
            PointF a = new(MathF.Cos(angle) * inner, MathF.Sin(angle) * inner);
            PointF b = new(MathF.Cos(angle) * outer, MathF.Sin(angle) * outer);
            Color color = LerpColor(settings.InnerTrailStartColor, settings.OuterTrailEndColor, i / (float)spikes);
            DrawTrailSegment(g, a, b, color, 3.2f, 0.62f);
            DrawTrailSegment(g, a, b, settings.CoreTrailStartColor, 1f, 0.9f);
        }

    }

    private void DrawScatterShards(Graphics g, ProjectileEffectSettings settings)
    {
        int count = Math.Max(1, settings.ScatterShardCount);
        float cone = settings.ScatterConeAngle * MathF.PI / 180f;
        float wave = (MathF.Sin(_time * settings.ScatterWaveSpeed) + 1f) * 0.5f;
        DrawRadialGlow(g, PointF.Empty, WithAlpha(settings.HeadGlowOuterColor, 0.35f), settings.HeadSize * 3f);

        for (int i = 0; i < count; i++)
        {
            float ratio = count == 1 ? 0.5f : i / (float)(count - 1);
            float angle = -cone * 0.5f + cone * ratio;
            float dist = settings.ScatterShardLength * (0.35f + 0.65f * wave) * (0.8f + 0.35f * ratio);
            PointF dir = new(MathF.Cos(angle), MathF.Sin(angle));
            PointF perp = new(-dir.Y, dir.X);
            PointF tip = Scale(dir, dist);
            PointF tail = Scale(dir, Math.Max(5f, dist - settings.ScatterShardLength * 0.45f));
            PointF left = Add(tail, Scale(perp, settings.HeadSize * 0.35f));
            PointF right = Add(tail, Scale(perp, -settings.HeadSize * 0.35f));
            Color color = LerpColor(settings.OuterTrailStartColor, settings.OuterTrailEndColor, ratio);

            using var path = new GraphicsPath();
            path.AddPolygon(new[] { tip, left, right });
            using var brush = new PathGradientBrush(path)
            {
                CenterPoint = tip,
                CenterColor = settings.InnerTrailStartColor,
                SurroundColors = new[] { WithAlpha(color, 0.42f), WithAlpha(color, 0.16f), WithAlpha(color, 0.16f) }
            };
            g.FillPath(brush, path);
            DrawTrailSegment(g, PointF.Empty, tip, color, settings.CoreTrailWidthStart, 0.35f);
            DrawRadialGlow(g, tip, WithAlpha(color, 0.35f), settings.HeadSize * 1.8f);
        }

    }

    private static void DrawRadialGlow(Graphics g, PointF center, Color color, float radius)
    {
        if (radius <= 0.1f || color.A <= 0)
            return;

        using var path = new GraphicsPath();
        path.AddEllipse(center.X - radius, center.Y - radius, radius * 2f, radius * 2f);
        using var brush = new PathGradientBrush(path)
        {
            CenterPoint = center,
            CenterColor = color,
            SurroundColors = new[] { Color.Transparent }
        };
        g.FillEllipse(brush, center.X - radius, center.Y - radius, radius * 2f, radius * 2f);
    }

    private static void DrawGrid(Graphics g, Rectangle viewport, Color background)
    {
        int baseAlpha = background.GetBrightness() < 0.18f ? 28 : 18;
        using var minor = new Pen(Color.FromArgb(baseAlpha, 120, 150, 170), 1f);
        using var major = new Pen(Color.FromArgb(baseAlpha + 12, 120, 180, 210), 1f);

        int step = 48;
        for (int x = viewport.Left; x < viewport.Right; x += step)
            g.DrawLine(x % (step * 4) == 0 ? major : minor, x, viewport.Top, x, viewport.Bottom);
        for (int y = viewport.Top; y < viewport.Bottom; y += step)
            g.DrawLine(y % (step * 4) == 0 ? major : minor, viewport.Left, y, viewport.Right, y);
    }

    private void DrawImpactParticles(Graphics g, ProjectileEffectSettings settings)
    {
        if (!_impactActive && _impactParticles.Count == 0)
            return;

        float progress = _impactDuration <= 0f ? 1f : Math.Clamp(_impactAge / _impactDuration, 0f, 1f);

        switch (settings.ImpactKind)
        {
            case ImpactEffectKind.RingShockwave:
                DrawImpactRingShockwave(g, settings, progress);
                DrawImpactSparkBurst(g, settings, progress, 0.45f);
                break;
            case ImpactEffectKind.CrossSlash:
                DrawImpactCrossSlash(g, settings, progress);
                DrawImpactSparkBurst(g, settings, progress, 0.35f);
                break;
            case ImpactEffectKind.SmokeBloom:
                DrawImpactSmokeBloom(g, settings, progress);
                break;
            case ImpactEffectKind.LightningFork:
                DrawImpactLightningFork(g, settings, progress);
                DrawImpactSparkBurst(g, settings, progress, 0.25f);
                break;
            case ImpactEffectKind.ShardSpray:
                DrawImpactShardSpray(g, settings, progress);
                break;
            default:
                DrawImpactSparkBurst(g, settings, progress, 1f);
                break;
        }
    }

    private void DrawImpactSparkBurst(Graphics g, ProjectileEffectSettings settings, float eventProgress, float opacityScale)
    {
        foreach (var p in _impactParticles)
        {
            if (p.MaxLife <= 0f)
                continue;

            float progress = 1f - Math.Clamp(p.Life / p.MaxLife, 0f, 1f);
            float alpha = MathF.Pow(1f - progress, 1.25f) * opacityScale;
            float size = p.Size * (0.6f + progress * 1.2f);
            var pos = new PointF(p.X, p.Y);

            DrawRadialGlow(g, pos, WithAlpha(p.Color, alpha * 0.42f), size * 2.6f);

            using var core = new SolidBrush(WithAlpha(p.Color, alpha));
            g.FillEllipse(core, p.X - size * 0.5f, p.Y - size * 0.5f, size, size);

            if (size > 2.2f)
            {
                using var line = new Pen(WithAlpha(Color.White, alpha * 0.65f), 1f);
                float spark = size * 1.25f;
                g.DrawLine(line, p.X - spark, p.Y, p.X + spark, p.Y);
                g.DrawLine(line, p.X, p.Y - spark, p.X, p.Y + spark);
            }
        }
    }

    private void DrawImpactRingShockwave(Graphics g, ProjectileEffectSettings settings, float progress)
    {
        int rings = Math.Max(1, settings.ImpactRingCount);
        DrawRadialGlow(g, _impactCenter, WithAlpha(settings.ImpactColorA, 0.34f * (1f - progress)), settings.ImpactRingRadius * (0.25f + progress * 0.65f));

        for (int ring = 0; ring < rings; ring++)
        {
            float delay = ring / (float)(rings + 1) * 0.38f;
            float local = Math.Clamp((progress - delay) / Math.Max(0.1f, 1f - delay), 0f, 1f);
            float radius = settings.ImpactRingRadius * (0.2f + local * 0.95f);
            float alpha = (1f - local) * (0.78f - ring * 0.08f);
            if (alpha <= 0f)
                continue;

            Color color = LerpColor(settings.ImpactColorA, settings.ImpactColorB, ring / (float)Math.Max(1, rings - 1));
            using var pen = new Pen(WithAlpha(color, alpha), Math.Max(1f, settings.ImpactDustScale * (2.2f - ring * 0.18f)));
            g.DrawEllipse(pen, _impactCenter.X - radius, _impactCenter.Y - radius, radius * 2f, radius * 2f);
        }
    }

    private void DrawImpactCrossSlash(Graphics g, ProjectileEffectSettings settings, float progress)
    {
        float alpha = MathF.Pow(1f - progress, 0.85f);
        float length = settings.ImpactRingRadius * (0.45f + progress * 0.85f);
        float width = Math.Max(2f, settings.ImpactDustScale * 4.2f * (1f - progress * 0.45f));
        float spin = progress * MathF.PI * 0.42f;

        for (int i = 0; i < 4; i++)
        {
            float angle = spin + MathF.PI * 0.25f + i * MathF.PI * 0.5f;
            PointF dir = new(MathF.Cos(angle), MathF.Sin(angle));
            PointF start = Add(_impactCenter, Scale(dir, -length * 0.16f));
            PointF end = Add(_impactCenter, Scale(dir, length));
            Color color = i % 2 == 0 ? settings.ImpactColorA : settings.ImpactColorB;
            DrawTrailSegment(g, start, end, color, width, alpha * 0.78f);
            DrawTrailSegment(g, Add(_impactCenter, Scale(dir, length * 0.22f)), end, Color.White, Math.Max(0.8f, width * 0.35f), alpha);
        }
    }

    private void DrawImpactSmokeBloom(Graphics g, ProjectileEffectSettings settings, float progress)
    {
        DrawRadialGlow(g, _impactCenter, WithAlpha(settings.ImpactColorB, 0.18f * (1f - progress)), settings.ImpactCloudSize * (1f + progress));

        foreach (var p in _impactParticles)
        {
            if (p.MaxLife <= 0f)
                continue;

            float local = 1f - Math.Clamp(p.Life / p.MaxLife, 0f, 1f);
            float alpha = MathF.Pow(1f - local, 1.7f) * 0.45f;
            float size = settings.ImpactCloudSize * 0.45f + p.Size * (1.1f + local * 2.6f);
            Color color = LerpColor(settings.ImpactColorA, settings.ImpactColorB, local);
            DrawRadialGlow(g, new PointF(p.X, p.Y), WithAlpha(color, alpha), size);
        }
    }

    private void DrawImpactLightningFork(Graphics g, ProjectileEffectSettings settings, float progress)
    {
        float alpha = MathF.Pow(1f - progress, 0.8f);
        if (alpha <= 0f)
            return;

        var rand = new Random(_impactSeed);
        int branches = Math.Max(1, settings.ImpactLightningBranches);
        for (int i = 0; i < branches; i++)
        {
            float angle = i / (float)branches * MathF.PI * 2f + (float)(rand.NextDouble() - 0.5) * 0.55f;
            float length = settings.ImpactRingRadius * (0.45f + (float)rand.NextDouble() * 0.75f) * (0.65f + progress * 0.55f);
            PointF end = Add(_impactCenter, new PointF(MathF.Cos(angle) * length, MathF.Sin(angle) * length));
            DrawJaggedLine(g, _impactCenter, end, rand, WithAlpha(settings.ImpactColorA, alpha), Math.Max(1.2f, settings.ImpactDustScale * 1.6f));

            if (rand.NextDouble() > 0.45)
            {
                float forkAngle = angle + ((float)rand.NextDouble() > 0.5f ? 1f : -1f) * (0.5f + (float)rand.NextDouble() * 0.35f);
                PointF forkStart = Add(_impactCenter, Scale(Subtract(end, _impactCenter), 0.55f));
                PointF forkEnd = Add(forkStart, new PointF(MathF.Cos(forkAngle) * length * 0.38f, MathF.Sin(forkAngle) * length * 0.38f));
                DrawJaggedLine(g, forkStart, forkEnd, rand, WithAlpha(settings.ImpactColorB, alpha * 0.72f), Math.Max(0.9f, settings.ImpactDustScale));
            }
        }

        DrawRadialGlow(g, _impactCenter, WithAlpha(settings.ImpactColorA, alpha * 0.42f), settings.ImpactCloudSize);
    }

    private void DrawImpactShardSpray(Graphics g, ProjectileEffectSettings settings, float progress)
    {
        foreach (var p in _impactParticles)
        {
            if (p.MaxLife <= 0f)
                continue;

            float local = 1f - Math.Clamp(p.Life / p.MaxLife, 0f, 1f);
            float alpha = MathF.Pow(1f - local, 1.1f);
            PointF dir = new(MathF.Cos(p.Angle), MathF.Sin(p.Angle));
            PointF perp = new(-dir.Y, dir.X);
            float length = p.Size * (2.2f + local * 1.6f);
            float width = Math.Max(1.2f, p.Size * 0.36f);
            PointF tip = Add(new PointF(p.X, p.Y), Scale(dir, length));
            PointF tail = Add(new PointF(p.X, p.Y), Scale(dir, -length * 0.24f));
            PointF left = Add(tail, Scale(perp, width));
            PointF right = Add(tail, Scale(perp, -width));

            using var path = new GraphicsPath();
            path.AddPolygon(new[] { tip, left, right });
            using var brush = new SolidBrush(WithAlpha(p.Color, alpha));
            g.FillPath(brush, path);
            using var pen = new Pen(WithAlpha(Color.White, alpha * 0.55f), 0.8f);
            g.DrawLine(pen, tail, tip);
        }
    }

    private static void DrawJaggedLine(Graphics g, PointF start, PointF end, Random rand, Color color, float width)
    {
        const int segments = 7;
        PointF previous = start;
        PointF dir = Normalize(Subtract(end, start));
        PointF perp = new(-dir.Y, dir.X);

        using var outer = new Pen(WithAlpha(color, color.A / 255f * 0.42f), width * 2.4f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        using var inner = new Pen(color, width)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };

        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            PointF point = i == segments
                ? end
                : Add(Add(start, Scale(Subtract(end, start), t)), Scale(perp, ((float)rand.NextDouble() * 2f - 1f) * 12f * (1f - Math.Abs(t - 0.5f))));
            g.DrawLine(outer, previous, point);
            g.DrawLine(inner, previous, point);
            previous = point;
        }
    }

    private static PointF CatmullRom(PointF p0, PointF p1, PointF p2, PointF p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        return new PointF(
            0.5f * ((2f * p1.X) + (-p0.X + p2.X) * t + (2f * p0.X - 5f * p1.X + 4f * p2.X - p3.X) * t2 + (-p0.X + 3f * p1.X - 3f * p2.X + p3.X) * t3),
            0.5f * ((2f * p1.Y) + (-p0.Y + p2.Y) * t + (2f * p0.Y - 5f * p1.Y + 4f * p2.Y - p3.Y) * t2 + (-p0.Y + 3f * p1.Y - 3f * p2.Y + p3.Y) * t3));
    }

    private static PointF Add(PointF a, PointF b) => new(a.X + b.X, a.Y + b.Y);
    private static PointF Subtract(PointF a, PointF b) => new(a.X - b.X, a.Y - b.Y);
    private static PointF Scale(PointF p, float scale) => new(p.X * scale, p.Y * scale);

    private static PointF Normalize(PointF p)
    {
        float len = MathF.Sqrt(p.X * p.X + p.Y * p.Y);
        return len <= 0.001f ? new PointF(1f, 0f) : new PointF(p.X / len, p.Y / len);
    }

    private static float Distance(PointF a, PointF b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    private static float Lerp(float a, float b, float t) => a + (b - a) * Math.Clamp(t, 0f, 1f);

    private static Color LerpColor(Color a, Color b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return Color.FromArgb(
            (int)(a.A + (b.A - a.A) * t),
            (int)(a.R + (b.R - a.R) * t),
            (int)(a.G + (b.G - a.G) * t),
            (int)(a.B + (b.B - a.B) * t));
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        return Color.FromArgb(Math.Clamp((int)(255f * alpha), 0, 255), color.R, color.G, color.B);
    }

    private sealed class TrailSample
    {
        public TrailSample(PointF position, float time)
        {
            Position = position;
            Time = time;
        }

        public PointF Position { get; }
        public float Time { get; }
    }
}
