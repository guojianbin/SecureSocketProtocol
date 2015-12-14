﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SecureSocketProtocol3.Utils
{
    public class TaskQueue<T>
    {
        private Action<T> callback;
        private Queue<T> tasks;
        private bool ThreadRunning = false;
        public uint MaxItems = 100;

        public TaskQueue(Action<T> Callback, uint MaxItems = 100)
        {
            this.tasks = new Queue<T>();
            this.callback = Callback;
            this.MaxItems = MaxItems;
        }

        public void Enqueue(T value)
        {
            lock (tasks)
            {
                while (tasks.Count > MaxItems && !ThreadRunning)
                    ExecuteTasks();

                tasks.Enqueue(value);
                if (!ThreadRunning)
                {
                    ThreadRunning = true;
                    ThreadPool.QueueUserWorkItem((object obj) => WorkerThread());
                }
            }
        }

        private void WorkerThread()
        {
            ExecuteTasks();
            ThreadRunning = false;
        }

        public void ClearTasks()
        {
            lock (tasks)
            {
                tasks.Clear();
            }
        }

        private void ExecuteTasks()
        {
            lock (tasks)
            {
                while (tasks.Count > 0)
                {
                    try
                    {
                        T obj;
                        lock (tasks)
                        {
                            obj = tasks.Dequeue();
                        }
                        callback(obj);
                    }
                    catch (Exception ex)
                    {
                        SysLogger.Log(ex.Message, SysLogType.Error, ex);

                    }
                }
            }
        }
    }
}
