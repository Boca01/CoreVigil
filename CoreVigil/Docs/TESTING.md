# Pruebas de CoreVigil

## Estrategia actual

CoreVigil ha completado la validacion funcional y prolongada principal de los modos Full y Mini. Siguen predominando las pruebas manuales y el diagnostico runtime.

No se debe presentar una validacion manual como equivalente a una suite automatizada. El siguiente frente es convertir la logica adecuada en pruebas automatizadas y utilizar la evidencia diagnostica como criterio de regresion.

## Validaciones manuales realizadas

### Ventana y ciclo de vida

- La aplicacion permanece residente en bandeja al ocultar la ventana.
- La salida explicita cierra el proceso.
- La instancia unica ha sido validada: una segunda ejecucion no abre otro CoreVigil.

### HUD Full

- CPU global visible y actualizable.
- RAM visible y actualizable.
- Frecuencia visible.
- Uptime de sistema y aplicacion.
- Actividad CPU expandible/contraible.
- Conservacion del borde inferior al cambiar la altura.
- Topmost.
- Translucidez.

### HUD Mini

- CPU y RAM visibles.
- LED dinamico.
- Regreso a Full.
- Topmost heredado de la ventana.
- Translucidez.
- Movimiento independiente.
- Restauracion de posicion independiente.
- El modo Mini no aparece en la barra de tareas y continua accesible desde la bandeja.

### Opacidad

- La opcion `Opacidad...` esta disponible desde la bandeja.
- El selector aparece de forma temporal.
- El deslizador modifica la opacidad de fondo en tiempo real.
- Full y Mini comparten el valor configurado.
- El rango funcional utilizado es de 30 % a 100 %.
- Un clic fuera cierra el selector.
- La cabecera permite mover el selector sin interferir con el deslizador.
- La opacidad persiste despues de reiniciar CoreVigil.
- La posicion personalizada del selector persiste despues de reiniciar CoreVigil.

### Persistencia

- Full y Mini conservan posiciones diferentes durante la ejecucion.
- Estado persistente ampliado para Full/Mini.
- Estado de expansion de Actividad CPU persistente.
- Topmost persistente.
- Modo Mini/Full persistente.
- Opacidad de fondo persistente.
- Offsets del selector de opacidad persistentes.

### Procesos

- Vista INDIVIDUAL.
- Vista AGRUPAR.
- Grupos desplegables.
- PID por instancia.
- Top 8 limitado visualmente.
- Top 8 corregido para medir primero todas las instancias disponibles y aplicar despues el limite individual o agrupado.
- Comparacion visual con Administrador de tareas realizada durante actividad real.

## Pruebas prolongadas completadas

### Full

Sesion: `runtime-20261109-115635.csv`.

- Duracion aproximada: 2 h 06 min.
- 7455 muestras.
- CPU propia promedio: aproximadamente 0.224 %.
- P95: aproximadamente 0.519 %.
- P99: aproximadamente 0.766 %.
- Intervalo medio: aproximadamente 1014.34 ms.
- Working Set promedio: aproximadamente 179.58 MB.
- Heap administrado promedio: aproximadamente 9.06 MB.
- Sin evidencia de crecimiento progresivo.

### Mini

Sesion: `runtime-20261110-075548.csv`.

- Duracion aproximada: 2 h 48 min.
- 9940 muestras.
- CPU propia promedio: aproximadamente 0.168 %.
- P95: aproximadamente 0.390 %.
- P99: aproximadamente 0.646 %.
- Intervalo promedio: aproximadamente 1012.83 ms.
- Working Set promedio: aproximadamente 164.53 MB.
- Heap administrado promedio: aproximadamente 9.25 MB.
- Sin evidencia de crecimiento progresivo.
- Pico aislado de intervalo de aproximadamente 5523 ms.
- Dos lecturas invalidas de heap fueron correctamente identificadas con `IsManagedHeapReadingValid=False`.

## Dictamen de estabilidad

Full y Mini son estables con la evidencia disponible. No existe evidencia clara de fugas ni degradacion progresiva en las sesiones analizadas.

La Prioridad 0 de validacion queda tecnicamente completada.

## Pruebas pendientes recomendadas

- Convertir hallazgos diagnosticos en criterios de regresion.
- Agregar pruebas automatizadas para servicios puros y logica independiente de WPF/Windows.
- Priorizar agrupacion de procesos, persistencia y transformaciones de datos.
- Validacion multimonitor y DPI mixto.
- Cambio de resolucion/escala mientras CoreVigil esta abierto.
- Restauracion tras cambios de disposicion de monitores.
- Pruebas especificas de procesos que nacen y terminan entre muestras.
- Repetir sesiones prolongadas solo si aparece nueva evidencia que lo justifique.

## Criterio de estabilidad

Una caracteristica puede considerarse estable cuando:
- compila sin errores;
- su comportamiento objetivo se reproduce consistentemente;
- no rompe Full, Mini, bandeja o persistencia;
- las pruebas relevantes no muestran degradacion atribuible al cambio.
