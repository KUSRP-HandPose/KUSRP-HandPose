using UnityEngine;
using Unity.Mathematics;

namespace MediaPipe.HandPose {

//
// Miscellaneous math functions
//
static class MathUtil
{
    public static float4x4 Mul
      (float4x4 m1, float4x4 m2, float4x4 m3)
      => math.mul(math.mul(m1, m2), m3);

    public static float4x4 Mul
      (float4x4 m1, float4x4 m2, float4x4 m3, float4x4 m4)
      => math.mul(math.mul(math.mul(m1, m2), m3), m4);

    public static float4x4 Mul
      (float4x4 m1, float4x4 m2, float4x4 m3, float4x4 m4, float4x4 m5)
      => math.mul(math.mul(math.mul(math.mul(m1, m2), m3), m4), m5);

    public static float Angle(float2 v)
      => math.atan2(v.y, v.x);

    public static float4x4 ZRotateAtCenter(float angle)
      => Mul(float4x4.Translate(math.float3(0.5f, 0.5f, 0)),
             float4x4.RotateZ(angle),
             float4x4.Translate(math.float3(-0.5f, -0.5f, 0)));

    public static float4x4 CropMatrix(float angle, float2 scale, float2 offset)
      => Mul(float4x4.Translate(math.float3(offset, 0)),
             float4x4.Scale(math.float3(scale, 1)),
             MathUtil.ZRotateAtCenter(angle));
}

} // namespace MediaPipe.HandPose