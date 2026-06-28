using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace tmcreator.Models.EffectPreview;

public static class EffectProjectileExporter
{
    private static readonly Regex IdentifierRegex = new("[^A-Za-z0-9_]", RegexOptions.Compiled);

    public static string GenerateProjectileCode(EffectPreset preset)
    {
        ProjectileEffectSettings s = preset.Projectile;
        string className = NormalizeClassName(s.ExportClassName);
        string namespaceName = NormalizeNamespace(s.ExportNamespace);
        string texture = string.IsNullOrWhiteSpace(s.TextureOverride)
            ? "\"Terraria/Images/Projectile_\" + ProjectileID.Bullet"
            : $"\"{EscapeString(s.TextureOverride.Trim().Replace('\\', '/'))}\"";

        var sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using Microsoft.Xna.Framework;");
        sb.AppendLine("using Microsoft.Xna.Framework.Graphics;");
        sb.AppendLine("using Terraria;");
        sb.AppendLine("using Terraria.GameContent;");
        sb.AppendLine("using Terraria.ID;");
        sb.AppendLine("using Terraria.ModLoader;");
        sb.AppendLine();
        sb.AppendLine($"namespace {namespaceName}");
        sb.AppendLine("{");
        sb.AppendLine($"    public class {className} : ModProjectile");
        sb.AppendLine("    {");
        sb.AppendLine($"        private const int TrailSubdivisionSteps = {Math.Max(1, s.TrailSubdivisionSteps)};");
        sb.AppendLine($"        private const int MaxTrailPoints = {Math.Max(3, s.MaxTrailPoints)};");
        sb.AppendLine($"        private const float TrailBreakDistance = {F(s.TrailBreakDistance)};");
        sb.AppendLine($"        private const bool UseSpeedCurve = {BoolLiteral(s.UseSpeedCurve)};");
        sb.AppendLine($"        private const float LaunchSpeed = {F(s.LaunchSpeed / 60f)};");
        sb.AppendLine($"        private const float TargetSpeed = {F(s.TargetSpeed / 60f)};");
        sb.AppendLine($"        private const int SpeedRampTicks = {Math.Max(1, (int)Math.Round(s.SpeedRampSeconds * 60f))};");
        sb.AppendLine();
        sb.AppendLine($"        public override string Texture => {texture};");
        sb.AppendLine();
        sb.AppendLine("        public override void SetStaticDefaults()");
        sb.AppendLine("        {");
        sb.AppendLine("            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;");
        sb.AppendLine($"            ProjectileID.Sets.TrailCacheLength[Projectile.type] = {GetEffectiveTrailCacheLength(s)};");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        public override void SetDefaults()");
        sb.AppendLine("        {");
        sb.AppendLine($"            Projectile.width = {Math.Max(1, s.Width)};");
        sb.AppendLine($"            Projectile.height = {Math.Max(1, s.Height)};");
        sb.AppendLine("            Projectile.aiStyle = -1;");
        sb.AppendLine("            Projectile.friendly = true;");
        sb.AppendLine("            Projectile.hostile = false;");
        sb.AppendLine("            Projectile.DamageType = DamageClass.Ranged;");
        sb.AppendLine($"            Projectile.penetrate = {Math.Max(-1, s.Penetrate)};");
        sb.AppendLine($"            Projectile.timeLeft = {Math.Max(1, s.TimeLeft)};");
        sb.AppendLine($"            Projectile.extraUpdates = {Math.Max(0, s.ExtraUpdates)};");
        if (s.UsesLocalNpcImmunity)
        {
            sb.AppendLine("            Projectile.usesLocalNPCImmunity = true;");
            sb.AppendLine($"            Projectile.localNPCHitCooldown = {Math.Max(1, s.LocalNpcHitCooldown)};");
        }
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        public override void AI()");
        sb.AppendLine("        {");
        sb.AppendLine("            Projectile.localAI[0]++;");
        if (s.UseSpeedCurve)
            sb.AppendLine("            ApplySpeedCurve();");
        sb.AppendLine($"            Lighting.AddLight(Projectile.Center, {Vector3Expr(s.LightColor, s.LightIntensity)});");
        sb.AppendLine("            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        public override bool PreDraw(ref Color lightColor)");
        sb.AppendLine("        {");
        sb.AppendLine("            Texture2D texture = TextureAssets.Projectile[Type].Value;");
        sb.AppendLine("            Rectangle sourceRect = texture.Bounds;");
        sb.AppendLine("            Vector2 origin = sourceRect.Size() / 2f;");
        sb.AppendLine();
        switch (s.EffectKind)
        {
            case ProjectileEffectKind.NovaBurst:
                sb.AppendLine("            DrawNovaBurst();");
                break;
            case ProjectileEffectKind.ScatterShard:
                sb.AppendLine("            DrawScatterShards();");
                break;
            default:
                if (s.ShowMuzzleFlame)
                    sb.AppendLine("            DrawMuzzleFlame();");
                if (s.ShowTrail)
                    sb.AppendLine("            DrawSmoothFlowTrail();");
                if (s.EffectKind == ProjectileEffectKind.SpiralComet)
                    sb.AppendLine("            DrawSpiralStrands();");
                if (s.EffectKind == ProjectileEffectKind.OrbitingOrbs)
                    sb.AppendLine("            DrawOrbitingOrbs();");
                if (s.ShowHead)
                    sb.AppendLine("            DrawHeadGlow();");
                sb.AppendLine();
                sb.AppendLine("            Main.EntitySpriteDraw(");
                sb.AppendLine("                texture,");
                sb.AppendLine("                Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),");
                sb.AppendLine("                sourceRect,");
                sb.AppendLine("                lightColor,");
                sb.AppendLine("                Projectile.rotation,");
                sb.AppendLine("                origin,");
                sb.AppendLine("                Projectile.scale,");
                sb.AppendLine("                SpriteEffects.None,");
                sb.AppendLine("                0);");
                break;
        }
        sb.AppendLine();
        sb.AppendLine("            return false;");
        sb.AppendLine("        }");
        sb.AppendLine();
        AppendOnKill(sb, s);
        if (s.UseSpeedCurve)
            AppendSpeedCurveMethod(sb);
        if (s.ShowTrail)
            AppendTrailMethod(sb, s);
        if (s.ShowMuzzleFlame)
            AppendMuzzleFlameMethod(sb, s);
        if (s.EffectKind == ProjectileEffectKind.SpiralComet)
            AppendSpiralMethod(sb, s);
        if (s.EffectKind == ProjectileEffectKind.OrbitingOrbs)
            AppendOrbitMethod(sb, s);
        if (s.EffectKind == ProjectileEffectKind.NovaBurst)
            AppendNovaMethod(sb, s);
        if (s.EffectKind == ProjectileEffectKind.ScatterShard)
            AppendScatterMethod(sb, s);
        if (s.ShowHead)
            AppendHeadMethod(sb, s);
        if (s.UseSpeedCurve || s.ShowMuzzleFlame || s.EffectKind is ProjectileEffectKind.SpiralComet or ProjectileEffectKind.OrbitingOrbs or ProjectileEffectKind.NovaBurst or ProjectileEffectKind.ScatterShard)
            AppendDirectionMethod(sb);
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static void AppendOnKill(StringBuilder sb, ProjectileEffectSettings s)
    {
        sb.AppendLine("        public override void OnKill(int timeLeft)");
        sb.AppendLine("        {");
        if (!s.ShowImpact || s.ImpactDustCount <= 0)
        {
            sb.AppendLine("        }");
            sb.AppendLine();
            return;
        }

        switch (s.ImpactKind)
        {
            case ImpactEffectKind.RingShockwave:
                AppendRingImpactCode(sb, s);
                break;
            case ImpactEffectKind.CrossSlash:
                AppendCrossSlashImpactCode(sb, s);
                break;
            case ImpactEffectKind.SmokeBloom:
                AppendSmokeImpactCode(sb, s);
                break;
            case ImpactEffectKind.LightningFork:
                AppendLightningImpactCode(sb, s);
                break;
            case ImpactEffectKind.ShardSpray:
                AppendShardImpactCode(sb, s);
                break;
            default:
                AppendSparkImpactCode(sb, s);
                break;
        }
        sb.AppendLine("        }");
        sb.AppendLine();
    }

    private static void AppendSparkImpactCode(StringBuilder sb, ProjectileEffectSettings s)
    {
        sb.AppendLine($"            for (int i = 0; i < {Math.Max(0, s.ImpactDustCount)}; i++)");
        sb.AppendLine("            {");
        sb.AppendLine($"                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, {Math.Max(0, s.ImpactDustType)}, 0f, 0f, 100, default, {F(s.ImpactDustScale)});");
        sb.AppendLine("                dust.noGravity = true;");
        sb.AppendLine($"                dust.velocity = Main.rand.NextVector2Unit() * {F(s.ImpactDustSpeed)};");
        sb.AppendLine($"                dust.color = Color.Lerp({ColorExpr(s.ImpactColorA)}, {ColorExpr(s.ImpactColorB)}, Main.rand.NextFloat());");
        sb.AppendLine("            }");
    }

    private static void AppendRingImpactCode(StringBuilder sb, ProjectileEffectSettings s)
    {
        sb.AppendLine($"            int dustPerRing = Math.Max(6, {Math.Max(0, s.ImpactDustCount)} / Math.Max(1, {Math.Max(1, s.ImpactRingCount)}));");
        sb.AppendLine($"            for (int ring = 0; ring < {Math.Max(1, s.ImpactRingCount)}; ring++)");
        sb.AppendLine("            {");
        sb.AppendLine("                for (int i = 0; i < dustPerRing; i++)");
        sb.AppendLine("                {");
        sb.AppendLine("                    float angle = MathHelper.TwoPi * i / dustPerRing + ring * 0.18f;");
        sb.AppendLine("                    Vector2 direction = angle.ToRotationVector2();");
        sb.AppendLine($"                    Dust dust = Dust.NewDustDirect(Projectile.Center + direction * (ring + 1) * 4f, 2, 2, {Math.Max(0, s.ImpactDustType)}, 0f, 0f, 100, default, {F(s.ImpactDustScale)});");
        sb.AppendLine("                    dust.noGravity = true;");
        sb.AppendLine($"                    dust.velocity = direction * ({F(s.ImpactDustSpeed)} + ring * 0.55f);");
        sb.AppendLine($"                    dust.color = Color.Lerp({ColorExpr(s.ImpactColorA)}, {ColorExpr(s.ImpactColorB)}, ring / (float)Math.Max(1, {Math.Max(1, s.ImpactRingCount)} - 1));");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
    }

    private static void AppendCrossSlashImpactCode(StringBuilder sb, ProjectileEffectSettings s)
    {
        sb.AppendLine("            for (int slash = 0; slash < 4; slash++)");
        sb.AppendLine("            {");
        sb.AppendLine("                float baseAngle = MathHelper.PiOver4 + slash * MathHelper.PiOver2;");
        sb.AppendLine($"                for (int i = 0; i < Math.Max(4, {Math.Max(0, s.ImpactDustCount)} / 4); i++)");
        sb.AppendLine("                {");
        sb.AppendLine("                    float offset = (i - 3f) * 4f;");
        sb.AppendLine("                    Vector2 direction = baseAngle.ToRotationVector2();");
        sb.AppendLine("                    Vector2 position = Projectile.Center + direction * offset;");
        sb.AppendLine($"                    Dust dust = Dust.NewDustDirect(position, 2, 2, {Math.Max(0, s.ImpactDustType)}, 0f, 0f, 100, default, {F(s.ImpactDustScale)});");
        sb.AppendLine("                    dust.noGravity = true;");
        sb.AppendLine($"                    dust.velocity = direction * {F(s.ImpactDustSpeed)} * (0.6f + i * 0.08f);");
        sb.AppendLine($"                    dust.color = slash % 2 == 0 ? {ColorExpr(s.ImpactColorA)} : {ColorExpr(s.ImpactColorB)};");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
    }

    private static void AppendSmokeImpactCode(StringBuilder sb, ProjectileEffectSettings s)
    {
        sb.AppendLine($"            for (int i = 0; i < {Math.Max(0, s.ImpactDustCount)}; i++)");
        sb.AppendLine("            {");
        sb.AppendLine("                Vector2 direction = Main.rand.NextVector2Unit();");
        sb.AppendLine($"                Dust dust = Dust.NewDustDirect(Projectile.Center + direction * Main.rand.NextFloat({F(Math.Max(2f, s.ImpactCloudSize * 0.35f))}), 2, 2, {Math.Max(0, s.ImpactDustType)}, 0f, 0f, 120, default, {F(s.ImpactDustScale)} * Main.rand.NextFloat(0.8f, 1.9f));");
        sb.AppendLine("                dust.noGravity = false;");
        sb.AppendLine($"                dust.velocity = direction * Main.rand.NextFloat({F(s.ImpactDustSpeed * 0.15f)}, {F(Math.Max(0.2f, s.ImpactDustSpeed * 0.75f))});");
        sb.AppendLine($"                dust.color = Color.Lerp({ColorExpr(s.ImpactColorA)}, {ColorExpr(s.ImpactColorB)}, Main.rand.NextFloat());");
        sb.AppendLine("            }");
    }

    private static void AppendLightningImpactCode(StringBuilder sb, ProjectileEffectSettings s)
    {
        sb.AppendLine($"            for (int branch = 0; branch < {Math.Max(1, s.ImpactLightningBranches)}; branch++)");
        sb.AppendLine("            {");
        sb.AppendLine($"                float angle = MathHelper.TwoPi * branch / {Math.Max(1, s.ImpactLightningBranches)} + Main.rand.NextFloat(-0.22f, 0.22f);");
        sb.AppendLine("                Vector2 direction = angle.ToRotationVector2();");
        sb.AppendLine("                Vector2 position = Projectile.Center;");
        sb.AppendLine("                for (int i = 0; i < 5; i++)");
        sb.AppendLine("                {");
        sb.AppendLine("                    position += direction.RotatedBy(Main.rand.NextFloat(-0.35f, 0.35f)) * Main.rand.NextFloat(8f, 18f);");
        sb.AppendLine($"                    Dust dust = Dust.NewDustDirect(position, 2, 2, {Math.Max(0, s.ImpactDustType)}, 0f, 0f, 100, default, {F(s.ImpactDustScale)});");
        sb.AppendLine("                    dust.noGravity = true;");
        sb.AppendLine($"                    dust.velocity = direction * Main.rand.NextFloat({F(s.ImpactDustSpeed * 0.35f)}, {F(Math.Max(0.4f, s.ImpactDustSpeed))});");
        sb.AppendLine($"                    dust.color = Color.Lerp({ColorExpr(s.ImpactColorA)}, {ColorExpr(s.ImpactColorB)}, Main.rand.NextFloat());");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
    }

    private static void AppendShardImpactCode(StringBuilder sb, ProjectileEffectSettings s)
    {
        sb.AppendLine("            Vector2 baseDirection = Projectile.velocity.LengthSquared() > 0.001f ? Vector2.Normalize(Projectile.velocity) : Vector2.UnitX;");
        sb.AppendLine($"            for (int i = 0; i < {Math.Max(0, s.ImpactDustCount)}; i++)");
        sb.AppendLine("            {");
        sb.AppendLine("                Vector2 direction = baseDirection.RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver2, MathHelper.PiOver2));");
        sb.AppendLine($"                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, {Math.Max(0, s.ImpactDustType)}, 0f, 0f, 100, default, {F(s.ImpactDustScale)} * Main.rand.NextFloat(0.7f, 1.45f));");
        sb.AppendLine("                dust.noGravity = true;");
        sb.AppendLine($"                dust.velocity = direction * Main.rand.NextFloat({F(s.ImpactDustSpeed * 0.45f)}, {F(s.ImpactDustSpeed * 1.45f)});");
        sb.AppendLine($"                dust.color = Color.Lerp({ColorExpr(s.ImpactColorA)}, {ColorExpr(s.ImpactColorB)}, Main.rand.NextFloat());");
        sb.AppendLine("            }");
    }

    private static void AppendSpeedCurveMethod(StringBuilder sb)
    {
        sb.AppendLine("        private void ApplySpeedCurve()");
        sb.AppendLine("        {");
        sb.AppendLine("            Vector2 direction = GetDrawDirection();");
        sb.AppendLine("            float progress = MathHelper.Clamp(Projectile.localAI[0] / SpeedRampTicks, 0f, 1f);");
        sb.AppendLine("            progress = progress * progress * (3f - 2f * progress);");
        sb.AppendLine("            float speed = MathHelper.Lerp(LaunchSpeed, TargetSpeed, progress);");
        sb.AppendLine("            Projectile.velocity = direction * speed;");
        sb.AppendLine("        }");
        sb.AppendLine();
    }

    private static void AppendTrailMethod(StringBuilder sb, ProjectileEffectSettings s)
    {
        sb.AppendLine("        private void DrawSmoothFlowTrail()");
        sb.AppendLine("        {");
        sb.AppendLine("            List<Vector2> rawPoints = new List<Vector2> { Projectile.Center };");
        sb.AppendLine();
        sb.AppendLine("            for (int i = 0; i < Projectile.oldPos.Length; i++)");
        sb.AppendLine("            {");
        sb.AppendLine("                if (Projectile.oldPos[i] == Vector2.Zero)");
        sb.AppendLine("                    break;");
        sb.AppendLine();
        sb.AppendLine("                Vector2 oldCenter = Projectile.oldPos[i] + Projectile.Size * 0.5f;");
        sb.AppendLine("                if (Vector2.Distance(rawPoints[rawPoints.Count - 1], oldCenter) > TrailBreakDistance)");
        sb.AppendLine("                    break;");
        sb.AppendLine();
        sb.AppendLine("                rawPoints.Add(oldCenter);");
        sb.AppendLine("                if (rawPoints.Count >= MaxTrailPoints)");
        sb.AppendLine("                    break;");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            if (rawPoints.Count < 3)");
        sb.AppendLine("                return;");
        sb.AppendLine();
        sb.AppendLine("            List<Vector2> smoothPath = new List<Vector2>();");
        sb.AppendLine("            for (int i = 0; i < rawPoints.Count - 1; i++)");
        sb.AppendLine("            {");
        sb.AppendLine("                Vector2 p1 = rawPoints[i];");
        sb.AppendLine("                Vector2 p2 = rawPoints[i + 1];");
        sb.AppendLine("                Vector2 p0 = i > 0 ? rawPoints[i - 1] : p1 + (p1 - p2);");
        sb.AppendLine("                Vector2 p3 = i + 2 < rawPoints.Count ? rawPoints[i + 2] : p2 + (p2 - p1);");
        sb.AppendLine();
        sb.AppendLine("                for (int j = 0; j < TrailSubdivisionSteps; j++)");
        sb.AppendLine("                    smoothPath.Add(Vector2.CatmullRom(p0, p1, p2, p3, j / (float)TrailSubdivisionSteps));");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            if (smoothPath.Count < 2)");
        sb.AppendLine("                return;");
        sb.AppendLine();
        sb.AppendLine($"            float pulse = 0.95f + {F(s.TrailPulseAmount)} * (float)Math.Sin(Main.GlobalTimeWrappedHourly * {F(s.TrailPulseSpeed)});");
        sb.AppendLine("            Texture2D glowTexture = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;");
        sb.AppendLine("            Vector2 glowOrigin = glowTexture.Size() * 0.5f;");
        sb.AppendLine();
        sb.AppendLine("            for (int i = 0; i < smoothPath.Count - 1; i++)");
        sb.AppendLine("            {");
        sb.AppendLine("                Vector2 current = smoothPath[i];");
        sb.AppendLine("                Vector2 next = smoothPath[i + 1];");
        sb.AppendLine("                float progress = i / (float)(smoothPath.Count - 1);");
        sb.AppendLine($"                float opacity = Projectile.Opacity * MathHelper.Lerp({F(s.TrailOpacityStart)}, {F(s.TrailOpacityEnd)}, progress);");
        sb.AppendLine($"                float outerWidth = Projectile.scale * MathHelper.Lerp({F(s.OuterTrailWidthStart)}, {F(s.OuterTrailWidthEnd)}, progress) * pulse;");
        sb.AppendLine($"                float innerWidth = Projectile.scale * MathHelper.Lerp({F(s.InnerTrailWidthStart)}, {F(s.InnerTrailWidthEnd)}, progress);");
        sb.AppendLine($"                float coreWidth = Projectile.scale * MathHelper.Lerp({F(s.CoreTrailWidthStart)}, {F(s.CoreTrailWidthEnd)}, progress);");
        sb.AppendLine();
        sb.AppendLine("                Vector2 drawCurrent = current - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);");
        sb.AppendLine("                Vector2 drawNext = next - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);");
        sb.AppendLine();
        sb.AppendLine($"                Color outerColor = Color.Lerp({ColorExpr(s.OuterTrailStartColor)}, {ColorExpr(s.OuterTrailEndColor)}, progress) * (opacity * {F(s.OuterOpacityScale)});");
        sb.AppendLine($"                Color innerColor = Color.Lerp({ColorExpr(s.InnerTrailStartColor)}, {ColorExpr(s.InnerTrailEndColor)}, progress) * opacity;");
        sb.AppendLine($"                Color coreColor = Color.Lerp({ColorExpr(s.CoreTrailStartColor)}, {ColorExpr(s.CoreTrailEndColor)}, progress) * (opacity * {F(s.CoreOpacityScale)});");
        sb.AppendLine();
        sb.AppendLine("                Utils.DrawLine(Main.spriteBatch, drawCurrent, drawNext, outerColor, outerColor, outerWidth);");
        sb.AppendLine("                Utils.DrawLine(Main.spriteBatch, drawCurrent, drawNext, innerColor, innerColor, innerWidth);");
        sb.AppendLine("                Utils.DrawLine(Main.spriteBatch, drawCurrent, drawNext, coreColor, coreColor, coreWidth);");
        sb.AppendLine();
        sb.AppendLine("                if (i < smoothPath.Count - 2)");
        sb.AppendLine("                {");
        sb.AppendLine($"                    float glowScale = Projectile.scale * MathHelper.Lerp({F(s.TrailGlowScaleStart)}, {F(s.TrailGlowScaleEnd)}, progress) * pulse;");
        sb.AppendLine("                    Main.EntitySpriteDraw(glowTexture, drawCurrent, null, innerColor * 0.42f, 0f, glowOrigin, glowScale, SpriteEffects.None, 0);");
        sb.AppendLine("                    Main.EntitySpriteDraw(glowTexture, drawCurrent, null, coreColor * 0.22f, 0f, glowOrigin, glowScale * 0.55f, SpriteEffects.None, 0);");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();
    }

    private static void AppendMuzzleFlameMethod(StringBuilder sb, ProjectileEffectSettings s)
    {
        sb.AppendLine("        private void DrawMuzzleFlame()");
        sb.AppendLine("        {");
        sb.AppendLine($"            if (Projectile.localAI[0] > {Math.Max(1, (int)Math.Round(s.MuzzleFlameDuration * 60f))})");
        sb.AppendLine("                return;");
        sb.AppendLine();
        sb.AppendLine("            Texture2D glowTexture = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;");
        sb.AppendLine("            Vector2 glowOrigin = glowTexture.Size() * 0.5f;");
        sb.AppendLine("            Vector2 center = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);");
        sb.AppendLine("            Vector2 direction = GetDrawDirection();");
        sb.AppendLine("            Vector2 back = -direction;");
        sb.AppendLine($"            float progress = Projectile.localAI[0] / {Math.Max(1, (int)Math.Round(s.MuzzleFlameDuration * 60f))}f;");
        sb.AppendLine("            float fade = (float)Math.Pow(1f - MathHelper.Clamp(progress, 0f, 1f), 1.35f);");
        sb.AppendLine($"            float length = Projectile.scale * {F(s.MuzzleFlameLength)} * (0.55f + 0.45f * fade);");
        sb.AppendLine($"            float width = Projectile.scale * {F(s.MuzzleFlameWidth)} * (0.55f + 0.45f * fade);");
        sb.AppendLine("            Vector2 basePos = center + back * Math.Max(4f, Projectile.width * 0.42f);");
        sb.AppendLine("            Vector2 tail = basePos + back * length;");
        sb.AppendLine();
        sb.AppendLine($"            Color outer = {ColorExpr(s.MuzzleFlameEndColor)} * (0.42f * fade);");
        sb.AppendLine($"            Color inner = {ColorExpr(s.MuzzleFlameStartColor)} * (0.82f * fade);");
        sb.AppendLine("            Utils.DrawLine(Main.spriteBatch, tail, basePos, outer, inner, width);");
        sb.AppendLine("            Utils.DrawLine(Main.spriteBatch, Vector2.Lerp(tail, basePos, 0.45f), basePos, inner, Color.White * (0.42f * fade), Math.Max(1f, width * 0.38f));");
        sb.AppendLine("            Main.EntitySpriteDraw(glowTexture, tail, null, outer, 0f, glowOrigin, Projectile.scale * Math.Max(0.04f, width / 110f), SpriteEffects.None, 0);");
        sb.AppendLine("            Main.EntitySpriteDraw(glowTexture, basePos, null, inner * 0.5f, 0f, glowOrigin, Projectile.scale * Math.Max(0.03f, width / 150f), SpriteEffects.None, 0);");
        sb.AppendLine("        }");
        sb.AppendLine();
    }

    private static void AppendSpiralMethod(StringBuilder sb, ProjectileEffectSettings s)
    {
        sb.AppendLine("        private void DrawSpiralStrands()");
        sb.AppendLine("        {");
        sb.AppendLine("            Texture2D glowTexture = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;");
        sb.AppendLine("            Vector2 glowOrigin = glowTexture.Size() * 0.5f;");
        sb.AppendLine("            Vector2 center = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);");
        sb.AppendLine("            Vector2 direction = GetDrawDirection();");
        sb.AppendLine("            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);");
        sb.AppendLine();
        sb.AppendLine($"            for (int strand = 0; strand < {Math.Max(1, s.SpiralStrands)}; strand++)");
        sb.AppendLine("            {");
        sb.AppendLine($"                float basePhase = strand / (float){Math.Max(1, s.SpiralStrands)} * MathHelper.TwoPi + Main.GlobalTimeWrappedHourly * {F(s.OrbitSpeed)};");
        sb.AppendLine("                for (int i = 0; i < 28; i++)");
        sb.AppendLine("                {");
        sb.AppendLine("                    float progress = i / 27f;");
        sb.AppendLine($"                    Vector2 pos = center - direction * ({F(Math.Max(30f, s.PreviewPathLength * 0.42f))} * progress);");
        sb.AppendLine($"                    float wave = (float)Math.Sin(basePhase + progress * {F(s.SpiralTwist)} * MathHelper.TwoPi);");
        sb.AppendLine($"                    float radius = {F(s.SpiralRadius)} * (1f - progress * 0.45f);");
        sb.AppendLine("                    pos += perpendicular * wave * radius;");
        sb.AppendLine("                    float alpha = (1f - progress) * 0.72f;");
        sb.AppendLine($"                    Color color = Color.Lerp({ColorExpr(s.InnerTrailStartColor)}, {ColorExpr(s.OuterTrailEndColor)}, progress);");
        sb.AppendLine($"                    Main.EntitySpriteDraw(glowTexture, pos, null, color * alpha, 0f, glowOrigin, Projectile.scale * {F(Math.Max(0.02f, s.SpiralDotSize / 120f))} * (1f - progress * 0.45f), SpriteEffects.None, 0);");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();
    }

    private static void AppendOrbitMethod(StringBuilder sb, ProjectileEffectSettings s)
    {
        sb.AppendLine("        private void DrawOrbitingOrbs()");
        sb.AppendLine("        {");
        sb.AppendLine("            Texture2D glowTexture = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;");
        sb.AppendLine("            Vector2 glowOrigin = glowTexture.Size() * 0.5f;");
        sb.AppendLine("            Vector2 center = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);");
        sb.AppendLine($"            for (int i = 0; i < {Math.Max(1, s.OrbitCount)}; i++)");
        sb.AppendLine("            {");
        sb.AppendLine($"                float angle = Main.GlobalTimeWrappedHourly * {F(s.OrbitSpeed)} + i / (float){Math.Max(1, s.OrbitCount)} * MathHelper.TwoPi;");
        sb.AppendLine($"                Vector2 pos = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * {F(s.OrbitRadius)};");
        sb.AppendLine($"                Color color = Color.Lerp({ColorExpr(s.InnerTrailStartColor)}, {ColorExpr(s.OuterTrailEndColor)}, i / (float)Math.Max(1, {Math.Max(1, s.OrbitCount)} - 1));");
        sb.AppendLine($"                Main.EntitySpriteDraw(glowTexture, pos, null, color * 0.85f, 0f, glowOrigin, Projectile.scale * {F(Math.Max(0.03f, s.OrbitDotSize / 95f))}, SpriteEffects.None, 0);");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();
    }

    private static void AppendNovaMethod(StringBuilder sb, ProjectileEffectSettings s)
    {
        sb.AppendLine("        private void DrawNovaBurst()");
        sb.AppendLine("        {");
        sb.AppendLine("            Texture2D glowTexture = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;");
        sb.AppendLine("            Vector2 glowOrigin = glowTexture.Size() * 0.5f;");
        sb.AppendLine("            Vector2 center = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);");
        sb.AppendLine($"            float pulse = 0.5f + 0.5f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * {F(s.NovaSpinSpeed * 2.4f)});");
        sb.AppendLine();
        sb.AppendLine($"            Main.EntitySpriteDraw(glowTexture, center, null, {ColorExpr(s.HeadGlowOuterColor)} * 0.48f, 0f, glowOrigin, Projectile.scale * ({F(s.NovaRadius / 140f)} + pulse * 0.08f), SpriteEffects.None, 0);");
        sb.AppendLine($"            Main.EntitySpriteDraw(glowTexture, center, null, {ColorExpr(s.HeadGlowInnerColor)} * 0.82f, 0f, glowOrigin, Projectile.scale * {F(Math.Max(0.08f, s.HeadSize / 42f))}, SpriteEffects.None, 0);");
        sb.AppendLine();
        sb.AppendLine($"            for (int ring = 0; ring < {Math.Max(1, s.NovaRingCount)}; ring++)");
        sb.AppendLine("            {");
        sb.AppendLine($"                float progress = (ring + pulse) / {Math.Max(1, s.NovaRingCount)}f;");
        sb.AppendLine($"                float radius = {F(s.NovaRadius)} * (0.28f + progress * 0.72f);");
        sb.AppendLine($"                Color color = Color.Lerp({ColorExpr(s.OuterTrailStartColor)}, {ColorExpr(s.OuterTrailEndColor)}, ring / (float)Math.Max(1, {Math.Max(1, s.NovaRingCount)} - 1));");
        sb.AppendLine("                for (int i = 0; i < 24; i++)");
        sb.AppendLine("                {");
        sb.AppendLine("                    float a0 = i / 24f * MathHelper.TwoPi;");
        sb.AppendLine("                    float a1 = (i + 1) / 24f * MathHelper.TwoPi;");
        sb.AppendLine("                    Vector2 p0 = center + new Vector2((float)Math.Cos(a0), (float)Math.Sin(a0)) * radius;");
        sb.AppendLine("                    Vector2 p1 = center + new Vector2((float)Math.Cos(a1), (float)Math.Sin(a1)) * radius;");
        sb.AppendLine("                    Utils.DrawLine(Main.spriteBatch, p0, p1, color * 0.62f, color * 0.62f, 2.2f + ring * 0.4f);");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            for (int i = 0; i < 16; i++)");
        sb.AppendLine("            {");
        sb.AppendLine($"                float angle = Main.GlobalTimeWrappedHourly * {F(s.NovaSpinSpeed)} + i / 16f * MathHelper.TwoPi;");
        sb.AppendLine("                Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));");
        sb.AppendLine($"                Vector2 start = center + dir * ({F(s.NovaRadius)} * 0.24f);");
        sb.AppendLine($"                Vector2 end = center + dir * ({F(s.NovaRadius * 0.24f + s.NovaSpikeLength)} * (0.72f + 0.18f * pulse));");
        sb.AppendLine($"                Utils.DrawLine(Main.spriteBatch, start, end, {ColorExpr(s.InnerTrailStartColor)} * 0.82f, {ColorExpr(s.OuterTrailEndColor)} * 0.2f, 3.2f);");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();
    }

    private static void AppendScatterMethod(StringBuilder sb, ProjectileEffectSettings s)
    {
        sb.AppendLine("        private void DrawScatterShards()");
        sb.AppendLine("        {");
        sb.AppendLine("            Texture2D glowTexture = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;");
        sb.AppendLine("            Vector2 glowOrigin = glowTexture.Size() * 0.5f;");
        sb.AppendLine("            Vector2 center = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);");
        sb.AppendLine("            Vector2 forward = GetDrawDirection();");
        sb.AppendLine($"            float cone = MathHelper.ToRadians({F(s.ScatterConeAngle)});");
        sb.AppendLine($"            float wave = 0.5f + 0.5f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * {F(s.ScatterWaveSpeed)});");
        sb.AppendLine();
        sb.AppendLine($"            for (int i = 0; i < {Math.Max(1, s.ScatterShardCount)}; i++)");
        sb.AppendLine("            {");
        sb.AppendLine($"                float ratio = {Math.Max(1, s.ScatterShardCount)} == 1 ? 0.5f : i / (float)({Math.Max(1, s.ScatterShardCount)} - 1);");
        sb.AppendLine("                float angle = -cone * 0.5f + cone * ratio;");
        sb.AppendLine("                Vector2 dir = forward.RotatedBy(angle);");
        sb.AppendLine($"                Vector2 end = center + dir * ({F(s.ScatterShardLength)} * (0.45f + wave * 0.55f));");
        sb.AppendLine($"                Color color = Color.Lerp({ColorExpr(s.OuterTrailStartColor)}, {ColorExpr(s.OuterTrailEndColor)}, ratio);");
        sb.AppendLine($"                Utils.DrawLine(Main.spriteBatch, center, end, color * 0.2f, color * 0.85f, {F(Math.Max(1f, s.OuterTrailWidthStart * 0.42f))});");
        sb.AppendLine($"                Utils.DrawLine(Main.spriteBatch, Vector2.Lerp(center, end, 0.55f), end, {ColorExpr(s.InnerTrailStartColor)} * 0.92f, color * 0.3f, {F(Math.Max(0.7f, s.CoreTrailWidthStart))});");
        sb.AppendLine($"                Main.EntitySpriteDraw(glowTexture, end, null, color * 0.46f, 0f, glowOrigin, Projectile.scale * {F(Math.Max(0.04f, s.HeadSize / 60f))}, SpriteEffects.None, 0);");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();
    }

    private static void AppendHeadMethod(StringBuilder sb, ProjectileEffectSettings s)
    {
        sb.AppendLine("        private void DrawHeadGlow()");
        sb.AppendLine("        {");
        sb.AppendLine("            Texture2D glowTexture = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;");
        sb.AppendLine("            Vector2 glowOrigin = glowTexture.Size() * 0.5f;");
        sb.AppendLine("            Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);");
        sb.AppendLine($"            float pulse = 1f + {F(s.HeadPulseAmount)} * (float)Math.Sin(Main.GlobalTimeWrappedHourly * {F(s.HeadPulseSpeed)});");
        sb.AppendLine();
        sb.AppendLine($"            Main.EntitySpriteDraw(glowTexture, drawPos, null, {ColorExpr(s.HeadGlowOuterColor)} * 1.05f, 0f, glowOrigin, Projectile.scale * {F(s.HeadOuterScale / 10f)} * pulse, SpriteEffects.None, 0);");
        sb.AppendLine($"            Main.EntitySpriteDraw(glowTexture, drawPos, null, {ColorExpr(s.HeadGlowInnerColor)} * 0.58f, 0f, glowOrigin, Projectile.scale * {F(s.HeadInnerScale / 10f)} * pulse, SpriteEffects.None, 0);");
        sb.AppendLine("        }");
        sb.AppendLine();
    }

    private static void AppendDirectionMethod(StringBuilder sb)
    {
        sb.AppendLine("        private Vector2 GetDrawDirection()");
        sb.AppendLine("        {");
        sb.AppendLine("            if (Projectile.velocity.LengthSquared() > 0.001f)");
        sb.AppendLine("                return Vector2.Normalize(Projectile.velocity);");
        sb.AppendLine("            return Vector2.UnitX;");
        sb.AppendLine("        }");
        sb.AppendLine();
    }

    public static string NormalizeClassName(string? value)
    {
        value = string.IsNullOrWhiteSpace(value) ? "GeneratedBulletEffect" : IdentifierRegex.Replace(value.Trim(), "");
        if (string.IsNullOrWhiteSpace(value))
            value = "GeneratedBulletEffect";
        if (!char.IsLetter(value[0]) && value[0] != '_')
            value = "Generated" + value;
        return value;
    }

    public static string NormalizeNamespace(string? value)
    {
        value = string.IsNullOrWhiteSpace(value) ? "YourMod.Projectiles" : value.Trim();
        string[] parts = value.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeClassName)
            .ToArray();
        return parts.Length == 0 ? "YourMod.Projectiles" : string.Join(".", parts);
    }

    private static string EscapeString(string value) => value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static string BoolLiteral(bool value) => value ? "true" : "false";

    private static int GetEffectiveTrailCacheLength(ProjectileEffectSettings s)
    {
        if (!s.ShowTrail)
            return 2;

        int durationFrames = Math.Max(2, (int)Math.Round(Math.Max(0.02f, s.TrailDurationSeconds) * 60f * Math.Max(1, s.ExtraUpdates + 1)));
        return Math.Clamp(Math.Max(s.TrailCacheLength, durationFrames), 2, 80);
    }

    private static string F(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture) + "f";
    }

    private static string ColorExpr(System.Drawing.Color color)
    {
        return $"new Color({color.R}, {color.G}, {color.B}, {color.A})";
    }

    private static string Vector3Expr(System.Drawing.Color color, float intensity)
    {
        return $"new Vector3({F(color.R / 255f)}, {F(color.G / 255f)}, {F(color.B / 255f)}) * {F(intensity)}";
    }
}
