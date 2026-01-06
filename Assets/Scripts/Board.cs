using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
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

    public enum BoardState { wait, move}
    public BoardState currentState = BoardState.move;

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
        //matchFinder.FindAllMatches();
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
        Gem gem = Instantiate(gemToSpawn, new Vector2(pos.x, pos.y + height), Quaternion.identity);
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

    private void DestroyMatchedGemAt(Vector2Int pos)
    {
        if (allGems[pos.x, pos.y] != null)
        {
            if (allGems[pos.x, pos.y].isMatched)
            {
                Destroy(allGems[pos.x, pos.y].gameObject); // Destroy the game object. This can be optimized. OPTIMIZE IT!
                allGems[pos.x, pos.y] = null; // Destroy from board
            }
        }
    }

    public void DestroyMatches()
    {
        for (int i = 0; i < matchFinder.currentMatches.Count; i++)
        {
            if (matchFinder.currentMatches[i] != null)
            {
                DestroyMatchedGemAt(matchFinder.currentMatches[i].posIndex);
            }
        }

        StartCoroutine(DecreaseRowCo());
    }

    private IEnumerator DecreaseRowCo()
    {
        yield return new WaitForSeconds(.2f);   

        int nullCounter = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allGems[x, y] == null)
                {
                    nullCounter++;
                } else if (nullCounter > 0)
                {
                    allGems[x, y].posIndex.y -= nullCounter;
                    allGems[x, y - nullCounter] = allGems[x, y];
                    allGems[x, y] = null;
                }
            }
            nullCounter = 0;
        }

        StartCoroutine(FillBoardCo());
    }

    private IEnumerator FillBoardCo()
    {
        yield return new WaitForSeconds(.45f);
        RefillBoard();

        yield return new WaitForSeconds(.5f);
        matchFinder.FindAllMatches();

        if (matchFinder.currentMatches.Count > 0)
        {
            yield return new WaitForSeconds(.5f);
            DestroyMatches(); 
        }
        else
        {
            yield return new WaitForSeconds(.5f);
            currentState = Board.BoardState.move;
        }
    }

    private void RefillBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allGems[x, y] == null)
                {
                    int gemToUse = Random.Range(0, gems.Length);

                    SpawnGem(new Vector2Int(x, y), gems[gemToUse], (Gem.GemType)gemToUse);
                }
            }
        }

        CheckMisplacedGems();
    }

    private void CheckMisplacedGems()
    {
        List<Gem> foundGems = new List<Gem>();

        // Updated to use FindObjectsByType with FindObjectSortMode.None for better performance  
        foundGems.AddRange(FindObjectsByType<Gem>(FindObjectsSortMode.None));

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (foundGems.Contains(allGems[x, y]))
                {
                    foundGems.Remove(allGems[x, y]);
                }
            }
        }

        foreach (Gem g in foundGems)
        {
            Destroy(g.gameObject);
        }
    }
}
