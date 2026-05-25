using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace LABA2_14AST
{
    public class ArraySorter
    {
       
        private long _totalComparisons; //накап общ колво сравнений
        private readonly object _locker = new object(); //синхра

        
        public delegate void SortCompletedHandler(int[] sortedArray, long comparisons, double elapsedMilliseconds); //делагат + шаблон для методоов
        public event SortCompletedHandler BubbleSortCompleted; 
        public event SortCompletedHandler QuickSortCompleted;
        public event SortCompletedHandler InsertionSortCompleted;
        public event SortCompletedHandler HeapSortCompleted;

        
        public delegate void ProgressChangedHandler(string sortName, int progress);
        public event ProgressChangedHandler ProgressChanged;

        
        public long TotalComparisons => _totalComparisons; //доступ к общ счетчику

        
        public int[] GenerateRandomArray(int size) //созд случ массива
        {
            Random rand = new Random();
            int[] array = new int[size];
            for (int i = 0; i < size; i++)
                array[i] = rand.Next(1000); //0 999
            return array;
        }

        
        private int[] CopyArray(int[] source) //кажд поток получает копию исходного массива чтобы не мешать другим потокам
        {
            int[] copy = new int[source.Length];
            Array.Copy(source, copy, source.Length);
            return copy;
        }

        
        //сихнр
        //пузЫрь    
        public void BubbleSort(int[] originalArray)
        {
            int[] array = CopyArray(originalArray); //копия
            long comparisons = 0; //счетчик
            var watch = Stopwatch.StartNew(); //таймер

            for (int i = 0; i < array.Length - 1; i++)
            {
                for (int j = 0; j < array.Length - 1 - i; j++)
                {
                    comparisons++; //считаем каждое сравнение
                    if (array[j] > array[j + 1]) //если чето не так
                    {
                        //меняем местами
                        int temp = array[j];
                        array[j] = array[j + 1];
                        array[j + 1] = temp;
                    }
                }
            }

            watch.Stop();

            lock (_locker)
            {
                _totalComparisons += comparisons;
            }

            BubbleSortCompleted?.Invoke(array, comparisons, watch.Elapsed.TotalMilliseconds);
        }

        //пузЫрь с прогрессом
        public void BubbleSortWithProgress(int[] originalArray, string sortName)
        {
            int[] array = CopyArray(originalArray);
            long comparisons = 0;
            var watch = Stopwatch.StartNew();

            int totalIterations = array.Length * (array.Length - 1) / 2;
            int currentIteration = 0;
            int lastProgress = 0;

            for (int i = 0; i < array.Length - 1; i++)
            {
                for (int j = 0; j < array.Length - 1 - i; j++)
                {
                    comparisons++;
                    if (array[j] > array[j + 1])
                    {
                        int temp = array[j];
                        array[j] = array[j + 1];
                        array[j + 1] = temp;
                    }
                    currentIteration++;

                    // отправляем прогресс каждые ван просент
                    int progress = (int)((double)currentIteration / totalIterations * 100);
                    if (progress > lastProgress)
                    {
                        lastProgress = progress;
                        ProgressChanged?.Invoke(sortName, progress);
                    }
                }
            }

            watch.Stop();

            lock (_locker)
            {
                _totalComparisons += comparisons;
            }

            BubbleSortCompleted?.Invoke(array, comparisons, watch.Elapsed.TotalMilliseconds);
        }

        //пузЫрь с отменой
        public void BubbleSortWithCancellation(int[] originalArray, CancellationToken token)
        {
            int[] array = CopyArray(originalArray);
            long comparisons = 0;
            var watch = Stopwatch.StartNew();

            for (int i = 0; i < array.Length - 1; i++)
            {
                // проверка запроса отмены
                token.ThrowIfCancellationRequested();

                for (int j = 0; j < array.Length - 1 - i; j++)
                {
                    comparisons++;
                    if (array[j] > array[j + 1])
                    {
                        int temp = array[j];
                        array[j] = array[j + 1];
                        array[j + 1] = temp;
                    }
                }
            }

            watch.Stop();

            lock (_locker)
            {
                _totalComparisons += comparisons;
            }

            if (!token.IsCancellationRequested)
            {
                BubbleSortCompleted?.Invoke(array, comparisons, watch.Elapsed.TotalMilliseconds);
            }
        }

        //фастовая сортировка
        public void QuickSort(int[] originalArray)
        {
            int[] array = CopyArray(originalArray);
            long comparisons = 0;
            var watch = Stopwatch.StartNew();

            QuickSortRecursive(array, 0, array.Length - 1, ref comparisons);

            watch.Stop();

            lock (_locker)
            {
                _totalComparisons += comparisons;
            }

            QuickSortCompleted?.Invoke(array, comparisons, watch.Elapsed.TotalMilliseconds);
        }

        private void QuickSortRecursive(int[] arr, int left, int right, ref long comparisons) //реф лонг передается по ссылке чтобы рекурсивные вызовы могли менять 1 счетчик
        {
            if (left < right)
            {
                int pivotIndex = Partition(arr, left, right, ref comparisons);
                QuickSortRecursive(arr, left, pivotIndex - 1, ref comparisons);
                QuickSortRecursive(arr, pivotIndex + 1, right, ref comparisons);
            }
        }

        private int Partition(int[] arr, int left, int right, ref long comparisons)
        {
            int pivot = arr[right];
            int i = left - 1;

            for (int j = left; j < right; j++)
            {
                comparisons++;
                if (arr[j] < pivot)
                {
                    i++;
                    int temp = arr[i];
                    arr[i] = arr[j];
                    arr[j] = temp;
                }
            }

            int temp1 = arr[i + 1];
            arr[i + 1] = arr[right];
            arr[right] = temp1;

            return i + 1;
        }

        //вставка
        public void InsertionSort(int[] originalArray)
        {
            int[] array = CopyArray(originalArray);
            long comparisons = 0;
            var watch = Stopwatch.StartNew();

            for (int i = 1; i < array.Length; i++)
            {
                int key = array[i];
                int j = i - 1;

                while (j >= 0 && array[j] > key)
                {
                    comparisons++;
                    array[j + 1] = array[j];
                    j--;
                }
                comparisons++; // учёт последнего сравнения когда условие не выполнено
                array[j + 1] = key;
            }

            watch.Stop();

            lock (_locker)
            {
                _totalComparisons += comparisons;
            }

            InsertionSortCompleted?.Invoke(array, comparisons, watch.Elapsed.TotalMilliseconds);
        }

        //вставка с прогрессом
        public void InsertionSortWithProgress(int[] originalArray, string sortName)
        {
            int[] array = CopyArray(originalArray);
            long comparisons = 0;
            var watch = Stopwatch.StartNew();

            int totalIterations = array.Length - 1;
            int lastProgress = 0;

            for (int i = 1; i < array.Length; i++)
            {
                int key = array[i];
                int j = i - 1;

                while (j >= 0 && array[j] > key)
                {
                    comparisons++;
                    array[j + 1] = array[j];
                    j--;
                }
                comparisons++;
                array[j + 1] = key;

                //отправляем прогресс
                int progress = (int)((double)i / totalIterations * 100);
                if (progress > lastProgress)
                {
                    lastProgress = progress;
                    ProgressChanged?.Invoke(sortName, progress);
                }
            }

            watch.Stop();

            lock (_locker)
            {
                _totalComparisons += comparisons;
            }

            InsertionSortCompleted?.Invoke(array, comparisons, watch.Elapsed.TotalMilliseconds);
        }

        //пирамида
        public void HeapSort(int[] originalArray)
        {
            int[] array = CopyArray(originalArray);
            long comparisons = 0;
            var watch = Stopwatch.StartNew();

            int n = array.Length;

            // перегруппировка массива(куча)
            for (int i = n / 2 - 1; i >= 0; i--)
                Heapify(array, n, i, ref comparisons);

            // по очереди извлекаем из кучи
            for (int i = n - 1; i >= 0; i--)
            {
                //перемещение тк корня в конец
                int temp = array[0];
                array[0] = array[i];
                array[i] = temp;

                //вызов heapify на уменьшенной куче
                Heapify(array, i, 0, ref comparisons);
            }

            watch.Stop();

            lock (_locker)
            {
                _totalComparisons += comparisons;
            }

            HeapSortCompleted?.Invoke(array, comparisons, watch.Elapsed.TotalMilliseconds);
        }

        private void Heapify(int[] arr, int n, int i, ref long comparisons)
        {
            int largest = i; // Инициализируем наибольший элемент как корень
            int left = 2 * i + 1; 
            int right = 2 * i + 2; 

            
            if (left < n)
            {
                comparisons++;
                if (arr[left] > arr[largest])
                    largest = left;
            }

            
            if (right < n)
            {
                comparisons++;
                if (arr[right] > arr[largest])
                    largest = right;
            }

            //if самый большой элемент не корень
            if (largest != i)
            {
                int swap = arr[i];
                arr[i] = arr[largest];
                arr[largest] = swap;

                // Рекурсивно преобразуем в двоичную кучу затронутое поддерево
                Heapify(arr, n, largest, ref comparisons);
            }
        }

            
        public void BubbleSortOnSharedArray(int[] sharedArray, object lockObject)
        {
            long comparisons = 0;
            var watch = Stopwatch.StartNew();

            for (int i = 0; i < sharedArray.Length - 1; i++)
            {
                for (int j = 0; j < sharedArray.Length - 1 - i; j++)
                {
                    lock (lockObject)
                    {
                        comparisons++;
                        if (sharedArray[j] > sharedArray[j + 1])
                        {
                            int temp = sharedArray[j];
                            sharedArray[j] = sharedArray[j + 1];
                            sharedArray[j + 1] = temp;
                        }
                    }
                }
            }

            watch.Stop();

            lock (_locker)
            {
                _totalComparisons += comparisons;
            }
        }

        

        
        //асинхр
        public Task<SortResult> BubbleSortAsync(int[] originalArray)
        {
            return Task.Run(() =>
            {
                int[] array = CopyArray(originalArray);
                long comparisons = 0;
                var watch = Stopwatch.StartNew();

                for (int i = 0; i < array.Length - 1; i++)
                {
                    for (int j = 0; j < array.Length - 1 - i; j++)
                    {
                        comparisons++;
                        if (array[j] > array[j + 1])
                        {
                            int temp = array[j];
                            array[j] = array[j + 1];
                            array[j + 1] = temp;
                        }
                    }
                }

                watch.Stop();

                lock (_locker)
                {
                    _totalComparisons += comparisons;
                }

                return new SortResult
                {
                    SortedArray = array,
                    Comparisons = comparisons,
                    ElapsedMilliseconds = watch.Elapsed.TotalMilliseconds
                };
            });
        }

        public Task<SortResult> QuickSortAsync(int[] originalArray)
        {
            return Task.Run(() =>
            {
                int[] array = CopyArray(originalArray);
                long comparisons = 0;
                var watch = Stopwatch.StartNew();

                QuickSortRecursive(array, 0, array.Length - 1, ref comparisons);

                watch.Stop();

                lock (_locker)
                {
                    _totalComparisons += comparisons;
                }

                return new SortResult
                {
                    SortedArray = array,
                    Comparisons = comparisons,
                    ElapsedMilliseconds = watch.Elapsed.TotalMilliseconds
                };
            });
        }

        public Task<SortResult> InsertionSortAsync(int[] originalArray)
        {
            return Task.Run(() =>
            {
                int[] array = CopyArray(originalArray);
                long comparisons = 0;
                var watch = Stopwatch.StartNew();

                for (int i = 1; i < array.Length; i++)
                {
                    int key = array[i];
                    int j = i - 1;

                    while (j >= 0 && array[j] > key)
                    {
                        comparisons++;
                        array[j + 1] = array[j];
                        j--;
                    }
                    comparisons++;
                    array[j + 1] = key;
                }

                watch.Stop();

                lock (_locker)
                {
                    _totalComparisons += comparisons;
                }

                return new SortResult
                {
                    SortedArray = array,
                    Comparisons = comparisons,
                    ElapsedMilliseconds = watch.Elapsed.TotalMilliseconds
                };
            });
        }

        // НОВЫЙ АСИНХРОННЫЙ МЕТОД: Пирамидальная сортировка
        public Task<SortResult> HeapSortAsync(int[] originalArray)
        {
            return Task.Run(() =>
            {
                int[] array = CopyArray(originalArray);
                long comparisons = 0;
                var watch = Stopwatch.StartNew();

                int n = array.Length;

                for (int i = n / 2 - 1; i >= 0; i--)
                    Heapify(array, n, i, ref comparisons);

                for (int i = n - 1; i >= 0; i--)
                {
                    int temp = array[0];
                    array[0] = array[i];
                    array[i] = temp;
                    Heapify(array, i, 0, ref comparisons);
                }

                watch.Stop();

                lock (_locker)
                {
                    _totalComparisons += comparisons;
                }

                return new SortResult
                {
                    SortedArray = array,
                    Comparisons = comparisons,
                    ElapsedMilliseconds = watch.Elapsed.TotalMilliseconds
                };
            });
        }

        // НОВЫЙ АСИНХРОННЫЙ МЕТОД: Сортировка с прогрессом
        public async Task<SortResult> BubbleSortWithProgressAsync(int[] originalArray, IProgress<int> progress, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                int[] array = CopyArray(originalArray);
                long comparisons = 0;
                var watch = Stopwatch.StartNew();

                int totalIterations = array.Length * (array.Length - 1) / 2;
                int currentIteration = 0;
                int lastProgress = 0;

                for (int i = 0; i < array.Length - 1; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    for (int j = 0; j < array.Length - 1 - i; j++)
                    {
                        comparisons++;
                        if (array[j] > array[j + 1])
                        {
                            int temp = array[j];
                            array[j] = array[j + 1];
                            array[j + 1] = temp;
                        }
                        currentIteration++;

                        int currentProgress = (int)((double)currentIteration / totalIterations * 100);
                        if (currentProgress > lastProgress)
                        {
                            lastProgress = currentProgress;
                            progress?.Report(currentProgress);
                        }
                    }
                }

                watch.Stop();

                lock (_locker)
                {
                    _totalComparisons += comparisons;
                }

                return new SortResult
                {
                    SortedArray = array,
                    Comparisons = comparisons,
                    ElapsedMilliseconds = watch.Elapsed.TotalMilliseconds
                };
            }, cancellationToken);
        }

        
    }

    // Класс для результата сортировки (для Task версии)
    public class SortResult
    {
        public int[] SortedArray { get; set; }
        public long Comparisons { get; set; }
        public double ElapsedMilliseconds { get; set; }
    }
}