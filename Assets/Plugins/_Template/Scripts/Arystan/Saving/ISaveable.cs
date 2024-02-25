namespace Arystan.Saving
{
    public interface ISaveable
    {
        void SaveData();
        void LoadData();
        void AddMeToSavingManager();
    }
}
