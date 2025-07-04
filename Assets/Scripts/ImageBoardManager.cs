using UnityEngine;

public class ImageBoardManager : MonoBehaviour
{
    public Texture2D[] textures;       // 7 texturas
    public GameObject boardPrefab;     // Prefab ImageBoard
    public Vector3[] positions;        // 7 posiciones en la escena
    public Vector3[] rotations;        // 7 rotaciones en Euler

    void Start()
    {
        int count = Mathf.Min(textures.Length, positions.Length, rotations.Length);

        for (int i = 0; i < count; i++)
        {
            Quaternion rot = Quaternion.Euler(rotations[i]);
            var board = Instantiate(boardPrefab, positions[i], rot, transform);

            var mat = new Material(Shader.Find("Unlit/Texture")) { mainTexture = textures[i] };
            board.GetComponent<MeshRenderer>().material = mat;
        }
    }
}
