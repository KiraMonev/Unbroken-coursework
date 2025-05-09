using UnityEngine;

public class SimplePerceptron
{
    private int inputSize;
    private int hiddenSize;
    private int outputSize = 4; // 4 действия

    private float[,] weights1;
    private float[] biases1;
    private float[] hidden;

    private float[,] weights2;
    private float[] biases2;

    public SimplePerceptron(int inputSize, int hiddenSize)
    {
        this.inputSize = inputSize;
        this.hiddenSize = hiddenSize;

        weights1 = new float[inputSize, hiddenSize];
        biases1 = new float[hiddenSize];
        hidden = new float[hiddenSize];

        weights2 = new float[hiddenSize, outputSize];
        biases2 = new float[outputSize];

        InitializeWeights();
    }

    private void InitializeWeights()
    {
        System.Random rnd = new System.Random();
        for (int i = 0; i < inputSize; i++)
            for (int j = 0; j < hiddenSize; j++)
                weights1[i, j] = (float)(rnd.NextDouble() * 2 - 1);

        for (int j = 0; j < hiddenSize; j++)
        {
            biases1[j] = (float)(rnd.NextDouble() * 2 - 1);
            for (int k = 0; k < outputSize; k++)
                weights2[j, k] = (float)(rnd.NextDouble() * 2 - 1);
        }

        for (int k = 0; k < outputSize; k++)
            biases2[k] = (float)(rnd.NextDouble() * 2 - 1);
    }

    private float[] Softmax(float[] logits)
    {
        float maxLogit = Mathf.Max(logits);
        float sumExp = 0f;
        float[] exp = new float[logits.Length];
        for (int i = 0; i < logits.Length; i++)
        {
            exp[i] = Mathf.Exp(logits[i] - maxLogit);
            sumExp += exp[i];
        }

        for (int i = 0; i < logits.Length; i++)
            exp[i] /= sumExp;

        return exp;
    }

    public int Predict(float[] inputs)
    {
        for (int j = 0; j < hiddenSize; j++)
        {
            float sum = biases1[j];
            for (int i = 0; i < inputSize; i++)
                sum += inputs[i] * weights1[i, j];
            hidden[j] = Mathf.Tan(sum);
        }

        float[] output = new float[outputSize];
        for (int k = 0; k < outputSize; k++)
        {
            float sum = biases2[k];
            for (int j = 0; j < hiddenSize; j++)
                sum += hidden[j] * weights2[j, k];
            output[k] = sum;
        }

        float[] probs = Softmax(output);
        int bestIndex = 0;
        float bestValue = probs[0];
        for (int i = 1; i < probs.Length; i++)
        {
            if (probs[i] > bestValue)
            {
                bestValue = probs[i];
                bestIndex = i;
            }
        }

        return bestIndex; // 0 - patrol, 1 - chase, 2 - attack, 3 - flee
    }
}
