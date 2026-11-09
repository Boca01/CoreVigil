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

El flujo vigente realiza flush periodico y un flush final al detener el diagnostico limpiamente.

## Activacion

El diagnostico se puede activar o desactivar desde la bandeja de CoreVigil. La bandeja tambien dispone de una accion para abrir la carpeta de logs.

## Principio de trabajo

CoreVigil sigue esta regla:

> Medir primero; optimizar despues.

Un consumo puntual no basta para justificar un cambio. Se buscan tendencias sostenidas y diferencias reproducibles.

## Prueba prolongada actual

Estado: EN CURSO.

Plan de validacion:

1. 2 horas con HUD Full visible.
2. 2 horas con HUD Mini visible.
3. Diagnostico interno activado durante ambas fases.
4. Uso normal del equipo, intentando mantener condiciones comparables.
5. Analisis posterior de los CSV.

## Criterios de analisis

Cuando finalicen las sesiones se compararan:

- CPU propia media y distribucion de picos;
- Working Set inicial, final y tendencia;
- heap administrado inicial, final y tendencia;
- estabilidad del intervalo de muestreo;
- diferencias Full frente a Mini;
- comportamiento mientras el HUD permanece visible;
- cualquier crecimiento sostenido que pueda indicar fuga o retencion innecesaria.

## Resultado anterior disponible

Una sesion previa de aproximadamente 1 h 42 min fue analizada antes de la actual ronda de cambios. El checkpoint conservado registra CPU propia estabilizada alrededor de 0.176 % de promedio, P95 aproximado de 0.389 %, intervalo cercano a 1015 ms, Working Set alrededor de 173 MB y heap administrado alrededor de 10.7 MB, sin evidencia de crecimiento progresivo en esa muestra.

Ese resultado es una referencia historica, no una sustitucion de la prueba actual Full/Mini.

## Regla de interpretacion

No atribuir diferencias Full/Mini al modo visual si las sesiones tuvieron cargas de trabajo radicalmente diferentes. Si el resultado es ambiguo, repetir con una prueba mas larga o condiciones mas controladas.
