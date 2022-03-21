TEXTURE2D(_CameraDepthTexture);
SAMPLER(sampler_CameraDepthTexture);

TEXTURE2D(_CameraDepthNormalsTexture);
SAMPLER(sampler_CameraDepthNormalsTexture);
float4 _CameraDepthNormalsTexture_TexelSize;

float3 DecodeNormal(float4 enc)
{
    float kScale = 1.7777;
    float3 nn = enc.xyz*float3(2*kScale,2*kScale,0) + float3(-kScale,-kScale,1);
    float g = 2.0 / dot(nn.xyz,nn.xyz);
    float3 n;
    n.xy = g*nn.xy;
    n.z = g-1;
    return n;
}

void Outline_float(float2 UV, float depthThreshold, float normalThreshold, out float4 Out)
{
    float2 Texel = (0.6) / float2(_CameraDepthNormalsTexture_TexelSize.z, _CameraDepthNormalsTexture_TexelSize.w);

    float2 uvSamples[4];
    float depthSamples[4];
    //float3 normalSamples[4], colorSamples[4];
    float3 normalSamples[4];

    uvSamples[0] = UV + float2(-Texel.x, 0); // left
    uvSamples[1] = UV + float2(Texel.x,  0); // right
    uvSamples[2] = UV + float2(0,  Texel.y); // up
    uvSamples[3] = UV + float2(0, -Texel.y); // down

    for (int i = 0; i < 4; i++)
    {
        depthSamples[i] = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, uvSamples[i]).r;
        normalSamples[i] = DecodeNormal(SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, uvSamples[i]));
        //colorSamples[i] = SAMPLE_TEXTURE2D(_CameraColorTexture, sampler_CameraColorTexture, uvSamples[i]);
    }

    // Depth
	//float edgeDepth = sqrt(pow(depthFiniteDifference0, 2) + pow(depthFiniteDifference1, 2)) * 100;
    //float depthThreshold = (1 / 0.1) * depthSamples[0];
    //edgeDepth = edgeDepth > 0.2 ? 1 : 0;
    //edgeDepth = smoothstep(0.19, 0.22, edgeDepth);
    float depthFiniteDifference0 = abs(depthSamples[1] - depthSamples[0]);
    float depthFiniteDifference1 = abs(depthSamples[3] - depthSamples[2]);

    float edgeDepth = smoothstep(depthThreshold, depthThreshold + 0.1, depthFiniteDifference0 + depthFiniteDifference1);


    // Normals
    //float3 normalFiniteDifference0 = normalSamples[1] - normalSamples[0];
    //float3 normalFiniteDifference1 = normalSamples[3] - normalSamples[2];
    //float edgeNormal = sqrt(dot(normalFiniteDifference0, normalFiniteDifference0) + dot(normalFiniteDifference1, normalFiniteDifference1));
    //edgeNormal = edgeNormal > (1 / 0.9) ? 1 : 0;
    float normDot0 = 1.0 - abs(dot(normalSamples[0], normalSamples[1]));
    float normDot1 = 1.0 - abs(dot(normalSamples[2], normalSamples[3]));

    float edgeNormal = smoothstep(normalThreshold, normalThreshold + 0.1, normDot0 + normDot1);


    /*
    // Color
    float3 colorFiniteDifference0 = colorSamples[1] - colorSamples[0];
    float3 colorFiniteDifference1 = colorSamples[3] - colorSamples[2];
    float edgeColor = sqrt(dot(colorFiniteDifference0, colorFiniteDifference0) + dot(colorFiniteDifference1, colorFiniteDifference1));
	edgeColor = edgeColor > (1/ColorSensitivity) ? 1 : 0;
	*/

    float edge = smoothstep(0.8, 0.9, edgeDepth + edgeNormal);

    float4 background = float4(1.0, 0.99, 0.98, 1);
    //Out = ((1 - edge) * background) + (edge * lerp(background, 1, 1));

    //Out = edge;
    Out = lerp(background, 0, edge);
    
    //Out = float4(edgeNormal,edgeNormal,edgeNormal,1);
}