shader_type canvas_item;

vec2 GetGradient(vec2 intPos, float t) {
    float rand = fract(sin(dot(intPos, vec2(12.9898, 78.233))) * 43758.5453);;
    float angle = 6.283185 * rand + 4.0 * t * rand;
    return vec2(cos(angle), sin(angle));
}

float Pseudo3dNoise(vec3 pos) {
    vec2 i = floor(pos.xy);
    vec2 f = pos.xy - i;
    vec2 blend = f * f * (3.0 - 2.0 * f);
    float noiseVal = 
        mix(
            mix(
                dot(GetGradient(i + vec2(0, 0), pos.z), f - vec2(0, 0)),
                dot(GetGradient(i + vec2(1, 0), pos.z), f - vec2(1, 0)),
                blend.x),
            mix(
                dot(GetGradient(i + vec2(0, 1), pos.z), f - vec2(0, 1)),
                dot(GetGradient(i + vec2(1, 1), pos.z), f - vec2(1, 1)),
                blend.x),
        blend.y
    );
    return noiseVal / 0.7; // normalize to about [-1..1]
}

vec2 DistortUV(vec2 SPS, vec2 uv, float strength, float speed, vec2 displacement){
	float ratio = SPS.x / SPS.y;
	vec2 noise_uv = uv / vec2(ratio, 1.0);
    float noiseVal = 0.5 + 0.5 * Pseudo3dNoise(vec3(noise_uv * 10.0 * strength, TIME * speed));
    float xNoiseMul = 1.0 - abs((uv.x - 0.5) / 0.5);
	float yNoiseMul = 1.0 - abs((uv.y - 0.5) / 0.5);
	uv.x += (noiseVal * displacement.x - displacement.x * 0.5) * xNoiseMul;
    uv.y += (noiseVal * displacement.y - displacement.y * 0.5) * yNoiseMul;
	return uv;
}

uniform sampler2D input_texture;
uniform float strength = 1.0;
uniform float speed = 1.0;
uniform vec2 displacement = vec2(1.0, 1.0);

void fragment() { 
	vec2 uv = UV;
	
	if(strength > 0.0){
		uv = DistortUV(SCREEN_PIXEL_SIZE, UV, strength, speed, displacement);
	}
	
	COLOR = texture(input_texture, uv);
}