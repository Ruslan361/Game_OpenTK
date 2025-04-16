#version 330 core
out vec4 FragColor;

in vec2 texCoord;
in vec3 Normal;
in vec3 FragPos;

uniform sampler2D texture0;

void main()
{

    vec3 baseColor = vec3(0.8, 0.2, 0.2);
    

    vec3 lightDir = normalize(vec3(1.0, 1.0, 1.0));
    vec3 norm = normalize(Normal);
    float diff = max(dot(norm, lightDir), 0.3); 
    
    vec3 result = baseColor * diff;
    FragColor = vec4(result, 1.0);
    
}
