using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace MotionCaptureData
{
    public sealed class MotionCapture : MonoBehaviour
    {
        private static MotionCapture _instance;
        private static List<MotionItemData> _currentData = null;

        private static bool _isPlaying;
        private static bool _isRecording;
        private static float _currentTime;
        private static int _currentIdx;
        private static Action<MotionItemData> _cachedEvent;

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            if (_instance == null)
            {
                var go = new GameObject();
                go.name = "MotionCapture";
                _instance = go.AddComponent<MotionCapture>();
                DontDestroyOnLoad(_instance);
            }
        }
        
        private static void Load(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("[MOTIONCAP] Invalid path");
                return;
            }
            _instance.MCLoad(path);
        }

        public static void Play(Action<MotionItemData> OnPlayFrame, string path)
        {
            Init();
            Load(path);
            if (_currentData == null)
            {
                Debug.LogError("[MOTIONCAP] Data is Empty, call Load first");
                return;
            }
            if (_instance.MCLoad(path))
            {
                _cachedEvent = OnPlayFrame;
                _currentTime = 0f;
                _currentIdx = 0;
                _isPlaying = true;
            }
        }

        public static void StartRecording()
        {
            Init();
            _currentData = new List<MotionItemData>();
            _isRecording = true;
            _isPlaying = false;
            _currentTime = 0;
        }

        public static void Record(Dictionary<int, MotionNode> data)
        {
            Init();
            if (_isRecording)
            {
                var md = new MotionItemData()
                {
                    data = data,
                    playbacktime = _currentTime
                };
                _currentData.Add(md);
            }
        }

        public static void EndRecording(string path)
        {
            Init();
            if (_isRecording)
            {
                _isRecording = false;
                if (_currentData == null)
                {
                    Debug.LogError("[MOTIONCAP] Data is Empty");
                    return;
                }
                _instance.MCSave(path);
            }
        }

        private static void ProcessPlayback()
        {
            if (_currentData == null) return;
            if (_currentTime > _currentData[_currentIdx].playbacktime)
            {
                
                if (_currentIdx >= _currentData.Count-1)
                {
                    _isPlaying = false;
                }
                else
                {
                    _currentIdx++;
                    _cachedEvent?.Invoke(_currentData[_currentIdx]);
                }
            }
        }

        private void Update()
        {
            if (_isRecording)
            {
                _currentTime += Time.deltaTime;
            }
            else if (_isPlaying)
            {
                ProcessPlayback();
                _currentTime += Time.deltaTime;
            }
        }

        private void MCSave(string path)
        {
            if (_currentData == null)
            {
                Debug.LogError("[MOTIONCAP] Data is Empty, call Load first");
                return;
            }
            var json = JsonConvert.SerializeObject(_currentData);
            System.IO.File.WriteAllText(path, json);
        }

        private bool MCLoad(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                Debug.LogError("[MOTIONCAP] File doesnt exist");
                return false;
            }
            var json = System.IO.File.ReadAllText(path);
            _currentData = JsonConvert.DeserializeObject<List<MotionItemData>>(json);
            return true;
        }

    }

    public class MotionItemData
    {
        public Dictionary<int, MotionNode> data;
        public float playbacktime;
    }

    public sealed class MotionNode
    {
        public Vector3 postion;
        public Vector3 rotation;
    }
}