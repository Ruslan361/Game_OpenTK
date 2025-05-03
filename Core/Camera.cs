using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using Microsoft.Extensions.Logging;

namespace Simple3DGame.Core
{
    public class Camera
    {
        private Vector3 _position;
        private Vector3 _front = -Vector3.UnitZ;
        private Vector3 _up = Vector3.UnitY;
        private Vector3 _right = Vector3.UnitX;
        private Vector3 _worldUp;

        private float _yaw = -MathHelper.PiOver2; // Initialize facing -Z
        private float _pitch;

        private float _movementSpeed;
        private float _mouseSensitivity;
        private float _zoom;
        private float _aspectRatio;

        private ILogger<Camera> _logger;

        // Public properties with setters for Game.cs
        public Vector3 Position { get; set; }
        public Vector3 Front => _front;
        public Vector3 Up => _up;
        public Vector3 Right => _right;
        public float AspectRatio { get; set; }
        public float Yaw => _yaw;
        public float Pitch => _pitch;

        // Configurable movement/sensitivity
        public float MovementSpeed { get; set; } = 2.5f;
        public float MouseSensitivity { get; set; } = 0.025f; // Reduced from 0.1f to 0.025f for smoother camera rotation
        public float ZoomSensitivity { get; set; } = 1.0f;
        public float MinFov { get; set; } = MathHelper.DegreesToRadians(1.0f); // Using 1.0 degree as minimum to avoid zero
        public float MaxFov { get; set; } = MathHelper.DegreesToRadians(90.0f); // Adjust max FOV

        // Correct constructor signature
        public Camera(Vector3 position, float aspectRatio, ILogger<Camera> logger)
        {
            Position = position;
            AspectRatio = aspectRatio;
            _logger = logger;
            _zoom = MathHelper.DegreesToRadians(45.0f); // Initialize with a default 45-degree FOV
            UpdateCameraVectors(); // Initial calculation of vectors
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + _front, _up);
        }

        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(_zoom, AspectRatio, 0.1f, 100.0f);
        }

        public void ProcessKeyboard(CameraMovement direction, float deltaTime)
        {
            float velocity = MovementSpeed * deltaTime;
            if (direction == CameraMovement.Forward)
                Position += _front * velocity;
            if (direction == CameraMovement.Backward)
                Position -= _front * velocity;
            if (direction == CameraMovement.Left)
                Position -= _right * velocity;
            if (direction == CameraMovement.Right)
                Position += _right * velocity;
            // Optional: Add vertical movement (Up/Down)
            // if (direction == CameraMovement.Up)
            //     Position += _up * velocity;
            // if (direction == CameraMovement.Down)
            //     Position -= _up * velocity;
        }

        public void ProcessMouseMovement(float xOffset, float yOffset, bool constrainPitch = true)
        {
            xOffset *= MouseSensitivity;
            yOffset *= MouseSensitivity;

            _yaw += xOffset;
            _pitch -= yOffset; // Corrected: Subtract yoffset for standard pitch control

            // Constrain pitch
            if (constrainPitch)
            {
                _pitch = MathHelper.Clamp(_pitch, -MathHelper.DegreesToRadians(89.0f), MathHelper.DegreesToRadians(89.0f));
            }

            UpdateCameraVectors();
        }

        public void ProcessMouseScroll(float yOffset)
        {
            _zoom -= yOffset * ZoomSensitivity * MathHelper.DegreesToRadians(1.0f); // Adjust zoom sensitivity
            // Make sure _zoom is clamped to stay above zero and within min/max range
            _zoom = MathHelper.Clamp(_zoom, Math.Max(0.001f, MinFov), MaxFov);
        }

        private void UpdateCameraVectors()
        {
            // Calculate the new Front vector
            Vector3 front;
            front.X = MathF.Cos(_pitch) * MathF.Cos(_yaw);
            front.Y = MathF.Sin(_pitch);
            front.Z = MathF.Cos(_pitch) * MathF.Sin(_yaw);
            _front = Vector3.Normalize(front);

            // Recalculate the Right and Up vector
            _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, _front));
        }
    }

    public enum CameraMovement
    {
        Forward,
        Backward,
        Left,
        Right
    }
}