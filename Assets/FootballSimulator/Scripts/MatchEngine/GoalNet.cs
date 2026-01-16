using UnityEngine;

using FStudio.MatchEngine.Players;
using FStudio.MatchEngine.Balls;

using System.Linq;
using System.Collections.Generic;

namespace FStudio.MatchEngine {
    public class GoalNet : MonoBehaviour {
        [SerializeField] private Transform middlePoint = default;

        public GameObject GoalColliders, OutColliders;
        public Transform[] goalPoints;

        public Transform leftLimit;
        public Transform rightLimit;

        public Vector3 GetShootingVectorFromPoint (PlayerBase playerBase, 
            Transform point) {

            var playerPosition = playerBase.Position;

            var skill = playerBase.MatchPlayer.GetShooting();

            var dir = point.position - playerPosition;

            var dirErrorApplied = Ball.ApplyDirectionError(dir, skill);

            // restore by distance
            var dist = Vector3.Distance(point.position, playerPosition);
            dirErrorApplied = Vector3.Lerp(dirErrorApplied, dir, 
                EngineSettings.Current.ShootErrorRemoveByDistance.Evaluate(dist));
            // 

            var errorAppliedAngle = Mathf.Abs(Vector3.SignedAngle(dir, dirErrorApplied, Vector3.up));

            //normalize it.
            dirErrorApplied = dirErrorApplied.normalized;

            var dir2D = dirErrorApplied;
            dir2D.y = 0;

            var dirUp = dirErrorApplied;
            dirUp.x = dirUp.z = 0;

            // add multipliers.
            dirErrorApplied += dir2D * EngineSettings.Current.ShootingForwardAxisMultiplier;
            //

            dirErrorApplied *= EngineSettings.Current.ShootPowerByDistanceCurve.Evaluate(dir.magnitude);

            dirErrorApplied *= EngineSettings.Current.ShootPowerBySkillCurve.
                Evaluate(playerBase.MatchPlayer.GetShooting() / 100f);

            Debug.Log($"[Shootpoint found] {dirErrorApplied}");

            dirErrorApplied += Vector3.up * dir.magnitude * EngineSettings.Current.ShootingUpAxisDistanceMultiplier;

            Debug.Log($"[Shooting point y fixed] {dirErrorApplied}");

            Debug.DrawRay(playerPosition, dir, Color.yellow, 1);
            Debug.DrawRay(playerPosition, dirErrorApplied, Color.green, 1);

            if (dirErrorApplied.y < 1) {
                dirErrorApplied.y = 1;
            }

            return dirErrorApplied;
        }

        /// <summary>
        /// Checks all goal points, 
        /// and return shooting velocities with direction error applied.
        /// </summary>
        /// <param name="playerBase">Shooter</param>
        /// <param name="colliders">Possible colliders</param>
        /// <returns>Velocity, and applied error.</returns>
        public (Transform shootPoint, float angleFree)
            GetShootingVector (PlayerBase playerBase, PlayerBase[] colliders) {
            
            if (goalPoints.Length == 0) {
                return default;
            }

            var fieldSizeY = MatchManager.Current.SizeOfField.y;

            var mPosition = playerBase.Position;

            float minAngle (Transform m_point) {
                var pointToPlayer = m_point.position - mPosition;

                float min = colliders.Select(x => Mathf.Min (Mathf.Abs (Vector3.SignedAngle(x.Position - mPosition, pointToPlayer, Vector3.up)), 45)).
                OrderBy (x=>x).FirstOrDefault ();
                return min;
            }

            var shootingVector =
                goalPoints.Select (x=>(x, minAngle (x))).OrderBy(x => 
                 Random.Range (-5, 5) +
                 Mathf.Abs(x.x.position.x - fieldSizeY / 2) + 
                 Random.Range (0, x.x.position.y) + 
                 (45-x.Item2)/2).
                 FirstOrDefault();

            return shootingVector;
        }
        /* 
        PSEUDOCODE / PLAN (detailed)
        - Purpose: Compute a shooting velocity vector that shoots in the direction the player is currently facing.
        - Inputs: PlayerBase playerBase, optional targetDistance override.
        - Steps:
          1. Read player position and shooting skill from playerBase.
          2. Determine facing direction:
             - Use playerBase.Direction if available and non-zero.
             - Fallback to Vector3.forward if necessary.
             - Normalize this direction.
          3. Determine a target distance to give the direction magnitude:
             - If caller provided targetDistance (>0) use it.
             - Otherwise derive a reasonable default from the field size (MatchManager.Current.SizeOfField.y),
               clamped to a sensible min/max so resulting power curves behave well.
          4. Build a raw direction vector (dir * distance) to pass into Ball.ApplyDirectionError.
             - Ball.ApplyDirectionError expects a non-normalized vector because it may use magnitude inside.
          5. Apply directional error using Ball.ApplyDirectionError using player's shooting skill.
          6. Partially restore original direction depending on distance using the same ShootErrorRemoveByDistance curve:
             - Lerp between error-applied direction and original raw vector by the curve evaluated at distance.
          7. Compute error angle for potential logging (same as original method).
          8. Normalize the resulting direction and split into horizontal (dir2D) and vertical (dirUp) components.
          9. Apply forward-axis multiplier, scale by power curves (by distance and by skill) similar to existing method:
             - Multiply by ShootPowerByDistanceCurve.Evaluate(distance)
             - Multiply by ShootPowerBySkillCurve.Evaluate(skill / 100f)
         10. Add upward bias using ShootingUpAxisDistanceMultiplier scaled by distance.
         11. Debug draw rays and logs analogous to existing method.
         12. Ensure a minimum upward component (y >= 1) to avoid very-flat shots.
         13. Return the final velocity vector ready to be used in Ball.Shoot.
        - Result: Vector3 representing shot velocity in player's facing direction with error & power applied.
        */

        /// <summary>
        /// Compute a shooting velocity vector in the direction the player is currently facing.
        /// Uses the same error, power and multipliers as GetShootingVectorFromPoint so behaviours are consistent.
        /// </summary>
        public Vector3 GetShootingVectorFromFacing(PlayerBase playerBase, float targetDistance = -1f)
        {
            var playerPosition = playerBase.Position;
            var skill = playerBase.MatchPlayer.GetShooting();

            // Determine facing direction
            var facing = playerBase.Direction;
            if (facing == Vector3.zero)
            {
                // fallback if Direction isn't provided
                facing = Vector3.forward;
            }
            facing = facing.normalized;

            // Determine a sensible target distance for shot magnitude
            float dist;
            if (targetDistance > 0f)
            {
                dist = targetDistance;
            }
            else
            {
                // derive default from field size, clamped to sensible bounds
                var fieldSizeY = MatchManager.Current.SizeOfField.y;
                dist = Mathf.Clamp(fieldSizeY / 4f, 10f, 40f);
            }

            // Build the non-normalized direction vector that Ball.ApplyDirectionError expects
            var rawDir = facing * dist;

            // Apply direction error based on skill
            var dirErrorApplied = Ball.ApplyDirectionError(rawDir, skill);

            // Restore some of the original direction based on distance using same curve as other method
            dirErrorApplied = Vector3.Lerp(dirErrorApplied, rawDir, EngineSettings.Current.ShootErrorRemoveByDistance.Evaluate(dist));

            // For debugging / analytics parity
            var errorAppliedAngle = Mathf.Abs(Vector3.SignedAngle(rawDir, dirErrorApplied, Vector3.up));

            // Normalize for axis splitting and multiplier application
            dirErrorApplied = dirErrorApplied.normalized;

            var dir2D = dirErrorApplied;
            dir2D.y = 0;

            var dirUp = dirErrorApplied;
            dirUp.x = dirUp.z = 0;

            // Add forward multiplier and apply power curves (distance & skill)
            dirErrorApplied += dir2D * EngineSettings.Current.ShootingForwardAxisMultiplier;

            dirErrorApplied *= EngineSettings.Current.ShootPowerByDistanceCurve.Evaluate(dist);
            dirErrorApplied *= EngineSettings.Current.ShootPowerBySkillCurve.Evaluate(skill / 100f);

            Debug.Log($"[Shoot facing] {dirErrorApplied}");

            // Add vertical bias based on distance
            dirErrorApplied += Vector3.up * dist * EngineSettings.Current.ShootingUpAxisDistanceMultiplier;

            Debug.Log($"[Shooting facing y fixed] {dirErrorApplied}");

            Debug.DrawRay(playerPosition, rawDir, Color.yellow, 1);
            Debug.DrawRay(playerPosition, dirErrorApplied, Color.green, 1);

            if (dirErrorApplied.y < 1f)
            {
                dirErrorApplied.y = 1f;
            }

            return dirErrorApplied;
        }
        /// <summary>
        /// Middle point of the goal net.
        /// </summary>
        public Vector3 Position => middlePoint.position;

        /// <summary>
        /// Direction of the goal net. System will use this direction for attacking & defending.
        /// </summary>
        public Vector3 Direction => middlePoint.forward;
    }
}
