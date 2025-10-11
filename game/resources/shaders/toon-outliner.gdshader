shader_type spatial;
render_mode unshaded;

uniform sampler2D DEPTH_TEXTURE : hint_depth_texture, filter_nearest;
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_nearest;
uniform sampler2D NORMAL_TEXTURE : hint_normal_roughness_texture, filter_nearest;

uniform bool shadows_enabled = true;
uniform bool highlights_enabled = true;
uniform float shadow_strength : hint_range(0.0, 1.0, 0.01) = 0.4;     // how much to darken
uniform float highlight_strength : hint_range(0.0, 1.0, 0.01) = 0.1;  // how much to lighten

uniform bool is_perspective_camera = true;

varying mat4 model_view_matrix;

float getDepth(vec2 screen_uv, sampler2D depth_texture, mat4 inv_projection_matrix) {
    float raw_depth = texture(depth_texture, screen_uv).r;
    vec3 ndc = vec3(screen_uv * 2.0 - 1.0, raw_depth);
    vec4 view_space = inv_projection_matrix * vec4(ndc, 1.0);
    view_space.xyz /= view_space.w;
    return -view_space.z;
}

float normalIndicator(vec3 normalEdgeBias, vec3 baseNormal, vec3 newNormal, float depth_diff) {
    float normalDiff = dot(baseNormal - newNormal, normalEdgeBias);
    float normalIndicator = clamp(smoothstep(-0.01, 0.01, normalDiff), 0.0, 1.0);
    float depthIndicator = clamp(sign(depth_diff * 0.25 + 0.0025), 0.0, 1.0);
    return (1.0 - dot(baseNormal, newNormal)) * depthIndicator * normalIndicator;
}

void vertex() {
    model_view_matrix = VIEW_MATRIX * mat4(INV_VIEW_MATRIX[0], INV_VIEW_MATRIX[1], INV_VIEW_MATRIX[2], MODEL_MATRIX[3]);
    POSITION = vec4(VERTEX.xy, 1.0, 1.0);
}

// --- Color utilities: lighten/darken the sampled screen color ---
vec3 lighten(vec3 c, float amt) {
    // lerp toward white
    return clamp(mix(c, vec3(1.0), amt), 0.0, 1.0);
}
vec3 darken(vec3 c, float amt) {
    // lerp toward black
    return clamp(mix(c, vec3(0.0), amt), 0.0, 1.0);
}

void fragment() {
    vec2 e = vec2(1.0 / VIEWPORT_SIZE.xy);

    // Shadows (depth-based)
    float depth_diff = 0.0;
    float neg_depth_diff = 0.5;
    if (shadows_enabled) {
        float depth = getDepth(SCREEN_UV, DEPTH_TEXTURE, INV_PROJECTION_MATRIX);
        float du = getDepth(SCREEN_UV + vec2(0.0, -1.0) * e, DEPTH_TEXTURE, INV_PROJECTION_MATRIX);
        float dr = getDepth(SCREEN_UV + vec2(1.0, 0.0) * e, DEPTH_TEXTURE, INV_PROJECTION_MATRIX);
        float dd = getDepth(SCREEN_UV + vec2(0.0, 1.0) * e, DEPTH_TEXTURE, INV_PROJECTION_MATRIX);
        float dl = getDepth(SCREEN_UV + vec2(-1.0, 0.0) * e, DEPTH_TEXTURE, INV_PROJECTION_MATRIX);

        float clamp_min = is_perspective_camera ? -1.0 : 0.0;
        depth_diff += clamp(du - depth, clamp_min, 1.0);
        depth_diff += clamp(dd - depth, clamp_min, 1.0);
        depth_diff += clamp(dr - depth, clamp_min, 1.0);
        depth_diff += clamp(dl - depth, clamp_min, 1.0);

        neg_depth_diff += depth - du;
        neg_depth_diff += depth - dd;
        neg_depth_diff += depth - dr;
        neg_depth_diff += depth - dl;

        neg_depth_diff = clamp(neg_depth_diff, 0.0, 1.0);
        neg_depth_diff = clamp(smoothstep(0.5, 0.5, neg_depth_diff) * 10.0, 0.0, 1.0);
        depth_diff = smoothstep(0.2, 0.3, depth_diff);
    }

    // Highlights (normal-based)
    float normal_diff = 0.0;
    if (highlights_enabled) {
        vec3 normal = texture(NORMAL_TEXTURE, SCREEN_UV).rgb * 2.0 - 1.0;
        vec3 nu = texture(NORMAL_TEXTURE, SCREEN_UV + vec2(0.0, -1.0) * e).rgb * 2.0 - 1.0;
        vec3 nr = texture(NORMAL_TEXTURE, SCREEN_UV + vec2(1.0, 0.0) * e).rgb * 2.0 - 1.0;
        vec3 nd = texture(NORMAL_TEXTURE, SCREEN_UV + vec2(0.0, 1.0) * e).rgb * 2.0 - 1.0;
        vec3 nl = texture(NORMAL_TEXTURE, SCREEN_UV + vec2(-1.0, 0.0) * e).rgb * 2.0 - 1.0;

        vec3 normal_edge_bias = vec3(1.0);
        normal_diff += normalIndicator(normal_edge_bias, normal, nu, depth_diff);
        normal_diff += normalIndicator(normal_edge_bias, normal, nr, depth_diff);
        normal_diff += normalIndicator(normal_edge_bias, normal, nd, depth_diff);
        normal_diff += normalIndicator(normal_edge_bias, normal, nl, depth_diff);

        normal_diff = smoothstep(0.2, 0.8, normal_diff);
        normal_diff = clamp(normal_diff - neg_depth_diff, 0.0, 1.0);
    }

    // Base color from screen
    vec3 original_color = texture(SCREEN_TEXTURE, SCREEN_UV).rgb;

    // Derived outline colors from the pixel itself
    vec3 derived_highlight = lighten(original_color, highlight_strength);
    vec3 derived_shadow   = darken(original_color, shadow_strength);

    // Compose
    vec3 final = original_color;
    if (highlights_enabled) {
        final = mix(final, derived_highlight, normal_diff);
    }
    if (shadows_enabled) {
        final = mix(final, derived_shadow, depth_diff);
    }
    ALBEDO = final;

    float alpha_mask = depth_diff * float(shadows_enabled) + normal_diff * float(highlights_enabled);
    ALPHA = clamp(alpha_mask * 5.0, 0.0, 1.0);
}
