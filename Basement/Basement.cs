using System.Collections.Generic;

public class Basement
{
    public Grid<BasementRoomElement> Grid { get; set; }
    public List<Item> Items { get; set; } = new();
}
