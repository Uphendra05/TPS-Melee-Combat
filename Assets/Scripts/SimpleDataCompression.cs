using UnityEngine;

public class SimpleDataCompression : MonoBehaviour
{
    public Transform actualPlayer;
    public Transform remotePlayer;

    Vector3 lastActualPosition;    // Tracks previous Actual Player position
    Vector3 lastRemotePosition;    // Tracks previous Remote Player position

    float min = -100f;
    float max = 100f;

    public Vector3 offset;

    void Start()
    {
        // Place them at different spots!
        actualPlayer.position = new Vector3(-5f, 0f, 0f);
        remotePlayer.position = new Vector3(5, 0f, 0f);
        lastActualPosition = actualPlayer.position;
        lastRemotePosition = remotePlayer.position;
    }

    void Update()
    {
        // Move Actual Player with WASD
        Vector3 move = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) move += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) move += Vector3.back;
        if (Input.GetKey(KeyCode.A)) move += Vector3.left;
        if (Input.GetKey(KeyCode.D)) move += Vector3.right;
        actualPlayer.position += move * 5f * Time.deltaTime;

        // Calculate delta movement (what would be sent over network)
        Vector3 delta = actualPlayer.position  - lastActualPosition;

        // Quantize the delta (simulate compression)
        short qx = Quantize(delta.x, -1f, 1f); // limit delta range per frame
        short qy = Quantize(delta.y, -1f, 1f);
        short qz = Quantize(delta.z, -1f, 1f);

        // Pack into bytes
        byte[] packed = new byte[6];
        packed[0] = (byte)(qx >> 8); packed[1] = (byte)(qx & 0xFF);
        packed[2] = (byte)(qy >> 8); packed[3] = (byte)(qy & 0xFF);
        packed[4] = (byte)(qz >> 8); packed[5] = (byte)(qz & 0xFF);

        Debug.Log($"Sent delta: {delta} | Packed size: {packed.Length} bytes");

        // Unpack
        short rx = (short)((packed[0] << 8) | packed[1]);
        short ry = (short)((packed[2] << 8) | packed[3]);
        short rz = (short)((packed[4] << 8) | packed[5]);
        Vector3 receivedDelta = new Vector3(
            Dequantize(rx, -1f, 1f),
            Dequantize(ry, -1f, 1f),
            Dequantize(rz, -1f, 1f)
        );

        Debug.Log($"Remote received delta: {receivedDelta}");

        // Move remote relative to its own position 
        remotePlayer.position += receivedDelta;

        // Update previous positions
        lastActualPosition = actualPlayer.position;
        lastRemotePosition = remotePlayer.position;

        
    }

    short Quantize(float val, float min, float max)
    {
        float norm = Mathf.Clamp01((val - min) / (max - min));
        return (short)Mathf.RoundToInt(norm * short.MaxValue);
    }

    float Dequantize(short qval, float min, float max)
    {
        float norm = qval / (float)short.MaxValue;
        return min + norm * (max - min);
    }
}
