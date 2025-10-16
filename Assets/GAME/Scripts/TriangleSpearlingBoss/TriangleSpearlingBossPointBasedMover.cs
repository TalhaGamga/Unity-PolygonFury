using R3;
using UnityEngine;

namespace TriangleSpearlingBoss
{
    [System.Serializable]
    public class TriangleBossPointBasedMover : IMover
    {
        public void Construct(MovementSystem.Context externalSources)
        {
        }

        public void End()
        {
        }

        public void HandleInput(InputSignal inputSignal)
        {
        }

        public void Init(Subject<Unit> transitionStream)
        {
        }

        public void Update()
        {
        }

        [System.Serializable]
        private class Context
        {
            public CharacterAction CurrentAction;

            [HideInInspector] public Rigidbody2D Rb;
            [HideInInspector] public Transform MoverTransform;
            [HideInInspector] public Transform OrientationTransform;
            [HideInInspector] public float HorizontalTargetVelocity;
            public float HorizontalCurrentVelocity;
            [HideInInspector] public int LastFaceX;
            public bool IsGrounded;

            public float HorizontalMaxSpeed;
            public float Acceleration;
            public Vector2 MoveInput;
            public LayerMask PlatformLayer;
            public float GroundCheckDistance;
            public float FaceDeadzone;
            public float InitialHeight;
            public float UpDownDistance;
            public float UpDownDuration;
        }
    }
}