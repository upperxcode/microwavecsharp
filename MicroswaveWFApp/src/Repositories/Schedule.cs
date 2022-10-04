using System;

namespace Benner
{
    class Schedule
    {
        public Schedule(String name, String instruction, char character, int time, int potency)
        {
            this.Name = name;
            this.Instruction = instruction;
            this.Character = character;
            this.Time = time;
            this.Potency = potency;
        }

        public String Print()
        {
            String vStr = "----------------------------------------------\n";
            vStr += $" Nome: {Name}\n";
            vStr += $" Instrunção: {Instruction}\n";
            vStr += $" Tempo de preparo: {Time}\n";
            vStr += $" Potência: {Potency}\n";
            vStr += $" Caracter de aquecimento: {Character}\n";
            vStr += "----------------------------------------------\n";
            return vStr;
        }

        public bool IsCompatible(String nameProgram)
        {
            return nameProgram.ToLower().Contains(Name.ToLower());
        }

        public String Name { get; set; }

        public String Instruction { get; set; }

        public char Character { get; set; }

        public int Time { get; set; }

        public int Potency { get; set; }
    }
}
