using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading;
using System.Windows;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LABA2_14AST
{
    public partial class MainViewModelThread : ObservableObject
    {
        private readonly ArraySorter _sorter;
        private readonly SynchronizationContext _uiContext; 
        private int[] _originalArray; //оригинальный массив
        private CancellationTokenSource _cancellationTokenSource; //отмена операции
            
        
        private int _arraySize = 1000; //задано 1000 по дефолту
        public int ArraySize
        {
            get => _arraySize;
            set
            {
                if (_arraySize != value)
                {
                    _arraySize = value;
                    OnPropertyChanged(nameof(ArraySize));
                }
            }
        }

        private string _originalArrayString;
        public string OriginalArrayString
        {
            get => _originalArrayString;
            set
            {
                if (_originalArrayString != value)
                {
                    _originalArrayString = value;
                    OnPropertyChanged(nameof(OriginalArrayString));
                }
            }
        }

        private string _bubbleSortResult;
        public string BubbleSortResult
        {
            get => _bubbleSortResult;
            set
            {
                if (_bubbleSortResult != value)
                {
                    _bubbleSortResult = value;
                    OnPropertyChanged(nameof(BubbleSortResult));
                }
            }
        }

        private string _quickSortResult;
        public string QuickSortResult
        {
            get => _quickSortResult;
            set
            {
                if (_quickSortResult != value)
                {
                    _quickSortResult = value;
                    OnPropertyChanged(nameof(QuickSortResult));
                }
            }
        }

        private string _insertionSortResult;
        public string InsertionSortResult
        {
            get => _insertionSortResult;
            set
            {
                if (_insertionSortResult != value)
                {
                    _insertionSortResult = value;
                    OnPropertyChanged(nameof(InsertionSortResult));
                }
            }
        }

        
        private string _heapSortResult;
        public string HeapSortResult
        {
            get => _heapSortResult;
            set
            {
                if (_heapSortResult != value)
                {
                    _heapSortResult = value;
                    OnPropertyChanged(nameof(HeapSortResult));
                }
            }
        }

        private string _totalComparisons = "Общее число сравнений: 0";
        public string TotalComparisons
        {
            get => _totalComparisons;
            set
            {
                if (_totalComparisons != value)
                {
                    _totalComparisons = value;
                    OnPropertyChanged(nameof(TotalComparisons));
                }
            }
        }

        private bool _canGenerate = true;
        public bool CanGenerate
        {
            get => _canGenerate;
            set
            {
                if (_canGenerate != value)
                {
                    _canGenerate = value;
                    OnPropertyChanged(nameof(CanGenerate));
                }
            }
        }

        //потоки
        private int _selectedThreadCount = 2;//2 дефолт
        public int SelectedThreadCount
        {
            get => _selectedThreadCount;
            set
            {
                if (_selectedThreadCount != value)
                {
                    _selectedThreadCount = value;
                    OnPropertyChanged(nameof(SelectedThreadCount));
                }
            }
        }

        //инфо по времени
        private string _executionTimeInfo;
        public string ExecutionTimeInfo
        {
            get => _executionTimeInfo;
            set
            {
                if (_executionTimeInfo != value)
                {
                    _executionTimeInfo = value;
                    OnPropertyChanged(nameof(ExecutionTimeInfo));
                }
            }
        }

        
        private int _bubbleSortProgress;
        public int BubbleSortProgress
        {
            get => _bubbleSortProgress;
            set
            {
                if (_bubbleSortProgress != value)
                {
                    _bubbleSortProgress = value;
                    OnPropertyChanged(nameof(BubbleSortProgress));
                }
            }
        }

        private int _quickSortProgress;
        public int QuickSortProgress
        {
            get => _quickSortProgress;
            set
            {
                if (_quickSortProgress != value)
                {
                    _quickSortProgress = value;
                    OnPropertyChanged(nameof(QuickSortProgress));
                }
            }
        }

        private int _heapSortProgress;
        public int HeapSortProgress
        {
            get => _heapSortProgress;
            set
            {
                if (_heapSortProgress != value)
                {
                    _heapSortProgress = value;
                    OnPropertyChanged(nameof(HeapSortProgress));
                }
            }
        }

        
        private int _insertionSortProgress;
        public int InsertionSortProgress
        {
            get => _insertionSortProgress;
            set
            {
                if (_insertionSortProgress != value)
                {
                    _insertionSortProgress = value;
                    OnPropertyChanged(nameof(InsertionSortProgress));
                }
            }
        }

        // Команды
        public RelayCommand GenerateArrayCommand { get; private set; }
        public RelayCommand BubbleSortCommand { get; private set; }
        public RelayCommand QuickSortCommand { get; private set; }
        public RelayCommand InsertionSortCommand { get; private set; }
        public RelayCommand HeapSortCommand { get; private set; }
        public RelayCommand RunMultiThreadedSortCommand { get; private set; }
        public RelayCommand CancelAllCommand { get; private set; }
        public RelayCommand RunSharedArraySortCommand { get; private set; }

        // Конструктор
        public MainViewModelThread()
        {
            _sorter = new ArraySorter();

            //сохраняем контекст синхронизации уи потока
            _uiContext = SynchronizationContext.Current ?? new SynchronizationContext();

            //подписка на события завершения сортировки
            _sorter.BubbleSortCompleted += OnBubbleSortCompleted;
            _sorter.QuickSortCompleted += OnQuickSortCompleted;
            _sorter.InsertionSortCompleted += OnInsertionSortCompleted;
            _sorter.HeapSortCompleted += OnHeapSortCompleted;
            _sorter.ProgressChanged += OnProgressChanged;

            //инициализация команд
            GenerateArrayCommand = new RelayCommand(GenerateArray, CanGenerateArray);
            BubbleSortCommand = new RelayCommand(BubbleSort, CanSortBubble);
            QuickSortCommand = new RelayCommand(QuickSort, CanSortQuick);
            InsertionSortCommand = new RelayCommand(InsertionSort, CanSortInsertion);
            HeapSortCommand = new RelayCommand(HeapSort, CanSortHeap);
            RunMultiThreadedSortCommand = new RelayCommand(RunMultiThreadedSort, CanRunMultiThreadedSort);
            CancelAllCommand = new RelayCommand(CancelAll, CanCancel);
            RunSharedArraySortCommand = new RelayCommand(RunSharedArraySort, CanRunSharedArraySort);
        }

        
        private void GenerateArray()
        {
            try
            {
                _originalArray = _sorter.GenerateRandomArray(ArraySize);

                //выводим первые 20 элементов
                OriginalArrayString = "Исходный массив: " +
                    string.Join(", ", _originalArray, 0, Math.Min(20, _originalArray.Length)) +
                    (ArraySize > 20 ? "..." : "");

                //сброс предыдущий результатов
                BubbleSortResult = "";
                QuickSortResult = "";
                InsertionSortResult = "";
                HeapSortResult = "";
                TotalComparisons = "Общее число сравнений: 0";

                //сброс прогресса
                BubbleSortProgress = 0;
                QuickSortProgress = 0;
                InsertionSortProgress = 0;
                HeapSortProgress = 0;

                //обновляем состояние команд сортировок
                BubbleSortCommand.NotifyCanExecuteChanged();
                QuickSortCommand.NotifyCanExecuteChanged();
                InsertionSortCommand.NotifyCanExecuteChanged();
                HeapSortCommand.NotifyCanExecuteChanged();
                RunMultiThreadedSortCommand.NotifyCanExecuteChanged();
                RunSharedArraySortCommand.NotifyCanExecuteChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при генерации массива: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanGenerateArray()
        {
            return CanGenerate;
        }

        //пузЫрь
        private bool CanSortBubble()
        {
            return _originalArray != null &&
                   (BubbleSortResult == null || !BubbleSortResult.Contains("Сортируется..."));
        }

        private void BubbleSort()
        {
            try
            {
                // Создаём новый токен для этой операции
                _cancellationTokenSource = new CancellationTokenSource();
                CancelAllCommand.NotifyCanExecuteChanged();

                BubbleSortResult = "Сортируется...";
                BubbleSortProgress = 0;
                BubbleSortCommand.NotifyCanExecuteChanged();

                Thread thread = new Thread(() =>
                {
                    try
                    {
                        _sorter.BubbleSortWithProgress(_originalArray, "Bubble");
                    }
                    catch (Exception ex)
                    {
                        _uiContext.Post(_ =>
                        {
                            BubbleSortResult = $"Ошибка: {ex.Message}";
                            BubbleSortCommand.NotifyCanExecuteChanged();
                        }, null);
                    }
                });
                thread.Start(); //foreground potok
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запуске пузырьковой сортировки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                BubbleSortResult = "";
                BubbleSortCommand.NotifyCanExecuteChanged();
            }
        }

        //фастовая
        private bool CanSortQuick()
        {
            return _originalArray != null &&
                   (QuickSortResult == null || !QuickSortResult.Contains("Сортируется..."));
        }

        private void QuickSort()
        {
            try
            {
                QuickSortResult = "Сортируется...";
                QuickSortProgress = 0;
                QuickSortCommand.NotifyCanExecuteChanged();

                Thread thread = new Thread(() => _sorter.QuickSort(_originalArray));
                thread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запуске быстрой сортировки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                QuickSortResult = "";
                QuickSortCommand.NotifyCanExecuteChanged();
            }
        }

        //вставка с прогрессом
        private bool CanSortInsertion()
        {
            return _originalArray != null &&
                   (InsertionSortResult == null || !InsertionSortResult.Contains("Сортируется..."));
        }

        private void InsertionSort()
        {
            try
            {
                InsertionSortResult = "Сортируется...";
                InsertionSortProgress = 0;
                InsertionSortCommand.NotifyCanExecuteChanged();

                Thread thread = new Thread(() => _sorter.InsertionSortWithProgress(_originalArray, "Insertion"));
                thread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запуске сортировки вставками: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                InsertionSortResult = "";
                InsertionSortCommand.NotifyCanExecuteChanged();
            }
        }

        //пирамида
        private bool CanSortHeap()
        {
            return _originalArray != null &&
                   (HeapSortResult == null || !HeapSortResult.Contains("Сортируется..."));
        }

        private void HeapSort()
        {
            try
            {
                HeapSortResult = "Сортируется...";
                HeapSortProgress = 0;
                HeapSortCommand.NotifyCanExecuteChanged();

                Thread thread = new Thread(() => _sorter.HeapSort(_originalArray));
                thread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запуске пирамидальной сортировки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                HeapSortResult = "";
                HeapSortCommand.NotifyCanExecuteChanged();
            }
        }

        // Многопоточная сортировкач    
        private bool CanRunMultiThreadedSort()
        {
            return _originalArray != null;
        }

        private async void RunMultiThreadedSort()
        {
            try
            {
                ExecutionTimeInfo = "Выполняется...";
                var watch = System.Diagnostics.Stopwatch.StartNew();

                var tasks = new List<Task>();

                //получили колво потокв
                int threadCount = SelectedThreadCount;

                for (int i = 0; i < threadCount; i++)
                {
                    int threadIndex = i; //захват переменной в цикле
                    tasks.Add(Task.Run(() =>
                    {
                        //копир массив для каждого потока
                        int[] arrayCopy = new int[_originalArray.Length];
                        Array.Copy(_originalArray, arrayCopy, _originalArray.Length);

                        Array.Sort(arrayCopy);

                        System.Diagnostics.Debug.WriteLine($"Поток {threadIndex + 1} завершил сортировку");
                    }));
                }

                await Task.WhenAll(tasks);
                watch.Stop();

                ExecutionTimeInfo = $"Время выполнения с {threadCount} потоками: {watch.Elapsed.TotalMilliseconds:F2} мс";

                MessageBox.Show($"Сортировка с {threadCount} потоками завершена за {watch.Elapsed.TotalMilliseconds:F2} мс",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //сорт на общем массиве
        private bool CanRunSharedArraySort()
        {
            return _originalArray != null;
        }

        //Сортировка на общем массиве с синхронизацией
        private void RunSharedArraySort()
        {
            try
            {
                //созд копию массива для сорт
                int[] sharedArray = new int[_originalArray.Length];
                Array.Copy(_originalArray, sharedArray, _originalArray.Length);

                object lockObject = new object();
                var watch = System.Diagnostics.Stopwatch.StartNew();

                //запуск нескольких потоков для сорт на общем массиве
                var threads = new List<Thread>();
                for (int i = 0; i < SelectedThreadCount; i++)
                {
                    Thread thread = new Thread(() =>
                    {
                        _sorter.BubbleSortOnSharedArray(sharedArray, lockObject);
                    });
                    threads.Add(thread);
                    thread.Start();
                }

                foreach (var thread in threads)
                {
                    thread.Join();
                }

                watch.Stop();

                MessageBox.Show($"Сортировка на общем массиве с {SelectedThreadCount} потоками завершена за {watch.Elapsed.TotalMilliseconds:F2} мс\n" +
                    "Проверьте корректность сортировки в консоли отладки!", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //отмена
        private bool CanCancel()
        {
            return _cancellationTokenSource != null;
        }

        private void CancelAll()
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                MessageBox.Show("Команда на отмену отправлена всем потокам", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отмене: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчики событий завершения
        private void OnBubbleSortCompleted(int[] sortedArray, long comparisons, double elapsedMs)
        {
            _uiContext.Post(_ =>
            {
                try
                {
                    BubbleSortResult = $"Пузырьковая: {FormatArray(sortedArray)}, время: {elapsedMs:F2} мс, сравнений: {comparisons}";
                    BubbleSortProgress = 100;
                    UpdateTotalComparisons();
                    BubbleSortCommand.NotifyCanExecuteChanged();

                    // Сбрасываем токен отмены
                    _cancellationTokenSource = null;
                    CancelAllCommand.NotifyCanExecuteChanged();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обновлении UI: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }, null);
        }

        private void OnQuickSortCompleted(int[] sortedArray, long comparisons, double elapsedMs)
        {
            _uiContext.Post(_ =>
            {
                try
                {
                    QuickSortResult = $"Быстрая: {FormatArray(sortedArray)}, время: {elapsedMs:F2} мс, сравнений: {comparisons}";
                    QuickSortProgress = 100;
                    UpdateTotalComparisons();
                    QuickSortCommand.NotifyCanExecuteChanged();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обновлении UI: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }, null);
        }

        private void OnInsertionSortCompleted(int[] sortedArray, long comparisons, double elapsedMs)
        {
            _uiContext.Post(_ =>
            {
                try
                {
                    InsertionSortResult = $"Вставками: {FormatArray(sortedArray)}, время: {elapsedMs:F2} мс, сравнений: {comparisons}";
                    InsertionSortProgress = 100;
                    UpdateTotalComparisons();
                    InsertionSortCommand.NotifyCanExecuteChanged();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обновлении UI: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }, null);
        }

        private void OnHeapSortCompleted(int[] sortedArray, long comparisons, double elapsedMs)
        {
            _uiContext.Post(_ =>
            {
                try
                {
                    HeapSortResult = $"Пирамидальная: {FormatArray(sortedArray)}, время: {elapsedMs:F2} мс, сравнений: {comparisons}";
                    HeapSortProgress = 100;
                    UpdateTotalComparisons();
                    HeapSortCommand.NotifyCanExecuteChanged();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обновлении UI: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }, null);
        }

        //просто обновляем соответствующий прогресс в UI-потоке.
        private void OnProgressChanged(string sortName, int progress)
        {
            _uiContext.Post(_ =>
            {
                switch (sortName)
                {
                    case "Bubble":
                        BubbleSortProgress = progress;
                        break;
                    case "Quick":
                        QuickSortProgress = progress;
                        break;
                    case "Heap":
                        HeapSortProgress = progress;
                        break;
                    case "Insertion":
                        InsertionSortProgress = progress;
                        break;
                }
            }, null);
        }

        private void UpdateTotalComparisons()
        {
            TotalComparisons = $"Общее число сравнений: {_sorter.TotalComparisons}";
        }

        private string FormatArray(int[] arr)
        {
            if (arr == null) return "";

            if (arr.Length <= 10)
                return string.Join(", ", arr);
            else
                return string.Join(", ", arr, 0, 5) + "...";
        }
    }
}