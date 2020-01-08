using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralNetwork.NetworkTrainer.SupportClasses
{
    public class Permutator
    {
        private Random _gen = new Random();
        private int[] _index;

        public int this[int i]
        {
            get
            {
                return _index[i];
            }
        }
      
        public Permutator(int Size)
        {
            _index = new int[Size];

            for (int i = 0; i < Size; i++)
                _index[i] = i;

            Permute(Size);
        }

        public void Permute(int nTimes)
        {
            int i, j, t;

            for (int n = 0; n < nTimes; n++)
            {
                i = _gen.Next(_index.Length);
                j = _gen.Next(_index.Length);

                if (i != j)
                {
                    t = _index[i];
                    _index[i] = _index[j];
                    _index[j] = t;
                }
            }
        }
    }
}
