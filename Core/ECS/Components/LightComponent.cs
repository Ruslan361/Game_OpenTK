using OpenTK.Mathematics;
using System;

namespace Simple3DGame.Core.ECS.Components
{
    public enum LightType
    {
        Directional,
        Point,
        Spot
    }

    public struct LightComponent : IComponent
    {
        public LightType Type { get; private set; }
        
        // Общие свойства для всех типов света
        public Vector3 Ambient { get; private set; }
        public Vector3 Diffuse { get; private set; }
        public Vector3 Specular { get; private set; }
        
        // Свойства для DirectionalLight
        public Vector3 Direction { get; private set; }
        
        // Свойства для PointLight и SpotLight
        public float Constant { get; private set; }
        public float Linear { get; private set; }
        public float Quadratic { get; private set; }
        
        // Дополнительные свойства для SpotLight
        public float CutOffAngleCosine { get; private set; }
        public float OuterCutOffAngleCosine { get; private set; }

        // Создание направленного света
        public static LightComponent CreateDirectionalLight(
            Vector3 direction,
            Vector3? ambient = null,
            Vector3? diffuse = null,
            Vector3? specular = null)
        {
            return new LightComponent
            {
                Type = LightType.Directional,
                Direction = direction.Normalized(),
                Ambient = ambient ?? new Vector3(0.1f),
                Diffuse = diffuse ?? new Vector3(0.8f),
                Specular = specular ?? new Vector3(1.0f)
            };
        }

        // Создание точечного света
        public static LightComponent CreatePointLight(
            Vector3 position, // Позиция указывается для удобства, но не хранится в компоненте
            Vector3? ambient = null,
            Vector3? diffuse = null,
            Vector3? specular = null,
            float constant = 1.0f,
            float linear = 0.09f,
            float quadratic = 0.032f)
        {
            return new LightComponent
            {
                Type = LightType.Point,
                Ambient = ambient ?? new Vector3(0.1f),
                Diffuse = diffuse ?? new Vector3(0.8f),
                Specular = specular ?? new Vector3(1.0f),
                Constant = constant,
                Linear = linear,
                Quadratic = quadratic
            };
        }

        // Создание прожектора (например, для камеры)
        public static LightComponent CreateSpotLight(
            Vector3 position, // Позиция указывается для удобства, но не хранится в компоненте
            Vector3 direction,
            float cutOffDegrees = 12.5f,
            float outerCutOffDegrees = 17.5f,
            Vector3? ambient = null,
            Vector3? diffuse = null,
            Vector3? specular = null,
            float constant = 1.0f,
            float linear = 0.09f,
            float quadratic = 0.032f)
        {
            // Конвертируем из градусов в косинусы для эффективных вычислений в шейдере
            float cutOffCos = MathF.Cos(MathHelper.DegreesToRadians(cutOffDegrees));
            float outerCutOffCos = MathF.Cos(MathHelper.DegreesToRadians(outerCutOffDegrees));
            
            return new LightComponent
            {
                Type = LightType.Spot,
                Direction = direction.Normalized(),
                Ambient = ambient ?? new Vector3(0.1f),
                Diffuse = diffuse ?? new Vector3(0.8f),
                Specular = specular ?? new Vector3(1.0f),
                Constant = constant,
                Linear = linear,
                Quadratic = quadratic,
                CutOffAngleCosine = cutOffCos,
                OuterCutOffAngleCosine = outerCutOffCos
            };
        }
    }
}
