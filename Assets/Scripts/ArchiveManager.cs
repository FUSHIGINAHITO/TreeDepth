using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class ArchiveManager : MonoBehaviour
{
    public bool debug;

    public static ArchiveManager instance => _instance;
    private static ArchiveManager _instance;

    [Serializable]
    public class Archive
    {
        public int maxLevel = -1;
        public int zenMaxScore = -1;
        public int zenMaxLevel = -1;
        public List<LevelData> levelData = new();

        public int MaxLevel
        {
            get => maxLevel;
            set
            {
                if (maxLevel < value)
                {
                    maxLevel = value;
                }
            }
        }

        public int ZenMaxScore
        {
            get => zenMaxScore;
            set
            {
                if (zenMaxScore < value)
                {
                    zenMaxScore = value;
                }
            }
        }

        public int ZenMaxLevel
        {
            get => zenMaxLevel;
            set
            {
                if (zenMaxLevel < value)
                {
                    zenMaxLevel = value;
                }
            }
        }

        public int? GetStep(int id)
        {
            id--;
            if (levelData.Count > id)
            {
                var step = levelData[id].MinStep;
                if (step < 0)
                {
                    return null;
                }
                return step;
            }

            return null;
        }

        public void SetStep(int id, int v)
        {
            id--;
            int add = id - levelData.Count;
            for (int i = 0; i <= add; i++)
            {
                levelData.Add(new());
            } 

            levelData[id].MinStep = v;
        }
    }

    [Serializable]
    public class LevelData
    {
        public int minStep = -1;

        public int MinStep
        {
            get => minStep;
            set
            {
                if (minStep < 0 || minStep > value)
                {
                    minStep = value;
                }
            }
        }
    }

    public Archive archive = null;

    private void Awake()
    {
        _instance = this;
        LoadArchive();

#if !UNITY_EDITOR
        debug = false;
#endif
    }

    private void LoadArchive()
    {
        if (!debug)
        {
            FileInfo m_file = new(Application.persistentDataPath + "/savings.txt");

            if (!m_file.Exists)
            {
                m_file.CreateText();
            }
            else
            {
                StreamReader sr = new(Application.persistentDataPath + "/savings.txt");
                string jsonData = sr.ReadToEnd();
                archive = JsonUtility.FromJson<Archive>(jsonData);
                sr.Close();
            }
        }

        archive ??= new();
    }

    public void SaveArchive()
    {
        FileInfo m_file = new(Application.persistentDataPath + "/savings.txt");

        if (!m_file.Exists)
        {
            m_file.CreateText();
        }

        StreamWriter sw = new(Application.persistentDataPath + "/savings.txt");
        sw.WriteLine(JsonUtility.ToJson(archive));
        sw.Close();
        sw.Dispose();
    }
}