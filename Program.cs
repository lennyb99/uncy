using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Gib deinen Namen ein:");
        string name = Console.ReadLine();
        Console.WriteLine($"Hallo, {name}!");
    }
}