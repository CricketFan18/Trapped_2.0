using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    public float MaxTime = 7200f; // 2 Hours
    public float CurrentTime;
    public bool IsGamePaused = false;
    public bool HasEscaped = false;

    private void Awake()
    {
        // Ensure only one Manager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        CurrentTime = MaxTime;
        // Lock cursor for FPS
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!IsGamePaused && !HasEscaped)
        {
            CurrentTime -= Time.deltaTime;

            if (CurrentTime <= 0)
            {
                CurrentTime = 0;
                GameOver();
            }
        }
    }

    public void GameOver()
    {
        IsGamePaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("GAME OVER: TIME EXPIRED");
        // UIManager.Instance.ShowGameOver(); // We will add this later
    }

    public void WinGame()
    {
        HasEscaped = true;
        IsGamePaused = true;
        Debug.Log("YOU ESCAPED!");
    }

    public void SpawnXORPuzzle(Transform puzzleRoot, Texture2D keyTex)
    {
        // --- UV Flashlight ---
        GameObject flashlight = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        flashlight.name = "UV_Flashlight";
        flashlight.transform.SetParent(puzzleRoot.transform);
        flashlight.transform.localPosition = new Vector3(0.6f, 0.6f, 0);
        flashlight.transform.localScale = new Vector3(0.05f, 0.15f, 0.05f);
        flashlight.transform.localRotation = Quaternion.Euler(90, 0, 0);
        flashlight.layer = 6;

        flashlight.GetComponent<Renderer>().material.color = Color.black;

        GameObject lightObj = new GameObject("LightSource");
        lightObj.transform.SetParent(flashlight.transform);
        lightObj.transform.localPosition = new Vector3(0, 1.0f, 0);
        lightObj.transform.localRotation = Quaternion.Euler(-90, 0, 0);

        Light l = lightObj.AddComponent<Light>();
        l.type = LightType.Spot;
        l.color = new Color(0.5f, 0f, 1f);
        l.intensity = 5f;
        l.range = 5f;
        l.spotAngle = 60f;

        var uvScript = flashlight.AddComponent<UVLight>();
        uvScript.FlashlightInfo = l;

        var rbF = flashlight.AddComponent<Rigidbody>();
        rbF.collisionDetectionMode = CollisionDetectionMode.Continuous;

        var grabF = flashlight.AddComponent<GrabbableObject>();
        grabF.ObjectName = "UV Flashlight";
        grabF.HeldRotationOffset = Vector3.zero;
        grabF.PointAtCamera = true;

        // Allow dragging on the light table
        flashlight.AddComponent<TableDraggable>();

        Debug.Log("XOR Puzzle Spawned with UV Flashlight");
    }

    // --- Texture Generation Helpers ---

    // ... existing ...

    private Texture2D GenerateNoiseTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] cols = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Alignment Hint Borders
                // If we are at the edge (2 pixels thick)
                if (x < 4 || x >= size - 4 || y < 4 || y >= size - 4)
                {
                    // Force White (1)
                    cols[y * size + x] = Color.white;
                }
                else
                {
                    // Binary Noise (0 or 1)
                    float val = Random.value > 0.5f ? 1f : 0f;
                    cols[y * size + x] = new Color(val, val, val, 1f);
                }
            }
        }

        tex.SetPixels(cols);
        tex.Apply();
        return tex;
    }
}