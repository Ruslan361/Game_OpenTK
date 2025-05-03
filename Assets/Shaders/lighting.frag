#version 330 core
//In this tutorial it might seem like a lot is going on, but really we just combine the last tutorials, 3 pieces of source code into one
//and added 3 extra point lights.
struct Material {
    sampler2D diffuse;
    sampler2D specular;
    float     shininess;
};
//This is the directional light struct, where we only need the directions
struct DirLight {
    vec3 direction;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};
uniform DirLight dirLight;
//This is our pointlight where we need the position aswell as the constants defining the attenuation of the light.
struct PointLight {
    vec3 position;

    float constant;
    float linear;
    float quadratic;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};
//We have a total of 4 point lights now, so we define a preprossecor directive to tell the gpu the size of our point light array
#define NR_POINT_LIGHTS 4
uniform PointLight pointLights[NR_POINT_LIGHTS];
//This is our spotlight where we need the position, attenuation along with the cutoff and the outer cutoff. Plus the direction of the light
struct SpotLight{
    vec3  position;
    vec3  direction;
    float cutOff;
    float outerCutOff;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;

    float constant;
    float linear;
    float quadratic;
};
uniform SpotLight spotLight;

uniform Material material;
uniform vec3 viewPos;

// New uniforms
uniform int useDirLight; // 0 = false, 1 = true
uniform int numPointLights; // Number of active point lights (0 to MAX_POINT_LIGHTS)
uniform int useCameraSpotLight; // 0 = false, 1 = true

out vec4 FragColor;

in vec3 Normal;
in vec3 FragPos;
in vec2 TexCoords;

//Here we have some function prototypes, these are the signatures the gpu will use to know how the
//parameters of each light calculation is layed out.
//We have one function per light, since this makes it so we dont have to take up to much space in the main function.
vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir, vec3 texDiffuse, vec3 texSpecular);
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 texDiffuse, vec3 texSpecular);
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 texDiffuse, vec3 texSpecular);

void main()
{
    // Sample material textures
    vec3 texDiffuseColor = texture(material.diffuse, TexCoords).rgb;
    vec3 texSpecularColor = texture(material.specular, TexCoords).rgb;

    // Properties needed for lighting calculations
    vec3 norm = normalize(Normal);
    vec3 viewDir = normalize(viewPos - FragPos);

    // --- Calculate Lighting ---
    vec3 result = vec3(0.0); // Initialize total lighting

    // 1. Directional light contribution
    if (useDirLight > 0) {
        result += CalcDirLight(dirLight, norm, viewDir, texDiffuseColor, texSpecularColor);
    }

    // 2. Point light contributions
    for(int i = 0; i < numPointLights; i++) { // Loop up to the actual number active
        result += CalcPointLight(pointLights[i], norm, FragPos, viewDir, texDiffuseColor, texSpecularColor);
    }

    // 3. Camera spotlight contribution
    if (useCameraSpotLight > 0) {
        result += CalcSpotLight(spotLight, norm, FragPos, viewDir, texDiffuseColor, texSpecularColor);
    }

    // 4. Other spotlight contributions (if implemented)
    // for(int i = 0; i < numSpotLights; i++) {
    //     result += CalcSpotLight(spotLights[i], norm, FragPos, viewDir, texDiffuseColor, texSpecularColor);
    // }

    // If no lights are active, apply a minimal ambient term based on the diffuse texture
    // This prevents completely black objects when no lights are configured.
    if (useDirLight == 0 && numPointLights == 0 && useCameraSpotLight == 0 /* && numSpotLights == 0 */) {
         // Use a very small fraction of the directional light's ambient or a fixed small value
         // result = dirLight.ambient * texDiffuseColor; // Use dirLight's ambient if available
         result = vec3(0.05) * texDiffuseColor; // Or just a fixed minimal ambient
    }

    FragColor = vec4(result, 1.0);
    // FragColor = vec4(texDiffuseColor, 1.0); // Debug: Show diffuse texture
    // FragColor = vec4(norm * 0.5 + 0.5, 1.0); // Debug: Show normals
}

// --- Lighting Calculation Functions ---

vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir, vec3 texDiffuse, vec3 texSpecular)
{
    vec3 lightDir = normalize(-light.direction); // Direction TO the light

    // Diffuse
    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = light.diffuse * diff * texDiffuse;

    // Specular
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    vec3 specular = light.specular * spec * texSpecular; // Use specular map color

    // Ambient
    vec3 ambient = light.ambient * texDiffuse; // Ambient usually modulated by diffuse color

    return (ambient + diffuse + specular);
}

vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 texDiffuse, vec3 texSpecular)
{
    vec3 lightDir = normalize(light.position - fragPos); // Direction from frag to light

    // Diffuse
    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = light.diffuse * diff * texDiffuse;

    // Specular
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    vec3 specular = light.specular * spec * texSpecular;

    // Ambient
    vec3 ambient = light.ambient * texDiffuse;

    // Attenuation
    float distance = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));

    // Apply attenuation
    ambient *= attenuation;
    diffuse *= attenuation;
    specular *= attenuation;

    return (ambient + diffuse + specular);
}

vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 texDiffuse, vec3 texSpecular)
{
    vec3 lightDir = normalize(light.position - fragPos); // Direction from frag to light

    // Check if fragment is within the spotlight cone
    float theta = dot(lightDir, normalize(-light.direction)); // Angle between light dir and vector to frag
    float epsilon = light.cutOff - light.outerCutOff;       // Difference between inner and outer cosine values
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0); // Smooth falloff [0, 1]

    // If outside the outer cone, intensity is 0, so no contribution
    if (intensity <= 0.0) {
        return vec3(0.0);
    }

    // Diffuse
    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = light.diffuse * diff * texDiffuse;

    // Specular
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    vec3 specular = light.specular * spec * texSpecular;

    // Ambient
    vec3 ambient = light.ambient * texDiffuse;

    // Attenuation
    float distance = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));

    // Apply attenuation and spotlight intensity
    ambient *= attenuation * intensity;
    diffuse *= attenuation * intensity;
    specular *= attenuation * intensity;

    return (ambient + diffuse + specular);
}