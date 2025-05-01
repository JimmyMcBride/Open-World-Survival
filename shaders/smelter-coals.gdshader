shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_disabled, diffuse_toon, specular_toon;

uniform vec4 albedo : source_color;
uniform sampler2D color_gradient;
uniform sampler2D fresnel_gradient;
uniform float emission_strength : hint_range(0.0, 10.0) = 1.0;
uniform vec3 emission_color : source_color = vec3(1.0, 0.3, 0.0);

float fresnel(float amount, vec3 normal, vec3 view) {
	return pow((1.0 - clamp(dot(normalize(normal), normalize(view)), 0.0, 1.0)), (amount / 0.25));
}

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	float basic_fresnel = fresnel(4.0, NORMAL, VIEW);
	EMISSION = emission_color * emission_strength;
	// Get a lighter version of the albedo color
    vec3 lightAlbedoColor = albedo.rgb * 3.0; // Adjust the multiplier to make it lighter or darker
    // Combine the albedo color with the fresnel effect
    ALBEDO = albedo.rgb + (basic_fresnel * lightAlbedoColor * texture(fresnel_gradient, vec2(basic_fresnel, 0.0)).rgb);
}

void light() {
	// Called for every pixel for every light affecting the material.
	float dotproduct = clamp(dot(NORMAL, LIGHT), -0.9, 0.9);
	float sample = clamp((dotproduct + 1.0) * ATTENUATION / 2.1, 0.05, 0.95);
	vec4 shaded = texture(color_gradient, vec2(sample, 0.0));
	DIFFUSE_LIGHT += clamp((shaded.rgb * LIGHT_COLOR), 0.0, 1.0);
}
