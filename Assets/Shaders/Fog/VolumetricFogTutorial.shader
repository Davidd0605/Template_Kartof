Shader "Fog/VolumetricFog"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1) //fog color
        _MaxDistance("Max distance", float) = 100 // upper bound of how long a ray can travel
        _StepSize("Step size", Range(0.1, 20)) = 1 // step size between the transmittance gains, DO NOT MAKE SMALLER DO NOT HAVE MULTIPLE CAMERAS RENDERING THIS AT THE SAME TIME
        _DensityMultiplier("Density multiplier", Range(0, 10)) = 1 // transmittance gain per step
        _NoiseOffset("Noise offset", float) = 0 // Noise offset for ray starting position


        [HDR]_LightContribution("Light contribution", Color) = (1, 1, 1, 1) // light color to blend with smoke color
        _LightScattering("Light scattering", Range(0, 1)) = 0.2
    }

        SubShader
        {
            Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

            Pass
            {
                HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment frag

                #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
                #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

                float4 _Color;
                float _MaxDistance;
                float _DensityMultiplier;
                float _StepSize;
                float _NoiseOffset;
                float4 _LightContribution;
                float _LightScattering;

                float henyey_greenstein(float angle, float scattering)
                {
                    return (1.0 - angle * angle) / (4.0 * PI * pow(1.0 + scattering * scattering - (2.0 * scattering) * angle, 1.5f));
                }

                float get_density()
                {
                    return _DensityMultiplier;
                }


                //Fragment shader
                half4 frag(Varyings IN) : SV_Target
                {
                    float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, IN.texcoord); //get color of fragment (current fragment)
                    float depth = SampleSceneDepth(IN.texcoord); // screen depth buffer, get depth of all fragments
                    float3 worldPos = ComputeWorldSpacePosition(IN.texcoord, depth, UNITY_MATRIX_I_VP); // worlds position of current pixel, needs tex coordinate and inverse 

                    float3 entryPoint = _WorldSpaceCameraPos; // start of the ray
                    float3 viewDir = worldPos - _WorldSpaceCameraPos;  // vector direction from entry point to current fragment

                    float viewLength = length(viewDir); // magnitude of view direction
                    float3 rayDir = normalize(viewDir); // direction of view direction

                    float2 pixelCoords = IN.texcoord * _BlitTexture_TexelSize.zw; // pixel screen coordinates multiply texture coordinate ([0,1]) with the texture width and height -> pixel screen coordinates
                    float distLimit = min(viewLength, _MaxDistance); // how far does the ray go for this particular fragment it either goes out of bounds or it hits something

                    float distTravelled = InterleavedGradientNoise(pixelCoords, (int)(_Time.y / max(HALF_EPS, unity_DeltaTime.x))) * _NoiseOffset; // distance traveled by ray so far
                                                                                                                                                   // introduce noise in order to reducing banding effect for steps                                                                                                                                              // by having each ray be offseted by a random amount from the start.
                    float transmittance = 1; // accumulated transmittance

                    float4 fogCol = _Color; // color of the fog that is blended with the fragment color


                    while (distTravelled < distLimit) // march the ray until established limit is hit
                    {
                        float3 rayPos = entryPoint + rayDir * distTravelled; // calculate current ray position using the ray direction unit vetor * traveled distance
                        float density = get_density();
                        if (density > 0)
                        {
                            Light mainLight = GetMainLight(TransformWorldToShadowCoord(rayPos)); // reference to the scene main light
                            fogCol.rgb += mainLight.color.rgb * _LightContribution.rgb 
                                * henyey_greenstein(dot(rayDir, mainLight.direction), _LightScattering) 
                                * density * mainLight.shadowAttenuation * _StepSize;

                            //Additional lights (TBA)

                            transmittance *= exp(-density * _StepSize); // Beer's law, get smoother transmittance transitions based on step size and desnity, desnity is multiplied by stepsize so it remains consistent across different
                                                                        // step sizes
                        }

                        distTravelled += _StepSize; //increment traveled distance by the step size
                    }

                    return lerp(col, fogCol, 1.0 - saturate(transmittance)); //interpolate between fragment color and for color based on transmittance
                }
                ENDHLSL
            }
        }
}