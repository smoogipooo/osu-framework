#ifdef GL_ES
    precision mediump float;
#endif

in vec4 v_Colour;
in vec2 v_TexCoord;

out lowp vec4 f_Colour;

uniform float g_Radius;
uniform vec4 g_TexRect;

vec2 max3(vec2 a, vec2 b, vec2 c)
{
	return max(max(a, b), c);
}

float distanceFromRoundedRect()
{
	vec2 topLeftOffset = g_TexRect.xy - v_TexCoord;
	vec2 bottomRightOffset = v_TexCoord - g_TexRect.zw;

	vec2 distanceFromShrunkRect = max3(vec2(0.0), bottomRightOffset + vec2(g_Radius), topLeftOffset + vec2(g_Radius));
	return length(distanceFromShrunkRect);
}

void main(void)
{
	float dist = g_Radius == 0.0 ? 0.0 : distanceFromRoundedRect();
	f_Colour = v_Colour;

	// This correction is needed to avoid fading of the alpha value for radii below 1px.
	float radiusCorrection = max(0.0, 1.0 - g_Radius);
	if (dist > g_Radius - 1.0 + radiusCorrection)
		f_Colour.a *= max(0.0, g_Radius - dist + radiusCorrection);
}
