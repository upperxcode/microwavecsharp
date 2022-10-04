using System;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Benner
{
    
    public class Microwave
    {
        private const int UnitTime = 1000;
        private const int MaxTime = 120;
        private const int MaxPotency = 10;
        private const char DefaultOutputChar = '.';

        
        public const String ClosedDoor = "Fechar porta";
        public const String OpenDoor = "Abrir porta";
        public const String StatePuased = "aquecimento pausado";
        public const String StateDone = "Aquecida";

        private const String StateStandBy = "Modo de espera";
        private const String StateWarmingUp = "Aquecendo";
        private const String StateCanceled = "Aquecido cancelado";

        private char characterOutput;

        public int TimeLeft { get; private set; }
        public MwState State { get; private set; }

        public int Time { get; set; }

        public int Potency { get; set; }

        public bool QuickStart { get; set; }

        private int scheduleIndex;

        public String ErrorMessage { get; private set; }

        private String FileName;
        private bool IsInputFile;
        StreamWriter OutPutFile = null;

        readonly List<Schedule> schedule;


        public Microwave()
        {
            schedule = new List<Schedule>();
            AddSchedule("descongelar", "instrucao", '-', 120, 7);
            AddSchedule("pipoca", "instrucao", '#', 100, 9);
            AddSchedule("batata", "instrucao", '@', 60, 10);
            AddSchedule("vegetais", "instrucao", '*', 120, 5);
            AddSchedule("arroz", "instrucao", '$', 120, 10);
            characterOutput = '.';
            ErrorMessage = "";
            FileName = null;
            Reset();
        }
        public void NotifyError(String mensagem)
        {
            ErrorMessage = mensagem;
            Console.WriteLine(mensagem);
        }

        public String ShowPogress
        {
            get
            {
                if (State == MwState.mwPaused)
                {
                    return StatePuased;
                }
                if (State == MwState.mwDone)
                {
                    return StateDone;
                }
                int repeatChar = 1;
                if (State == MwState.mwWarmingUp)
                {
                    repeatChar = Potency;
                }
                return "".PadLeft(repeatChar, characterOutput);
            }
        }

        private String ScheduledString()
        {
            TimeSpan time = TimeSpan.FromSeconds(Time);
            String formatedTime = "";
            if (time.Minutes > 0)
            {
                formatedTime +=
                    (time.Minutes > 1) ? $"{time.Minutes} minutos" : $"{time.Minutes} minuto";
            }
            if (time.Seconds > 0)
            {
                String str =
                    (time.Seconds > 1) ? $"{time.Seconds} segundos" : $"{time.Seconds} segundo";
                formatedTime += (formatedTime.Length > 0) ? $" e {str}" : str;
            }

            formatedTime = $"tempo: {formatedTime} | potencia: {Potency}";

            return (scheduleIndex >= 0)
                ? $"{schedule[scheduleIndex].Name} | {formatedTime}"
                : $"Programação avulsa | {formatedTime}";
        }

        public String ShowScheduled() => ScheduledString();

        public void Open() => State = MwState.mwOpenend;

        public void Close() => Reset();

        public void Pause()
        {
            State = MwState.mwPaused;
        }

        public void Cancel()
        {
            SelectSchedule("");
            State = MwState.mwCanceled;
        }

        public void Continue()
        {
            if (State == MwState.mwPaused)
            {
                State = MwState.mwWarmingUp;
            }
            if (State == MwState.mwCanceled)
            {
                Reset();
            }
        }

        

        private void WarmingUp()
        {
            State = MwState.mwWarmingUp;
            TimeLeft = Time;
        }

        public String State2String()
        {
            return State switch
            {
                MwState.mwWarmingUp => StateWarmingUp,
                MwState.mwPaused => StatePuased,
                MwState.mwOpenend => OpenDoor,
                MwState.mwStandBy => StateStandBy,
                MwState.mwDone => StateDone,
                MwState.mwCanceled => StateCanceled,
                _ => "estado desconhecido",
            };
        }

 

        /// <summary>
        /// Este método configura os parametros para o aquecimento.
        /// deve ser chamado antes de Start()
        /// <param name="schedule">Seleciona uma programação</param>
        /// <remarks>parametro opcional, se omitido faz a configuração sem programação associada.</remarks>
        /// <exception cref="StateErrorMicroWaveException"></exception>
        /// <exception cref="RangeErrorMicroWaveException"></exception>
        /// </summary>
        public void Prepare(String schedule = "")
        {
            if (State == MwState.mwCanceled && !QuickStart)
            {
                Time = 0;
                TimeLeft = 0;
                return;
            }

            if (State == MwState.mwPaused) 
            {
                State = MwState.mwWarmingUp;
                return;
            }

            if (State == MwState.mwOpenend || State == MwState.mwWarmingUp)
            {
                throw new StateErrorMicroWaveException(
                    $"o precesso não pode ser inciado no estado atual: {State2String()}."
                );
            }

            if (QuickStart && String.IsNullOrEmpty(schedule))
            {
                Time = 30;
                Potency = 8;
                characterOutput = DefaultOutputChar;
            }
            else if (schedule?.Length > 0)
            {
                SelectSchedule(schedule!);
            }

            if (Time < 1 || Time > 120)
            {
                throw new RangeErrorMicroWaveException(
                    $"o tempo deve estar entre 1 segundo e {MaxTime / 60} minutos.\nTempo atual {Time}"
                );
            }

            if (Potency < 1 || Potency > MaxPotency)
            {
                throw new RangeErrorMicroWaveException(
                    $"a potência deve estar entre 1 e 10.\nPotência atual {Potency}"
                );
            }

            IsValidFile(schedule);

            WarmingUp();
        }

        private static void Process()
        {
            Thread.Sleep(UnitTime);
        }

        /// <summary>
        /// Este método inicia o aquecimento, deve ser chamada depois de prepare(). 
        /// Obrigatoriamente como lambda:
        /// <example>
        /// Start((progress, process) =>
        /// {
        ///     Console.Write(progress);
        ///     return process;
        /// });
        /// </example>
        /// Para atualização da tela deve ser inserido dentro da 
        /// chamada uma função de processo de fila de messagens como DoEvents(). Se for 
        /// fornecido como entrada um nome de arquivo válido, ele é atualizado automaticamente
        /// </summary>
        public bool Start(Func<String, bool, bool> execute)
        {
            if (IsInputFile && (FileName != null) && (OutPutFile == null))
            {
                try
                {
                    OutPutFile = new StreamWriter(FileName, true);
                    OutPutFile!.WriteLine($"\n{ShowScheduled()} {DateTime.Now}");
                } 
                catch(Exception ex)
                {
                    throw new FileErrorMicroWaveException($"erro ao abrir o arquivo: {FileName}\n{ex.Message}");

                }
            }

            if (State != MwState.mwWarmingUp) //controla a pause o cancelamento
            {
                return false;
            }

            TimeLeft--;
            Process();

            var outPutProgress = ShowPogress;

            if (IsInputFile)
            {
                try
                {
                    OutPutFile!.Write(outPutProgress);
                }
               catch (Exception ex)
                {
                    throw new FileErrorMicroWaveException($"erro ao salvar no arquivo: {FileName}\n{ex.Message}");

                }
            }
            execute(outPutProgress, State != MwState.mwWarmingUp); //controla a pause o cancelamento
            if (TimeLeft > 0)
            {
                return Start(execute);
            }

            if (IsInputFile)
            {
                try
                {
                    OutPutFile!.WriteLine($"\nFim: {DateTime.Now}");
                    OutPutFile!.Flush();
                    OutPutFile!.Close();
                }
                catch (Exception ex)
                {
                    throw new FileErrorMicroWaveException($"erro no arquivo: {FileName}\n{ex.Message}");

                }
                //OutPutFile = null;
                IsInputFile = false;
                FileName = "";
            }
            State = MwState.mwDone;
            QuickStart = false;
            return false;
        }

       

        public List<String> ScheduleNames()
        {
            List<String> list = new List<String>();
            foreach(var item in schedule)
            {
                list.Add(item.Name);
            }
            return list;

        }


        public void AddSchedule(
            String name,
            String instruction,
            char character,
            int time,
            int potency
        )
        {
            schedule.Add(new Schedule(name, instruction, character, time, potency));
        }

        private Schedule ScheduleCompatible(String nameSchedule)
        {

            for(int index=0; index < schedule.Count; index++)
            {
                var item = schedule[index];
                if (item.IsCompatible(nameSchedule))
                {
                    scheduleIndex = index;
                    Console.WriteLine($"index => {index}");
                    return item;
                }
            }
            return null;
        }

        public String ShowSchedule(String nameSchedule)
        {
            Schedule _schedule = ScheduleCompatible(nameSchedule);
            return (_schedule != null)
                ? _schedule.Print()
                : $"Não existe um programa para {nameSchedule}";
        }

        public String ShowAllschedule()
        {
            String str = "";
            foreach (var item in schedule)
            {
                str += item.Print() + "\n";
            }
            return str;
        }


        private void IsValidFile(String nameSchedule)
        {
            IsInputFile = File.Exists(nameSchedule);
            if (IsInputFile)
            {
                FileName = nameSchedule;
            }
        }

        private String NameFileSearch(String nameSchedule)
        {
            string directory = Path.GetDirectoryName(nameSchedule);
            if (String.IsNullOrEmpty(directory))
            {
                return nameSchedule;
            }
            if (!File.Exists(nameSchedule))
            {
                throw new FileErrorMicroWaveException("A entrada é um arquivo, porém ele não existe!");
            }
            string fileWhithoutExt = Path.GetFileNameWithoutExtension(nameSchedule);
            return fileWhithoutExt.Replace("_", " ");
        }

        public bool SelectSchedule(String nameSchedule)
        {
            if (String.IsNullOrEmpty(nameSchedule))
            {
                scheduleIndex = -1;
                return false;

            }
            Schedule _schedule = ScheduleCompatible(NameFileSearch(nameSchedule));
            if (_schedule == null)
            {
                return false;
            }
            Time = _schedule.Time;
            Potency = _schedule.Potency;
            characterOutput = _schedule.Character;
            State = MwState.mwStandBy;
            return true;
        }

        private void Reset()
        {
            Time = 0;
            Potency = 10;
            QuickStart = false;
            scheduleIndex = -1;
            TimeLeft = 0;
            State = MwState.mwStandBy;
        }

    
    }
}
