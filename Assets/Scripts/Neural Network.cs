using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class NeuralNetwork : IComparable<NeuralNetwork>
{
    private int[] layers;
    private float[][] neurons;
    private float[][] biases;
    private float[][][] weights;
    private int[] activations;
    public float fitness = 0;

    public NeuralNetwork(int[] layers) //Populates needed arrays from specified network dimentions
    {
        this.layers = new int[layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            this.layers[i] = layers[i];
        }
        InitializeNeurons();
        InitializeBiases();
        InitializeWeights();
    }

    private void InitializeNeurons() //Creates empty array to store nurons from network dimentions specified in layers
    {
        List<float[]> neuronsList = new List<float[]>();
        for (int i = 0; i < layers.Length; i++)
        {
            neuronsList.Add(new float[layers[i]]);
        }
        neurons = neuronsList.ToArray();
    }

    private void InitializeBiases() //Like InitializeNeurons, Creates array from network dimentions specified in layers, but each slot is populated with a random bias
    {
        List<float[]> biasList = new List<float[]>();
        for (int i = 0; i < layers.Length; i++)
        {
            float[] bias = new float[layers[i]];

            for (int j = 0; j < layers[i]; j++)
            {
                bias[j] = UnityEngine.Random.Range(-0.5f, 0.5f);
            }
            biasList.Add(bias);
        }
        biases = biasList.ToArray();
    }

    private void InitializeWeights() //initializes array for the weights of each dendrite connecting nodes between layers, and assigns them a random weight.
    {
        List<float[][]> weightsList = new List<float[][]>();
        for (int i = 1; i < layers.Length; i++) //for each layer excluding input
        {
            List<float[]> layerWeightsList = new List<float[]>();
            int neuronsInPreviousLayer = layers[i - 1]; //Get number of nurons in previous layer

            for (int j = 0; j < neurons[i].Length; j++) //For each neuron in currrent layer
            {
                float[] neuronWeights = new float[neuronsInPreviousLayer];

                for (int k = 0; k < neuronsInPreviousLayer; k++) //for each nuron in previous layer
                {
                    neuronWeights[k] = UnityEngine.Random.Range(-0.5f, 0.5f); //Generate random weight for the dendrite between the selected nuron in the current layer (j) and the selected nuron in the previous layer (k)
                }
                layerWeightsList.Add(neuronWeights);
            }
            weightsList.Add(layerWeightsList.ToArray());
        }
        weights = weightsList.ToArray();
    }

    public float[] FeedForward(float[] inputs) //Passes inputs through the network and returns the outputs
    {
        for (int i = 0; i < inputs.Length; i++) //Assign inputs to the first layer of the network
        {
            neurons[0][i] = inputs[i];
        }
        for (int i = 0; i < (layers.Length - 1); i++) //for each layer excluding the output
        {
            for (int j = 0; j < neurons[i + 1].Length; j++) //for each nuron in the next layer
            {
                float value = 0f;
                for (int k = 0; k < neurons[i].Length; k++) //for each nuron in the current layer
                {
                    value += weights[i][j][k] * neurons[i][k]; //multiply nuron value by the weight of dendrite connecting to the nuron in next layer
                }
                neurons[i + 1][j] = (float)Math.Tanh(value + biases[i + 1][j]); // Feed weighted Sum ((current nuron * all weights of connected dendrites to the nuron in next layer) + bias of nuron in next layer) into the activation function (Tanh)
            }
        }
        return neurons[neurons.Length - 1]; //return output
    }


    public void Mutate(int chance, float range) //Adjustes biases and weights based on chance of change and range of change, 
    {
        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                biases[i][j] = (UnityEngine.Random.Range(0f, chance) <= 5) ? biases[i][j] += UnityEngine.Random.Range(-range, range) : biases[i][j];
            }
        }

        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    weights[i][j][k] = (UnityEngine.Random.Range(0f, chance) <= 5) ? weights[i][j][k] += UnityEngine.Random.Range(-range, range) : weights[i][j][k];
                }
            }
        }
    }

    public int CompareTo(NeuralNetwork other) //Compares the fitness of two neural networks to determin what one is performing better
    {
        if (other == null) return 1;

        if (fitness > other.fitness)
            return 1;
        else if (fitness < other.fitness)
            return -1;
        else
            return 0;
    }

    public NeuralNetwork copy(NeuralNetwork network) //Creates a deep copy of network
    {
        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                network.biases[i][j] = biases[i][j];
            }
        }
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    network.weights[i][j][k] = weights[i][j][k];
                }
            }
        }
        return network;
    }

    public void Load(string path) //Loads biases and weights of network from file
    {
        TextReader tr = new StreamReader(path);
        int NumberOfLines = (int)new FileInfo(path).Length;
        string[] ListLines = new string[NumberOfLines];
        int index = 1;
        for (int i = 1; i < NumberOfLines; i++)
        {
            ListLines[i] = tr.ReadLine();
        }
        tr.Close();
        if (new FileInfo(path).Length > 0)
        {
            for (int i = 0; i < biases.Length; i++)
            {
                for (int j = 0; j < biases[i].Length; j++)
                {
                    biases[i][j] = float.Parse(ListLines[index]);
                    index++;
                }
            }

            for (int i = 0; i < weights.Length; i++)
            {
                for (int j = 0; j < weights[i].Length; j++)
                {
                    for (int k = 0; k < weights[i][j].Length; k++)
                    {
                        weights[i][j][k] = float.Parse(ListLines[index]); ;
                        index++;
                    }
                }
            }
        }
    }

    public void Save(string path) //Saves biases and weights of network from file
    {
        File.Create(path).Close();
        StreamWriter writer = new StreamWriter(path, true);

        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                writer.WriteLine(biases[i][j]);
            }
        }

        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    writer.WriteLine(weights[i][j][k]);
                }
            }
        }
        writer.Close();
    }
}
