using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;


namespace WpfAsyncTest.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private bool _CanComputeExecute = true;

        private double _ComputeProgress;
        public double ComputeProgress
        {
            get => _ComputeProgress;
            set => Set(ref _ComputeProgress, value);
        }

        private int? _Result;
        public int? Result
        {
            get => _Result;
            set => Set(ref _Result, value);
        }

        public ICommand ComputeSummCommand { get; }
        public ICommand CancelSummCalculationCommand { get; }
        public MainViewModel()
        {
            ComputeSummCommand = new RelayCommand(OnComputeSummCommandExecuted, CanComputeSummCommandExecute);
            CancelSummCalculationCommand = new RelayCommand(OnCancelSummCalculationCommandExecuted, CanCancelSummCalculationCommandExecute);
        }

        private bool CanCancelSummCalculationCommandExecute() => _ComputeSummCancellation != null;
        private void OnCancelSummCalculationCommandExecuted() => _ComputeSummCancellation?.Cancel();

        private bool CanComputeSummCommandExecute() => _CanComputeExecute;
        private CancellationTokenSource _ComputeSummCancellation;
        private async void OnComputeSummCommandExecuted()
        {
            _CanComputeExecute = false;

            IProgress<double> progress = new Progress<double>(percent => ComputeProgress = percent);
            var cancellation_token_source = new CancellationTokenSource();
            _ComputeSummCancellation = cancellation_token_source;

            var cancel = cancellation_token_source.Token;
            try
            {
                var result = await ComputeSummAsync(100, Progress: progress, Cancel: cancel);
                Result = result;
            }
            catch (OperationCanceledException e)
            {
                Result = null;
                progress.Report(0);
                
            }
            _CanComputeExecute = true;
            _ComputeSummCancellation = null;
            CommandManager.InvalidateRequerySuggested();
        }

        private static Task<int> ComputeSummAsync(int N, int Timeout = 75,
            IProgress<double> Progress = null, CancellationToken Cancel = default)
        {
            return Task.Run(() => ComputeSumm(N, Timeout, Progress, Cancel));
        }

        private static int ComputeSumm(int N, int Timeout = 75,
            IProgress<double> Progress = null, CancellationToken Cancel = default)
        {
            var result = 0;
            for (var i = 0; i <= N; i++)
            {
                result += i;
                Thread.Sleep(Timeout);
                Progress?.Report(i / (double)N);

                Cancel.ThrowIfCancellationRequested();

            }

            Progress?.Report(1);
            return result;
        }

    }
}