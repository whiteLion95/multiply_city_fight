using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Deslab.Utils;

[CreateAssetMenu(fileName = "RandomUsersData", menuName = "Deslab/Utils/RandomUserSO", order = 1)]
public class RandomUserSO : ScriptableObject
{
    [SerializeField] private TextAsset UsernamesAsset;
    [SerializeField] private List<Sprite> FlagsSprite = new List<Sprite>();
    [SerializeField] private List<string> Usernames = new List<string>();
    [SerializeField] private bool addSimplePlayerName = false;
    [ShowIf("addSimplePlayerName")]
    [SerializeField] private int simplePlayerNameCount = 150;

    public RandomUser RandomUser()
    {
        RandomUser randomUser;
        randomUser.FlagSprite = FlagsSprite[Random.Range(0, FlagsSprite.Count)];
        randomUser.Username = Usernames[Random.Range(0, Usernames.Count)];
        return randomUser;
    }


    [Button(ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1)]
    public void LoadUsernamesAndShuffleLists()
    {
        Usernames.Clear();
        Usernames.AddRange(UsernamesAsset.text.Split('\n'));

        if (addSimplePlayerName)
        {
            string name = "";
            for (int i = 0; i < simplePlayerNameCount; i++)
            {
                name = "Player" + Random.Range(1150, 26516051);
                Usernames.Add(name);
            }
        }

        Usernames.Shuffle();
        FlagsSprite.Shuffle();
    }
}
