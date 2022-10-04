using Benner;

namespace Microwave.Tests
{
    public class MicrowaveTest
    {
        readonly Benner.Microwave microwave = new();
        [Fact]
        public void SelectSchedule()
        {
            Assert.True(microwave.SelectSchedule("batata"));
            Assert.Equal(60, microwave.Time);
            Assert.Equal(10, microwave.Potency);
        }

        [Fact]
        public void SelectScheduleFalha()
        {
            Assert.False(microwave.SelectSchedule("carneDePanela"));
        }

        [Fact]
        public void SelectScheduleArquivo()
        {
            Assert.True(microwave.SelectSchedule("C:\\temp\\batata.txt"));
            Assert.Equal(60, microwave.Time);
            Assert.Equal(10, microwave.Potency);
        }

        [Fact]
        public void SelectScheduleArquivoNaoExiste()
        {
            var exception = Assert.Throws<FileErrorMicroWaveException>(() => microwave.SelectSchedule("C:\\temp\\batata1.txt"));
            Assert.Equal("A entrada é um arquivo, porém ele não existe!", exception.Message);
        }

        [Fact]
        public void Prepare()
        {
            microwave.Time = 120;
            microwave.Prepare();
            Assert.Equal(MwState.mwWarmingUp, microwave.State);
        }


        [Fact]
        public void PrepareWithSchedule()
        {
            microwave.Prepare("batata");
            Assert.Equal(MwState.mwWarmingUp, microwave.State);
            Assert.Equal(60, microwave.Time);
            Assert.Equal(10, microwave.Potency);
        }


        [Fact]
        public void PrepareWithoutTime()
        {
            var exception = Assert.Throws<RangeErrorMicroWaveException>(() => microwave.Prepare());
            Assert.Equal($"o tempo deve estar entre 1 segundo e 2 minutos.\nTempo atual {microwave.Time}", exception.Message);
        }

        [Fact]
        public void PrepareExceededPotency()
        {
            microwave.Time = 120;
            microwave.Potency = 11;
            var exception = Assert.Throws<RangeErrorMicroWaveException>(() => microwave.Prepare());
            Assert.Equal($"a potência deve estar entre 1 e 10.\nPotência atual {microwave.Potency}", exception.Message);
        }


        [Fact]
        public void Start()
        {
            PrepareWithSchedule();
            CommomStart();
            Assert.Equal(MwState.mwDone, microwave.State);
        }


        [Fact]
        public void StartWithPause()
        {
            PrepareWithSchedule();
            microwave.Start(
                     (progress, process) =>
                     {
                         if (microwave.TimeLeft == 20)
                         {
                             microwave.Pause();
                         }
                         return process;
                     }
                 );

            Assert.Equal(20, microwave.TimeLeft);
            Assert.Equal(MwState.mwPaused, microwave.State);

            PrepareWithSchedule();
            CommomStart();

            Assert.Equal(MwState.mwDone, microwave.State);

        }

        [Fact]
        public void StartWithCancel()
        {
            microwave.Prepare("batata");
            microwave.Start(
                     (progress, process) =>
                     {
                         if (microwave.TimeLeft == 20)
                         {
                             microwave.Cancel();
                         }
                         return process;
                     }
                 );

            Assert.Equal(20, microwave.TimeLeft);
            Assert.Equal(MwState.mwCanceled, microwave.State);

            microwave.Prepare("batata");
            CommomStart();
            Assert.Equal(MwState.mwCanceled, microwave.State);

        }

        private void CommomStart()
        {
            microwave.Start(
               (progress, process) =>
               {
                   return process;
               }
           );
        }


    }
}