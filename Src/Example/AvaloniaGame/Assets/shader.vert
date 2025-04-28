//Here we specify the version of our shader.
//These lines specify the location and type of our attributes,
//the attributes here are prefixed with a "v" as they are our inputs to the vertex shader
//this isn't strictly necessary though, but a good habit.
attribute vec3 vPos;
attribute vec4 vColor;

//This is how we declare a uniform, they can be used in all our shaders and share the same name.
//This is prefixed with a u as it's our uniform.
uniform float uBlue;

//This is our output variable, notice that this is prefixed with an f as it's the input of our fragment shader.
varying vec4 fColor;

void main()
{
    //gl_Position, is a built-in variable on all vertex shaders that will specify the position of our vertex.
    gl_Position = vec4(vPos, 1.0);
    //The rest of this code looks like plain old c (almost c#)
    vec4 color = vec4(vColor.rb / 2.0, uBlue, vColor.a);//Swizzling and constructors in glsl.
    fColor = color;
}