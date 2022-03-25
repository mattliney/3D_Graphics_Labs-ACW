#version 330 
 
uniform vec4 uLightPosition; 
uniform sampler2D uTextureSampler;
 
in vec4 oNormal; 
in vec4 oSurfacePosition; 
in vec2 oTexCoords; 

out vec4 FragColour; 
 
void main()  
{  
	 vec4 lightDir = normalize(uLightPosition - oSurfacePosition); 
	 float diffuseFactor = max(dot(oNormal, lightDir), 0); 

	 if(oTexCoords.xy == vec2(0,0))
	 {
	    FragColour = vec4(vec3(diffuseFactor), 1); 
	 }
	 else
	 {
		FragColour = texture(uTextureSampler, oTexCoords) * diffuseFactor; 
	 }
} 