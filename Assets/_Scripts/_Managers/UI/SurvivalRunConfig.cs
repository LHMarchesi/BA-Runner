public static class SurvivalRunConfig
{
    public static SurvivalMode SelectedMode
    {
        get;
        private set;
    } = SurvivalMode.Solo;

    public static void SetMode(
        SurvivalMode mode
    )
    {
        SelectedMode = mode;
    }
}