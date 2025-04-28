#ifdef GL_ES
precision mediump float;
#endif
//Specifying the version like in our vertex shader.
//The input variables, again prefixed with an f as they are the input variables of our fragment shader.
//These have to share name for now even though there is a way around this later on.
uniform sampler2D tex0;
  
//The output of our fragment shader, this just has to be a vec3 or a vec4, containing the color information about
//each "fragment" or pixel of our geometry.
varying vec4 fColor;

void main()
{
    //Here we are setting our output variable, for which the name is not important.
    gl_FragColor = fColor;
}