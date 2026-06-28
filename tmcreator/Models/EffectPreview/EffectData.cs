using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace tmcreator.Models.EffectPreview;

public enum EffectEditorLayer
{
    Projectile,
    Head,
    Trail,
    Impact
}

public enum ProjectileEffectKind
{
    EmptyProjectile,
    FlowBullet,
    SpiralComet,
    OrbitingOrbs,
    NovaBurst,
    ScatterShard
}

public static class ProjectileEffectKindNames
{
    public static string GetDisplayName(ProjectileEffectKind kind)
    {
        return kind switch
        {
            ProjectileEffectKind.EmptyProjectile => "空弹幕",
            ProjectileEffectKind.FlowBullet => "流光弹",
            ProjectileEffectKind.SpiralComet => "螺旋彗尾",
            ProjectileEffectKind.OrbitingOrbs => "环绕卫星弹",
            ProjectileEffectKind.NovaBurst => "新星爆发",
            ProjectileEffectKind.ScatterShard => "散弹碎片",
            _ => "流光弹"
        };
    }
}

public enum ProjectileVisualSource
{
    PresetDraw,
    CustomTexture
}

public static class ProjectileVisualSourceNames
{
    public static string GetDisplayName(ProjectileVisualSource source)
    {
        return source switch
        {
            ProjectileVisualSource.PresetDraw => "预设绘制",
            ProjectileVisualSource.CustomTexture => "自选贴图",
            _ => "预设绘制"
        };
    }
}

public enum ImpactEffectKind
{
    SparkBurst,
    RingShockwave,
    CrossSlash,
    SmokeBloom,
    LightningFork,
    ShardSpray
}

public static class ImpactEffectKindNames
{
    public static string GetDisplayName(ImpactEffectKind kind)
    {
        return kind switch
        {
            ImpactEffectKind.SparkBurst => "星点爆散",
            ImpactEffectKind.RingShockwave => "冲击环",
            ImpactEffectKind.CrossSlash => "十字斩爆",
            ImpactEffectKind.SmokeBloom => "烟雾云",
            ImpactEffectKind.LightningFork => "闪电裂解",
            ImpactEffectKind.ShardSpray => "碎片喷射",
            _ => "星点爆散"
        };
    }
}

public sealed class ProjectileEffectSettings
{
    public ProjectileEffectKind EffectKind { get; set; } = ProjectileEffectKind.FlowBullet;
    public ProjectileVisualSource VisualSource { get; set; } = ProjectileVisualSource.PresetDraw;
    public string ExportNamespace { get; set; } = "YourMod.Projectiles";
    public string ExportClassName { get; set; } = "GeneratedBulletEffect";
    public string TextureOverride { get; set; } = "";
    public string PreviewTexturePath { get; set; } = "";

    public int Width { get; set; } = 8;
    public int Height { get; set; } = 8;
    public int TimeLeft { get; set; } = 600;
    public int Penetrate { get; set; } = 1;
    public int ExtraUpdates { get; set; } = 1;
    public int TrailCacheLength { get; set; } = 16;
    public int TrailSubdivisionSteps { get; set; } = 8;
    public int MaxTrailPoints { get; set; } = 13;
    public float TrailBreakDistance { get; set; } = 140f;
    public float TrailDurationSeconds { get; set; } = 0.32f;
    public bool UsesLocalNpcImmunity { get; set; } = true;
    public int LocalNpcHitCooldown { get; set; } = 10;

    public bool UseSpeedCurve { get; set; }
    public float LaunchSpeed { get; set; } = 220f;
    public float TargetSpeed { get; set; } = 260f;
    public float SpeedRampSeconds { get; set; } = 0.4f;
    public float PreviewSpeed { get; set; } = 260f;
    public float PreviewPathLength { get; set; } = 620f;
    public float PreviewArcAmplitude { get; set; } = 42f;
    public float PreviewArcFrequency { get; set; } = 1.2f;
    public float PreviewZoom { get; set; } = 1f;
    public bool AutoImpactOnLoop { get; set; } = true;

    public bool ShowHead { get; set; } = true;
    public float HeadSize { get; set; } = 12f;
    public float HeadOuterScale { get; set; } = 2.7f;
    public float HeadInnerScale { get; set; } = 1.25f;
    public float HeadPulseSpeed { get; set; } = 18f;
    public float HeadPulseAmount { get; set; } = 0.08f;
    public Color HeadGlowOuterColor { get; set; } = Color.FromArgb(255, 232, 156);
    public Color HeadGlowInnerColor { get; set; } = Color.White;
    public Color HeadCoreColor { get; set; } = Color.FromArgb(255, 250, 220);

    public bool ShowTrail { get; set; } = true;
    public Color OuterTrailStartColor { get; set; } = Color.FromArgb(255, 224, 132);
    public Color OuterTrailEndColor { get; set; } = Color.FromArgb(255, 182, 88);
    public Color InnerTrailStartColor { get; set; } = Color.FromArgb(255, 250, 232);
    public Color InnerTrailEndColor { get; set; } = Color.FromArgb(255, 228, 152);
    public Color CoreTrailStartColor { get; set; } = Color.White;
    public Color CoreTrailEndColor { get; set; } = Color.FromArgb(255, 250, 220);
    public float OuterTrailWidthStart { get; set; } = 12.5f;
    public float OuterTrailWidthEnd { get; set; } = 2.2f;
    public float InnerTrailWidthStart { get; set; } = 7.4f;
    public float InnerTrailWidthEnd { get; set; } = 1.45f;
    public float CoreTrailWidthStart { get; set; } = 3.4f;
    public float CoreTrailWidthEnd { get; set; } = 0.9f;
    public float TrailOpacityStart { get; set; } = 1.3f;
    public float TrailOpacityEnd { get; set; } = 0.16f;
    public float OuterOpacityScale { get; set; } = 0.56f;
    public float CoreOpacityScale { get; set; } = 1.08f;
    public float TrailPulseSpeed { get; set; } = 18f;
    public float TrailPulseAmount { get; set; } = 0.08f;
    public float TrailGlowScaleStart { get; set; } = 0.14f;
    public float TrailGlowScaleEnd { get; set; } = 0.045f;

    public bool ShowMuzzleFlame { get; set; } = true;
    public float MuzzleFlameDuration { get; set; } = 0.18f;
    public float MuzzleFlameLength { get; set; } = 42f;
    public float MuzzleFlameWidth { get; set; } = 12f;
    public Color MuzzleFlameStartColor { get; set; } = Color.FromArgb(255, 246, 178);
    public Color MuzzleFlameEndColor { get; set; } = Color.FromArgb(255, 140, 64);

    public Color LightColor { get; set; } = Color.FromArgb(255, 214, 76);
    public float LightIntensity { get; set; } = 1.15f;

    public bool ShowImpact { get; set; } = true;
    public ImpactEffectKind ImpactKind { get; set; } = ImpactEffectKind.SparkBurst;
    public int ImpactDustType { get; set; } = 57;
    public int ImpactDustCount { get; set; } = 15;
    public float ImpactDustSpeed { get; set; } = 3f;
    public float ImpactDustScale { get; set; } = 1.2f;
    public float ImpactPreviewLife { get; set; } = 0.65f;
    public Color ImpactColorA { get; set; } = Color.FromArgb(255, 246, 194);
    public Color ImpactColorB { get; set; } = Color.FromArgb(255, 190, 82);
    public int ImpactRingCount { get; set; } = 2;
    public float ImpactRingRadius { get; set; } = 82f;
    public int ImpactSpikeCount { get; set; } = 12;
    public int ImpactLightningBranches { get; set; } = 6;
    public float ImpactCloudSize { get; set; } = 42f;

    public int SpiralStrands { get; set; } = 3;
    public float SpiralRadius { get; set; } = 18f;
    public float SpiralTwist { get; set; } = 3.5f;
    public float SpiralDotSize { get; set; } = 6f;

    public int OrbitCount { get; set; } = 4;
    public float OrbitRadius { get; set; } = 28f;
    public float OrbitSpeed { get; set; } = 5f;
    public float OrbitDotSize { get; set; } = 6.5f;

    public int NovaRingCount { get; set; } = 3;
    public float NovaRadius { get; set; } = 92f;
    public float NovaSpikeLength { get; set; } = 78f;
    public float NovaSpinSpeed { get; set; } = 1.5f;

    public int ScatterShardCount { get; set; } = 7;
    public float ScatterConeAngle { get; set; } = 64f;
    public float ScatterShardLength { get; set; } = 84f;
    public float ScatterWaveSpeed { get; set; } = 2.4f;

    public ProjectileEffectSettings Clone()
    {
        return (ProjectileEffectSettings)MemberwiseClone();
    }
}

public sealed class EffectPreset
{
    public string Name { get; set; } = "New Effect";
    public string Description { get; set; } = "";
    public Color BackgroundColor { get; set; } = Color.FromArgb(10, 16, 24);
    public float WorldCenterX { get; set; }
    public float WorldCenterY { get; set; }
    public ProjectileEffectSettings Projectile { get; set; } = new();

    public EffectPreset Clone()
    {
        return new EffectPreset
        {
            Name = Name,
            Description = Description,
            BackgroundColor = BackgroundColor,
            WorldCenterX = WorldCenterX,
            WorldCenterY = WorldCenterY,
            Projectile = Projectile.Clone()
        };
    }
}

public sealed class ImpactParticle
{
    public float X { get; set; }
    public float Y { get; set; }
    public float VX { get; set; }
    public float VY { get; set; }
    public float Life { get; set; }
    public float MaxLife { get; set; }
    public float Size { get; set; }
    public float Angle { get; set; }
    public float Spin { get; set; }
    public Color Color { get; set; }
}

public static class EffectPresets
{
    public static List<EffectPreset> GetAllPresets()
    {
        return new List<EffectPreset>
        {
            CreateEmptyProjectile(),
            CreateGoldenTrackingTrail(),
            CreateSpiralComet(),
            CreateOrbitingOrbs(),
            CreateNovaBurst(),
            CreateScatterShard(),
            CreatePlatinumColdTrail(),
            CreateImpactStarburst(),
            CreateFire(),
            CreateMagic()
        };
    }

    public static EffectPreset CreateEmptyProjectile()
    {
        return new EffectPreset
        {
            Name = "空弹幕模板",
            Description = "只保留弹幕基础逻辑，可选贴图、拖影、发射尾焰和碰撞效果",
            BackgroundColor = Color.FromArgb(8, 12, 18),
            Projectile = new ProjectileEffectSettings
            {
                EffectKind = ProjectileEffectKind.EmptyProjectile,
                ExportClassName = "BlankCustomBullet",
                VisualSource = ProjectileVisualSource.PresetDraw,
                Width = 10,
                Height = 10,
                ShowHead = false,
                ShowTrail = false,
                ShowMuzzleFlame = false,
                ImpactKind = ImpactEffectKind.SparkBurst,
                ImpactDustCount = 12,
                ImpactDustSpeed = 2.4f,
                ImpactRingRadius = 58f,
                HeadGlowOuterColor = Color.FromArgb(170, 230, 255),
                HeadGlowInnerColor = Color.White,
                LightColor = Color.FromArgb(160, 220, 255),
                ImpactColorA = Color.White,
                ImpactColorB = Color.FromArgb(120, 210, 255)
            }
        };
    }

    public static EffectPreset CreateGoldenTrackingTrail()
    {
        return new EffectPreset
        {
            Name = "金色追踪流光",
            Description = "高亮弹头、多层金色尾焰、轻微弧线",
            BackgroundColor = Color.FromArgb(9, 14, 21),
            Projectile = new ProjectileEffectSettings
            {
                EffectKind = ProjectileEffectKind.FlowBullet,
                ExportClassName = "GoldenTrackingBullet",
                ImpactDustType = 57,
                PreviewSpeed = 250f,
                LaunchSpeed = 220f,
                TargetSpeed = 285f,
                UseSpeedCurve = true,
                PreviewArcAmplitude = 36f,
                PreviewArcFrequency = 1.1f,
                HeadGlowOuterColor = Color.FromArgb(255, 232, 156),
                HeadGlowInnerColor = Color.White,
                HeadCoreColor = Color.FromArgb(255, 246, 198),
                OuterTrailStartColor = Color.FromArgb(255, 224, 132),
                OuterTrailEndColor = Color.FromArgb(255, 182, 88),
                InnerTrailStartColor = Color.FromArgb(255, 250, 232),
                InnerTrailEndColor = Color.FromArgb(255, 228, 152),
                CoreTrailStartColor = Color.White,
                CoreTrailEndColor = Color.FromArgb(255, 250, 220),
                MuzzleFlameStartColor = Color.FromArgb(255, 250, 202),
                MuzzleFlameEndColor = Color.FromArgb(255, 158, 66),
                LightColor = Color.FromArgb(255, 214, 76),
                ImpactKind = ImpactEffectKind.SparkBurst,
                ImpactColorA = Color.FromArgb(255, 248, 198),
                ImpactColorB = Color.FromArgb(255, 186, 70)
            }
        };
    }

    public static EffectPreset CreateSpiralComet()
    {
        return new EffectPreset
        {
            Name = "螺旋彗尾弹",
            Description = "弹体前进时两到多股光点绕尾焰缠绕",
            BackgroundColor = Color.FromArgb(8, 11, 26),
            Projectile = new ProjectileEffectSettings
            {
                EffectKind = ProjectileEffectKind.SpiralComet,
                ExportClassName = "SpiralCometBullet",
                PreviewSpeed = 240f,
                PreviewArcAmplitude = 52f,
                PreviewArcFrequency = 1.35f,
                TrailCacheLength = 20,
                MaxTrailPoints = 16,
                SpiralStrands = 3,
                SpiralRadius = 22f,
                SpiralTwist = 4.2f,
                SpiralDotSize = 6.5f,
                HeadGlowOuterColor = Color.FromArgb(170, 120, 255),
                HeadGlowInnerColor = Color.FromArgb(230, 255, 255),
                OuterTrailStartColor = Color.FromArgb(150, 105, 255),
                OuterTrailEndColor = Color.FromArgb(35, 204, 255),
                InnerTrailStartColor = Color.FromArgb(244, 230, 255),
                InnerTrailEndColor = Color.FromArgb(150, 245, 255),
                CoreTrailEndColor = Color.FromArgb(225, 250, 255),
                LightColor = Color.FromArgb(145, 130, 255),
                ImpactKind = ImpactEffectKind.LightningFork,
                ImpactDustType = 27,
                ImpactColorA = Color.FromArgb(245, 235, 255),
                ImpactColorB = Color.FromArgb(80, 210, 255)
            }
        };
    }

    public static EffectPreset CreateOrbitingOrbs()
    {
        return new EffectPreset
        {
            Name = "环绕卫星弹",
            Description = "主弹周围有小型卫星光球旋转保护",
            BackgroundColor = Color.FromArgb(9, 15, 18),
            Projectile = new ProjectileEffectSettings
            {
                EffectKind = ProjectileEffectKind.OrbitingOrbs,
                ExportClassName = "OrbitingOrbsBullet",
                PreviewSpeed = 205f,
                PreviewArcAmplitude = 26f,
                TrailCacheLength = 10,
                MaxTrailPoints = 8,
                OrbitCount = 5,
                OrbitRadius = 34f,
                OrbitSpeed = 5.8f,
                OrbitDotSize = 7.2f,
                HeadSize = 13f,
                HeadGlowOuterColor = Color.FromArgb(124, 255, 190),
                HeadGlowInnerColor = Color.White,
                OuterTrailStartColor = Color.FromArgb(84, 255, 185),
                OuterTrailEndColor = Color.FromArgb(50, 160, 255),
                InnerTrailStartColor = Color.FromArgb(230, 255, 246),
                InnerTrailEndColor = Color.FromArgb(120, 238, 255),
                LightColor = Color.FromArgb(90, 255, 198),
                ImpactKind = ImpactEffectKind.RingShockwave,
                ImpactDustType = 87,
                ImpactColorA = Color.White,
                ImpactColorB = Color.FromArgb(90, 255, 185)
            }
        };
    }

    public static EffectPreset CreateNovaBurst()
    {
        return new EffectPreset
        {
            Name = "新星爆发",
            Description = "原地脉冲扩张的多层光环和放射尖刺",
            BackgroundColor = Color.FromArgb(16, 12, 20),
            Projectile = new ProjectileEffectSettings
            {
                EffectKind = ProjectileEffectKind.NovaBurst,
                ExportClassName = "NovaBurstProjectile",
                Width = 24,
                Height = 24,
                TimeLeft = 120,
                Penetrate = -1,
                ExtraUpdates = 0,
                PreviewSpeed = 0f,
                PreviewArcAmplitude = 0f,
                ShowTrail = false,
                ShowImpact = true,
                HeadSize = 18f,
                NovaRingCount = 4,
                NovaRadius = 112f,
                NovaSpikeLength = 92f,
                NovaSpinSpeed = 1.8f,
                HeadGlowOuterColor = Color.FromArgb(255, 120, 180),
                HeadGlowInnerColor = Color.White,
                OuterTrailStartColor = Color.FromArgb(255, 95, 170),
                OuterTrailEndColor = Color.FromArgb(255, 190, 80),
                InnerTrailStartColor = Color.White,
                InnerTrailEndColor = Color.FromArgb(255, 230, 170),
                LightColor = Color.FromArgb(255, 110, 170),
                ImpactKind = ImpactEffectKind.RingShockwave,
                ImpactRingCount = 4,
                ImpactRingRadius = 130f,
                ImpactDustType = 15,
                ImpactDustCount = 46,
                ImpactDustSpeed = 6.2f,
                ImpactPreviewLife = 1.15f,
                ImpactColorA = Color.White,
                ImpactColorB = Color.FromArgb(255, 110, 170)
            }
        };
    }

    public static EffectPreset CreateScatterShard()
    {
        return new EffectPreset
        {
            Name = "散弹碎片",
            Description = "多个短光刃呈扇形飞散，适合霰弹/破片",
            BackgroundColor = Color.FromArgb(13, 15, 20),
            Projectile = new ProjectileEffectSettings
            {
                EffectKind = ProjectileEffectKind.ScatterShard,
                ExportClassName = "ScatterShardBullet",
                Width = 10,
                Height = 10,
                PreviewSpeed = 180f,
                PreviewArcAmplitude = 0f,
                ShowTrail = false,
                ScatterShardCount = 9,
                ScatterConeAngle = 76f,
                ScatterShardLength = 96f,
                ScatterWaveSpeed = 2.8f,
                HeadSize = 8f,
                HeadGlowOuterColor = Color.FromArgb(255, 218, 126),
                OuterTrailStartColor = Color.FromArgb(255, 210, 120),
                OuterTrailEndColor = Color.FromArgb(255, 98, 78),
                InnerTrailStartColor = Color.White,
                InnerTrailEndColor = Color.FromArgb(255, 180, 118),
                LightColor = Color.FromArgb(255, 170, 95),
                ImpactKind = ImpactEffectKind.ShardSpray,
                ImpactDustType = 6,
                ImpactDustCount = 30,
                ImpactDustSpeed = 5.5f,
                ImpactColorA = Color.White,
                ImpactColorB = Color.FromArgb(255, 115, 80)
            }
        };
    }

    public static EffectPreset CreatePlatinumColdTrail()
    {
        return new EffectPreset
        {
            Name = "铂金冷光尾焰",
            Description = "冷白弹头、蓝白流光、细线核心",
            BackgroundColor = Color.FromArgb(8, 13, 22),
            Projectile = new ProjectileEffectSettings
            {
                EffectKind = ProjectileEffectKind.FlowBullet,
                ExportClassName = "PlatinumFlowBullet",
                ImpactDustType = 58,
                PreviewSpeed = 285f,
                PreviewArcAmplitude = 24f,
                HeadGlowOuterColor = Color.FromArgb(226, 232, 255),
                HeadGlowInnerColor = Color.White,
                HeadCoreColor = Color.FromArgb(245, 248, 255),
                OuterTrailStartColor = Color.FromArgb(210, 220, 255),
                OuterTrailEndColor = Color.FromArgb(175, 190, 235),
                InnerTrailStartColor = Color.FromArgb(248, 250, 255),
                InnerTrailEndColor = Color.FromArgb(220, 230, 245),
                CoreTrailStartColor = Color.White,
                CoreTrailEndColor = Color.FromArgb(245, 248, 255),
                OuterTrailWidthStart = 10.5f,
                InnerTrailWidthStart = 5.8f,
                CoreTrailWidthStart = 2.8f,
                LightColor = Color.FromArgb(235, 240, 255),
                LightIntensity = 1.05f,
                ImpactKind = ImpactEffectKind.CrossSlash,
                ImpactColorA = Color.FromArgb(248, 252, 255),
                ImpactColorB = Color.FromArgb(168, 194, 255)
            }
        };
    }

    public static EffectPreset CreateImpactStarburst()
    {
        return new EffectPreset
        {
            Name = "撞击星爆散射",
            Description = "较短尾焰、重心在命中爆闪",
            BackgroundColor = Color.FromArgb(12, 16, 24),
            Projectile = new ProjectileEffectSettings
            {
                EffectKind = ProjectileEffectKind.ScatterShard,
                ExportClassName = "StarburstImpactBullet",
                PreviewSpeed = 220f,
                PreviewArcAmplitude = 18f,
                TrailCacheLength = 12,
                MaxTrailPoints = 10,
                OuterTrailWidthStart = 9f,
                InnerTrailWidthStart = 5f,
                CoreTrailWidthStart = 2.4f,
                ImpactDustType = 15,
                ImpactKind = ImpactEffectKind.CrossSlash,
                ImpactDustCount = 34,
                ImpactDustSpeed = 5.2f,
                ImpactDustScale = 1.45f,
                ImpactPreviewLife = 0.95f,
                ScatterShardCount = 6,
                ScatterConeAngle = 360f,
                ScatterShardLength = 72f,
                HeadGlowOuterColor = Color.FromArgb(255, 242, 172),
                OuterTrailStartColor = Color.FromArgb(255, 228, 134),
                OuterTrailEndColor = Color.FromArgb(255, 128, 70),
                InnerTrailStartColor = Color.White,
                InnerTrailEndColor = Color.FromArgb(255, 210, 96),
                ImpactColorA = Color.White,
                ImpactColorB = Color.FromArgb(255, 132, 70)
            }
        };
    }

    public static EffectPreset CreateSparkle()
    {
        return new EffectPreset
        {
            Name = "星光闪烁",
            Description = "细碎星点与明亮核心，适合神圣/光属性",
            BackgroundColor = Color.FromArgb(10, 16, 24),
            Projectile = new ProjectileEffectSettings
            {
                EffectKind = ProjectileEffectKind.SpiralComet,
                ExportClassName = "SparkleBullet",
                PreviewSpeed = 210f,
                PreviewArcAmplitude = 50f,
                TrailPulseAmount = 0.14f,
                HeadSize = 10f,
                OuterTrailStartColor = Color.FromArgb(255, 244, 170),
                OuterTrailEndColor = Color.FromArgb(170, 220, 255),
                InnerTrailStartColor = Color.White,
                InnerTrailEndColor = Color.FromArgb(255, 248, 182),
                CoreTrailEndColor = Color.FromArgb(255, 255, 235),
                ImpactDustType = 58,
                ImpactKind = ImpactEffectKind.SparkBurst,
                ImpactDustCount = 24,
                ImpactColorA = Color.White,
                ImpactColorB = Color.FromArgb(120, 210, 255)
            }
        };
    }

    public static EffectPreset CreateFire()
    {
        return new EffectPreset
        {
            Name = "火焰燃烧",
            Description = "橙红尾焰和热浪脉冲，适合燃烧弹",
            BackgroundColor = Color.FromArgb(16, 10, 9),
            Projectile = new ProjectileEffectSettings
            {
                EffectKind = ProjectileEffectKind.ScatterShard,
                ExportClassName = "BurningBullet",
                PreviewSpeed = 230f,
                PreviewArcAmplitude = 30f,
                OuterTrailStartColor = Color.FromArgb(255, 190, 72),
                OuterTrailEndColor = Color.FromArgb(230, 72, 28),
                InnerTrailStartColor = Color.FromArgb(255, 246, 170),
                InnerTrailEndColor = Color.FromArgb(255, 128, 36),
                CoreTrailStartColor = Color.White,
                CoreTrailEndColor = Color.FromArgb(255, 210, 72),
                HeadGlowOuterColor = Color.FromArgb(255, 156, 54),
                HeadCoreColor = Color.FromArgb(255, 235, 128),
                LightColor = Color.FromArgb(255, 120, 35),
                ImpactKind = ImpactEffectKind.SmokeBloom,
                ImpactDustType = 6,
                ImpactDustCount = 28,
                ImpactDustSpeed = 4.5f,
                ImpactColorA = Color.FromArgb(255, 235, 120),
                ImpactColorB = Color.FromArgb(255, 78, 28)
            }
        };
    }

    public static EffectPreset CreateMagic()
    {
        return new EffectPreset
        {
            Name = "魔法光环",
            Description = "蓝紫流光和柔亮弹头，适合魔法弹幕",
            BackgroundColor = Color.FromArgb(8, 10, 26),
            Projectile = new ProjectileEffectSettings
            {
                EffectKind = ProjectileEffectKind.SpiralComet,
                ExportClassName = "ArcaneFlowBullet",
                PreviewSpeed = 235f,
                PreviewArcAmplitude = 62f,
                PreviewArcFrequency = 1.7f,
                OuterTrailStartColor = Color.FromArgb(154, 118, 255),
                OuterTrailEndColor = Color.FromArgb(60, 180, 255),
                InnerTrailStartColor = Color.FromArgb(235, 220, 255),
                InnerTrailEndColor = Color.FromArgb(130, 228, 255),
                CoreTrailStartColor = Color.White,
                CoreTrailEndColor = Color.FromArgb(210, 244, 255),
                HeadGlowOuterColor = Color.FromArgb(170, 120, 255),
                HeadGlowInnerColor = Color.FromArgb(235, 255, 255),
                LightColor = Color.FromArgb(140, 120, 255),
                ImpactKind = ImpactEffectKind.LightningFork,
                ImpactDustType = 27,
                ImpactDustCount = 22,
                ImpactColorA = Color.FromArgb(240, 230, 255),
                ImpactColorB = Color.FromArgb(92, 200, 255)
            }
        };
    }
}

public sealed class DrawingColorJsonConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Color must be a string.");

        string? value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
            return Color.Empty;

        value = value.Trim();
        if (value.StartsWith('#'))
            value = value[1..];

        if (value.Length == 6)
        {
            int rgb = Convert.ToInt32(value, 16);
            return Color.FromArgb(255, (rgb >> 16) & 255, (rgb >> 8) & 255, rgb & 255);
        }

        if (value.Length == 8)
        {
            uint argb = Convert.ToUInt32(value, 16);
            return Color.FromArgb((int)((argb >> 24) & 255), (int)((argb >> 16) & 255), (int)((argb >> 8) & 255), (int)(argb & 255));
        }

        return ColorTranslator.FromHtml(value);
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        writer.WriteStringValue($"#{value.A:X2}{value.R:X2}{value.G:X2}{value.B:X2}");
    }
}
