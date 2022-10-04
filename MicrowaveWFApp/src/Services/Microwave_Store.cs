using Benner;
using System;
using System.Collections.Generic;

namespace Store
{
    class MicroWaveStore
    {
        static readonly Microwave microWave = new();
        static public int Time { get; private set; }

        static private int potency = 10;

        public static int Potency
        {
            get => potency;
        }
        
        static public String Schedule { get; private set; }
        static public String State 
        { 
            get => microWave.State2String(); 
        }
        static public String StrTime { get; private set; }



    static public String SelectSchedule(String nameSchedule)
        {
            bool scheduleIsValid = microWave.SelectSchedule(nameSchedule);
            if (scheduleIsValid)
            {
                Time = microWave.Time;
                potency = microWave.Potency;
                Schedule = nameSchedule;
                return $"Selecionada a programação {microWave.ShowScheduled()}";
            }
            return $"A programação {nameSchedule} não é compatível";
        }

        static public String ShowProgress()
        {
            return microWave.ShowPogress;
        }

        static public bool InWarmingUp()
        {
            return microWave.State == MwState.mwWarmingUp;
        }

        static public void QuickStart()
        {
            if (InWarmingUp())
            {
                return;
            }
                Schedule = null;
            microWave.SelectSchedule("");
            microWave.QuickStart = true;

        }

        static public void Interrupt(bool cancel)
        {   
            if (!InWarmingUp()) 
            {
                return;
            }

            if (cancel)
            {
                Time = 0;
                Schedule = null;
                microWave.Cancel();
            } 
            else
            {
                Time = microWave.TimeLeft;
                microWave.Pause();
            }
        }

 

        static public int AddSeconds(int valor = 10)
        {
            Time += valor;
            microWave.Time = Time;
            return Time;
        }

        static public int RemoveSeconds()
        {
            Time -= 10;
            microWave.Time = Time;
            return Time;
        }

        static public int IncrementPontency()
        {
            return ++potency;
         }

        static public int DecrementPontency()
        {
            return --potency;
        }

        public int Seconds() => microWave.Time;

        static public String Open()
        {
            microWave.Open();
            UpdateAll();
            return "Porta aberta!";
        }

        static private void UpdateAll()
        {
            Time = microWave.Time;
            potency = microWave.Potency;
        }

        static public String Close()
        {
            microWave.Close();
            UpdateAll();
            return "Porta fechada!";
        }

        static public String TimeFormated()
        {
            if (microWave.State == MwState.mwCanceled)
            {
                Time = microWave.Time;
            }

            TimeSpan time = TimeSpan.FromSeconds(Time);

            return time.Minutes.ToString() + ":" + time.Seconds.ToString().PadLeft(2, '0');
        }

        static public void StrTimeFormat(char character)
        {
            
            StrTime += character;
            int min = 0;
            int sec;
            if (StrTime.Length == 4)
            {
                StrTime = StrTime.Remove(0, 1);
            }
            
            if (StrTime.Length == 3)
            {
                min = int.Parse(""+StrTime[0]);
                sec = int.Parse(StrTime.Remove(0,1));
            } 
            else 
            {
                sec = int.Parse(StrTime);
            }

            Time = min * 60 + sec;
            microWave.Time = Time;
            
        }

        static public List<String> ScheduleNames()
        {
            return microWave.ScheduleNames();
        }

        static public String ShowAllSchedule()
        {
            return microWave.ShowAllschedule();
        }

        static public String ShowSchedule(String nameSchedule)
        {
            return microWave.ShowSchedule(nameSchedule);
        }

        static public bool AddSchedule(String name, String instruction, char character, int time, int potency)
        {
            microWave.AddSchedule(name, instruction, character, time, potency);
            return true;
        }

        static public bool Start(Func<String, bool, bool> func) 
        {
            microWave.Time = Time;
            microWave.Potency = Potency;
            microWave.Prepare(Schedule);
            return microWave.Start((progress, status)=> 
            {
                Time = microWave.TimeLeft;
                potency = microWave.Potency;
                func(progress, status);
                return status;
                }
             
            );
           
        }
    }
}
