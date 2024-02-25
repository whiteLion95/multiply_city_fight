using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LunarConsolePlugin;

public class LunarConsoleManager : MonoBehaviour
{
    [SerializeField] private bool consoleIsEnabled;

    // Start is called before the first frame update
    void Start()
    {
        LunarConsole.SetConsoleEnabled(consoleIsEnabled);
    }

    public void ShowConsole()
    {
        LunarConsole.Show();
    }
}
