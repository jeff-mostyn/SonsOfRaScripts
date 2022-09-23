Shader "Custom/DepthMask"
{
    SubShader
    {
        Tags { "Queue" = "Geometry+1" }
        
		//Don't draw in the RGVA channels; just the depth buffer

		ColorMask 0
		ZWrite On

		// Do nothing specific in the pass:

        Pass {}
    }
}
