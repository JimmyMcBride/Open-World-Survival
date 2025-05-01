shader_type spatial;

uniform sampler2D color_gradient;  // the GradientTexture1D

// Which scalar drives the ramp?
uniform bool use_viewspace = true;         // view-locked or world-locked
uniform vec3 ramp_axis = vec3(0.0, 1.0, 0.0); // y-up by default

void fragment() {
    // 1. Pick a scalar in the ­-1 … +1 range
    float t;
    if (use_viewspace) {
        // view-space Y component: tilts with the camera like a MatCap lite
        vec3 n_view = (INV_VIEW_MATRIX * vec4(NORMAL, 0.0)).xyz;
        t = n_view.y;
    } else {
        // world-space direction (static lighting)
        t = dot(NORMAL, normalize(ramp_axis));
    }

    // 2. Map ­-1…+1 → 0…1, then sample the gradient
    float u = t * 0.5 + 0.5;
    vec3 col = texture(color_gradient, vec2(u, 0.0)).rgb;

    ALBEDO = col;
}