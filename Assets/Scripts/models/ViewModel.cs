using System.Collections.Generic;

public class ViewModel
{
    public static readonly ViewModel Instance = new ViewModel();

    public List<EmitterBrick> Emitters = new List<EmitterBrick>();

}