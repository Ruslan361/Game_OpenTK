using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Simple3DGame.Core.ECS.Components;
using Simple3DGame.Rendering;

namespace Simple3DGame.Core.ECS.Systems
{
    public class RenderSystem : ISystem
    {
        private readonly Camera _camera;
        private List<(TransformComponent transform, LightComponent light)> _pointLightsData = new();
        private List<(TransformComponent transform, LightComponent light)> _spotLightsData = new();
        private (Vector3 direction, LightComponent light)? _directionalLightData = null;
        private const int MAX_POINT_LIGHTS = 4;
        private const int MAX_SPOT_LIGHTS = 1;
        
        // Keep track of which shaders support lighting and which don't
        private HashSet<string> _lightingEnabledShaders = new HashSet<string>();
        private HashSet<string> _lightingDisabledShaders = new HashSet<string>();

        public RenderSystem(Camera camera)
        {
            _camera = camera;
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
        }

        public void Update(World world, float deltaTime, KeyboardState? keyboardState = null, MouseState? mouseState = null)
        {
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            CacheLights(world);

            // Получаем сущности с компонентами Transform и Render
            var renderableEntities = world.GetEntitiesWithComponents(typeof(TransformComponent), typeof(RenderComponent));

            foreach (var entity in renderableEntities)
            {
                // Получаем компоненты
                if (!world.TryGetComponent<TransformComponent>(entity, out var transform))
                {
                    continue;
                }

                var render = world.GetComponent<RenderComponent>(entity);
                if (render == null || render.Model == null || render.Shader == null)
                {
                    continue;
                }

                // Настраиваем шейдер и рендерим модель
                render.Shader.Use();
                render.Shader.SetMatrix4("view", _camera.GetViewMatrix());
                render.Shader.SetMatrix4("projection", _camera.GetProjectionMatrix());
                
                // Check if this shader supports lighting
                bool supportsLighting = SupportsLighting(render.Shader);
                
                if (supportsLighting)
                {
                    // Only set lighting-related uniforms for shaders that support them
                    render.Shader.SetVector3("viewPos", _camera.Position);
                    render.Shader.SetInt("material.diffuse", 0);
                    render.Shader.SetInt("material.specular", 1);
                    render.Shader.SetFloat("material.shininess", render.Shininess);
                    
                    SetLightUniforms(render.Shader);
                }

                render.Shader.SetMatrix4("model", transform.GetModelMatrix());
                render.Model.Render();
            }

            GL.UseProgram(0);
        }
        
        private bool SupportsLighting(Shader shader)
        {
            string shaderPath = shader.GetFilePath();
            
            // First check our cache to avoid redundant GL calls
            if (_lightingEnabledShaders.Contains(shaderPath))
            {
                return true;
            }
            
            // Also check if we already know this shader doesn't support lighting
            if (_lightingDisabledShaders.Contains(shaderPath))
            {
                return false;
            }
            
            // Check if this is the lighting fragment shader by looking for a key uniform
            int location = GL.GetUniformLocation(shader.GetHandle(), "material.diffuse");
            
            if (location != -1)
            {
                // This shader has the material.diffuse uniform, so it supports lighting
                _lightingEnabledShaders.Add(shaderPath);
                return true;
            }
            else
            {
                // This shader does not have lighting uniforms, cache that too
                _lightingDisabledShaders.Add(shaderPath);
                return false;
            }
        }

        private void CacheLights(World world)
        {
            _pointLightsData.Clear();
            _spotLightsData.Clear();
            _directionalLightData = null;

            var lightEntities = world.GetEntitiesWithComponents(typeof(LightComponent));

            foreach (var entity in lightEntities)
            {
                if (!world.TryGetComponent<LightComponent>(entity, out var light))
                {
                    continue;
                }

                bool hasTransform = world.TryGetComponent<TransformComponent>(entity, out var transform);

                switch (light.Type)
                {
                    case LightType.Point:
                        if (hasTransform)
                        {
                            _pointLightsData.Add((transform, light));
                        }
                        break;
                    case LightType.Spot:
                        if (hasTransform)
                        {
                            _spotLightsData.Add((transform, light));
                        }
                        break;
                    case LightType.Directional:
                        if (!_directionalLightData.HasValue)
                        {
                            _directionalLightData = (light.Direction, light);
                        }
                        break;
                }
            }
        }

        private void SetLightUniforms(Shader shader)
        {
            // Directional Light
            if (_directionalLightData.HasValue)
            {
                var (dir, light) = _directionalLightData.Value;
                shader.SetInt("useDirLight", 1);
                shader.SetVector3("dirLight.direction", dir);
                shader.SetVector3("dirLight.ambient", light.Ambient);
                shader.SetVector3("dirLight.diffuse", light.Diffuse);
                shader.SetVector3("dirLight.specular", light.Specular);
            }
            else
            {
                shader.SetInt("useDirLight", 0);
                // Don't set other directional light uniforms when not using directional light
            }

            // Point Lights
            int pointLightCount = Math.Min(_pointLightsData.Count, MAX_POINT_LIGHTS);
            shader.SetInt("numPointLights", pointLightCount);
            
            // Only set point light uniforms if we have at least one
            if (pointLightCount > 0)
            {
                for (int i = 0; i < pointLightCount; i++)
                {
                    var (transform, light) = _pointLightsData[i];
                    string prefix = $"pointLights[{i}]";
                    shader.SetVector3($"{prefix}.position", transform.Position);
                    shader.SetVector3($"{prefix}.ambient", light.Ambient);
                    shader.SetVector3($"{prefix}.diffuse", light.Diffuse);
                    shader.SetVector3($"{prefix}.specular", light.Specular);
                    shader.SetFloat($"{prefix}.constant", light.Constant);
                    shader.SetFloat($"{prefix}.linear", light.Linear);
                    shader.SetFloat($"{prefix}.quadratic", light.Quadratic);
                }
                
                // Zero out unused array elements only if we're using point lights at all
                for (int i = pointLightCount; i < MAX_POINT_LIGHTS; i++)
                {
                    string prefix = $"pointLights[{i}]";
                    shader.SetVector3($"{prefix}.ambient", Vector3.Zero);
                    shader.SetVector3($"{prefix}.diffuse", Vector3.Zero);
                    shader.SetVector3($"{prefix}.specular", Vector3.Zero);
                }
            }

            // Camera Spot Light (фонарик камеры)
            bool cameraSpotlightFound = false;
            foreach(var (transform, light) in _spotLightsData)
            {
                // Для простоты предполагаем, что первый прожектор - это прожектор камеры
                if (light.Type == LightType.Spot)
                {
                    shader.SetInt("useCameraSpotLight", 1);
                    shader.SetVector3("spotLight.position", _camera.Position);
                    shader.SetVector3("spotLight.direction", _camera.Front);
                    shader.SetVector3("spotLight.ambient", light.Ambient);
                    shader.SetVector3("spotLight.diffuse", light.Diffuse);
                    shader.SetVector3("spotLight.specular", light.Specular);
                    shader.SetFloat("spotLight.constant", light.Constant);
                    shader.SetFloat("spotLight.linear", light.Linear);
                    shader.SetFloat("spotLight.quadratic", light.Quadratic);
                    shader.SetFloat("spotLight.cutOff", light.CutOffAngleCosine);
                    shader.SetFloat("spotLight.outerCutOff", light.OuterCutOffAngleCosine);
                    cameraSpotlightFound = true;
                    break;
                }
            }
            if (!cameraSpotlightFound)
            {
                shader.SetInt("useCameraSpotLight", 0);
                // Don't set other spotlight uniforms when not using spotlight
            }
        }
    }
}
