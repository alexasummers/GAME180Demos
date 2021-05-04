using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GAUIControl : MonoBehaviour
{
    public Text txtTargetWord, txtPoolSize, txtMaxGenerations, lblTopScorers, lblGenerations;
    public float mutationRate = 0.001f;

    string targetPhrase;
    int poolSize, maxGenerations;

    string genePool = "abcdefghijklmnopqrstuvwxyz ";

    string RandChar() {
        return genePool[UnityEngine.Random.Range(0, genePool.Length)].ToString();
    }

    void Start()
    {

    }

    void Update()
    {
        
    }

    List<string> generation;
    List<WeightedEntry> breedingGround = new List<WeightedEntry>();

    public void btnGenerate_OnClick() {

        if (generation == null) {

            targetPhrase = txtTargetWord.text;
            poolSize = int.Parse(txtPoolSize.text);
            maxGenerations = int.Parse(txtMaxGenerations.text);
            // generate the pool
            generation = new List<string>();
            for (int i = 0; i < poolSize; i++) {
                System.Text.StringBuilder s = new System.Text.StringBuilder();
                for (int c = 0; c < targetPhrase.Length; c++) {
                    s.Append(genePool[UnityEngine.Random.Range(0, genePool.Length)]);
                }

                generation.Add(s.ToString());
            }
        } 
        else 
        {
            List<string> breeders = new List<string>();
            for (int i = 0; i < breedingGround.Count / 2; i++) breeders.Add(breedingGround[i].original);

            List<string> nextGeneration = new List<string>();
            //for (int i = 0; i < breeders.Count; i++) nextGeneration.Add(breeders[i]);

            while (nextGeneration.Count < poolSize) {
                // pick two breeders and cross 'em up!
                int a = UnityEngine.Random.Range(0, breeders.Count), b = UnityEngine.Random.Range(0, breeders.Count);
                int c = 5;
                while (a == b && c-- > 0) b = UnityEngine.Random.Range(0, breeders.Count);
                string parentA = breeders[a], parentB = breeders[b];
                int cut = UnityEngine.Random.Range(1, parentA.Length-1);        

                string childA = parentA, childB = parentB;
                try {
                    childA = parentA.Substring(0, cut) + parentB.Substring(cut);
                    childB = parentB.Substring(0, cut) + parentA.Substring(cut);
                } catch {
                    print("parent A is " + parentA + " length " + parentA.Length);
                    print("parent B is " + parentB + " length " + parentB.Length);
                    print("cut is " + cut);
                }

                if (childA.Length != parentA.Length || childA.Length != parentB.Length || childB.Length != parentA.Length || childB.Length != parentB.Length) {
                    print("parent A is " + parentA + " length " + parentA.Length);
                    print("parent B is " + parentB + " length " + parentB.Length);
                    print("child A is " + childA + " length " + childA.Length);
                    print("child B is " + childB + " length " + childB.Length);
                    print("cut is " + cut);
                }

                // mutation
                while (UnityEngine.Random.Range(0.0f, 1.0f) <= mutationRate) childA = Mutate(childA);
                while (UnityEngine.Random.Range(0.0f, 1.0f) <= mutationRate) childB = Mutate(childB);

                nextGeneration.Add(childA);
                nextGeneration.Add(childB);
            }
            generation = nextGeneration;
        }

        // evaluate the current generation's fitness and sort
        breedingGround = new List<WeightedEntry>();
        for (int i = 0; i < generation.Count; i++) {
            breedingGround.Add(new WeightedEntry(EvaluateFitness(generation[i]), generation[i] ));
        }
        breedingGround.Sort();
        breedingGround.Reverse();
    
        string winners = "";
        for (int i = 0; i < 20; i++) {
            winners += breedingGround[i].original + "\n";
        }

        lblTopScorers.text = winners;

        int intGenerations = int.Parse(lblGenerations.text);
        lblGenerations.text = (intGenerations + 1).ToString();

    }

    private string Mutate(string s) {
        int mutation = UnityEngine.Random.Range(0, s.Length);
        string prefix = string.Empty, suffix = string.Empty;
        if (mutation > 0) prefix = s.Substring(0, mutation);
        if (mutation < s.Length - 1) suffix = s.Substring(mutation + 1);
        return prefix + RandChar() + suffix;
    }

    private double EvaluateFitness(string s) {
        // compare to targetPhrase; how many characters do we have right?
        double max = targetPhrase.Length, total = 0.0;
        for (int c = 0; c < targetPhrase.Length && c < s.Length; c++) {
            if (s[c] == targetPhrase[c]) total += 1.0;
        }
        return total / max;
    }


    class WeightedEntry : IComparable
    {
        static System.Random random = new System.Random();

        public double value;
        public double binomial = 1;

        public string original;

        public int CompareTo(object obj)
        {
            if (!(obj is WeightedEntry o)) return 0;
            if (value * binomial < o.value * o.binomial) return -1;
            if (value * binomial == o.value * o.binomial) return 0;
            return 1;
        }

        public WeightedEntry(double v, string o)
        {
            value = v;
            original = o;
            binomial = random.NextDouble() * random.NextDouble();
        }
    }

}
