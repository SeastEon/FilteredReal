using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ThreadDataRequester : MonoBehaviour {

    static ThreadDataRequester instance;
    Queue<ThreadInfo> datQaueue = new Queue<ThreadInfo>();

    void Awake() {
        instance = FindObjectOfType<ThreadDataRequester>();
    }

    public static void RequestData(Func<object> generateData, Action<object> callback)  {
        ThreadStart threadstart = delegate { instance.DataThread(generateData, callback); };
        new Thread(threadstart).Start();
    }

    void DataThread(Func<object> generateData, Action<object> callback){
        object data = generateData();
        lock (datQaueue){
            datQaueue.Enqueue(new ThreadInfo(callback, data));
        }
    }

    void Update() {
        if (datQaueue.Count > 0){
            for (int i = 0; i < datQaueue.Count; i++){
                ThreadInfo threadInfo = datQaueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    struct ThreadInfo {
        public readonly Action<object> callback;
        public readonly object parameter;

        public ThreadInfo(Action<object> callback, object parameter) {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}
