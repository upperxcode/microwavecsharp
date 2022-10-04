using System;

namespace Benner
{
    public class MicroWaveException : Exception
    {
        public MicroWaveException() : base()
        {
            Console.WriteLine("erro de execução!");
        }

        public MicroWaveException(string mensagem) : base(mensagem)
        {
            Console.WriteLine($"erro: {mensagem}");
        }

        public MicroWaveException(string mensagem, Exception exception) : base(mensagem)
        {
            Console.WriteLine($"erro: {mensagem}\n{exception}");
        }
    }

    public class StateErrorMicroWaveException : MicroWaveException
    {
        public StateErrorMicroWaveException() : base() { }

        public StateErrorMicroWaveException(string mensagem) : base(mensagem) { }

        public StateErrorMicroWaveException(string mensagem, Exception exception)
            : base(mensagem, exception) { }
    }

    public class RangeErrorMicroWaveException : MicroWaveException
    {
        public RangeErrorMicroWaveException() : base() { }

        public RangeErrorMicroWaveException(string mensagem) : base(mensagem) { }

        public RangeErrorMicroWaveException(string mensagem, Exception exception)
            : base(mensagem, exception) { }
    }

    public class FileErrorMicroWaveException : MicroWaveException
    {
        public FileErrorMicroWaveException() : base() { }

        public FileErrorMicroWaveException(string mensagem) : base(mensagem) { }

        public FileErrorMicroWaveException(string mensagem, Exception exception)
            : base(mensagem, exception) { }
    }
}
