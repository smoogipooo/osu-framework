#ifdef GL_ES
    precision mediump float;
#endif

in vec4 v_Colour;

out lowp vec4 f_Colour;

void main(void)
{
	f_Colour = v_Colour;
}