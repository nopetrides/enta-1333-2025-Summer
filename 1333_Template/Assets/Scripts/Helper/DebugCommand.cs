using IngameDebugConsole;
using UnityEngine;

public class DebugCommand : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;
    private void OnEnable()
    {
        DebugLogConsole.AddCommand("HelloWorld", "Test command", HelloWorld);

        DebugLogConsole.AddCommand<string>("StartGame", "Print name", StartGame);
    }

    private void OnDisable()
    {
        DebugLogConsole.RemoveCommand("HelloWorld");

        DebugLogConsole.RemoveCommand("StartGame");
    }

    private void HelloWorld()
    {
        Debug.Log("Hello world!!!");
    }

    private void StartGame(string name)
    {
        _gameManager.StartGame(name);
    }
}
