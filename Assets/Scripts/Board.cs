using UnityEngine;

public class Board : MonoBehaviour
{
    // Board Size
    public int width { get; private set; }
    public int height { get; private set; }

    // Prefab
    [SerializeField] private GameObject bgTilePrefab;
    [SerializeField] private Gem[] gems;

    // Inboard gems
    public Gem[,] allGems;
    public float gemSpeed { get; private set; }

    // Matchfinder
    public MatchFinder matchFinder;

    private void Awake()
    {
        matchFinder = FindFirstObjectByType<MatchFinder>();
    }

    void Start()
    {
        // private sets
        width = 9;
        height = 9;
        gemSpeed = 7;

        allGems = new Gem[width, height];

        Setup(); 
    }

    private void Update()
    {
        matchFinder.FindAllMatches();
    }

    private void Setup()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                GameObject bgTile = Instantiate(bgTilePrefab, (Vector2)pos, Quaternion.identity);
                bgTile.transform.parent = transform;
                bgTile.name = $"BG Tile - ({x}, {y})";

                int gemToUse = Random.Range(0, gems.Length);

                int iterations = 0;
                while (matchesAt(new Vector2Int(x, y), (Gem.GemType)gemToUse) && iterations < 100)
                {
                    gemToUse = Random.Range(0, gems.Length);
                    iterations++;
                }
                SpawnGem(pos, gems[gemToUse], (Gem.GemType)gemToUse);
            }
        }
    }

    private void SpawnGem(Vector2Int pos ,Gem gemToSpawn, Gem.GemType type)
    {
        Gem gem = Instantiate(gemToSpawn, (Vector2)pos, Quaternion.identity);
        gem.transform.parent = transform;
        gem.name = $"Gem - ({pos.x}, {pos.y})";

        allGems[pos.x, pos.y] = gem;

        gem.SetupGem(pos, this, type);
    }

    bool matchesAt(Vector2Int posToCheck, Gem.GemType type)
    {
        if (posToCheck.x > 1)
        {
            if (allGems[posToCheck.x -  1, posToCheck.y].type == type && allGems[posToCheck.x - 2, posToCheck.y].type == type)
            {
                return true;
            }
        }

        if (posToCheck.y > 1)
        {
            if (allGems[posToCheck.x, posToCheck.y - 1].type == type && allGems[posToCheck.x, posToCheck.y - 2].type == type)
            {
                return true;
            }
        }

        return false;
    }
}
