using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;

[ExecuteAlways]
public class Read_Collisions : MonoBehaviour
{
    private static readonly int kReadback_CollisionID = Shader.PropertyToID("Readback_Collisions");

    private GraphicsBuffer m_Buffer;
    private AsyncGPUReadbackRequest m_Readback;

    void OnEnable()
    {
        m_Buffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw, GraphicsBuffer.UsageFlags.None, 16 * 6 + 1 , Marshal.SizeOf(typeof(uint)));
        m_Buffer.SetData(new[] { 0 });

        GetComponent<VisualEffect>().SetGraphicsBuffer(kReadback_CollisionID, m_Buffer);
    }

    void OnReadback(AsyncGPUReadbackRequest asyncGpuReadbackRequest)
    {
        var count = m_Readback.GetData<uint>()[0];
        if (count > 0)
        {
            var data = m_Readback.GetData<float>();
            var cursor = 1;
            for (uint index = 0; index < count && index < 16; ++index)
            {
                var position = new Vector3(data[cursor++], data[cursor++], data[cursor++]);
                var normal = new Vector3(data[cursor++], data[cursor++], data[cursor++]);
                Debug.DrawLine(position, position + normal * 0.1f, Color.red, 0.5f);
            }
        }
    }


    void Update()
    {
        if (m_Readback.done)
        {
            m_Readback = AsyncGPUReadback.Request(m_Buffer, OnReadback);
            m_Buffer.SetData(new[] { 0 }); //This isn't perfect, we can miss event this way
        }
    }

    void OnDisable()
    {
        m_Buffer.Release();
    }
}
