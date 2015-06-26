public class GameManager
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameManager();
            }
            return _instance;
        }
    }

    private GameManager()
    {

    }

    public int Points { get; private set; }

    public void Reset()
    {
        Points = 0;
    }

    public void AddPoints(int points)
    {
        Points += points;
    }

    public void ResetPoints(int points)
    {
        Points = points;
    }
}