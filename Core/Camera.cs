using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using Microsoft.Extensions.Logging;

namespace Simple3DGame.Core
{
    public class Camera
    {
        private Vector3 _position;
        private Vector3 _front;
        private Vector3 _up;
        private Vector3 _right;
        private Vector3 _worldUp;

        private float _yaw;
        private float _pitch;

        private float _movementSpeed;
        private float _mouseSensitivity;
        private float _zoom;
        private float _aspectRatio;

        private ILogger<Camera> _logger;

        // Public properties with setters for Game.cs
        public Vector3 Position => _position;
        public Vector3 Front => _front;
        public float Fov 
        { 
            get => _zoom;
            set 
            { 
                _zoom = value;
                if (_zoom <= 1.0f) _zoom = 1.0f;
                if (_zoom >= 45.0f) _zoom = 45.0f;
            }
        }
        public float AspectRatio 
        { 
            get => _aspectRatio;
            set => _aspectRatio = value;
        }

        public Camera(Vector3 position, float aspectRatio, ILogger<Camera> logger)
            : this(position, Vector3.UnitY, -90.0f, 0.0f, logger)
        {
            _aspectRatio = aspectRatio;
        }

        public Camera(Vector3 position, Vector3 up, float yaw, float pitch, ILogger<Camera> logger)
        {
            _position = position;
            _worldUp = up;
            _yaw = yaw;
            _pitch = pitch;
            _front = Vector3.UnitZ;
            _movementSpeed = 2.5f;
            _mouseSensitivity = 0.1f;
            _zoom = 45.0f;
            _aspectRatio = 1.0f; // Default value, should be set correctly
            _logger = logger;

            UpdateCameraVectors();
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(_position, _position + _front, _up);
        }

        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(_zoom),
                _aspectRatio,
                0.1f,
                100.0f);
        }

        public void ProcessKeyboard(CameraMovement direction, float deltaTime)
        {
            float velocity = _movementSpeed * deltaTime;
            if (direction == CameraMovement.Forward)
                _position += _front * velocity;
            if (direction == CameraMovement.Backward)
                _position -= _front * velocity;
            if (direction == CameraMovement.Left)
                _position -= _right * velocity;
            if (direction == CameraMovement.Right)
                _position += _right * velocity;
        }

        public void ProcessMouseMovement(float xOffset, float yOffset, bool constrainPitch = true)
        {
            xOffset *= _mouseSensitivity;
            yOffset *= _mouseSensitivity;

            _yaw += xOffset;
            _pitch += yOffset;

            if (constrainPitch)
            {
                if (_pitch > 89.0f)
                    _pitch = 89.0f;
                if (_pitch < -89.0f)
                    _pitch = -89.0f;
            }

            UpdateCameraVectors();
        }

        public void ProcessMouseScroll(float yOffset)
        {
            if (_zoom >= 1.0f && _zoom <= 45.0f)
                _zoom -= yOffset;
            if (_zoom <= 1.0f)
                _zoom = 1.0f;
            if (_zoom >= 45.0f)
                _zoom = 45.0f;
        }

        public float GetZoom()
        {
            return _zoom;
        }

        private void UpdateCameraVectors()
        {
            Vector3 front;
            front.X = MathF.Cos(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));
            front.Y = MathF.Sin(MathHelper.DegreesToRadians(_pitch));
            front.Z = MathF.Sin(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));
            _front = Vector3.Normalize(front);
            _right = Vector3.Normalize(Vector3.Cross(_front, _worldUp));
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