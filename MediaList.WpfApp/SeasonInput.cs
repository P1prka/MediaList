namespace MediaList.WpfApp;

public class SeasonInput
{
    public int Number { get; }

    public string Label => $"S{Number}";

    public string EpisodesText { get; set; } = "10";

    public SeasonInput(int number)
    {
        Number = number;
    }
}


