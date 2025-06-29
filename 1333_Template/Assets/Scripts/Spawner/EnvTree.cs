using UnityEngine;

public class EnvTree : MonoBehaviour
{
    [Header("Grid Size (in cells)")]
    [SerializeField] private int _width = 1;
    [SerializeField] private int _height = 1;
    public int Width => _width;
    public int Height => _height;
}