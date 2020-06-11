#include "sh_Utils.h"
#include "sh_TextureWrapping.h"

in lowp vec4 v_Colour;
in mediump vec2 v_TexCoord;
in mediump vec4 v_TexRect;

out lowp vec4 f_Colour;

uniform lowp sampler2D m_Sampler;

void main(void)
{
	f_Colour = toSRGB(v_Colour * wrappedSampler(wrap(v_TexCoord, v_TexRect), v_TexRect, m_Sampler, -0.9));
}
