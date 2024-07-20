shader_type canvas_item;

uniform int blur_type = 0;

/////////////
// DISTORT //
/////////////

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;
uniform float distort_strength = 1.0;
uniform float distort_speed = 1.0;
uniform vec2 distort_displacement = vec2(1.0, 1.0);

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

vec2 DistortUV(vec2 SPS, vec2 uv){
	float ratio = SPS.x / SPS.y;
	vec2 noise_uv = uv / vec2(ratio, 1.0);
    float noiseVal = 0.5 + 0.5 * Pseudo3dNoise(vec3(noise_uv * 10.0 * distort_strength, TIME * distort_speed));
    float xNoiseMul = 1.0 - abs((uv.x - 0.5) / 0.5);
	float yNoiseMul = 1.0 - abs((uv.y - 0.5) / 0.5);
	uv.x += (noiseVal * distort_displacement.x - distort_displacement.x * 0.5) * xNoiseMul;
    uv.y += (noiseVal * distort_displacement.y - distort_displacement.y * 0.5) * yNoiseMul;
	return uv;
}

///////////////////
// GAUSSIAN BLUR //
///////////////////

const int gaussian_blur_type = 0;
uniform float blur_amount = 16;
const float blur_radius = 1.0;
const vec2 blur_direction = vec2(1, 1);
uniform int blur_iterations = 16;
uniform int blur_quality = 4;

// BLUR BLURRINESS (Default 8.0)
// BLUR ITERATIONS (Default 16.0 - More is better but slower)
// BLUR QUALITY (Default 4.0 - More is better but slower)
vec4 texture_xorgaussian(sampler2D tex, vec2 uv, vec2 pixel_size, float blurriness, int iterations, int quality){
	float pi = 6.28;
	vec2 radius = blurriness / (1.0 / pixel_size).xy;
	vec4 blurred_tex = texture(tex, uv);
	
	for(float d = 0.0; d < pi; d += pi / float(iterations)){
		for( float i = 1.0 / float(quality); i <= 1.0; i += 1.0 / float(quality) ){
			vec2 directions = uv + vec2(cos(d), sin(d)) * radius * i;
			blurred_tex += texture(tex, directions);
		}
	}
	blurred_tex /= float(quality) * float(iterations) + 1.0;
	
	return blurred_tex;
}

// BLUR ITERATIONS (Default 16.0 - More is better but slower)
// BLUR DIRECTION (Direction in which the blur is apllied, use vec2(1, 0) for first pass and vec2(0, 1) for second pass)
vec4 texture_monksgaussian_multipass(sampler2D tex, vec2 uv, vec2 pixel_size, int iterations, vec2 direction){
	vec4 blurred_tex = vec4(0.0);
	vec2 resolution = 1.0 / pixel_size;
	
	for (int i=0; i < iterations; i++){
		float size = float(iterations - i);
		
		vec2 off1 = vec2(1.3846153846) * (direction * size);
		vec2 off2 = vec2(3.2307692308) * (direction * size);

		blurred_tex += texture(tex, uv) * 0.2270270270;
		blurred_tex += texture(tex, uv + (off1 / resolution)) * 0.3162162162;
		blurred_tex += texture(tex, uv - (off1 / resolution)) * 0.3162162162;
		blurred_tex += texture(tex, uv + (off2 / resolution)) * 0.0702702703;
		blurred_tex += texture(tex, uv - (off2 / resolution)) * 0.0702702703;
	}
	
	blurred_tex /= float(iterations) + 1.0;
	
	return blurred_tex;
}

// BLUR BLURRINESS (Default 8.0 - More is better but slower)
// BLUR RADIUS (Default 1.5)
// BLUR DIRECTION (Direction in which the blur is apllied, use vec2(1, 0) for first pass and vec2(0, 1) for second pass)
vec4 texture_nodevgaussian_singlepass(sampler2D tex, vec2 uv, vec2 pixel_size, float blurriness, float radius){
	float pi = 3.14;
	float n = 0.0015;
	
	vec4 blurred_tex = vec4(0);
	
	float weight;
	for (float i = -blurriness; i <= blurriness; i++){
		float d = i / pi;
		vec2 anchor = vec2(cos(d), sin(d)) * radius * i;
		vec2 directions = uv + pixel_size * anchor;
		blurred_tex += texture(tex, directions) * n;
		if (i <= 0.0) {n += 0.0015; }
		if (i > 0.0) {n -= 0.0015; }
		weight += n;
	}
	
	float norm = 1.0 / weight;
	blurred_tex *= norm;
	return blurred_tex;
}

vec4 texture_nodevgaussian_multipass(sampler2D tex, vec2 uv, vec2 pixel_size, float blurriness, vec2 direction){
	float n = 0.0015;
	
	vec4 blurred_tex = vec4(0);
	
	float weight;
	for (float i = -blurriness; i <= blurriness; i++){
		vec2 directions = uv + pixel_size * (direction * i);
		blurred_tex += texture(tex, directions) * n;
		if (i <= 0.0) {n += 0.0015; }
		if (i > 0.0) {n -= 0.0015; }
		weight += n;
	}
	
	float norm = 1.0 / weight;
	blurred_tex *= norm;
	return blurred_tex;
}

vec4 GaussianBlur(vec2 SPS, vec2 uv){
	if (gaussian_blur_type == 0) 
	{
		vec4 xorgaussian = texture_xorgaussian(screen_texture, uv, SPS, blur_amount, blur_iterations, blur_quality);
		return xorgaussian;
	} 
	else if (gaussian_blur_type == 1) 
	{
		vec4 monksgaussian_multipass = texture_monksgaussian_multipass(screen_texture, uv, SPS, int(blur_amount), blur_direction);
		return monksgaussian_multipass;
	} 
	else if (gaussian_blur_type == 2) 
	{
		vec4 nodevgaussian_singlepass = texture_nodevgaussian_singlepass(screen_texture, uv, SPS, blur_amount, blur_radius);
		return nodevgaussian_singlepass;
	} 
	else if (gaussian_blur_type == 3) 
	{
		vec4 nodevgaussian_multipass = texture_nodevgaussian_multipass(screen_texture, uv, SPS, blur_amount, blur_direction);
		return nodevgaussian_multipass;
	} 
	else 
	{
		return texture(screen_texture, uv);
	}
}

/////////////////
// RADIAL BLUR //
/////////////////

uniform vec2 radial_center = vec2( 0.5, 0.5 );
uniform float radial_strength : hint_range( 0.0, 1.0 ) = 0.01;
uniform int radial_sampling : hint_range( 1, 64 ) = 6;

vec3 RadialBlur(vec2 uv)
{
	vec2 direction = uv - radial_center;
	vec3 c = vec3( 0.0, 0.0, 0.0 );
	float f = 1.0 / float( radial_sampling );
	for( int i=0; i < radial_sampling; i++ ) {
		c += texture(screen_texture, uv - radial_strength * direction * float(i) ).rgb * f;
	}
	return c;
}

//////////////
// FRAGMENT //
//////////////

void fragment() { 
	vec2 uv = UV;
	
	if(distort_strength > 0.0){
		uv = DistortUV(SCREEN_PIXEL_SIZE, UV);
	}
	
	vec3 col = texture(screen_texture, uv).rgb;
	
	if(blur_type == 0 && blur_amount > 0.0){
		col = GaussianBlur(SCREEN_PIXEL_SIZE, uv).rgb;
	}
	else if(blur_type == 1 && radial_strength > 0.0){
		col = RadialBlur(uv);
	}
	
    COLOR.rgb = col;
}