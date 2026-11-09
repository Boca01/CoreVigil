# Arquitectura de CoreVigil

## Objetivo

CoreVigil mantiene separada la adquisicion de telemetria, el estado presentable, los servicios de plataforma y la interfaz WPF. El objetivo es que el HUD sea una representacion del estado monitorizado, no el propietario de la monitorizacion.

## Capas principales

### Core

Contiene contratos y modelos compartidos.

Responsabilidades:

- Definir abstracciones para monitores.
- Definir snapshots de CPU, memoria, frecuencia, procesos e informacion del procesador.
- Evitar que la UI dependa de detalles concretos de adquisicion.

### Monitoring

Implementa la lectura del sistema.

Componentes relevantes:

- `CpuMonitor`
- `CpuFrequencyMonitor`
- `MemoryMonitor`
- `ProcessMonitor`
- `SystemUptimeProvider`
- `WindowsProcessorInfoProvider`

`ProcessMonitor` calcula consumo reciente comparando tiempos acumulados de CPU entre muestras.

### Services

Servicios de soporte que no pertenecen directamente a la captura de telemetria.

Responsabilidades actuales:

- Agrupar procesos por nombre exacto.
- Seleccionar perfiles de layout.
- Guardar/restaurar preferencias del HUD.
- Gestionar el icono de bandeja.
- Gestionar el inicio automatico.

### Diagnostics

Observabilidad interna de CoreVigil.

Componentes:

- `RuntimeSample`
- `RuntimeDiagnosticsService`
- `RuntimeLogWriter`
- configuracion de diagnostico

Su funcion es medir el coste y estado de CoreVigil sin confundir esas metricas con la telemetria del sistema que la aplicacion presenta al usuario.

### ViewModels

`MainViewModel` coordina la adquisicion periodica y expone valores consumibles por WPF.

Entre otros datos expone:

- porcentaje global de CPU;
- frecuencia actual/maxima;
- RAM;
- uptime;
- Top 8 individual;
- Top 8 agrupado.

Para procesos, el estado actual mide primero todas las instancias disponibles y despues aplica los limites de Top 8 de manera independiente para las vistas individual y agrupada.

### UI

La ventana vigente es `CoreVigil.UI.VigilUI`.

La clase se divide en parciales para mantener responsabilidades acotadas:

- `VigilUI.xaml` / `VigilUI.xaml.cs`: composicion principal y ciclo de ventana.
- `VigilUI.ActivityPanel.cs`: expansion/contraccion de Actividad CPU y conservacion del borde inferior.
- `VigilUI.ProcessView.cs`: cambio entre AGRUPAR e INDIVIDUAL.
- `VigilUI.StatePersistence.cs`: restauracion y guardado del estado visual.
- `VigilUI.MiniMode.cs`: modo Mini, LED y transicion Full/Mini.

`MainWindow` pertenece a estados anteriores del proyecto y no es la arquitectura vigente.

## Flujo principal

```text
Windows
  |
Monitoring
  |
MainViewModel
  |----------------------|
  |                      |
VigilUI Full          VigilUI Mini
  |
Services / Persistence / Tray

Diagnostics observa la propia ejecucion de CoreVigil en paralelo.
```

## Decisiones vigentes

### Una sola fuente de monitorizacion

Full y Mini reutilizan el mismo `MainViewModel`. El modo Mini no inicia otro monitor de CPU o RAM.

### Full y Mini son modos, no ventanas independientes

Ambos viven dentro de `VigilUI`. Esto permite compartir Topmost, translucidez, DataContext y ciclo de vida.

### Posicion independiente por modo

Aunque ambos modos comparten ventana, las coordenadas Full y Mini se conservan por separado mediante estado dedicado.

### Top 8 despues de medir

El limite visual se aplica despues de obtener las muestras necesarias. Esto evita recortar instancias antes de agruparlas.

### Medir antes de optimizar

Las optimizaciones del nucleo deben justificarse con datos del diagnostico interno, especialmente CPU propia, Working Set, heap e intervalo de muestreo.
