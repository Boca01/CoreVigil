# Changelog

Todos los cambios notables de CoreVigil se documentaran en este archivo.

El formato esta basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/).

No se asignan versiones historicas que no hayan sido definidas formalmente. Los cambios actuales permanecen en Unreleased hasta que se establezca una version del proyecto.

## [Unreleased]

### Added

- HUD Mini para visualizacion compacta de CPU y RAM.
- LED dinamico en Mini para representar niveles de carga de CPU.
- Posiciones independientes para los modos Full y Mini.
- Persistencia del modo visual mediante IsMiniMode.
- Persistencia dedicada de FullLeft, FullTop, MiniLeft y MiniTop.
- Compatibilidad de persistencia con las coordenadas historicas Left y Top.
- Vista de Actividad CPU con modos AGRUPAR e INDIVIDUAL.
- Grupos desplegables con instancias y PID.
- Diagnostico interno basado en RuntimeSample, RuntimeDiagnosticsService y RuntimeLogWriter.
- Activacion del diagnostico y acceso a la carpeta de logs desde la bandeja.
- Iconos dinamicos de bandeja para estados Low, Medium, High y Critical.
- Inicio automatico configurable con Windows.
- Control de instancia unica de CoreVigil.
- Opcion `Opacidad...` en la bandeja del sistema.
- Selector temporal de opacidad con porcentaje y rango de 30 % a 100 %.
- Arrastre independiente del selector de opacidad mediante su cabecera.
- Persistencia de BackgroundOpacity.
- Persistencia de OpacityPopupHorizontalOffset y OpacityPopupVerticalOffset.
- VigilUI.Opacity.cs para separar la logica del selector y la aplicacion de opacidad.
- Proyecto `CoreVigil.Tests` integrado a la solucion.
- Suite automatizada inicial de 26 pruebas.
- Ocho pruebas para ProcessGroupingService.
- Seis pruebas para HudSettingsService.
- Cuatro pruebas para RuntimeDiagnosticsService.
- Ocho pruebas para RuntimeLogWriter.
- Rutas configurables en HudSettingsService y RuntimeLogWriter para ejecutar pruebas aisladas sin tocar configuracion ni logs reales.
- `SingleInstanceGuard` para adquirir/liberar el mutex de instancia única solo cuando se posee.
- Límite de 10.000 muestras en memoria para el diagnóstico y contador de muestras descartadas.
- Estado visible de error/reintento del diagnóstico en Full y en el tooltip de bandeja.
- Comprobación de errores Win32 al aplicar estilos de Alt+Tab.
- Fallback a la frecuencia actual reportada por Windows mientras PDH recopila un intervalo válido.
- Aceptación de los estados PDH `VALID_DATA` y `NEW_DATA` para evitar descartar muestras válidas.
- Pruebas para mutex, cancelación de procesos, capacidad del búfer y cálculo/fallback de frecuencia.
- Cuatro pruebas deterministas para el calculo de uso global de CPU.
- Cinco pruebas deterministas adicionales para el calculo de CPU por proceso, ampliando ProcessMonitorTests a seis pruebas.

### Changed

- La ventana principal fue reorganizada como CoreVigil.UI.VigilUI y dividida en parciales especializados.
- El HUD Full utiliza perfiles de layout adaptables.
- El cambio expandido/contraido de Actividad CPU conserva el borde inferior de la ventana.
- El Top 8 ahora parte de todas las instancias medidas disponibles antes de aplicar los limites de vista individual o agrupada.
- AGRUPAR suma el consumo de las instancias del mismo nombre antes de limitar la vista a ocho grupos.
- Full y Mini comparten el mismo MainViewModel y el mismo nucleo de monitorizacion.
- Full y Mini comparten el valor configurable de opacidad de fondo.
- El modo Mini deja de mostrarse en la barra de tareas y mantiene el acceso desde la bandeja del sistema.
- La enumeración de procesos se ejecuta fuera del hilo de UI, comprueba cancelación entre procesos y libera los objetos pendientes.
- La primera lectura de frecuencia deja de asumir 100 % de rendimiento y conserva frecuencias turbo por encima del máximo nominal.
- El calculo de uso global de CPU se separa de la adquisicion de tiempos de Windows para proteger su logica mediante pruebas deterministas.
- La opacidad se aplica al fondo visual del HUD sin reducir globalmente la opacidad de textos y controles.
- La Prioridad 0 de validacion prolongada Full/Mini queda completada.
- La Prioridad 1 de observabilidad y pruebas automatizadas queda completada con una linea base validada de 38/38 pruebas en verde.
- La Prioridad 2 de robustez de procesos inicia con el calculo de CPU por proceso aislado de la enumeracion del sistema y una linea base global validada de 43/43 pruebas en verde.
- La Prioridad 2 de robustez de procesos queda completada con una linea base global de 43/43 pruebas en verde y compilacion con 0 errores y 0 advertencias.
- La validacion de geometria y DPI queda completada en el entorno disponible de monitor unico; los escenarios reales multimonitor permanecen no ejecutados por limitacion del entorno.
- La experiencia Mini queda cerrada en el alcance actual por decision de diseño, conservandose como telemetria de un vistazo sin nuevos controles ni informacion adicional.

### Fixed

- Corregido el desplazamiento acumulativo del HUD al expandir y contraer Actividad CPU.
- Corregida la restauracion de posicion y estado expandido del HUD.
- Corregida la interferencia entre la posicion del modo Mini y la posicion del HUD Full.
- Corregido el recorte prematuro de procesos que podia impedir que el Top 8 agrupado representara los grupos con mayor consumo real de las muestras disponibles.
- Corregidas advertencias de nulabilidad en ProcessMonitor y en el ciclo de diagnostico de VigilUI sin suprimir CS8600 ni CS8602.

## Convenciones

Este changelog sigue las categorias recomendadas por Keep a Changelog cuando sean aplicables:
- Added
- Changed
- Deprecated
- Removed
- Fixed
- Security

Las secciones sin cambios notables se omiten para reducir ruido.
