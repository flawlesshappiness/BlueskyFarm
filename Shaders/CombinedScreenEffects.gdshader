shader_type canvas_item;

/////////////
// DISTORT //
/////////////

#define iResolution 1.0/SCREEN_PIXEL_SIZE
#define iTime TIME

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;
uniform sampler2D iChannel0;
uniform float strength = 1.0;
uniform float speed = 1.0;
uniform vec2 disp = vec2(1.0, 1.0);


vec2 GetGradient(vec2 intPos, float t) {
    
    // Uncomment for calculated rand
    //float rand = fract(sin(dot(intPos, vec2(12.9898, 78.233))) * 43758.5453);;
    
    // Texture-based rand (a bit faster on my GPU)
    float rand = texture(iChannel0, intPos / 64.0).r;
    
    // Rotate gradient: random starting rotation, random rotation rate
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
    float noiseVal = 0.5 + 0.5 * Pseudo3dNoise(vec3(noise_uv * 10.0 * strength, iTime * speed));
    
    float noiseWeight = 0.05;
    uv.x += noiseVal * noiseWeight * disp.x - disp.x * 0.025;
    uv.y += noiseVal * noiseWeight * disp.y - disp.y * 0.025;
	return uv;
}

///////////////////
// GAUSSIAN BLUR //
///////////////////

// Xor's gausian blur function 
// Link: https://xorshaders.weebly.com/tutorials/blur-shaders-5-part-2
// Defaults from: https://www.shadertoy.com/view/Xltfzj
//
// BLUR BLURRINESS (Default 8.0)
// BLUR ITERATIONS (Default 16.0 - More is better but slower)
// BLUR QUALITY (Default 4.0 - More is better but slower)
//
// Desc.: Don't have the best performance but will run on almost
// anything, although, if developing for mobile, is better to use 
// 'texture_nodevgaussian(...) instead'.
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

// Experience-Monks' fast gaussian blur function
// Link: https://github.com/Experience-Monks/glsl-fast-gaussian-blur/
//
// BLUR ITERATIONS (Default 16.0 - More is better but slower)
// BLUR DIRECTION (Direction in which the blur is apllied, use vec2(1, 0) for first pass and vec2(0, 1) for second pass)
//
// Desc.: ACTUALLY PRETTY SLOW but still pretty good for custom cinematic
// bloom effects, since this needs render 2 passes 
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

// u/_NoDev_'s gaussian blur function
// Discussion Link: https://www.reddit.com/r/godot/comments/klgfo9/help_with_shaders_in_gles2/
// Code Link: https://postimg.cc/7JDJw80d
//
// BLUR BLURRINESS (Default 8.0 - More is better but slower)
// BLUR RADIUS (Default 1.5)
// BLUR DIRECTION (Direction in which the blur is apllied, use vec2(1, 0) for first pass and vec2(0, 1) for second pass)
//
// Desc.: Really fast and GOOD FOR MOST CASES, but might NOT RUN IN THE WEB!
// use 'texture_xorgaussian' instead if you found any issues.
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

//  -- EXAMPLE CODE -- //
uniform int blur_type = 0;
uniform int blur_amount = 16;
uniform float blur_radius = 1;
uniform vec2 blur_direction = vec2(1, 1);

vec4 GaussianBlur(vec2 SPS, vec2 uv){
	if (blur_type == 0) 
	{
		vec4 xorgaussian = texture_xorgaussian(screen_texture, uv, SPS, float(blur_amount), 16, 4);
		return xorgaussian;
	} 
	else if (blur_type == 1) 
	{
		vec4 monksgaussian_multipass = texture_monksgaussian_multipass(screen_texture, uv, SPS, blur_amount, blur_direction);
		return monksgaussian_multipass;
	} 
	else if (blur_type == 2) 
	{
		vec4 nodevgaussian_singlepass = texture_nodevgaussian_singlepass(screen_texture, uv, SPS, float(blur_amount), blur_radius);
		return nodevgaussian_singlepass;
	} 
	else if (blur_type == 3) 
	{
		vec4 nodevgaussian_multipass = texture_nodevgaussian_multipass(screen_texture, uv, SPS, float(blur_amount), blur_direction);
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

uniform vec2 blur_center = vec2( 0.5, 0.5 );
uniform float blur_power : hint_range( 0.0, 1.0 ) = 0.01;
uniform int sampling_count : hint_range( 1, 64 ) = 6;

vec3 RadialBlur(vec2 uv)
{
	vec2 direction = uv - blur_center;
	vec3 c = vec3( 0.0, 0.0, 0.0 );
	float f = 1.0 / float( sampling_count );
	for( int i=0; i < sampling_count; i++ ) {
		c += texture(screen_texture, uv - blur_power * direction * float(i) ).rgb * f;
	}
	return c;
}

//////////////
// FRAGMENT //
//////////////

void fragment() { 
	vec2 uv = DistortUV(SCREEN_PIXEL_SIZE, UV);
	vec3 col = GaussianBlur(SCREEN_PIXEL_SIZE, uv).rgb;
	col = RadialBlur(uv);
    COLOR.rgb = col;
}