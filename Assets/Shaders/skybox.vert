#version 330 core
layout (location = 0) in vec3 aPos;

out vec3 TexCoords;

uniform mat4 projection;
uniform mat4 view;

void main() {
    TexCoords = vec3(aPos.x, aPos.y, -aPos.z); 
    mat4 viewRotation = mat4(mat3(view)); 
    vec4 pos = projection * viewRotation * vec4(aPos, 1.0);
    gl_Position = pos.xyww;
}