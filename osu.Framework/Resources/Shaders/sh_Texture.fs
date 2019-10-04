#include "sh_Utils.h"

in lowp vec4 v_Colour;
in mediump vec2 v_TexCoord;

out lowp vec4 f_Colour;

uniform lowp sampler2D m_Sampler;

void main(void)
{
	f_Colour = toSRGB(v_Colour * texture(m_Sampler, v_TexCoord, -0.9));
}
