using UnityEngine;

namespace NakiLegend.Prototype
{
    public sealed partial class NakiPrototype
    {
        // Sort contacts along the swept path, so a fast arrow cannot hit a rear
        // enemy first merely because it was spawned later in the enemy list.
        private static bool ProjectileCircleContact(Vector2 from, Vector2 to, Vector2 center,
            float radius, out float fraction)
        {
            fraction = 0;
            Vector2 offset = from - center, delta = to - from;
            float c = offset.sqrMagnitude - radius * radius;
            if (c <= 0) return true;
            float a = delta.sqrMagnitude;
            if (a < .0000001f) return false;
            float b = Vector2.Dot(offset, delta), discriminant = b * b - a * c;
            if (discriminant < 0) return false;
            fraction = (-b - Mathf.Sqrt(discriminant)) / a;
            return fraction >= 0 && fraction <= 1;
        }

        private float ProjectileWallContact(Vector2 from, Vector2 to, float radius)
        {
            float first = float.PositiveInfinity;
            Vector2 delta = to - from;
            foreach (Rect obstacle in projectileObstacles)
            {
                float enter = 0, leave = 1;
                if (ClipProjectileAxis(from.x, delta.x, obstacle.xMin - radius, obstacle.xMax + radius, ref enter, ref leave) &&
                    ClipProjectileAxis(from.y, delta.y, obstacle.yMin - radius, obstacle.yMax + radius, ref enter, ref leave))
                    first = Mathf.Min(first, enter);
            }
            return first;
        }

        private Enemy FirstProjectileEnemy(Shot shot, Vector2 from, Vector2 to, float wall, out float contact)
        {
            contact = wall;
            Enemy first = null;
            foreach (Enemy enemy in enemies)
            {
                if (enemy.hp <= 0 || shot.hitTargets.Contains(enemy)||shot.divineFamily!=null&&shot.divineFamily.hitTargets.Contains(enemy)) continue;
                float at;
                if (ProjectileCircleContact(from, to, enemy.position, enemy.radius + .15f, out at) && at < contact)
                {
                    first = enemy;
                    contact = at;
                }
            }
            return first;
        }

        private void RefreshTrackingTarget(Shot shot)
        {
            if (shot.hostile || !shot.tracking) return;
            if (shot.target != null && shot.target.hp > 0 && enemies.Contains(shot.target) &&
                !ProjectilePathBlocked(shot.position, shot.target.position, .06f)) return;

            shot.target = null;
            float closest = 6 * 6;
            foreach (Enemy candidate in enemies)
            {
                Vector2 offset = candidate.position - shot.position;
                if (candidate.hp <= 0 || shot.hitTargets.Contains(candidate) || offset.sqrMagnitude > closest ||
                    Vector2.Dot(shot.direction, offset.normalized) < -.25f ||
                    ProjectilePathBlocked(shot.position, candidate.position, .06f)) continue;
                closest = offset.sqrMagnitude;
                shot.target = candidate;
            }
            // Keep travelling if no target is available. A later visible target
            // may be acquired, but the lifetime and turn rate never reset.
        }

        private void ResolveProjectileEnemyHit(Shot shot, Enemy target, Vector2 contact)
        {
            shot.hitTargets.Add(target);if(shot.divineFamily!=null)shot.divineFamily.hitTargets.Add(target);float appliedDamage=TacticalProjectileDamage(shot,target);
            if (enableDesertJourney)
            {
                ProjectileHitFeedback(shot, target, contact);
                if (shot.skillAudio == "ice_spike") target.iceSlowUntil = clock + (target.boss ? .6f : 1.5f);
                if (shot.skillAudio != null) ApplySkillHit(target, appliedDamage, shot.element, shot.skillAudio);
                else ApplyWeaponDamage(target, appliedDamage > 0 ? appliedDamage : spellDamage, shot.element,shot.identityWeapon);
            }
            else DamageEnemy(target, appliedDamage > 0 ? appliedDamage : spellDamage);
            if(shot.identityWeapon>=0)IdentityImpact(shot.identityWeapon,contact,shot.direction);OriginProjectileHit(shot,target,contact);TrySoulSealHit(shot,target);RelicHit(shot,target,contact);ResolveChapterArc(shot, target);
            SplitDivineLight(shot,contact,false);shot.hitsLeft--;
        }

        private void StyleStaffProjectile(Shot shot)
        {
            SpriteRenderer renderer = shot.root.GetComponent<SpriteRenderer>();
            renderer.sprite = HostileBulletSprite;
            GameObject core = Draw("Staff bolt bright core", HostileBulletSprite, shot.position,
                Vector2.one * .055f, C("f5fff4"), renderer.sortingOrder + 1);
            core.transform.SetParent(shot.root.transform, true);
        }

        private void ImpactStroke(Vector2 at, Vector2 direction, float length, float width, Color tint)
        {
            GameObject streak = Stretch("Element impact streak", square, at, new Vector2(length, width), tint, 710);
            streak.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            fadingBodies.Add(new FadingBody { root = streak, renderer = streak.GetComponent<SpriteRenderer>(), expires = clock + .18f });
        }

        private void ProjectileHitFeedback(Shot shot, Enemy target, Vector2 contact)
        {
            if (shot.element == Element.Physical || clock < target.projectileFeedbackAt || fadingBodies.Count > 100) return;
            target.projectileFeedbackAt = clock + .08f;
            Color tint = SpellTint(shot.element);
            Vector2 at = contact + Vector2.up * .2f;
            float size = shot.skillAudio == null ? .13f : .25f;
            if (shot.element == Element.Lightning)
            {
                Vector2 p = at + new Vector2(-size, size), q = at + new Vector2(size * .2f, 0);
                ImpactStroke((p + q) * .5f, q - p, Vector2.Distance(p, q), .035f, tint);
                Vector2 end = at + new Vector2(-size * .2f, -size);
                ImpactStroke((q + end) * .5f, end - q, Vector2.Distance(q, end), .035f, tint);
            }
            else if (shot.element == Element.Wind)
            {
                Vector2 tangent = new Vector2(-shot.direction.y, shot.direction.x);
                ImpactStroke(at + tangent * .07f, shot.direction + tangent * .4f, size * 2, .035f, tint);
                ImpactStroke(at - tangent * .07f, shot.direction - tangent * .4f, size * 1.5f, .025f, tint);
            }
            else
            {
                int count = shot.element == Element.Ice ? 4 : 3;
                for (int i = 0; i < count; i++)
                {
                    float angle = i * Mathf.PI * 2 / count + .35f;
                    Vector2 ray = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    ImpactStroke(at + ray * size * .65f, ray, size, shot.element == Element.Ice ? .045f : .035f, tint);
                }
                if (shot.skillAudio != null)
                {
                    GameObject ring = Draw("Element impact ring", windRingSprite ?? disc, at,
                        Vector2.one * size * 2, tint, 709);
                    fadingBodies.Add(new FadingBody { root = ring, renderer = ring.GetComponent<SpriteRenderer>(), expires = clock + .16f });
                }
            }
        }

        private void AddDamageNumber(Enemy enemy, float amount)
        {
            foreach (FloatingHit hit in floatingHits)
                if (hit.owner == enemy && clock - hit.born <= .12f && hit.expires > clock)
                {
                    hit.amount += amount;
                    hit.text = Mathf.CeilToInt(hit.amount).ToString();
                    hit.expires = clock + .65f;
                    return;
                }
            if (floatingHits.Count > 63) floatingHits.RemoveAt(0);
            floatingHits.Add(new FloatingHit { owner = enemy, born = clock, amount = amount,
                text = Mathf.CeilToInt(amount).ToString(), at = enemy.position + Vector2.up * .7f,
                color = C("fff0bb"), expires = clock + .65f });
        }
    }
}
