using System.Collections.Generic;

public class Basement
{
    public BasementSettings Settings { get; set; }
    public Grid<BasementRoomElement> Grid { get; set; }
    public List<Item> Items { get; set; } = new();
}
