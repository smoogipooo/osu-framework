#ifdef GL_ES
	precision mediump float;
#endif

#include "sh_Utils.h"

varying vec4 v_Colour;
varying vec2 v_TexCoord;

uniform sampler2D m_Sampler;
uniform bool g_ForDepth;

void main(void)
{
	gl_FragColor = toSRGB(v_Colour * texture2D(m_Sampler, v_TexCoord, -0.9));
	if (g_ForDepth && gl_FragColor.a < 1.0)
		discard;
}
