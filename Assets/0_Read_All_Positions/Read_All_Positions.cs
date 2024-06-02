using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;

[ExecuteAlways]
public class Read_All_Positions : MonoBehaviour
{
    private static readonly int kReadback_All_PositionsID = Shader.PropertyToID("Readback_All_Positions");

    private GraphicsBuffer m_Buffer;
    private AsyncGPUReadbackRequest m_Readback;
    private NativeArray<Vector3> m_ReadbackBuffer;

    void OnEnable()
    {
        m_Buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, GraphicsBuffer.UsageFlags.None, 64, Marshal.SizeOf(typeof(Vector3)));
        GetComponent<VisualEffect>().SetGraphicsBuffer(kReadback_All_PositionsID, m_Buffer);

        m_ReadbackBuffer = new NativeArray<Vector3>(64, Allocator.Persistent, NativeArrayOptions.ClearMemory);
    }

    void OnReadback(AsyncGPUReadbackRequest asyncGpuReadbackRequest)
    {
        m_ReadbackBuffer.CopyFrom(asyncGpuReadbackRequest.GetData<Vector3>());
    }

    private static readonly float kUpdateRate = 0.0f; //Can be greater than 0.0 to avoid reading back every frames
    private float m_Wait = kUpdateRate;

    private static readonly float kDebugSize = 0.05f;
    void DrawDebugPosition(Vector3 position)
    {
        Debug.DrawLine(position + Vector3.back * kDebugSize, position + Vector3.forward * kDebugSize, Color.red, kUpdateRate);
        Debug.DrawLine(position + Vector3.up * kDebugSize, position + Vector3.down * kDebugSize, Color.red, kUpdateRate);
        Debug.DrawLine(position + Vector3.left * kDebugSize, position + Vector3.right * kDebugSize, Color.red, kUpdateRate);
    }
    void Update()
    {
        m_Wait -= Time.deltaTime;
        if (m_Wait < 0.0f)
        {
            m_Wait = kUpdateRate;

            if (m_Readback.done)
                m_Readback = AsyncGPUReadback.Request(m_Buffer, OnReadback);

            foreach (var position in m_ReadbackBuffer)
                DrawDebugPosition(position);
        }
    }

    void OnDisable()
    {
        m_Buffer.Release();
        m_ReadbackBuffer.Dispose();
    }
}
