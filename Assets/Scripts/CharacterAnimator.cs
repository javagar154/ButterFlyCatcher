using UnityEngine;

namespace ButterflyGame
{
    /// <summary>
    /// CharacterAnimator manages realistic humanoid character poses and animations:
    /// Idle breathing and weight shift, walk stride, sprint, and a full butterfly net scoop swing.
    /// Works cooperatively with Unity Animator when present or drives articulated limbs directly.
    /// </summary>
    public class CharacterAnimator : MonoBehaviour
    {
        [Header("Articulated Skeleton References")]
        [SerializeField] private Transform hips;
        [SerializeField] private Transform spine;
        [SerializeField] private Transform head;
        [SerializeField] private Transform leftArm;
        [SerializeField] private Transform rightArm;
        [SerializeField] private Transform leftLeg;
        [SerializeField] private Transform rightLeg;
        [SerializeField] private Transform leftShin;
        [SerializeField] private Transform rightShin;
        [SerializeField] private Transform rightHand;

        [Header("Animator Support")]
        [SerializeField] private Animator unityAnimator;

        [Header("Animation Tuning")]
        [SerializeField] private float walkStrideRate = 7.5f;
        [SerializeField] private float runStrideRate = 12.0f;
        [SerializeField] private float legStrideAngle = 36f;
        [SerializeField] private float armSwingAngle = 32f;

        private PlayerMovement movement;
        private float walkCycle = 0f;
        private bool isSwinging = false;
        private float swingTimer = 0f;
        private float swingDuration = 0.55f;

        // Default local rotations for reset
        private Quaternion defHipsRot = Quaternion.identity;
        private Quaternion defSpineRot = Quaternion.identity;
        private Quaternion defHeadRot = Quaternion.identity;
        private Quaternion defLeftArmRot = Quaternion.identity;
        private Quaternion defRightArmRot = Quaternion.identity;
        private Quaternion defLeftLegRot = Quaternion.identity;
        private Quaternion defRightLegRot = Quaternion.identity;

        public bool IsSwinging => isSwinging;
        public float SwingProgress => isSwinging ? Mathf.Clamp01(swingTimer / swingDuration) : 0f;
        public Transform RightHand => rightHand;

        private void Awake()
        {
            movement = GetComponent<PlayerMovement>();
            if (unityAnimator == null)
            {
                unityAnimator = GetComponentInChildren<Animator>();
            }

            CacheDefaultRotations();
        }

        private void CacheDefaultRotations()
        {
            if (hips != null) defHipsRot = hips.localRotation;
            if (spine != null) defSpineRot = spine.localRotation;
            if (head != null) defHeadRot = head.localRotation;
            if (leftArm != null) defLeftArmRot = leftArm.localRotation;
            if (rightArm != null) defRightArmRot = rightArm.localRotation;
            if (leftLeg != null) defLeftLegRot = leftLeg.localRotation;
            if (rightLeg != null) defRightLegRot = rightLeg.localRotation;
        }

        public void BindLimbs(
            Transform hipsT, Transform spineT, Transform headT,
            Transform lArm, Transform rArm,
            Transform lLeg, Transform rLeg,
            Transform lShin, Transform rShin,
            Transform rHand)
        {
            hips = hipsT;
            spine = spineT;
            head = headT;
            leftArm = lArm;
            rightArm = rArm;
            leftLeg = lLeg;
            rightLeg = rLeg;
            leftShin = lShin;
            rightShin = rShin;
            rightHand = rHand;

            CacheDefaultRotations();
        }

        public void TriggerCatchSwing(float duration = 0.55f)
        {
            isSwinging = true;
            swingTimer = 0f;
            swingDuration = duration;

            if (unityAnimator != null && unityAnimator.runtimeAnimatorController != null)
            {
                unityAnimator.SetTrigger("Catch");
            }
        }

        private void Update()
        {
            bool isMoving = movement != null && movement.IsMoving;
            bool isRunning = movement != null && movement.IsRunning;
            float speedRatio = movement != null ? movement.MovementSpeedRatio : 0f;

            // Sync with Unity Animator if available
            if (unityAnimator != null && unityAnimator.runtimeAnimatorController != null)
            {
                unityAnimator.SetFloat("Speed", speedRatio);
                unityAnimator.SetBool("IsMoving", isMoving);
                unityAnimator.SetBool("IsRunning", isRunning);
            }

            // Procedural articulation update
            if (isSwinging)
            {
                UpdateSwingAnimation();
            }
            else if (isMoving)
            {
                UpdateLocomotionAnimation(isRunning);
            }
            else
            {
                UpdateIdleAnimation();
            }
        }

        private void UpdateIdleAnimation()
        {
            float t = Time.time;
            float breath = Mathf.Sin(t * 2.2f);
            float sway = Mathf.Sin(t * 1.1f);

            // Subtle breathing and posture
            if (spine != null)
            {
                spine.localRotation = defSpineRot * Quaternion.Euler(breath * 1.8f, sway * 1.2f, 0f);
            }

            if (head != null)
            {
                head.localRotation = defHeadRot * Quaternion.Euler(-breath * 0.8f, sway * 2.5f, 0f);
            }

            // Left arm relaxed at side
            if (leftArm != null)
            {
                leftArm.localRotation = defLeftArmRot * Quaternion.Euler(breath * 2f, 0f, 4f);
            }

            // Right arm ready holding butterfly net in front
            if (rightArm != null)
            {
                rightArm.localRotation = defRightArmRot * Quaternion.Euler(-25f + breath * 1.5f, 15f, -12f);
            }

            // Legs relaxed standing
            if (leftLeg != null) leftLeg.localRotation = Quaternion.Slerp(leftLeg.localRotation, defLeftLegRot, Time.deltaTime * 6f);
            if (rightLeg != null) rightLeg.localRotation = Quaternion.Slerp(rightLeg.localRotation, defRightLegRot, Time.deltaTime * 6f);
            if (leftShin != null) leftShin.localRotation = Quaternion.identity;
            if (rightShin != null) rightShin.localRotation = Quaternion.identity;
        }

        private void UpdateLocomotionAnimation(bool isRunning)
        {
            float strideRate = isRunning ? runStrideRate : walkStrideRate;
            walkCycle += Time.deltaTime * strideRate;

            float legAngle = Mathf.Sin(walkCycle) * (isRunning ? legStrideAngle * 1.25f : legStrideAngle);
            float armAngle = Mathf.Sin(walkCycle) * (isRunning ? armSwingAngle * 1.3f : armSwingAngle);
            float bob = Mathf.Abs(Mathf.Sin(walkCycle)) * (isRunning ? 0.08f : 0.04f);

            // Pelvis bounce
            if (hips != null)
            {
                hips.localPosition = new Vector3(0f, -bob, 0f);
                hips.localRotation = defHipsRot * Quaternion.Euler(0f, Mathf.Sin(walkCycle) * 3f, 0f);
            }

            // Spine leans forward when running
            if (spine != null)
            {
                float lean = isRunning ? 14f : 5f;
                spine.localRotation = defSpineRot * Quaternion.Euler(lean, -Mathf.Sin(walkCycle) * 4f, 0f);
            }

            // Alternating legs
            if (leftLeg != null)
            {
                leftLeg.localRotation = defLeftLegRot * Quaternion.Euler(legAngle, 0f, 0f);
            }

            if (rightLeg != null)
            {
                rightLeg.localRotation = defRightLegRot * Quaternion.Euler(-legAngle, 0f, 0f);
            }

            // Shin flexion on backward stroke
            if (leftShin != null)
            {
                float flexL = Mathf.Clamp(legAngle * 0.9f, 0f, 45f);
                leftShin.localRotation = Quaternion.Euler(flexL, 0f, 0f);
            }

            if (rightShin != null)
            {
                float flexR = Mathf.Clamp(-legAngle * 0.9f, 0f, 45f);
                rightShin.localRotation = Quaternion.Euler(flexR, 0f, 0f);
            }

            // Left arm counter swing
            if (leftArm != null)
            {
                leftArm.localRotation = defLeftArmRot * Quaternion.Euler(-armAngle, 0f, 5f);
            }

            // Right arm holds net ready while maintaining natural swing
            if (rightArm != null)
            {
                float netSwingMod = armAngle * 0.45f;
                rightArm.localRotation = defRightArmRot * Quaternion.Euler(-30f + netSwingMod, 12f, -10f);
            }
        }

        /// <summary>
        /// Executes an energetic, realistic scoop swing with the butterfly net
        /// </summary>
        private void UpdateSwingAnimation()
        {
            swingTimer += Time.deltaTime;
            float progress = swingTimer / swingDuration;

            if (progress >= 1f)
            {
                isSwinging = false;
                return;
            }

            // Phase 1 (0 to 0.25): Wind up - raise net and twist back
            // Phase 2 (0.25 to 0.65): Swift downward/forward scoop arc
            // Phase 3 (0.65 to 1.0): Follow-through and recovery
            float armPitch = 0f;
            float armYaw = 0f;
            float spineTwist = 0f;

            if (progress < 0.25f)
            {
                float t = progress / 0.25f;
                armPitch = Mathf.Lerp(-25f, -85f, t);
                armYaw = Mathf.Lerp(15f, -25f, t);
                spineTwist = Mathf.Lerp(0f, -18f, t);
            }
            else if (progress < 0.65f)
            {
                float t = (progress - 0.25f) / 0.40f;
                // Fast powerful scoop down and forward
                armPitch = Mathf.Lerp(-85f, 45f, Mathf.SmoothStep(0f, 1f, t));
                armYaw = Mathf.Lerp(-25f, 35f, Mathf.SmoothStep(0f, 1f, t));
                spineTwist = Mathf.Lerp(-18f, 22f, Mathf.SmoothStep(0f, 1f, t));
            }
            else
            {
                float t = (progress - 0.65f) / 0.35f;
                // Recover to ready stance
                armPitch = Mathf.Lerp(45f, -25f, t);
                armYaw = Mathf.Lerp(35f, 15f, t);
                spineTwist = Mathf.Lerp(22f, 0f, t);
            }

            if (spine != null)
            {
                spine.localRotation = defSpineRot * Quaternion.Euler(10f, spineTwist, 0f);
            }

            if (rightArm != null)
            {
                rightArm.localRotation = defRightArmRot * Quaternion.Euler(armPitch, armYaw, -15f);
            }
        }
    }
}
