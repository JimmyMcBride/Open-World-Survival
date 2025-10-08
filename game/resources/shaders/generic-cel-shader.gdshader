shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_disabled, diffuse_toon, specular_toon;

uniform vec4 albedo : source_color;

uniform sampler2D color_gradient;
uniform sampler2D fresnel_gradient;

uniform vec3  rim_color    : source_color = vec3(1.0); // white by default
uniform float rim_power    = 4.0;   // higher = tighter rim
uniform float rim_strength = 0.8;   // overall intensity

float fresnel(float amount, vec3 normal, vec3 view) {
	return pow((1.0 - clamp(dot(normalize(normal), normalize(view)), 0.0, 1.0)), (amount / 0.25));
}

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
    float basic_fresnel = fresnel(4.0, NORMAL, VIEW);

    // ------------- existing fresnel tint -------------
    vec3 lightAlbedoColor = albedo.rgb * 3.0;
    vec3 base_color = albedo.rgb
                    + basic_fresnel
                      * lightAlbedoColor
                      * texture(fresnel_gradient, vec2(basic_fresnel, 0.0)).rgb;

    // ------------- NEW: silhouette rim -------------
    float rim  = pow(1.0 - clamp(dot(normalize(NORMAL), normalize(VIEW)), 0.0, 1.0),
                     rim_power);          // 0…1
    vec3 rim_col = rim * rim_strength * rim_color;

    ALBEDO = clamp(base_color + rim_col, 0.0, 1.0);
}


//void light() {
	//// Called for every pixel for every light affecting the material.
	//float dotproduct = clamp(dot(NORMAL, LIGHT), -0.9, 0.9);
	//float sample = clamp((dotproduct + 1.0) * ATTENUATION / 2.1, 0.05, 0.95);
	//vec4 shaded = texture(color_gradient, vec2(sample, 0.0));
	//DIFFUSE_LIGHT += clamp((shaded.rgb * LIGHT_COLOR), 0.0, 1.0);
//}

void light() {
    // Calculate base Lambertian term
    float lambertian = max(dot(NORMAL, LIGHT), 0.0);

    // Add an ambient term to ensure a minimum brightness
    float ambient_strength = 0.2; // Adjust this value (0.0 to 1.0) to control ambient light
    float lighting = lambertian + ambient_strength;

    // Quantize the lighting for cel-shading effect
    float levels = 4.0;
    lighting = floor(lighting * levels) / levels;

    // Adjust the sample range to avoid overly dark areas and account for ambient
    float sample = clamp(lighting * ATTENUATION, 0.2, 1.0); // Raised minimum from 0.05 to 0.2
    vec4 shaded = texture(color_gradient, vec2(sample, 0.0));
    DIFFUSE_LIGHT += clamp((shaded.rgb * LIGHT_COLOR), 0.0, 1.0);
}
