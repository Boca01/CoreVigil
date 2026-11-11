using CoreVigil.Core.Models;
using CoreVigil.Services;

namespace CoreVigil.Tests.Services
{
    /// <summary>
    /// Verifica la agrupacion, suma y ordenamiento de procesos.
    /// </summary>
    public sealed class ProcessGroupingServiceTests
    {
        [Fact]
        public void GroupByName_ConListaVacia_DevuelveColeccionVacia()
        {
            var service =
                new ProcessGroupingService();

            ProcessSnapshot[] processes =
                Array.Empty<ProcessSnapshot>();

            IReadOnlyList<ProcessGroupSnapshot> result =
                service.GroupByName(processes);

            Assert.Empty(result);
        }

        [Fact]
        public void GroupByName_ConListaNula_LanzaArgumentNullException()
        {
            var service =
                new ProcessGroupingService();

            Assert.Throws<ArgumentNullException>(
                () => service.GroupByName(null!));
        }

        [Fact]
        public void GroupByName_MismoNombre_AgrupaLasInstancias()
        {
            var service =
                new ProcessGroupingService();

            ProcessSnapshot[] processes =
            {
                new()
                {
                    ProcessId = 10,
                    Name = "chrome",
                    CpuUsagePercent = 5.0
                },
                new()
                {
                    ProcessId = 20,
                    Name = "chrome",
                    CpuUsagePercent = 7.0
                }
            };

            IReadOnlyList<ProcessGroupSnapshot> result =
                service.GroupByName(processes);

            ProcessGroupSnapshot group =
                Assert.Single(result);

            Assert.Equal(
                "chrome",
                group.Name);

            Assert.Equal(
                2,
                group.InstanceCount);

            Assert.Equal(
                12.0,
                group.CpuUsagePercent,
                precision: 6);
        }

        [Fact]
        public void GroupByName_ConCpuTotalIgual_OrdenaGruposPorNombre()
        {
            var service =
                new ProcessGroupingService();

            ProcessSnapshot[] processes =
            {
        new()
        {
            ProcessId = 10,
            Name = "Zulu",
            CpuUsagePercent = 5.0
        },
        new()
        {
            ProcessId = 20,
            Name = "Alpha",
            CpuUsagePercent = 2.0
        },
        new()
        {
            ProcessId = 30,
            Name = "Alpha",
            CpuUsagePercent = 3.0
        }
    };

            IReadOnlyList<ProcessGroupSnapshot> result =
                service.GroupByName(processes);

            Assert.Equal(
                new[] { "Alpha", "Zulu" },
                result
                    .Select(group => group.Name)
                    .ToArray());

            Assert.Equal(
                5.0,
                result[0].CpuUsagePercent,
                precision: 6);

            Assert.Equal(
                5.0,
                result[1].CpuUsagePercent,
                precision: 6);
        }

        [Fact]
        public void GroupByName_IgnoraMayusculasYMinusculas()
        {
            var service =
                new ProcessGroupingService();

            ProcessSnapshot[] processes =
            {
                new()
                {
                    ProcessId = 10,
                    Name = "chrome",
                    CpuUsagePercent = 1.0
                },
                new()
                {
                    ProcessId = 20,
                    Name = "Chrome",
                    CpuUsagePercent = 2.0
                },
                new()
                {
                    ProcessId = 30,
                    Name = "CHROME",
                    CpuUsagePercent = 3.0
                }
            };

            IReadOnlyList<ProcessGroupSnapshot> result =
                service.GroupByName(processes);

            ProcessGroupSnapshot group =
                Assert.Single(result);

            Assert.Equal(
                3,
                group.InstanceCount);

            Assert.Equal(
                6.0,
                group.CpuUsagePercent,
                precision: 6);
        }

        [Fact]
        public void GroupByName_OrdenaInstanciasPorCpuDescendente()
        {
            var service =
                new ProcessGroupingService();

            ProcessSnapshot[] processes =
            {
                new()
                {
                    ProcessId = 10,
                    Name = "App",
                    CpuUsagePercent = 2.0
                },
                new()
                {
                    ProcessId = 20,
                    Name = "App",
                    CpuUsagePercent = 9.0
                },
                new()
                {
                    ProcessId = 30,
                    Name = "App",
                    CpuUsagePercent = 5.0
                }
            };

            IReadOnlyList<ProcessGroupSnapshot> result =
                service.GroupByName(processes);

            ProcessGroupSnapshot group =
                Assert.Single(result);

            Assert.Equal(
                new[] { 20, 30, 10 },
                group.Processes
                    .Select(process => process.ProcessId)
                    .ToArray());
        }

        [Fact]
        public void GroupByName_ConCpuIgual_OrdenaInstanciasPorProcessId()
        {
            var service =
                new ProcessGroupingService();

            ProcessSnapshot[] processes =
            {
                new()
                {
                    ProcessId = 30,
                    Name = "App",
                    CpuUsagePercent = 5.0
                },
                new()
                {
                    ProcessId = 10,
                    Name = "App",
                    CpuUsagePercent = 5.0
                },
                new()
                {
                    ProcessId = 20,
                    Name = "App",
                    CpuUsagePercent = 5.0
                }
            };

            IReadOnlyList<ProcessGroupSnapshot> result =
                service.GroupByName(processes);

            ProcessGroupSnapshot group =
                Assert.Single(result);

            Assert.Equal(
                new[] { 10, 20, 30 },
                group.Processes
                    .Select(process => process.ProcessId)
                    .ToArray());
        }

        [Fact]
        public void GroupByName_OrdenaGruposPorCpuTotalDescendente()
        {
            var service =
                new ProcessGroupingService();

            ProcessSnapshot[] processes =
            {
                new()
                {
                    ProcessId = 10,
                    Name = "Alpha",
                    CpuUsagePercent = 5.0
                },
                new()
                {
                    ProcessId = 20,
                    Name = "Beta",
                    CpuUsagePercent = 8.0
                },
                new()
                {
                    ProcessId = 30,
                    Name = "Alpha",
                    CpuUsagePercent = 5.0
                }
            };

            IReadOnlyList<ProcessGroupSnapshot> result =
                service.GroupByName(processes);

            Assert.Equal(
                new[] { "Alpha", "Beta" },
                result
                    .Select(group => group.Name)
                    .ToArray());

            Assert.Equal(
                10.0,
                result[0].CpuUsagePercent,
                precision: 6);

            Assert.Equal(
                8.0,
                result[1].CpuUsagePercent,
                precision: 6);
        }
    }
}