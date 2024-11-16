shader_type canvas_item;

float interpolate(float a, float b, float t){
	return a + t * (b - a);
}

float cosine_curve(float v, float b)
{
	float cosv = cos(v*PI-PI);
	float b2 = pow(b,2.0f);
	return sqrt((1.0f + b2) / (1.0f + b2 * pow(cosv,2.0f))) * cosv * 0.5f + 0.5f;
}

float squeeze_value(float v, float t){
	if(t < 0.0f){
		t = min(1.0f, abs(t));
		return v * (1.0f - t) + t * 0.5f;
	}
	else if(t < 1.0f){
		float cosv = cos(v*PI-PI) * 0.5f + 0.5f;
		return interpolate(v, cosv, t);
	}
	else {
		v = cosine_curve(v, t-1.0f);
		return interpolate(0.0f, 1.0f, v);
	}
}

uniform sampler2D input_texture;
uniform float squeeze_x = 0.0f;
uniform float squeeze_y = 0.0f;

void fragment() {
	vec2 uv = UV;
	float x = squeeze_value(uv.x, squeeze_x);
	float y = squeeze_value(uv.y, squeeze_y);
	uv = vec2(x, y);
	COLOR = texture(input_texture, uv);
}