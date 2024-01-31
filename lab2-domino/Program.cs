using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

public enum DominoValue
{
    Empty = 0,
    One = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6
}

public struct Domino
{
    public DominoValue SideA { get; set; }
    public DominoValue SideB { get; set; }

    public Domino(DominoValue sideA, DominoValue sideB)
    {
        SideA = sideA;
        SideB = sideB;
    }

    public override string ToString()
    {
        return $"[{(int)SideA}|{(int)SideB}]";
    }
}



class DominoChain
{
    private List<Domino> chain = new List<Domino>();

    public DominoChain() { }

    public DominoChain(List<Domino> dominoes)
    {
        chain.AddRange(dominoes);
    }

    public void AddDomino(Domino domino)
    {
        chain.Add(domino);
    }

    public List<Domino> GetChain()
    {
        return chain;
    }

    public bool IsValidChain()
    {
        if (chain.Count == 0)
            return false;

        Domino firstDomino = chain[0];
        Domino lastDomino = chain[chain.Count - 1];

        if (firstDomino.SideA != lastDomino.SideB)
            return false;

        for (int i = 0; i < chain.Count - 1; i++)
        {
            if (chain[i].SideB != chain[i + 1].SideA)
                return false;
        }

        return true;
    }
}


class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Welcome to the Domino Chain Game!");

        int numberOfDominoes = GetNumberOfDominoes();

        List<Domino> randomDominoes = GenerateRandomDominoes(numberOfDominoes);

        Console.WriteLine("Generated Dominoes:");
        foreach (Domino domino in randomDominoes)
        {
            Console.Write(domino + " ");
        }
        Console.WriteLine();

        DominoChain dominoChain = BuildDominoChain(randomDominoes);

        if (dominoChain.IsValidChain())
        {
            Console.WriteLine("Valid Domino Chain:");
            foreach (Domino domino in dominoChain.GetChain())
            {
                Console.Write(domino + " ");
            }
            Console.WriteLine();

            Console.WriteLine("Possible Domino Chain Variations:");
            List<List<Domino>> chainVariations = FindChainVariations(dominoChain.GetChain()); // Use dominoChain.GetChain() here
            for (int i = 0; i < chainVariations.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {string.Join(" ", chainVariations[i])}");
            }

            SaveDominoChainToXml(randomDominoes);

            Console.WriteLine("Domino Chain saved successfully to dominoes.xml!");
        }
        else
        {
            Console.WriteLine("Invalid Domino Chain. Unable to build a valid chain.");
        }

        Console.WriteLine("Thank you for playing the Domino Chain Game!");
    }


    static int GetNumberOfDominoes()
    {
        int numberOfDominoes;
        do
        {
            Console.Write("Enter the number of dominoes to generate: ");
        } while (!int.TryParse(Console.ReadLine(), out numberOfDominoes) || numberOfDominoes <= 0);
        return numberOfDominoes;
    }

    static List<Domino> GenerateRandomDominoes(int numberOfDominoes)
    {
        Random random = new Random();
        List<Domino> randomDominoes = new List<Domino>();
        for (int i = 0; i < numberOfDominoes; i++)
        {
            Domino domino = new Domino((DominoValue)random.Next(0, 7), (DominoValue)random.Next(0, 7));
            randomDominoes.Add(domino);
        }
        return randomDominoes;
    }

    static DominoChain BuildDominoChain(List<Domino> randomDominoes)
    {
        DominoChain dominoChain = new DominoChain();
        List<Domino> remainingDominoes = new List<Domino>(randomDominoes);

        // Start with a random domino
        int startIndex = new Random().Next(0, remainingDominoes.Count);
        Domino currentDomino = remainingDominoes[startIndex];
        dominoChain.AddDomino(currentDomino);
        remainingDominoes.RemoveAt(startIndex);

        bool found;
        do
        {
            found = false;

            for (int i = 0; i < remainingDominoes.Count; i++)
            {
                Domino nextDomino = remainingDominoes[i];
                if (currentDomino.SideB == nextDomino.SideA)
                {
                    dominoChain.AddDomino(nextDomino);
                    currentDomino = nextDomino;
                    remainingDominoes.RemoveAt(i);
                    found = true;
                    break;
                }
                else if (currentDomino.SideB == nextDomino.SideB)
                {
                    // Flip the domino to make it match
                    nextDomino = new Domino(nextDomino.SideB, nextDomino.SideA);
                    dominoChain.AddDomino(nextDomino);
                    currentDomino = nextDomino;
                    remainingDominoes.RemoveAt(i);
                    found = true;
                    break;
                }
            }
        } while (found);

        return dominoChain;
    }

    static List<List<Domino>> FindChainVariations(List<Domino> randomDominoes)
    {
        List<List<Domino>> chainVariations = new List<List<Domino>>();

        void GenerateVariations(List<Domino> remaining, List<Domino> currentChain)
        {
            if (remaining.Count == 0)
            {
                DominoChain currentChainObj = new DominoChain(currentChain);
                if (currentChain.Count > 0 && currentChainObj.IsValidChain())
                {
                    chainVariations.Add(new List<Domino>(currentChain));
                }
                return;
            }

            Domino lastDominoInChain = currentChain.Count > 0 ? currentChain[currentChain.Count - 1] : default;

            for (int i = 0; i < remaining.Count; i++)
            {
                Domino nextDomino = remaining[i];

                if ((lastDominoInChain.SideB == nextDomino.SideA ||
                     (lastDominoInChain.SideB == nextDomino.SideB && lastDominoInChain.SideA != DominoValue.Empty)) ||
                    (lastDominoInChain.SideA == nextDomino.SideA && lastDominoInChain.SideB == DominoValue.Empty))
                {
                    List<Domino> newRemaining = new List<Domino>(remaining);
                    newRemaining.RemoveAt(i);

                    List<Domino> newChain = new List<Domino>(currentChain);
                    newChain.Add(nextDomino);

                    GenerateVariations(newRemaining, newChain);
                }
            }
        }

        GenerateVariations(randomDominoes, new List<Domino>());

        return chainVariations;
    }


    static void SaveDominoChainToXml(List<Domino> randomDominoes)
    {
        string fileName = $"dominoes_{DateTime.Now:yyyyMMddHHmmss}.xml";
        using (FileStream fs = new FileStream(fileName, FileMode.Create))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Domino>));
            serializer.Serialize(fs, randomDominoes);
        }
    }
}
