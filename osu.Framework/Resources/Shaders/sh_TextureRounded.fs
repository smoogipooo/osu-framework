#include "sh_Utils.h"
#include "sh_Masking.h"

out lowp vec4 f_Colour;

uniform lowp sampler2D m_Sampler;

void main(void) 
{
    f_Colour = getRoundedColor(toSRGB(texture2D(m_Sampler, v_TexCoord, -0.9)));
}
