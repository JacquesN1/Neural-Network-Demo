using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TestManager : MonoBehaviour
{
    public float testDuration;
    public int populationSize;
    public GameObject carPrefab;
    public int[] layers = new int[4] { 10, 5, 3, 2 }; //Array showing number of layers and number of nurons in each layer

    [Range(0.0001f, 1f)] public float MutationChance = 0.01f; //Chance of bias/weight mutation occuring 
    [Range(0f, 1f)] public float MutationStrength = 0.5f; //potential range of bias/weight change
    [Range(0.1f, 10f)] public float Gamespeed = 1f;

    public List<NeuralNetwork> networks;
    private List<CarController> cars;
    GameObject[] checkPoints;

    // Start is called before the first frame update
    void Start()
    {
        if (populationSize % 2 != 0) //if population size is odd, asign it to an even number
        {
            populationSize++;
        }

        GetCheckpoints();
        InitateNetworks();
        InvokeRepeating("CreateCars", 0.1f, testDuration); //Create new bots at the end of every test
    }

    public void InitateNetworks()
    {
        networks = new List<NeuralNetwork>();
        for (int i = 0; i < populationSize; i++) //for each car, create an network
        {
            NeuralNetwork net = new NeuralNetwork(layers);
            net.Load("Assets/Saves/SavedNetwork.txt"); //load the saved network
            networks.Add(net);
        }
    }

    public void GetCheckpoints()
    {
        checkPoints = GameObject.FindGameObjectsWithTag("Checkpoint").OrderBy(gameObject => gameObject.name).ToArray();
    }

    public void CreateCars()
    {
        Time.timeScale = Gamespeed;

        if (cars != null) //Destroy any cative cars
        {
            for (int i = 0; i < cars.Count; i++)
            {
                GameObject.Destroy(cars[i].gameObject);
            }

            SortNetworks();//this sorts networks and mutates them
        }

        cars = new List<CarController>();
        for (int i = 0; i < populationSize; i++)
        {
            CarController car = (Instantiate(carPrefab, new Vector3(-14f, -3f, 0f), Quaternion.identity)).GetComponent<CarController>();//create car from prefab
            car.network = networks[i]; //asign car with corrosponding network
            car.checkPoints = checkPoints;
            cars.Add(car);
        }

    }

    public void SortNetworks()
    {
        for (int i = 0; i < populationSize; i++)
        {
            cars[i].UpdateFitness();//updates each cars fitness
        }

        networks.Sort(); //sorts car list by fitness
        networks[populationSize - 1].Save("Assets/Saves/SavedNetwork.txt");//saves best performing network to file

        for (int i = 0; i < populationSize / 2; i++) //Keeps the best perfoming half of networks, and replaces the others with an mutated vesion of the best performing network
        {
            networks[i] = networks[i + populationSize / 2].copy(new NeuralNetwork(layers));
            networks[i].Mutate((int)(1 / MutationChance), MutationStrength);
        }
    }

}
