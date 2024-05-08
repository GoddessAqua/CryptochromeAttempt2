using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Cryptochrome
{
    [Serializable]//https://docs.microsoft.com/ru-ru/dotnet/csharp/programming-guide/concepts/serialization/
    //в данном случае используется для записи информации в бинарный файл
    class Matrices
    {
        //матрицы вероятностей
        public float[] level0;//одиночные буквы
        public float[,] level1;//пары букв-биграммы
        public float[,,] level2;//тройки букв-триграммы
        public float[,,,] level3;//четвёрки букв-тетраграммы
        public readonly Dictionary<char, int> letterNumbers;//словарик

        public Matrices(char[] chromosome)//конструктор класса
        {
            //матрицы вероятностей
            level0 = new float[chromosome.Length];
            level1 = new float[chromosome.Length, chromosome.Length];
            level2 = new float[chromosome.Length, chromosome.Length, chromosome.Length];
            level3 = new float[chromosome.Length, chromosome.Length, chromosome.Length, chromosome.Length];
            letterNumbers = new Dictionary<char, int>(chromosome.Length);
            for (int i = 0; i < chromosome.Length; i++)
                letterNumbers.Add(chromosome[i], i);
        }

        public void Fill(string[] text)//массив строчек-это текст
        {
            //матрицы частоты встречаемости
            int[] level0Mass = new int[level0.Length];//хранят число заданных одиночных букв в тексте 
            int[,] level1Mass = new int[level0.Length, level0.Length];//хранят число пар букв в тексте - число биграмм
            int[,,] level2Mass = new int[level0.Length, level0.Length, level0.Length];//хранят число троек букв в тексте - число триграмм
            int[,,,] level3Mass = new int[level0.Length, level0.Length, level0.Length, level0.Length];//хранят число четырёх в тексте - число тетраграмм

            for (int i = 0; i < text.Length; i++)//цикл строчек всего текста
            {
                text[i] = text[i].ToUpper();//переводим буквы текста в заглавные
                for (int j = 0; j < text[i].Length; j++)//цикл для конкретного символа строки
                {
                    char currChar;
                    char.TryParse(text[i].Substring(j, 1), out currChar);
                    if (letterNumbers.TryGetValue(currChar, out int letterNumber0))
                    {
                        level0Mass[letterNumber0]++;//записываем букву и её порядковый номер в словаре

                        if (j + 1 < text[i].Length)//для того,чтобы проверить,что следующий символ существует
                        {
                            char.TryParse(text[i].Substring(j + 1, 1), out currChar);//по аналогии с одиночными буквами
                            if (letterNumbers.TryGetValue(currChar, out int letterNumber1))
                            {
                                level1Mass[letterNumber0, letterNumber1]++;

                                if (j + 2 < text[i].Length)//проверка того,что существует третий символ 
                                {
                                    char.TryParse(text[i].Substring(j + 2, 1), out currChar);//по аналогии с биграммами
                                    if (letterNumbers.TryGetValue(currChar, out int letterNumber2))
                                    {
                                        level2Mass[letterNumber0, letterNumber1, letterNumber2]++;

                                        if (j + 3 < text[i].Length)//проверка того,что существует четвёртый символ 
                                        {
                                            char.TryParse(text[i].Substring(j + 3, 1), out currChar);//по аналогии с триграммами
                                            if (letterNumbers.TryGetValue(currChar, out int letterNumber3))
                                                level3Mass[letterNumber0, letterNumber1, letterNumber2, letterNumber3]++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            int level0MassMax = 0, level1MassMax = 0, level2MassMax = 0, level3MassMax = 0;//ищем вероятности

            for (int i = 0; i < level0.Length; i++)
            {
                if (level0Mass[i] > level0MassMax) level0MassMax = level0Mass[i];
                for (int j = 0; j < level0.Length; j++)
                {
                    if (level1Mass[i, j] > level1MassMax) level1MassMax = level1Mass[i, j];
                    for (int k = 0; k < level0.Length; k++)
                    {
                        if (level2Mass[i, j, k] > level2MassMax) level2MassMax = level2Mass[i, j, k];
                        for (int l = 0; l < level0.Length; l++)
                            if (level3Mass[i, j, k, l] > level3MassMax) level3MassMax = level3Mass[i, j, k, l];
                    }
                }
            }

            for (int i = 0; i < level0.Length; i++)
            {
                level0[i] = level0Mass[i] / (float)level0MassMax;
                for (int j = 0; j < level0.Length; j++)
                {
                    level1[i, j] = level1Mass[i, j] / (float)level1MassMax;
                    for (int k = 0; k < level0.Length; k++)
                    {
                        level2[i, j, k] = level2Mass[i, j, k] / (float)level2MassMax;
                        for (int l = 0; l < level0.Length; l++)
                            level3[i, j, k, l] = level3Mass[i, j, k, l] / (float)level3MassMax;
                    }
                }
            }
        }

        public void Save(string path)//сохраняем все матрицы в бинарные файлы
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, this);
            }
        }

        public static Matrices Load(string path)//загружаем из бинарного файла
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (Matrices)formatter.Deserialize(fs);
            }
        }
    }
}