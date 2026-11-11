# Diagnostico interno de CoreVigil

## Objetivo

El diagnostico interno existe para medir el comportamiento de CoreVigil durante sesiones reales y evitar optimizaciones basadas unicamente en percepcion.

## Componentes

### RuntimeSample

Representa una muestra de ejecucion.

La telemetria registrada incluye datos destinados a observar, entre otros aspectos:
- CPU propia de CoreVigil;
- Working Set;
- heap administrado;
- estado visible del HUD;
- estado Topmost;
- estado del icono de bandeja;
- intervalo de muestreo;
- datos del sistema observados que permitan contextualizar la sesion.

### RuntimeDiagnosticsService

Captura las muestras de diagnostico de la propia aplicacion.

### RuntimeLogWriter

Acumula y escribe muestras en sesiones CSV.

El flujo vigente realiza flush periodico y un flush final al detener el diagnostico limpiamente. El diagnostico mantiene sesiones CSV separadas y aplica la politica de retencion configurada para los logs.

## Activacion

El diagnostico se puede activar o desactivar desde la bandeja de CoreVigil. La bandeja tambien dispone de una accion para abrir la carpeta de logs.

## Principio de trabajo

CoreVigil sigue esta regla:

**Medir primero; optimizar despues.**

Un consumo puntual no basta para justificar un cambio. Se buscan tendencias sostenidas y diferencias reproducibles.

## Validacion prolongada completada

La validacion prolongada Full/Mini quedo completada y analizada.

### HUD Full

Sesion analizada: `runtime-20261109-115635.csv`.

Resultados registrados:
- duracion aproximada: 2 h 06 min;
- 7455 muestras;
- CPU propia promedio: aproximadamente 0.224 %;
- mediana de CPU: 0.130 %;
- P95 de CPU: aproximadamente 0.519 %;
- P99 de CPU: aproximadamente 0.766 %;
- maximo aislado de CPU: 2.939 %;
- intervalo medio: aproximadamente 1014.34 ms;
- P95 del intervalo: aproximadamente 1024.91 ms;
- Working Set promedio: aproximadamente 179.58 MB;
- el Working Set termino por debajo del valor inicial;
- heap administrado promedio: aproximadamente 9.06 MB;
- no se observo crecimiento progresivo del heap.

### HUD Mini

Sesion analizada: `runtime-20261110-075548.csv`.

Resultados registrados:
- duracion aproximada: 2 h 48 min;
- 9940 muestras;
- CPU propia promedio: aproximadamente 0.168 %;
- mediana de CPU: 0.129 %;
- P95 de CPU: aproximadamente 0.390 %;
- P99 de CPU: aproximadamente 0.646 %;
- Working Set promedio: aproximadamente 164.53 MB;
- Working Set final: aproximadamente 168.65 MB;
- heap administrado promedio: aproximadamente 9.25 MB;
- no se observo crecimiento progresivo del heap;
- intervalo promedio: aproximadamente 1012.83 ms;
- se observo un pico aislado del intervalo de aproximadamente 5523 ms;
- dos lecturas invalidas de heap fueron marcadas correctamente con `IsManagedHeapReadingValid=False`.

## Dictamen

Los modos Full y Mini se consideran estables con la evidencia disponible. No se observo evidencia clara de fugas ni degradacion progresiva durante las pruebas prolongadas.

La Prioridad 0 de validacion prolongada queda tecnicamente completada. Los resultados no justifican una optimizacion basada unicamente en intuicion.

## Resultado historico anterior

Una sesion previa de aproximadamente 1 h 42 min registro CPU propia alrededor de 0.176 % de promedio, P95 aproximado de 0.389 %, intervalo cercano a 1015 ms, Working Set alrededor de 173 MB y heap administrado alrededor de 10.7 MB, sin evidencia de crecimiento progresivo en esa muestra.

Este resultado se conserva como referencia historica.

## Regla de interpretacion

No atribuir diferencias Full/Mini al modo visual si las sesiones tuvieron cargas de trabajo diferentes. Si un resultado futuro es ambiguo, repetir con una prueba mas larga o condiciones mas controladas.
