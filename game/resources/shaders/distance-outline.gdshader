shader_type spatial;
render_mode unshaded;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture;
uniform sampler2D DEPTH_TEXTURE  : hint_depth_texture;
uniform vec4  outline_color : source_color = vec4(0.0,0.0,0.0,1.0);
uniform float outline_width  = 2.0;
uniform float depth_threshold = 0.01;
uniform bool  constant_width = true;

float depth_to_linear(float z)
{
    // Back-project depth into clip space (-1 .. 1)
    float z_clip = z_ndc * 2.0 - 1.0;

    // Row 3 of the inverse projection matrix gives us:
    // view_z = invP[3].z / (z_clip + invP[2].z)
    // See “Reconstructing Position from Depth” (OpenGL convention)
    return  INV_PROJECTION_MATRIX[3].z
          / (z_clip + INV_PROJECTION_MATRIX[2].z);
}

void fragment()
{
    vec3  base_rgb  = texture(SCREEN_TEXTURE, SCREEN_UV).rgb;
    float center_z  = texture(DEPTH_TEXTURE,  SCREEN_UV).r;
    float center_d  = depth_to_linear(center_z);

    vec2 pixel = 1.0 / VIEWPORT_SIZE;
    bool edge  = false;

    for (int y = -int(outline_width); y <= int(outline_width) && !edge; ++y)
    for (int x = -int(outline_width); x <= int(outline_width) && !edge; ++x)
    {
        vec2 uv_off = SCREEN_UV + vec2(float(x), float(y)) * pixel;
        if (uv_off.x < 0.0 || uv_off.x > 1.0 || uv_off.y < 0.0 || uv_off.y > 1.0)
            continue;

        float sample_z = texture(DEPTH_TEXTURE, uv_off).r;
        if (sample_z >= 1.0) continue;           // skip skybox / far plane

        float sample_d = depth_to_linear(sample_z);
        if (abs(sample_d - center_d) > depth_threshold)
            edge = true;
    }

    ALBEDO = edge ? outline_color.rgb : base_rgb;
    ALPHA  = 1.0;
}