# Driver Matching

## Описание задачи
Карта — прямоугольная сетка **N*M** (клетки 1x1), в каждой клетке может находиться только один водитель.  
Требуется реализовать механизм подбора **5 ближайших** к заказу водителей.

Метрика расстояния: **квадрат Евклидова** `dx^2 + dy^2` (быстро, без `sqrt`).  
Tie-break при равных расстояниях: по `DriverId`.

## Алгоритмы
1. **FullScanMatcher** — полный перебор всех водителей (baseline).
2. **RingGridMatcher** — обход клеток расширяющимися квадратными кольцами.
3. **BucketGridMatcher** — пространственный индекс по блокам `B*B` и поиск по кольцам бакетов.

## Запуск тестов
```bash
dotnet test
```

## Запуск бенчмарков
```bash
dotnet run -c Release --project DriverMatching.Benchmarks
```

## Результаты BenchmarkDotNet
// * Summary *

BenchmarkDotNet v0.14.0, Ubuntu 24.04.3 LTS (Noble Numbat) (container)
AMD EPYC 7763, 1 CPU, 2 logical cores and 1 physical core
.NET SDK 10.0.100
  [Host]     : .NET 10.0.0 (10.0.25.52411), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.0 (10.0.25.52411), X64 RyuJIT AVX2


| Method     | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------- |-------------:|-----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| FullScan   |   444.291 us |  7.9617 us | 14.9541 us | 1.001 |    0.05 |      - |     200 B |        1.00 |
| RingGrid   |     1.696 us |  0.0333 us |  0.0601 us | 0.004 |    0.00 | 0.0401 |     674 B |        3.37 |
| BucketGrid | 1,390.240 us | 27.5698 us | 70.1738 us | 3.133 |    0.19 |      - |     145 B |        0.72 |
