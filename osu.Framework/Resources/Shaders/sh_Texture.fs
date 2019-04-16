#include "sh_Utils.h"

in vec4 v_Colour;
in vec2 v_TexCoord;

out vec4 f_Colour;

uniform sampler2D m_Sampler;

void main(void)
{
	f_Colour = toSRGB(v_Colour * texture(m_Sampler, v_TexCoord, -0.9));
}
