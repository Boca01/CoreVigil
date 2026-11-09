# Pruebas de CoreVigil

## Estrategia actual

CoreVigil se encuentra en una fase de validacion funcional y observacion prolongada. Actualmente predominan las pruebas manuales y el diagnostico runtime.

No se debe presentar una validacion manual como equivalente a una suite automatizada.

## Validaciones manuales realizadas

### Ventana y ciclo de vida

- La aplicacion permanece residente en bandeja al ocultar la ventana.
- La salida explicita cierra el proceso.
- La instancia unica ha sido validada: una segunda ejecucion no abre otro CoreVigil.

### HUD Full

- CPU global visible y actualizable.
- RAM visible y actualizable.
- frecuencia visible.
- uptime de sistema y aplicacion.
- Actividad CPU expandible/contraible.
- conservacion del borde inferior al cambiar la altura.
- Topmost.
- translucidez.

### HUD Mini

- CPU y RAM visibles.
- LED dinamico.
- regreso a Full.
- Topmost heredado de la ventana.
- translucidez.
- movimiento independiente.
- restauracion de posicion independiente.

### Persistencia

- Full y Mini conservan posiciones diferentes durante la ejecucion.
- estado persistente ampliado para Full/Mini.
- estado de expansion de Actividad CPU persistente.
- Topmost persistente.
- modo Mini/Full persistente.

### Procesos

- vista INDIVIDUAL.
- vista AGRUPAR.
- grupos desplegables.
- PID por instancia.
- Top 8 limitado visualmente.
- Top 8 corregido para medir primero todas las instancias disponibles y aplicar despues el limite individual o agrupado.
- comparacion visual con Administrador de tareas realizada durante actividad real.

## Prueba prolongada en curso

### Fase A: Full

Duracion planificada: 2 horas.

Condiciones:

- HUD Full visible.
- diagnostico interno activado.
- uso normal del equipo.

### Fase B: Mini

Duracion planificada: 2 horas.

Condiciones:

- HUD Mini visible.
- diagnostico interno activado.
- uso normal comparable a la fase A.

## Datos a revisar

- CPU propia.
- Picos y percentiles de CPU cuando sea util.
- Working Set.
- heap administrado.
- intervalo efectivo de muestreo.
- estabilidad temporal.
- diferencias Full/Mini.

## Pruebas pendientes recomendadas

- Validacion multimonitor y DPI mixto.
- Cambio de resolucion/escala mientras CoreVigil esta abierto.
- Restauracion tras cambios de disposicion de monitores.
- Sesion prolongada de 6 a 8 horas si la prueba actual deja tendencias ambiguas.
- Pruebas automatizadas para servicios puros y logica de agrupacion/persistencia.
- Pruebas especificas de procesos que nacen y terminan entre muestras.

## Criterio de estabilidad

Una caracteristica puede considerarse estable cuando:

1. compila sin errores;
2. su comportamiento objetivo se reproduce consistentemente;
3. no rompe Full, Mini, bandeja o persistencia;
4. una prueba prolongada no muestra degradacion relevante atribuible al cambio.
