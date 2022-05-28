using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace GameJam
{
    public static partial class Utils
    {
        // pretty print seconds as hours:minutes:seconds(.milliseconds/100)s
        public static string PrettySeconds(float seconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            string res = "";
            if (t.Days > 0) res += t.Days + "d";
            if (t.Hours > 0) res += " " + t.Hours + "h";
            if (t.Minutes > 0) res += " " + t.Minutes + "m";
            // 0.5s, 1.5s etc. if any milliseconds. 1s, 2s etc. if any seconds
            if (t.Milliseconds > 0) res += " " + t.Seconds + "." + (t.Milliseconds / 100) + "s";
            else if (t.Seconds > 0) res += " " + t.Seconds + "s";
            // if the string is still empty because the value was '0', then at least
            // return the seconds instead of returning an empty string
            return res != "" ? res : "0s";
        }

        // helper function to calculate a bounds radius in WORLD SPACE
        // -> collider.radius is local scale
        // -> collider.bounds is world scale
        // -> use x+y extends average just to be sure (for capsules, x==y extends)
        // -> use 'extends' instead of 'size' because extends are the radius.
        //    in other words: if we come from the right, we only want to stop at
        //    the radius aka half the size, not twice the radius aka size.
        public static float BoundsRadius(Bounds bounds) =>
            (bounds.extents.x + bounds.extents.z) / 2;

        // Distance between two ClosestPoints
        // this is needed in cases where entites are really big. in those cases,
        // we can't just move to entity.transform.position, because it will be
        // unreachable. instead we have to go the closest point on the boundary.
        //
        // Vector3.Distance(a.transform.position, b.transform.position):
        //    _____        _____
        //   |     |      |     |
        //   |  x==|======|==x  |
        //   |_____|      |_____|
        //
        //
        // Utils.ClosestDistance(a.collider, b.collider):
        //    _____        _____
        //   |     |      |     |
        //   |     |x====x|     |
        //   |_____|      |_____|
        //
        public static float ClosestDistance(Collider a, Collider b)
        {
            // return 0 if both intersect or if one is inside another.
            // ClosestPoint distance wouldn't be > 0 in those cases otherwise.
            if (a.bounds.Intersects(b.bounds))
                return 0;

            // Unity offers ClosestPointOnBounds and ClosestPoint.
            // ClosestPoint is more accurate. OnBounds often doesn't get <1 because
            // it uses a point at the top of the player collider, not in the center.
            // (use Debug.DrawLine here to see the difference)
            return Vector3.Distance(a.ClosestPoint(b.transform.position),
                                    b.ClosestPoint(a.transform.position));
        }

        // Distance between two ClosestPoints
        // this is needed in cases where entities are really big. in those cases,
        // we can't just move to entity.transform.position, because it will be
        // unreachable. instead we have to go the closest point on the boundary.
        //
        // Vector3.Distance(a.transform.position, b.transform.position):
        //    _____        _____
        //   |     |      |     |
        //   |  x==|======|==x  |
        //   |_____|      |_____|
        //
        //
        // Utils.ClosestDistance(a.collider, b.collider):
        //    _____        _____
        //   |     |      |     |
        //   |     |x====x|     |
        //   |_____|      |_____|
        //
        // IMPORTANT:
        //   we always pass Entity instead of Collider, because
        //   entity.transform.position is animation independent while
        //   collider.transform.position changes during animations (the hips)!
        public static float ClosestDistance(Entity a, Entity b)
        {
            // IMPORTANT: DO NOT use the collider itself. the position changes
            //            during animations, causing situations where attacks are
            //            interrupted because the target's hips moved a bit out of
            //            attack range, even though the target didn't actually move!
            //            => use transform.position and collider.radius instead!
            //
            //            this is probably faster than collider.ClosestPoints too

            // at first calculate the distance from A to B, subtract both radius
            // IMPORTANT: use entity.transform.position not
            //            collider.transform.position. that would still be the hip!
            float distance = Vector3.Distance(a.transform.position, b.transform.position);

            // calculate both collider radius
            float radiusA = BoundsRadius(a.Collider.bounds);
            float radiusB = BoundsRadius(b.Collider.bounds);

            // subtract both radius
            float distanceInside = distance - radiusA - radiusB;

            // return distance. if it's <0 because they are inside each other, then
            // return 0.
            return Mathf.Max(distanceInside, 0);
        }

        // closest point from an entity's collider to another point
        // this is used all over the place, so let's put it into one place so it's
        // easier to modify the method if needed
        public static Vector3 ClosestPoint(Entity entity, Vector3 point)
        {
            // IMPORTANT: DO NOT use the collider itself. the position changes
            //            during animations, causing situations where attacks are
            //            interrupted because the target's hips moved a bit out of
            //            attack range, even though the target didn't actually move!
            //            => use transform.position and collider.radius instead!
            //
            //            this is probably faster than collider.ClosestPoints too

            // first of all, get radius but in WORLD SPACE not in LOCAL SPACE.
            // otherwise parent scales are not applied.
            float radius = BoundsRadius(entity.Collider.bounds);

            // now get the direction from point to entity
            // IMPORTANT: use entity.transform.position not
            //            collider.transform.position. that would still be the hip!
            Vector3 direction = entity.transform.position - point;
            //Debug.DrawLine(point, point + direction, Color.red, 1, false);

            // subtract radius from direction's length
            Vector3 directionSubtracted = Vector3.ClampMagnitude(direction, direction.magnitude - radius);

            // return the point
            //Debug.DrawLine(point, point + directionSubtracted, Color.green, 1, false);
            return point + directionSubtracted;
        }

        // random point on NavMesh for item drops, etc.
        public static Vector3 RandomUnitCircleOnNavMesh(Vector3 position, float radiusMultiplier)
        {
            // random circle point
            Vector2 r = UnityEngine.Random.insideUnitCircle * radiusMultiplier;

            // convert to 3d
            Vector3 randomPosition = new Vector3(position.x + r.x, position.y, position.z + r.y);

            // raycast to find valid point on NavMesh. otherwise return original one
            if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, radiusMultiplier * 2, NavMesh.AllAreas))
                return hit.position;
            return position;
        }

        // random point on NavMesh that has no obstacles (walls) between point and center
        // -> useful because items shouldn't be dropped behind walls, etc.
        public static Vector3 ReachableRandomUnitCircleOnNavMesh(Vector3 position, float radiusMultiplier, int solverAttempts)
        {
            for (int i = 0; i < solverAttempts; ++i)
            {
                // get random point on navmesh around position
                Vector3 candidate = RandomUnitCircleOnNavMesh(position, radiusMultiplier);

                // check if anything obstructs the way (walls etc.)
                if (!NavMesh.Raycast(position, candidate, out NavMeshHit hit, NavMesh.AllAreas))
                    return candidate;
            }

            // otherwise return original position if we can't find any good point.
            // in that case it's best to just drop it where the entity stands.
            return position;
        }

        // CastWithout functions all need a backups dictionary. this is in hot path
        // and creating a Dictionary for every single call would be insanity.
        static Dictionary<Transform, int> castBackups = new Dictionary<Transform, int>();

        // raycast while ignoring self (by setting layer to "Ignore Raycasts" first)
        // => setting layer to IgnoreRaycasts before casting is the easiest way to do it
        // => raycast + !=this check would still cause hit.point to be on player
        // => raycastall is not sorted and child objects might have different layers etc.
        public static bool RaycastWithout(Vector3 origin, Vector3 direction, out RaycastHit hit, float maxDistance, GameObject ignore, int layerMask = Physics.DefaultRaycastLayers)
        {
            // remember layers
            castBackups.Clear();

            // set all to ignore raycast
            foreach (Transform tf in ignore.GetComponentsInChildren<Transform>(true))
            {
                castBackups[tf] = tf.gameObject.layer;
                tf.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }

            // raycast
            bool result = Physics.Raycast(origin, direction, out hit, maxDistance, layerMask);

            // restore layers
            foreach (KeyValuePair<Transform, int> kvp in castBackups)
                kvp.Key.gameObject.layer = kvp.Value;

            return result;
        }

        // clamp a rotation around x axis
        // (e.g. camera up/down rotation so we can't look below character's pants etc.)
        // original source: Unity's standard assets MouseLook.cs
        public static Quaternion ClampRotationAroundXAxis(Quaternion q, float min, float max)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
            angleX = Mathf.Clamp(angleX, min, max);
            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }

    }
}