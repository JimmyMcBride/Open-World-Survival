shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_disabled, diffuse_toon, specular_toon;

uniform vec4 albedo : source_color;

uniform sampler2D color_gradient;
uniform sampler2D fresnel_gradient;

uniform vec3  rim_color    : source_color = vec3(1.0); // white by default
uniform float rim_power    = 4.0;   // higher = tighter rim
uniform float rim_strength = 0.8;   // overall intensity

uniform vec4 specular_color : source_color = vec4(1.0, 1.0, 1.0, 0.35);
uniform float specular_strength   = 1.0;   // overall brightness  (0-1)
uniform int   specular_steps      = 2;     // 1 = on/off, 2 = two bands
uniform float glossiness          = 64.0;  // higher = tighter highlight

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


void light() {
	// Called for every pixel for every light affecting the material.
	float dotproduct = clamp(dot(NORMAL, LIGHT), -0.9, 0.9);
	float sample = clamp((dotproduct + 1.0) * ATTENUATION / 2.1, 0.05, 0.95);
	vec4 shaded = texture(color_gradient, vec2(sample, 0.0));
	DIFFUSE_LIGHT += clamp((shaded.rgb * LIGHT_COLOR), 0.0, 1.0);

	    // ---------------- toon specular highlight --------------------------
    // 1. Phong term
    vec3  viewDir   = normalize(VIEW);
    vec3  reflDir   = reflect(-LIGHT, NORMAL);
    float spec      = pow(max(dot(viewDir, reflDir), 0.0), glossiness);

    // 2. Quantise to hard steps
    if (specular_steps > 1) {
        float stepSize = 1.0 / float(specular_steps);
        spec = stepSize * floor(spec / stepSize + 0.5);   // round to nearest band
    } else {
        spec = step(spec, 0.5);  // on/off if steps == 1
    }

    // 3. Apply attenuation & user strength
    spec *= ATTENUATION * specular_strength;

    // 4. Accumulate into specular channel
    SPECULAR_LIGHT += spec                     // phong term (0–1)
               * specular_color.rgb        // tint
               * specular_color.a          // “opacity”
               * LIGHT_COLOR;              // light’s own colour
}
