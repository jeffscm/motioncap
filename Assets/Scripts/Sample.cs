using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MotionCaptureData;

public class Sample : MonoBehaviour
{
    public Transform objectToPlay;
    public Transform objectToRecord;
    private bool _isRecording;

    public void Play()
    {
        MotionCapture.Play((d) => {
            objectToPlay.localPosition = d.data[0].postion;
            objectToPlay.localEulerAngles = d.data[0].rotation;
        }, Application.persistentDataPath + "/test.json");
    }

    public void Record()
    {
        MotionCapture.StartRecording();
        _isRecording = true;
    }

    private void SetData()
    {
        MotionCapture.Record(new Dictionary<int, MotionNode>()
        {
            {
                0,
                new MotionNode {
                    postion = objectToRecord.localPosition,
                    rotation = objectToRecord.localEulerAngles
                }
            }
        });
    }

    public void Stop()
    {
        _isRecording = false;
        MotionCapture.EndRecording(Application.persistentDataPath + "/test.json");
    }

    private void Update()
    {
        if (_isRecording)
        {
            SetData();
        }
    }
}
